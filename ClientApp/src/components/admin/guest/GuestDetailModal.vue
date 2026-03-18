<template>
  <BaseModal :is-open="isOpen" max-width="4xl" @close="emit('close')">
    <template #header>
      <h2 class="text-2xl font-bold">{{ guestDetail?.guestName }}</h2>
    </template>
    <template #body>
      <div v-if="loading" class="text-center py-8">로딩 중...</div>
      <template v-else>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
          <div>
            <h3 class="font-semibold mb-3">기본 정보</h3>
            <dl class="space-y-2 text-sm">
              <div>
                <dt class="text-gray-500 inline">전화번호:</dt>
                <dd class="inline ml-2">{{ guestDetail?.telephone }}</dd>
              </div>
              <div>
                <dt class="text-gray-500 inline">부서:</dt>
                <dd class="inline ml-2">
                  {{ guestDetail?.corpPart || '-' }}
                </dd>
              </div>
              <div>
                <dt class="text-gray-500 inline">주민번호:</dt>
                <dd class="inline ml-2">
                  {{ guestDetail?.residentNumber || '-' }}
                </dd>
              </div>
              <div>
                <dt class="text-gray-500 inline">소속:</dt>
                <dd class="inline ml-2">
                  {{ guestDetail?.affiliation || '-' }}
                </dd>
              </div>
            </dl>
          </div>

          <div
            v-if="
              guestDetail?.attributes &&
              Object.keys(guestDetail.attributes).length > 0
            "
          >
            <h3 class="font-semibold mb-3">속성 정보</h3>
            <dl class="space-y-2 text-sm">
              <div v-for="(value, key) in guestDetail.attributes" :key="key">
                <dt class="text-gray-500 inline">{{ key }}:</dt>
                <dd class="inline ml-2 font-medium">{{ value }}</dd>
              </div>
            </dl>
          </div>
        </div>

        <div v-if="guestDetail?.schedules && guestDetail.schedules.length > 0">
          <h3 class="font-semibold mb-3">배정된 일정</h3>
          <div class="space-y-4">
            <div
              v-for="schedule in guestDetail.schedules"
              :key="schedule.scheduleTemplateId"
              class="border rounded-lg overflow-hidden"
            >
              <div class="bg-gray-50 px-4 py-3 border-b">
                <h4 class="font-semibold">{{ schedule.courseName }}</h4>
                <p v-if="schedule.description" class="text-sm text-gray-600">
                  {{ schedule.description }}
                </p>
              </div>
              <div class="p-4">
                <div class="space-y-3">
                  <div
                    v-for="item in schedule.items"
                    :key="item.id"
                    class="flex gap-3 p-3 bg-gray-50 rounded-lg"
                  >
                    <div class="flex-shrink-0 w-24 text-sm">
                      <div class="font-medium">
                        {{ formatDate(item.scheduleDate) }}
                      </div>
                      <div class="text-gray-600">{{ item.startTime }}</div>
                    </div>
                    <div class="flex-1">
                      <p class="font-medium">{{ item.title }}</p>
                      <p v-if="item.location" class="text-sm text-gray-500">
                        {{ item.location }}
                      </p>
                      <p
                        v-if="item.content"
                        class="text-sm text-gray-600 mt-1 whitespace-pre-wrap"
                      >
                        {{ item.content }}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div v-else class="text-center text-gray-500 py-8 border rounded-lg">
          배정된 일정이 없습니다
        </div>
      </template>
    </template>
  </BaseModal>
</template>

<script setup>
import { ref, watch } from 'vue'
import apiClient from '@/services/api'
import BaseModal from '@/components/common/BaseModal.vue'

const props = defineProps({
  isOpen: { type: Boolean, required: true },
  conventionId: { type: Number, required: true },
  guestId: { type: Number, default: null },
})

const emit = defineEmits(['close'])

const guestDetail = ref(null)
const loading = ref(false)

const formatDate = (dateStr) => {
  const date = new Date(dateStr)
  return `${date.getMonth() + 1}/${date.getDate()}`
}

watch(
  () => props.guestId,
  async (newId) => {
    if (newId) {
      loading.value = true
      try {
        const response = await apiClient.get(`/admin/guests/${newId}/detail`)
        guestDetail.value = response.data
      } catch (error) {
        console.error('Failed to load guest detail:', error)
      } finally {
        loading.value = false
      }
    } else {
      guestDetail.value = null
    }
  },
)
</script>
