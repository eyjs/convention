<template>
  <div>
    <AdminPageHeader
      title="행사 관리"
      :description="`전체 ${conventions.length}개 행사`"
    >
      <AdminButton :icon="Plus" @click="showCreateModal = true">
        새 행사 만들기
      </AdminButton>
    </AdminPageHeader>

    <!-- 로딩 -->
    <div v-if="loading" class="text-center py-12 mt-6">
      <div
        class="inline-block w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"
      />
      <p class="text-gray-600 mt-4">로딩 중...</p>
    </div>

    <!-- 빈 상태 -->
    <div v-else-if="conventions.length === 0" class="mt-6">
      <AdminEmptyState
        :icon="Calendar"
        title="등록된 행사가 없습니다"
        description="새 행사를 만들어 시작하세요."
      >
        <AdminButton :icon="Plus" @click="showCreateModal = true">
          새 행사 만들기
        </AdminButton>
      </AdminEmptyState>
    </div>

    <template v-else>
      <!-- 진행중인 행사 -->
      <div v-if="activeConventions.length > 0" class="mt-6 mb-8">
        <h3 class="text-lg font-bold text-gray-900 mb-3 px-1">
          진행중인 스타투어
        </h3>
        
        <!-- Mobile: Horizontal Scroll, Desktop: Grid -->
        <div class="md:grid md:gap-4 md:grid-cols-2 lg:grid-cols-3 overflow-x-auto -mx-4 px-4 md:mx-0 md:px-0 scrollbar-hide">
          <div class="flex md:contents gap-4 pb-2">
            <div
              v-for="convention in activeConventions"
              :key="convention.id"
              class="flex-shrink-0 w-[260px] md:w-auto bg-white rounded-xl shadow-sm hover:shadow-md transition-all cursor-pointer overflow-hidden group border border-gray-100"
              @click="goToConvention(convention.id)"
            >
              <div class="relative h-[140px] overflow-hidden">
                <div
                  v-if="convention.conventionImg"
                  class="absolute inset-0 bg-cover bg-center group-hover:scale-105 transition-transform duration-500"
                  :style="{
                    backgroundImage: `url(${convention.conventionImg})`,
                  }"
                ></div>
                <div
                  v-else
                  class="absolute inset-0"
                  :style="getGradientStyle(convention.brandColor)"
                ></div>
                
                <div class="absolute top-2 right-2 flex items-center gap-1.5">
                  <div
                    v-if="getDDay(convention.startDate) > 0"
                    class="px-2 py-0.5 bg-black/50 backdrop-blur-sm text-white text-[10px] font-bold rounded-full"
                  >
                    D-{{ getDDay(convention.startDate) }}
                  </div>
                  <div
                    class="px-2 py-0.5 bg-green-500/90 backdrop-blur-sm text-white text-[10px] font-bold rounded-full"
                  >
                    진행중
                  </div>
                </div>

                <div
                  class="absolute top-2 left-2 px-1.5 py-0.5 text-[10px] font-bold rounded-full shadow-sm"
                  :class="
                    convention.conventionType === 'OVERSEAS'
                      ? 'bg-sky-500 text-white'
                      : 'bg-emerald-500 text-white'
                  "
                >
                  {{ convention.conventionType === 'OVERSEAS' ? '해외' : '국내' }}
                </div>

                <div class="absolute bottom-0 left-0 right-0 p-3 bg-gradient-to-t from-black/60 to-transparent">
                  <h4 class="text-white font-bold text-sm truncate">
                    {{ convention.title }}
                  </h4>
                </div>
              </div>
              
              <div class="p-3">
                <div class="flex flex-col gap-1 text-[11px] text-gray-500 mb-3">
                  <div class="flex items-center">
                    <Calendar :size="12" class="mr-1.5 text-gray-400" />
                    {{ formatDate(convention.startDate) }} ~ {{ formatDate(convention.endDate) }}
                  </div>
                  <div class="flex items-center">
                    <Users :size="12" class="mr-1.5 text-gray-400" />
                    참석자 {{ convention.guestCount?.toLocaleString() }}명
                  </div>
                </div>
                
                <div class="flex gap-2">
                  <button
                    class="flex-1 py-1.5 bg-gray-50 text-gray-600 rounded-lg font-medium hover:bg-gray-100 transition-colors text-xs border border-gray-200"
                    @click.stop="editConvention(convention)"
                  >
                    수정
                  </button>
                  <button
                    class="flex-1 py-1.5 bg-rose-50 text-rose-600 rounded-lg font-medium hover:bg-rose-100 transition-colors text-xs border border-rose-100"
                    @click.stop="completeConvention(convention.id)"
                  >
                    종료
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 지난 행사 -->
      <div v-if="completedConventions.length > 0" class="mt-4 mb-8">
        <h3 class="text-lg font-bold text-gray-400 mb-3 px-1">
          지난 스타투어
        </h3>
        
        <div class="md:grid md:gap-4 md:grid-cols-2 lg:grid-cols-3 overflow-x-auto -mx-4 px-4 md:mx-0 md:px-0 scrollbar-hide">
          <div class="flex md:contents gap-3 pb-2">
            <div
              v-for="convention in completedConventions"
              :key="convention.id"
              class="flex-shrink-0 w-[240px] md:w-auto bg-white rounded-xl shadow-sm hover:shadow-md transition-shadow cursor-pointer overflow-hidden opacity-80 border border-gray-100"
              @click="goToConvention(convention.id)"
            >
              <div class="relative h-[120px] overflow-hidden">
                <div
                  v-if="convention.conventionImg"
                  class="absolute inset-0 bg-cover bg-center grayscale"
                  :style="{
                    backgroundImage: `url(${convention.conventionImg})`,
                  }"
                ></div>
                <div
                  v-else
                  class="absolute inset-0 bg-gradient-to-br from-gray-400 to-gray-500"
                ></div>
                
                <div class="absolute top-2 left-2 flex items-center gap-1">
                  <span
                    class="px-1.5 py-0.5 bg-black/50 text-white text-[10px] rounded backdrop-blur-sm"
                  >
                    종료
                  </span>
                  <span
                    class="px-1.5 py-0.5 text-[10px] font-bold rounded backdrop-blur-sm"
                    :class="
                      convention.conventionType === 'OVERSEAS'
                        ? 'bg-sky-500/90 text-white'
                        : 'bg-emerald-500/90 text-white'
                    "
                  >
                    {{ convention.conventionType === 'OVERSEAS' ? '해외' : '국내' }}
                  </span>
                </div>
                
                <div class="absolute bottom-0 left-0 right-0 p-3 bg-gradient-to-t from-black/40 to-transparent">
                  <h4 class="text-white font-bold text-sm truncate">
                    {{ convention.title }}
                  </h4>
                </div>
              </div>
              
              <div class="p-3">
                <p class="text-[11px] text-gray-400 mb-3">
                  {{ formatDate(convention.startDate) }} 종료
                </p>
                
                <div class="flex gap-2">
                  <button
                    class="flex-1 py-1.5 bg-gray-50 text-gray-600 rounded-lg font-medium hover:bg-gray-100 transition-colors text-xs border border-gray-200"
                    @click.stop="editConvention(convention)"
                  >
                    수정
                  </button>
                  <button
                    class="flex-1 py-1.5 bg-blue-50 text-blue-600 rounded-lg font-medium hover:bg-blue-100 transition-colors text-xs border border-blue-100"
                    @click.stop="completeConvention(convention.id)"
                  >
                    재개
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>

    <ConventionFormModal
      v-if="showCreateModal"
      :convention="editingConvention"
      @close="showCreateModal = false"
      @save="handleSaveConvention"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/services/api'
