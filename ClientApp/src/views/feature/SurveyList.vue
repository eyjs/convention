<template>
  <div class="min-h-screen min-h-dvh bg-gray-50 dark:bg-gray-900">
    <MainHeader title="설문조사" :show-back="true" />

    <div class="px-4 py-6 max-w-2xl mx-auto">
      <div v-if="loading" class="text-center py-10">
        <p class="text-gray-500 dark:text-gray-400">
          설문 목록을 불러오는 중...
        </p>
      </div>

      <div v-else-if="surveys.length === 0" class="text-center py-10">
        <div
          class="w-16 h-16 mx-auto mb-4 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center"
        >
          <ClipboardList class="w-8 h-8 text-gray-400" />
        </div>
        <p class="text-gray-500 dark:text-gray-400">
          현재 참여 가능한 설문이 없습니다.
        </p>
      </div>

      <div v-else class="space-y-3">
        <!-- 미완료 설문 먼저 -->
        <router-link
          v-for="survey in sortedSurveys"
          :key="survey.id"
          :to="`/conventions/${conventionId}/surveys/${survey.id}`"
          class="block bg-white dark:bg-gray-800 rounded-lg shadow-sm p-4 hover:shadow-md transition-shadow border border-gray-100 dark:border-gray-700"
        >
          <div class="flex items-start justify-between">
            <div class="flex-1 min-w-0">
              <h3
                class="font-semibold text-gray-900 dark:text-gray-100 truncate"
              >
                {{ survey.title }}
              </h3>
              <p
                v-if="survey.description"
                class="text-sm text-gray-500 dark:text-gray-400 mt-1 line-clamp-2"
              >
                {{ survey.description }}
              </p>
              <p
                v-if="survey.endDate"
                class="text-xs text-gray-400 dark:text-gray-500 mt-2"
              >
                마감: {{ formatDateTime(survey.endDate) }}
              </p>
            </div>
            <span
              class="ml-3 flex-shrink-0 px-2.5 py-1 rounded-full text-xs font-medium"
              :class="getStatusBadgeClass(survey)"
            >
              {{ getStatusLabel(survey) }}
            </span>
          </div>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { ClipboardList } from 'lucide-vue-next'
import api from '@/services/api'
import { formatDateTime } from '@/utils/date'
import MainHeader from '@/components/common/MainHeader.vue'

const route = useRoute()
const surveys = ref([])
const loading = ref(true)

const conventionId = computed(() => route.params.conventionId)

function getStatus(survey) {
  const now = new Date()
  if (survey.isCompleted) return 'completed'
  if (survey.startDate && new Date(survey.startDate) > now) return 'upcoming'
  if (survey.endDate && new Date(survey.endDate) < now) return 'expired'
  return 'active'
}

function getStatusLabel(survey) {
  const status = getStatus(survey)
  return {
    active: '진행중',
    upcoming: '예정',
    expired: '기간만료',
    completed: '완료',
  }[status]
}

function getStatusBadgeClass(survey) {
  const status = getStatus(survey)
  return {
    active:
      'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400',
    upcoming:
      'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400',
    expired: 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400',
    completed:
      'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400',
  }[status]
}

const STATUS_ORDER = { active: 0, upcoming: 1, completed: 2, expired: 3 }

const sortedSurveys = computed(() => {
  return [...surveys.value].sort((a, b) => {
    return STATUS_ORDER[getStatus(a)] - STATUS_ORDER[getStatus(b)]
  })
})

onMounted(async () => {
  try {
    const response = await api.get(`/surveys/convention/${conventionId.value}`)
    surveys.value = response.data
  } catch {
    surveys.value = []
  } finally {
    loading.value = false
  }
})
</script>
