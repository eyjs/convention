<template>
  <div v-show="isOpen" class="fixed inset-0 z-50">
    <!-- Overlay -->
    <Transition name="fade">
      <div
        v-if="isOpen"
        class="absolute inset-0 bg-black/50"
        @click="close"
      ></div>
    </Transition>

    <!-- Sidebar -->
    <Transition name="slide-right">
      <div
        v-show="isOpen"
        class="absolute right-0 w-72 h-full bg-white shadow-xl flex flex-col"
      >
        <!-- 헤더: 사용자 정보 -->
        <div class="p-5 pb-4">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-3 min-w-0">
              <div
                class="w-10 h-10 bg-primary-100 text-primary-700 rounded-full flex items-center justify-center text-sm font-bold flex-shrink-0"
              >
                {{ userInitial }}
              </div>
              <div class="min-w-0">
                <p class="font-semibold text-gray-900 truncate">
                  {{ userName }}
                </p>
                <p class="text-xs text-gray-500 truncate">{{ userLoginId }}</p>
              </div>
            </div>
            <button
              class="p-1.5 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors flex-shrink-0"
              @click="close"
            >
              <X :size="20" />
            </button>
          </div>
        </div>

        <!-- 메뉴 목록 -->
        <div class="flex-1 overflow-y-auto px-3">
          <nav class="space-y-0.5">
            <router-link
              to="/home"
              class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 hover:bg-gray-50 rounded-lg transition-colors"
              @click="close"
            >
              <Home :size="18" class="text-gray-400" />
              <span>홈</span>
            </router-link>

            <router-link
              to="/my-profile"
              class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 hover:bg-gray-50 rounded-lg transition-colors"
              @click="close"
            >
              <UserIcon :size="18" class="text-gray-400" />
              <span>내 정보</span>
            </router-link>

            <!-- 내 여행 메뉴 숨김 (스타투어만 오픈)
            <router-link
              to="/trips"
              class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 hover:bg-gray-50 rounded-lg transition-colors"
              @click="close"
            >
              <Globe :size="18" class="text-gray-400" />
              <span>내 여행</span>
            </router-link>
            -->

            <!-- 동적 추가 메뉴 -->
            <router-link
              v-for="action in menuActions"
              :key="action.id"
              :to="action.mapsTo"
              style="display: none"
              class="flex items-center justify-between px-3 py-2.5 text-sm text-gray-700 hover:bg-gray-50 rounded-lg transition-colors"
              @click="close"
            >
              <span>{{ action.title }}</span>
              <span
                :class="[
                  'px-2 py-0.5 text-xs font-medium rounded',
                  action.isComplete
                    ? 'bg-[#17B185]/10 text-[#17B185]'
                    : 'bg-gray-100 text-gray-600',
                ]"
              >
                {{ action.isComplete ? '완료' : '미완료' }}
              </span>
            </router-link>

            <router-link
              v-if="authStore.isAdmin"
              to="/admin"
              class="flex items-center gap-3 px-3 py-2.5 text-sm text-gray-700 hover:bg-gray-50 rounded-lg transition-colors"
              @click="close"
            >
              <Settings :size="18" class="text-gray-400" />
              <span>관리자 페이지</span>
            </router-link>
          </nav>
        </div>

        <!-- 로그아웃 (하단 nav + safe-area 고려) -->
        <div class="border-t border-gray-200 p-3 pb-safe">
          <button
            class="flex items-center gap-3 w-full px-3 py-2.5 text-sm text-red-600 hover:bg-red-50 rounded-lg transition-colors"
            @click="handleLogout"
          >
            <LogOut :size="18" />
            <span>로그아웃</span>
          </button>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import { useRouter } from 'vue-router'
import { Home, User as UserIcon, Settings, LogOut, X } from 'lucide-vue-next'
import apiClient from '@/services/api'

const props = defineProps({
  isOpen: {
    type: Boolean,
    required: true,
  },
})

const emit = defineEmits(['close'])

const authStore = useAuthStore()
const conventionStore = useConventionStore()
const router = useRouter()
const menuActions = ref([])

const userName = computed(() => authStore.user?.name || '사용자')
const userLoginId = computed(() => authStore.user?.loginId || '')
const userInitial = computed(() => {
  const name = authStore.user?.name || authStore.user?.loginId || '?'
  return name.charAt(0).toUpperCase()
})

const close = () => {
  emit('close')
}

const handleLogout = async () => {
  if (confirm('로그아웃하시겠습니까?')) {
    await authStore.logout()
    router.push('/login')
    close()
  }
}

const loadMenuActions = async () => {
  try {
    const conventionId = conventionStore.currentConvention?.id
    if (!conventionId) return

    const [actionsResponse, statusesResponse] = await Promise.all([
      apiClient.get(`/conventions/${conventionId}/actions/menu`),
      apiClient.get(`/conventions/${conventionId}/actions/statuses`),
    ])

    const actions = actionsResponse.data || []
    const statuses = statusesResponse.data || []

    const statusMap = new Map(statuses.map((s) => [s.conventionActionId, s]))

    menuActions.value = actions.map((action) => ({
      ...action,
      isComplete: statusMap.get(action.id)?.isComplete || false,
    }))
  } catch (error) {
    console.error('Failed to load menu actions:', error)
    menuActions.value = []
  }
}

watch(
  () => props.isOpen,
  (isOpen) => {
    if (isOpen) {
      loadMenuActions()
    }
  },
)

onMounted(() => {
  if (props.isOpen) {
    loadMenuActions()
  }
})
</script>

<style scoped>
.slide-right-enter-active,
.slide-right-leave-active {
  transition: transform 0.3s ease-in-out;
}

.slide-right-enter-from,
.slide-right-leave-to {
  transform: translateX(100%);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease-in-out;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* 하단 nav(64px) + safe-area 여백 */
.pb-safe {
  padding-bottom: calc(64px + 12px);
}

@supports (padding-bottom: env(safe-area-inset-bottom)) {
  .pb-safe {
    padding-bottom: calc(64px + 12px + env(safe-area-inset-bottom));
  }
}
</style>
