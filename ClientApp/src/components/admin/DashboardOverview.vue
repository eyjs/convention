<template>
  <div>
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 mb-8">
      <AdminStatsCard
        label="총 참석자"
        :value="stats.totalGuests"
        :icon="Users"
        color="primary"
        :clickable="true"
        @click="navigateTo('guests')"
      />
      <AdminStatsCard
        label="총 일정 코스"
        :value="stats.totalSchedules"
        :icon="Calendar"
        color="green"
        :clickable="true"
        @click="navigateTo('schedules')"
      />
      <AdminStatsCard
        label="일정 배정"
        :value="stats.scheduleAssignments"
        :icon="ClipboardCheck"
        color="orange"
        :clickable="true"
        @click="openScheduleAssignedModal"
      />
    </div>

    <!-- 속성별 통계 -->
    <div v-if="attributeStats.length > 0" class="mb-8">
      <div class="bg-white rounded-lg shadow">
        <div class="p-4 sm:p-6 border-b flex justify-between items-center">
          <h3 class="text-lg font-semibold">속성별 통계</h3>
          <button
            class="text-sm text-primary-600 hover:text-primary-700"
            @click="showAllAttributes = !showAllAttributes"
          >
            {{ showAllAttributes ? '접기' : '전체 보기' }}
          </button>
        </div>
        <div class="p-4 sm:p-6">
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <div
              v-for="(attr, index) in displayedAttributes"
              :key="attr.attributeKey"
              class="border rounded-lg p-4 hover:shadow-md transition-shadow"
            >
              <div class="flex justify-between items-start mb-3">
                <h4 class="font-semibold text-gray-900">
                  {{ attr.attributeKey }}
                </h4>
                <span class="text-xs bg-gray-100 px-2 py-1 rounded"
                  >{{ attr.totalCount }}명</span
                >
              </div>

              <div class="space-y-2">
                <div
                  v-for="value in attr.values.slice(
                    0,
                    showAllValues[index] ? undefined : 3,
                  )"
                  :key="value.value"
                  class="flex items-center justify-between p-2 bg-gray-50 rounded cursor-pointer hover:bg-gray-100 transition-colors"
                  @click="openAttributeModal(attr.attributeKey, value.value)"
                >
                  <span class="text-sm text-gray-700 truncate">{{
                    value.value
                  }}</span>
                  <span class="text-sm font-semibold text-primary-600"
                    >{{ value.count }}명</span
                  >
                </div>

                <button
                  v-if="attr.values.length > 3"
                  class="text-xs text-primary-600 hover:text-primary-700 mt-2 w-full text-left"
                  @click="toggleShowAllValues(index)"
                >
                  {{
                    showAllValues[index]
                      ? '줄이기'
                      : `+${attr.values.length - 3}개 더보기`
                  }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <div class="bg-white rounded-lg shadow">
        <div class="p-4 sm:p-6 border-b flex justify-between items-center">
          <div class="flex items-center space-x-3">
            <h3 class="text-lg font-semibold">최근 문자 발송 이력</h3>
            <span class="text-xs text-gray-400">최근 20건</span>
          </div>
          <button
            class="text-sm text-primary-600 hover:text-primary-700 font-medium ml-auto"
            @click="showSmsModal = true"
          >
            + 문자 발송 관리
          </button>
        </div>
        <div class="p-4 sm:p-6">
          <div
            v-if="smsHistory.length === 0"
            class="text-center text-gray-500 py-8"
          >
            발송 이력이 없습니다
          </div>
          <div
            v-else
            class="space-y-3 max-h-[420px] overflow-y-auto pr-2 custom-scrollbar"
          >
            <div
              v-for="sms in smsHistory"
              :key="sms.id"
              class="flex items-center justify-between p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
            >
              <div class="flex-1 min-w-0 mr-4">
                <div class="flex justify-between mb-1">
                  <span class="font-medium text-gray-900">{{
                    sms.receiverName
                  }}</span>
                  <span class="text-xs text-gray-500">{{ sms.sentAt }}</span>
                </div>
                <p
                  class="text-sm text-gray-600 whitespace-pre-wrap line-clamp-2"
                >
                  {{ sms.message }}
                </p>
              </div>
              <div class="flex-shrink-0">
                <AdminBadge
                  :variant="
                    sms.status === 'success'
                      ? 'success'
                      : sms.status === 'failed'
                        ? 'danger'
                        : 'warning'
                  "
                  size="md"
                >
                  {{ sms.statusText }}
                </AdminBadge>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow">
        <div class="p-4 sm:p-6 border-b">
          <h3 class="text-lg font-semibold">일정 코스별 현황</h3>
        </div>
        <div class="p-4 sm:p-6">
          <div
            v-if="scheduleStats.length === 0"
            class="text-center text-gray-500 py-8"
          >
            등록된 일정이 없습니다
          </div>
          <div v-else class="space-y-3">
            <div
              v-for="schedule in scheduleStats"
              :key="schedule.id"
              class="flex items-center justify-between p-3 bg-gray-50 rounded-lg hover:shadow transition-shadow cursor-pointer"
              @click="openCourseModal(schedule)"
            >
              <div class="flex-1 min-w-0">
                <p class="font-medium truncate">{{ schedule.courseName }}</p>
                <p class="text-sm text-gray-500">
                  {{ schedule.itemCount }}개 항목
                </p>
              </div>
              <span class="text-sm font-medium text-primary-600"
                >{{ schedule.guestCount }}명</span
              >
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- SMS 관리 모달 -->
    <SmsManagementModal
      v-if="showSmsModal"
      :convention-id="conventionId"
      @close="showSmsModal = false"
    />

    <!-- 공통 명단 모달 -->
    <UserListModal
      :is-open="userListModal.open"
      :title="userListModal.title"
      :users="userListModal.users"
      :loading="userListModal.loading"
      :extra-label="userListModal.extraLabel"
      :extra-field="userListModal.extraField"
      @close="userListModal.open = false"
    />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { Users, Calendar, ClipboardCheck } from 'lucide-vue-next'
import apiClient from '@/services/api'
import AdminStatsCard from '@/components/admin/ui/AdminStatsCard.vue'
import AdminBadge from '@/components/admin/ui/AdminBadge.vue'
import SmsManagementModal from './sms/SmsManagementModal.vue'
import UserListModal from '@/components/admin/UserListModal.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

const router = useRouter()
const route = useRoute()

const showSmsModal = ref(false)
const stats = ref({
  totalGuests: 0,
  totalSchedules: 0,
  scheduleAssignments: 0,
})

const smsHistory = ref([])
const scheduleStats = ref([])
const attributeStats = ref([])
const showAllAttributes = ref(false)
const showAllValues = ref([])

const userListModal = reactive({
  open: false,
  title: '',
  users: [],
  loading: false,
  extraLabel: '',
  extraField: '',
})

const displayedAttributes = computed(() => {
  return showAllAttributes.value
    ? attributeStats.value
    : attributeStats.value.slice(0, 6)
})

const toggleShowAllValues = (index) => {
  showAllValues.value[index] = !showAllValues.value[index]
}

function navigateTo(subPath) {
  const convId = route.params.id
  router.push(`/admin/conventions/${convId}/${subPath}`)
}

async function openScheduleAssignedModal() {
  userListModal.title = '일정 배정 명단'
  userListModal.users = []
  userListModal.loading = true
  userListModal.extraLabel = '코스'
  userListModal.extraField = 'courseName'
  userListModal.open = true

  try {
    const res = await apiClient.get(
      `/admin/conventions/${props.conventionId}/stats/schedule-assigned-users`,
    )
    userListModal.users = res.data
  } catch {
    userListModal.users = []
  } finally {
    userListModal.loading = false
  }
}

async function openCourseModal(schedule) {
  userListModal.title = `${schedule.courseName} 명단`
  userListModal.users = []
  userListModal.loading = true
  userListModal.extraLabel = ''
  userListModal.extraField = ''
  userListModal.open = true

  try {
    const res = await apiClient.get(
      `/admin/conventions/${props.conventionId}/stats/schedule-course-users/${schedule.id}`,
    )
    userListModal.users = res.data
  } catch {
    userListModal.users = []
  } finally {
    userListModal.loading = false
  }
}

async function openAttributeModal(attributeKey, attributeValue) {
  userListModal.title = `${attributeKey}: ${attributeValue}`
  userListModal.users = []
  userListModal.loading = true
  userListModal.extraLabel = ''
  userListModal.extraField = ''
  userListModal.open = true

  try {
    const res = await apiClient.get(
      `/admin/conventions/${props.conventionId}/stats/attribute-users`,
      { params: { key: attributeKey, value: attributeValue } },
    )
    userListModal.users = res.data
  } catch {
    userListModal.users = []
  } finally {
    userListModal.loading = false
  }
}

const loadStats = async () => {
  try {
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/stats`,
    )

    stats.value = {
      totalGuests: response.data.totalGuests,
      totalSchedules: response.data.totalSchedules,
      scheduleAssignments: response.data.scheduleAssignments,
    }
    smsHistory.value = response.data.smsHistory || []
    scheduleStats.value = response.data.scheduleStats
    attributeStats.value = response.data.attributeStats || []
    showAllValues.value = new Array(attributeStats.value.length).fill(false)
  } catch (error) {
    console.error('Failed to load stats:', error)
  }
}

onMounted(() => {
  loadStats()
})
</script>
