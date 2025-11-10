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
        class="absolute right-0 w-72 h-full bg-white shadow-xl"
      >
        <div class="p-4">
          <h2 class="text-xl font-bold">메뉴</h2>
          <button
            @click="close"
            class="absolute top-4 right-4 text-gray-500 hover:text-gray-700"
          >
            ✕
          </button>
        </div>
        <ul>
          <li>
            <router-link
              to="/my-conventions"
              class="block w-full text-left px-4 py-3 text-sm text-gray-700 bg-gray-50 hover:bg-gray-200 rounded-md mb-2 shadow-sm"
            >
              행사 리스트
            </router-link>
          </li>

          <!-- 동적 추가 메뉴 -->
          <li v-for="action in menuActions" :key="action.id">
            <router-link
              :to="action.mapsTo"
              @click="close"
              class="block w-full text-left px-4 py-3 text-sm text-gray-700 hover:bg-gray-100 rounded-md mb-2"
            >
              <div class="flex items-center justify-between">
                <span>{{ action.title }}</span>
                <span
                  :class="[
                    'px-2 py-0.5 text-xs font-medium rounded ml-2',
                    action.isComplete
                      ? 'bg-[#17B185]/10 text-[#17B185]'
                      : 'bg-gray-100 text-gray-600'
                  ]"
                >
                  {{ action.isComplete ? '완료' : '미완료' }}
                </span>
              </div>
            </router-link>
          </li>

          <li v-if="authStore.isAdmin">
            <router-link
              to="/admin"
              class="block w-full text-left px-4 py-3 text-sm text-gray-700 hover:bg-gray-100 rounded-md mb-2"
            >
              관리자 페이지
            </router-link>
          </li>
          <li>
            <button
              @click="handleLogout"
              class="block w-full text-left px-4 py-3 text-sm text-red-600 hover:bg-gray-100 rounded-md mb-2"
            >
              로그아웃
            </button>
          </li>
        </ul>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import { useRouter } from 'vue-router'
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

    // 메뉴 액션과 상태 정보를 병렬로 가져오기
    const [actionsResponse, statusesResponse] = await Promise.all([
      apiClient.get(`/conventions/${conventionId}/actions/menu`),
      apiClient.get(`/conventions/${conventionId}/actions/statuses`),
    ])

    const actions = actionsResponse.data || []
    const statuses = statusesResponse.data || []

    // 상태 정보를 맵으로 변환
    const statusMap = new Map(statuses.map((s) => [s.conventionActionId, s]))

    // 액션에 isComplete 정보 추가
    menuActions.value = actions.map((action) => ({
      ...action,
      isComplete: statusMap.get(action.id)?.isComplete || false,
    }))
  } catch (error) {
    console.error('Failed to load menu actions:', error)
    menuActions.value = []
  }
}

// 메뉴가 열릴 때 액션 목록 로드
watch(() => props.isOpen, (isOpen) => {
  if (isOpen) {
    loadMenuActions()
  }
})

// 초기 로드
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
</style>