import { Plus, Calendar, Users } from 'lucide-vue-next'
import ConventionFormModal from '@/components/admin/ConventionFormModal.vue'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminButton from '@/components/admin/ui/AdminButton.vue'
import AdminEmptyState from '@/components/admin/ui/AdminEmptyState.vue'

const router = useRouter()

const conventions = ref([])
const loading = ref(false)
const showCreateModal = ref(false)
const editingConvention = ref(null)

function isConventionEnded(c) {
  if (c.completeYn === 'Y') return true
  if (!c.endDate) return false
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const end = new Date(c.endDate)
  end.setHours(0, 0, 0, 0)
  return end < today
}

const activeConventions = computed(() =>
  conventions.value
    .filter((c) => !isConventionEnded(c))
    .sort((a, b) => new Date(a.startDate) - new Date(b.startDate)),
)

const completedConventions = computed(() =>
  conventions.value
    .filter((c) => isConventionEnded(c))
    .sort((a, b) => new Date(b.startDate) - new Date(a.startDate)),
)

function getDDay(startDate) {
  if (!startDate) return 0
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const start = new Date(startDate)
  start.setHours(0, 0, 0, 0)
  const diff = Math.ceil((start - today) / (1000 * 60 * 60 * 24))
  return diff > 0 ? diff : 0
}

