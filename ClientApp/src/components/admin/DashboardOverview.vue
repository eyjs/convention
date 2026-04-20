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

    <!-- 여권 현황 (해외 행사) -->
    <div v-if="passportStats" class="mb-8">
      <div class="bg-white rounded-lg shadow">
        <div class="p-4 sm:p-6 border-b">
          <h3 class="text-lg font-semibold flex items-center gap-2">
            여권 현황
            <span
              class="text-xs font-normal px-2 py-0.5 rounded-full"
              :class="
                passportAlertCount > 0
                  ? 'bg-red-100 text-red-700'
                  : 'bg-green-100 text-green-700'
              "
            >
              {{
                passportAlertCount > 0
                  ? `${passportAlertCount}명 확인 필요`
                  : '전원 완료'
              }}
            </span>
          </h3>
        </div>
        <div class="p-4 sm:p-6">
          <!-- 요약 카드 -->
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3 mb-4">
            <div class="text-center p-3 bg-green-50 rounded-lg">
              <p class="text-2xl font-bold text-green-600">
                {{ passportStats.verifiedCount }}
              </p>
              <p class="text-xs text-green-700">승인 완료</p>
            </div>
            <div
              class="text-center p-3 rounded-lg cursor-pointer hover:shadow-md transition-shadow"
              :class="
                passportStats.unverified?.length > 0
                  ? 'bg-amber-50'
                  : 'bg-gray-50'
              "
              @click="showPassportList('unverified')"
            >
              <p
                class="text-2xl font-bold"
                :class="
                  passportStats.unverified?.length > 0
                    ? 'text-amber-600'
                    : 'text-gray-400'
                "
              >
                {{ passportStats.unverified?.length || 0 }}
              </p>
              <p class="text-xs text-amber-700">승인 대기</p>
            </div>
            <div
              class="text-center p-3 rounded-lg cursor-pointer hover:shadow-md transition-shadow"
              :class="
                passportStats.noPassport?.length > 0
                  ? 'bg-gray-100'
                  : 'bg-gray-50'
              "
              @click="showPassportList('noPassport')"
            >
              <p
                class="text-2xl font-bold"
                :class="
                  passportStats.noPassport?.length > 0
                    ? 'text-gray-600'
                    : 'text-gray-400'
                "
              >
                {{ passportStats.noPassport?.length || 0 }}
              </p>
              <p class="text-xs text-gray-600">미등록</p>
            </div>
            <div
              class="text-center p-3 rounded-lg cursor-pointer hover:shadow-md transition-shadow"
              :class="
                passportStats.expiringSoon?.length > 0
                  ? 'bg-orange-50'
                  : 'bg-gray-50'
              "
              @click="showPassportList('expiringSoon')"
            >
              <p
                class="text-2xl font-bold"
                :class="
                  passportStats.expiringSoon?.length > 0
                    ? 'text-orange-600'
                    : 'text-gray-400'
                "
              >
                {{ passportStats.expiringSoon?.length || 0 }}
              </p>
              <p class="text-xs text-orange-700">3개월 이내 만료</p>
            </div>
            <div
              class="text-center p-3 rounded-lg cursor-pointer hover:shadow-md transition-shadow"
              :class="
                passportStats.expired?.length > 0 ? 'bg-red-50' : 'bg-gray-50'
              "
              @click="showPassportList('expired')"
            >
              <p
                class="text-2xl font-bold"
                :class="
                  passportStats.expired?.length > 0
                    ? 'text-red-600'
                    : 'text-gray-400'
                "
              >
                {{ passportStats.expired?.length || 0 }}
              </p>
              <p class="text-xs text-red-700">만료됨</p>
            </div>
          </div>

          <!-- 프로그레스 바 -->
          <div class="bg-gray-200 rounded-full h-2.5 overflow-hidden">
            <div
              class="bg-green-500 h-2.5 rounded-full transition-all"
              :style="{
                width:
                  passportStats.total > 0
                    ? (passportStats.verifiedCount / passportStats.total) *
                        100 +
                      '%'
                    : '0%',
              }"
            ></div>
          </div>
          <p class="text-xs text-gray-400 mt-1 text-right">
            {{ passportStats.verifiedCount }} / {{ passportStats.total }}명 승인
          </p>
        </div>
      </div>
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
          <router-link
            :to="`/admin/conventions/${conventionId}/sns`"
            class="text-sm text-primary-600 hover:text-primary-700 font-medium"
          >
            SNS 발송 관리 →
          </router-link>
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
import UserListModal from '@/components/admin/UserListModal.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

const router = useRouter()
const route = useRoute()

const stats = ref({
  totalGuests: 0,
  totalSchedules: 0,
  scheduleAssignments: 0,
})

const smsHistory = ref([])
const scheduleStats = ref([])
const attributeStats = ref([])
const passportStats = ref(null)
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

const passportAlertCount = computed(() => {
  if (!passportStats.value) return 0
  return (
    (passportStats.value.unverified?.length || 0) +
    (passportStats.value.noPassport?.length || 0) +
    (passportStats.value.expiringSoon?.length || 0) +
    (passportStats.value.expired?.length || 0)
  )
})

function showPassportList(type) {
  const titleMap = {
    unverified: '여권 승인 대기',
    noPassport: '여권 미등록',
    expiringSoon: '여권 3개월 이내 만료',
    expired: '여권 만료',
  }
  const list = passportStats.value?.[type] || []
  if (list.length === 0) return

  userListModal.title = titleMap[type]
  userListModal.users = list
  userListModal.loading = false
  userListModal.extraLabel = type === 'noPassport' ? '' : '만료일'
  userListModal.extraField = type === 'noPassport' ? '' : 'passportExpiryDate'
  userListModal.open = true
}

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
    passportStats.value = response.data.passportStats || null
    showAllValues.value = new Array(attributeStats.value.length).fill(false)
  } catch (error) {
    console.error('Failed to load stats:', error)
  }
}

onMounted(() => {
  loadStats()
})
</script>
