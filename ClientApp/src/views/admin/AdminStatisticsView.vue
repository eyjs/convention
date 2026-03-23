<template>
  <div>
    <AdminPageHeader title="통계" description="시스템 전체 통계 및 분석" />

    <div class="grid gap-6 md:grid-cols-2 lg:grid-cols-4 mt-6 mb-6">
      <AdminStatsCard
        label="전체 행사"
        :value="conventions.length"
        :icon="FolderOpen"
        color="primary"
      />
      <AdminStatsCard
        label="진행중 행사"
        :value="activeConventions"
        :icon="CheckCircle"
        color="green"
      />
      <AdminStatsCard
        label="전체 참석자"
        :value="totalGuests"
        :icon="Users"
        color="orange"
      />
      <AdminStatsCard
        label="전체 일정"
        :value="totalSchedules"
        :icon="Calendar"
        color="primary"
      />
    </div>

    <div class="bg-white rounded-lg shadow p-6">
      <p class="text-gray-600">상세 통계 기능 구현 예정</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/services/api'
import { FolderOpen, CheckCircle, Users, Calendar } from 'lucide-vue-next'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminStatsCard from '@/components/admin/ui/AdminStatsCard.vue'

const conventions = ref([])

const activeConventions = computed(
  () => conventions.value.filter((c) => c.completeYn === 'N').length,
)
const totalGuests = computed(() =>
  conventions.value.reduce((sum, c) => sum + (c.guestCount || 0), 0),
)
const totalSchedules = computed(() =>
  conventions.value.reduce((sum, c) => sum + (c.scheduleCount || 0), 0),
)

onMounted(async () => {
  try {
    const response = await apiClient.get('/conventions')
    conventions.value = response.data
  } catch (error) {
    console.error('Failed to load conventions:', error)
  }
})
</script>