async function handleSaveConvention(conventionData) {
  try {
    if (editingConvention.value) {
      await apiClient.put(
        `/conventions/${editingConvention.value.id}`,
        conventionData,
      )
    } else {
      await apiClient.post('/conventions', conventionData)
    }
    showCreateModal.value = false
    editingConvention.value = null
    await loadConventions()
  } catch (error) {
    console.error('Failed to save convention:', error)
    alert('행사 저장에 실패했습니다.')
  }
}

async function editConvention(convention) {
  try {
    const res = await apiClient.get(`/conventions/${convention.id}`)
    editingConvention.value = res.data
    showCreateModal.value = true
  } catch (e) {
    console.error('행사 상세 로드 실패:', e)
    editingConvention.value = { ...convention }
    showCreateModal.value = true
  }
}

async function completeConvention(conventionId) {
  if (!confirm('행사를 종료/재개 처리하시겠습니까?')) return

  try {
    await apiClient.post(`/conventions/${conventionId}/complete`)
    await loadConventions()
  } catch (error) {
    console.error('Failed to complete convention:', error)
    alert('행사 종료 처리에 실패했습니다.')
  }
}

async function loadConventions() {
  loading.value = true
  try {
    const response = await apiClient.get('/conventions')
    conventions.value = response.data
  } catch (error) {
    console.error('Failed to load conventions:', error)
  } finally {
    loading.value = false
  }
}

function goToConvention(conventionId) {
  router.push(`/admin/conventions/${conventionId}`)
}

function formatDate(dateString) {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('ko-KR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  })
}

function getGradientStyle(color) {
  const c = color || '#6366f1'
  const r = parseInt(c.slice(1, 3), 16)
  const g = parseInt(c.slice(3, 5), 16)
  const b = parseInt(c.slice(5, 7), 16)
  const dr = Math.floor(r * 0.7)
  const dg = Math.floor(g * 0.7)
  const db = Math.floor(b * 0.7)
  return {
    background: `linear-gradient(135deg, rgb(${r},${g},${b}), rgb(${dr},${dg},${db}))`,
  }
}

function adjustColor(color, amount) {
  if (!color) return '#555'
  const num = parseInt(color.replace('#', ''), 16)
  const r = Math.max(0, Math.min(255, (num >> 16) + amount))
  const g = Math.max(0, Math.min(255, ((num >> 8) & 0x00ff) + amount))
  const b = Math.max(0, Math.min(255, (num & 0x0000ff) + amount))
  return '#' + ((r << 16) | (g << 8) | b).toString(16).padStart(6, '0')
}

onMounted(() => {
  loadConventions()
})
</script>

<style scoped>
.scrollbar-hide {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
.scrollbar-hide::-webkit-scrollbar {
  display: none;
}
</style>
