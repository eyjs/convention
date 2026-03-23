<template>
  <div>
    <AdminPageHeader
      title="행사 관리"
      :description="`전체 ${conventions.length}개 행사`"
    >
      <AdminButton
        variant="secondary"
        :icon="Printer"
        @click="$router.push('/admin/name-tag-printing')"
      >
        명찰 일괄 인쇄
      </AdminButton>
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

    <!-- 행사 목록 -->
    <div v-else class="grid gap-6 md:grid-cols-2 lg:grid-cols-3 mt-6">
      <div
        v-for="convention in conventions"
        :key="convention.id"
        class="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow overflow-hidden cursor-pointer"
        @click="goToConvention(convention.id)"
      >
        <div
          class="h-32 relative"
          :style="{
            background: `linear-gradient(135deg, ${convention.brandColor || '#6366f1'} 0%, ${adjustColor(convention.brandColor || '#6366f1', -20)} 100%)`,
          }"
        >
          <div class="absolute top-3 right-3">
            <span
              v-if="convention.completeYn === 'Y'"
              class="px-2 py-1 bg-gray-800/50 text-white text-xs rounded-full"
            >
              종료
            </span>
            <span
              v-else
              class="px-2 py-1 bg-green-500/80 text-white text-xs rounded-full"
            >
              진행중
            </span>
          </div>
          <div class="absolute bottom-4 left-4 right-4">
            <h3 class="text-white font-bold text-lg truncate">
              {{ convention.title }}
            </h3>
          </div>
        </div>
        <div class="p-4">
          <div class="space-y-2 text-sm text-gray-600">
            <div class="flex items-center">
              <Calendar :size="16" class="mr-2" />
              {{ formatDate(convention.startDate) }} ~
              {{ formatDate(convention.endDate) }}
            </div>
            <div class="flex items-center">
              <Users :size="16" class="mr-2" />
              참석자 {{ convention.guestCount }}명
            </div>
          </div>
          <div class="mt-4 pt-4 border-t flex justify-end space-x-2">
            <AdminButton
              variant="secondary"
              size="sm"
              @click.stop="editConvention(convention)"
            >
              수정
            </AdminButton>
            <!-- prettier-ignore -->
            <AdminButton
              :variant="convention.completeYn === 'Y' ? 'primary' : 'danger'"
              size="sm"
              @click.stop="completeConvention(convention.id)"
            >
              {{ convention.completeYn === 'Y' ? '재개' : '종료' }}
            </AdminButton>
          </div>
        </div>
      </div>
    </div>

    <ConventionFormModal
      v-if="showCreateModal"
      :convention="editingConvention"
      @close="showCreateModal = false"
      @save="handleSaveConvention"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import apiClient from '@/services/api'
import { Plus, Printer, Calendar, Users } from 'lucide-vue-next'
import ConventionFormModal from '@/components/admin/ConventionFormModal.vue'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminButton from '@/components/admin/ui/AdminButton.vue'
import AdminEmptyState from '@/components/admin/ui/AdminEmptyState.vue'

const router = useRouter()

const conventions = ref([])
const loading = ref(false)
const showCreateModal = ref(false)
const editingConvention = ref(null)

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

function editConvention(convention) {
  editingConvention.value = { ...convention }
  showCreateModal.value = true
}

async function completeConvention(conventionId) {
  if (!confirm('행사를 종료 처리하시겠습니까?')) return

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
