<template>
  <div class="relative">
    <button
      class="p-2 rounded-full hover:bg-white/20 transition-colors relative"
      @click="toggle"
    >
      <svg
        class="w-6 h-6"
        :class="textClass"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
        />
      </svg>
      <span
        v-if="unreadCount > 0"
        class="absolute -top-0.5 -right-0.5 w-5 h-5 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center"
      >
        {{ unreadCount > 99 ? '99+' : unreadCount }}
      </span>
    </button>

    <!-- 알림 목록 (화면 중앙 모달) -->
    <BaseModal :is-open="isOpen" max-width="md" @close="isOpen = false">
      <template #header>
        <div class="flex items-center justify-between w-full pr-8">
          <h3 class="text-lg font-bold">
            알림
            <span v-if="unreadCount > 0" class="text-red-500 text-sm"
              >({{ unreadCount }})</span
            >
          </h3>
          <button
            v-if="unreadCount > 0"
            class="text-xs text-blue-600 hover:underline"
            @click="markAllRead"
          >
            전체 읽음
          </button>
        </div>
      </template>
      <template #body>
        <!-- 탭: 전체 / 미읽음 / 읽음 (기본 미읽음) -->
        <div class="flex gap-1 mb-3">
          <button
            v-for="tab in filterTabs"
            :key="tab.value"
            class="px-3 py-1.5 rounded-full text-sm font-medium transition-colors"
            :class="
              activeFilter === tab.value
                ? 'bg-blue-600 text-white'
                : 'bg-gray-100 text-gray-600'
            "
            @click="activeFilter = tab.value"
          >
            {{ tab.label }}
            <span v-if="tab.count > 0" class="ml-1 text-xs opacity-75">{{
              tab.count
            }}</span>
          </button>
        </div>

        <div
          v-if="filteredNotifications.length === 0"
          class="text-center text-gray-400 py-8"
        >
          {{
            activeFilter === 'unread'
              ? '미읽음 알림이 없습니다'
              : activeFilter === 'read'
                ? '읽음 알림이 없습니다'
                : '알림이 없습니다'
          }}
        </div>
        <div v-else class="space-y-2">
          <div
            v-for="n in filteredNotifications"
            :key="n.id"
            class="p-3 rounded-lg cursor-pointer transition-colors"
            :class="
              n.isRead
                ? 'bg-white hover:bg-gray-50 border'
                : 'bg-blue-50 hover:bg-blue-100 border border-blue-200'
            "
            @click="onTap(n)"
          >
            <div class="flex items-start gap-3">
              <span class="text-xl flex-shrink-0 mt-0.5">{{
                typeIcon(n.type)
              }}</span>
              <div class="flex-1 min-w-0">
                <p
                  class="text-sm"
                  :class="
                    n.isRead ? 'text-gray-700' : 'font-bold text-gray-900'
                  "
                >
                  {{ n.title }}
                </p>
                <p class="text-sm text-gray-600 mt-1">{{ n.body }}</p>
                <div class="flex items-center gap-2 mt-1.5">
                  <p class="text-xs text-gray-400">
                    {{ timeAgo(n.createdAt) }}
                  </p>
                  <span v-if="n.linkUrl" class="text-xs text-blue-500"
                    >자세히 보기 →</span
                  >
                </div>
              </div>
              <span
                v-if="!n.isRead"
                class="w-2.5 h-2.5 rounded-full bg-blue-500 flex-shrink-0 mt-2"
              ></span>
            </div>
          </div>
        </div>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/services/api'
import BaseModal from '@/components/common/BaseModal.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
  textClass: { type: String, default: 'text-white' },
})

const router = useRouter()
const isOpen = ref(false)
const notifications = ref([])
const unreadCount = ref(0)
const activeFilter = ref('unread') // 기본: 미읽음

const filterTabs = computed(() => [
  {
    value: 'unread',
    label: '미읽음',
    count: notifications.value.filter((n) => !n.isRead).length,
  },
  {
    value: 'read',
    label: '읽음',
    count: notifications.value.filter((n) => n.isRead).length,
  },
  { value: 'all', label: '전체', count: notifications.value.length },
])

const filteredNotifications = computed(() => {
  if (activeFilter.value === 'unread')
    return notifications.value.filter((n) => !n.isRead)
  if (activeFilter.value === 'read')
    return notifications.value.filter((n) => n.isRead)
  return notifications.value
})
let pollTimer = null

const typeIcons = {
  TEXT: '💬',
  NOTICE: '📢',
  SURVEY: '📋',
  SCHEDULE: '📅',
  SEAT: '💺',
  LINK: '🔗',
}
function typeIcon(type) {
  return typeIcons[type] || '💬'
}

function timeAgo(dateStr) {
  const diff = (Date.now() - new Date(dateStr).getTime()) / 1000
  if (diff < 60) return '방금 전'
  if (diff < 3600) return `${Math.floor(diff / 60)}분 전`
  if (diff < 86400) return `${Math.floor(diff / 3600)}시간 전`
  return `${Math.floor(diff / 86400)}일 전`
}

function toggle() {
  isOpen.value = !isOpen.value
  if (isOpen.value) loadNotifications()
}

async function loadUnread() {
  try {
    const res = await apiClient.get(
      `/notifications/my/unread-count?conventionId=${props.conventionId}`,
    )
    unreadCount.value = res.data?.count || 0
  } catch {}
}

async function loadNotifications() {
  try {
    const res = await apiClient.get(
      `/notifications/my?conventionId=${props.conventionId}`,
    )
    notifications.value = res.data || []
  } catch {}
}

async function onTap(n) {
  // 읽음 처리
  if (!n.isRead) {
    try {
      await apiClient.put(`/notifications/${n.id}/read`)
      n.isRead = true
      unreadCount.value = Math.max(0, unreadCount.value - 1)
    } catch {}
  }
  // 딥링크 이동
  if (n.linkUrl) {
    isOpen.value = false
    router.push(n.linkUrl)
  }
}

async function markAllRead() {
  try {
    await apiClient.put(
      `/notifications/read-all?conventionId=${props.conventionId}`,
    )
    notifications.value.forEach((n) => {
      n.isRead = true
    })
    unreadCount.value = 0
  } catch {}
}

onMounted(() => {
  loadUnread()
  pollTimer = setInterval(loadUnread, 30000) // 30초 폴링
})
onUnmounted(() => {
  if (pollTimer) clearInterval(pollTimer)
})
</script>
