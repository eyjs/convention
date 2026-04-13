<template>
  <div class="relative">
    <button class="p-2 rounded-full hover:bg-white/20 transition-colors relative" @click="toggle">
      <svg class="w-6 h-6" :class="textClass" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
      </svg>
      <span v-if="unreadCount > 0" class="absolute -top-0.5 -right-0.5 w-5 h-5 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center">
        {{ unreadCount > 99 ? '99+' : unreadCount }}
      </span>
    </button>

    <!-- 알림 목록 -->
    <SlideUpModal :is-open="isOpen" @close="isOpen = false">
      <template #header-title>
        알림 <span v-if="unreadCount > 0" class="text-red-500">({{ unreadCount }})</span>
      </template>
      <template #header-left>
        <button v-if="unreadCount > 0" class="text-xs text-blue-600" @click="markAllRead">전체 읽음</button>
      </template>
      <template #body>
        <div v-if="notifications.length === 0" class="text-center text-gray-400 py-12">알림이 없습니다</div>
        <div v-else class="space-y-2">
          <div
            v-for="n in notifications" :key="n.id"
            class="p-3 rounded-lg cursor-pointer transition-colors"
            :class="n.isRead ? 'bg-white' : 'bg-blue-50'"
            @click="onTap(n)"
          >
            <div class="flex items-start gap-2">
              <span class="text-lg flex-shrink-0">{{ typeIcon(n.type) }}</span>
              <div class="flex-1 min-w-0">
                <p class="font-semibold text-gray-900 text-sm" :class="{ 'font-bold': !n.isRead }">{{ n.title }}</p>
                <p class="text-xs text-gray-600 line-clamp-2 mt-0.5">{{ n.body }}</p>
                <p class="text-[10px] text-gray-400 mt-1">{{ timeAgo(n.createdAt) }}</p>
              </div>
              <span v-if="!n.isRead" class="w-2 h-2 rounded-full bg-blue-500 flex-shrink-0 mt-1.5"></span>
            </div>
          </div>
        </div>
      </template>
    </SlideUpModal>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/services/api'
import SlideUpModal from '@/components/common/SlideUpModal.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
  textClass: { type: String, default: 'text-white' },
})

const router = useRouter()
const isOpen = ref(false)
const notifications = ref([])
const unreadCount = ref(0)
let pollTimer = null

const typeIcons = { TEXT: '💬', NOTICE: '📢', SURVEY: '📋', SCHEDULE: '📅', SEAT: '💺', LINK: '🔗' }
function typeIcon(type) { return typeIcons[type] || '💬' }

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
    const res = await apiClient.get(`/notifications/my/unread-count?conventionId=${props.conventionId}`)
    unreadCount.value = res.data?.count || 0
  } catch {}
}

async function loadNotifications() {
  try {
    const res = await apiClient.get(`/notifications/my?conventionId=${props.conventionId}`)
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
    await apiClient.put(`/notifications/read-all?conventionId=${props.conventionId}`)
    notifications.value.forEach((n) => { n.isRead = true })
    unreadCount.value = 0
  } catch {}
}

onMounted(() => {
  loadUnread()
  pollTimer = setInterval(loadUnread, 30000) // 30초 폴링
})
onUnmounted(() => { if (pollTimer) clearInterval(pollTimer) })
</script>
