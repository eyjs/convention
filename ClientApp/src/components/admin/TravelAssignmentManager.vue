<template>
  <div>
    <AdminPageHeader title="여행 배정 관리" class="mb-4" />

    <!-- 날짜 관리 -->
    <div class="mb-4 flex flex-wrap items-center gap-2">
      <input
        v-model="newDate"
        type="date"
        class="px-3 py-2 border border-gray-300 rounded-lg text-sm"
      />
      <button
        class="px-4 py-2 bg-primary-600 text-white rounded-lg text-sm hover:bg-primary-700"
        @click="addDate"
      >
        날짜 추가
      </button>
    </div>

    <!-- 날짜별 탭 -->
    <div v-if="dates.length > 0" class="border-b border-gray-200 mb-4">
      <nav class="-mb-px flex space-x-4 overflow-x-auto">
        <button
          v-for="date in dates"
          :key="date"
          :class="[
            'whitespace-nowrap py-3 px-3 border-b-2 font-medium text-sm',
            activeDate === date
              ? 'border-primary-500 text-primary-600'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
          ]"
          @click="activeDate = date"
        >
          {{ formatDateTab(date) }}
        </button>
      </nav>
    </div>

    <!-- 날짜 없을 때 -->
    <div
      v-if="dates.length === 0"
      class="text-center py-12 text-gray-400 text-sm"
    >
      날짜를 추가해주세요
    </div>

    <!-- 배정 테이블 -->
    <div v-if="activeDate && filteredUsers.length > 0">
      <!-- 일괄 작업 바 -->
      <div class="mb-3 flex flex-wrap items-center gap-2">
        <select
          v-model="bulkGroup"
          class="px-3 py-2 border border-gray-300 rounded-lg text-sm"
        >
          <option value="">전체 그룹</option>
          <option v-for="g in groups" :key="g" :value="g">{{ g }}</option>
        </select>
        <input
          v-model="bulkBus"
          type="text"
          placeholder="호차 (예: 3호차)"
          class="px-3 py-2 border border-gray-300 rounded-lg text-sm w-32"
        />
        <input
          v-model="bulkHotel"
          type="text"
          placeholder="호텔명"
          class="px-3 py-2 border border-gray-300 rounded-lg text-sm w-40"
        />
        <button
          class="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700"
          :disabled="!bulkBus && !bulkHotel"
          @click="applyBulk"
        >
          일괄 적용
        </button>
        <span class="text-xs text-gray-400">
          {{ filteredUsers.length }}명
        </span>
      </div>

      <!-- 테이블 -->
      <div class="overflow-x-auto border border-gray-200 rounded-lg">
        <table class="w-full text-sm">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-3 py-2 text-left font-medium text-gray-600 w-24">
                이름
              </th>
              <th class="px-3 py-2 text-left font-medium text-gray-600 w-20">
                그룹
              </th>
              <th class="px-3 py-2 text-left font-medium text-gray-600 w-28">
                호차
              </th>
              <th class="px-3 py-2 text-left font-medium text-gray-600 w-36">
                호텔
              </th>
              <th class="px-3 py-2 text-left font-medium text-gray-600 w-24">
                방번호
              </th>
              <th class="px-3 py-2 text-left font-medium text-gray-600">
                메모
              </th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="user in filteredUsers"
              :key="user.userId"
              class="border-t border-gray-100 hover:bg-gray-50"
            >
              <td class="px-3 py-1.5 font-medium text-gray-900">
                {{ user.userName }}
              </td>
              <td class="px-3 py-1.5 text-gray-500 text-xs">
                {{ user.groupName || '-' }}
              </td>
              <td class="px-3 py-1.5">
                <input
                  :value="getDayValue(user, 'bus')"
                  type="text"
                  class="w-full px-2 py-1 border border-gray-200 rounded text-sm focus:ring-1 focus:ring-primary-400 focus:border-primary-400"
                  @blur="updateField(user, 'bus', $event.target.value)"
                />
              </td>
              <td class="px-3 py-1.5">
                <input
                  :value="getDayValue(user, 'hotel')"
                  type="text"
                  class="w-full px-2 py-1 border border-gray-200 rounded text-sm focus:ring-1 focus:ring-primary-400 focus:border-primary-400"
                  @blur="updateField(user, 'hotel', $event.target.value)"
                />
              </td>
              <td class="px-3 py-1.5">
                <input
                  :value="getDayValue(user, 'room')"
                  type="text"
                  class="w-full px-2 py-1 border border-gray-200 rounded text-sm focus:ring-1 focus:ring-primary-400 focus:border-primary-400"
                  placeholder="미배정"
                  @blur="updateField(user, 'room', $event.target.value)"
                />
              </td>
              <td class="px-3 py-1.5">
                <input
                  :value="getDayValue(user, 'memo')"
                  type="text"
                  class="w-full px-2 py-1 border border-gray-200 rounded text-sm focus:ring-1 focus:ring-primary-400 focus:border-primary-400"
                  @blur="updateField(user, 'memo', $event.target.value)"
                />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- 저장 상태 -->
    <div
      v-if="saveStatus"
      class="fixed bottom-4 right-4 px-4 py-2 rounded-lg text-sm shadow-lg z-50"
      :class="
        saveStatus === 'saving'
          ? 'bg-blue-600 text-white'
          : saveStatus === 'saved'
            ? 'bg-green-600 text-white'
            : 'bg-red-600 text-white'
      "
    >
      {{
        saveStatus === 'saving'
          ? '저장 중...'
          : saveStatus === 'saved'
            ? '저장 완료'
            : '저장 실패'
      }}
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import apiClient from '@/services/api'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

const assignments = ref([])
const dates = ref([])
const activeDate = ref('')
const newDate = ref('')
const bulkGroup = ref('')
const bulkBus = ref('')
const bulkHotel = ref('')
const saveStatus = ref(null)
let saveTimeout = null

const groups = computed(() => {
  const set = new Set(
    assignments.value.filter((a) => a.groupName).map((a) => a.groupName),
  )
  return [...set].sort()
})

const filteredUsers = computed(() => {
  let list = assignments.value
  if (bulkGroup.value) {
    list = list.filter((a) => a.groupName === bulkGroup.value)
  }
  return list
})

function formatDateTab(dateStr) {
  const date = new Date(dateStr)
  const weekdays = ['일', '월', '화', '수', '목', '금', '토']
  return `${date.getMonth() + 1}/${date.getDate()} (${weekdays[date.getDay()]})`
}

function getDayValue(user, field) {
  const day = user.days.find((d) => d.date === activeDate.value)
  return day ? day[field] || '' : ''
}

function addDate() {
  if (!newDate.value || dates.value.includes(newDate.value)) return
  dates.value.push(newDate.value)
  dates.value.sort()
  activeDate.value = newDate.value
  newDate.value = ''
}

async function updateField(user, field, value) {
  const day = user.days.find((d) => d.date === activeDate.value)
  if (day) {
    if (day[field] === value) return
    day[field] = value || null
  } else {
    user.days.push({
      date: activeDate.value,
      bus: null,
      hotel: null,
      room: null,
      memo: null,
      [field]: value || null,
    })
  }

  await saveSingleDay(user.userId, activeDate.value, user)
}

async function saveSingleDay(userId, date, user) {
  const day = user.days.find((d) => d.date === date) || {}
  saveStatus.value = 'saving'
  try {
    await apiClient.put(
      `/admin/conventions/${props.conventionId}/travel-assignments/users/${userId}/day`,
      {
        date,
        bus: day.bus || null,
        hotel: day.hotel || null,
        room: day.room || null,
        memo: day.memo || null,
      },
    )
    saveStatus.value = 'saved'
  } catch {
    saveStatus.value = 'error'
  }
  clearTimeout(saveTimeout)
  saveTimeout = setTimeout(() => {
    saveStatus.value = null
  }, 1500)
}

async function applyBulk() {
  const targets = filteredUsers.value
  const updates = targets.map((user) => ({
    userId: user.userId,
    date: activeDate.value,
    bus: bulkBus.value || getDayValue(user, 'bus') || null,
    hotel: bulkHotel.value || getDayValue(user, 'hotel') || null,
    room: getDayValue(user, 'room') || null,
    memo: getDayValue(user, 'memo') || null,
  }))

  saveStatus.value = 'saving'
  try {
    await apiClient.put(
      `/admin/conventions/${props.conventionId}/travel-assignments/bulk`,
      updates,
    )

    // 로컬 데이터도 갱신
    for (const upd of updates) {
      const user = assignments.value.find((a) => a.userId === upd.userId)
      if (!user) continue
      const day = user.days.find((d) => d.date === upd.date)
      if (day) {
        day.bus = upd.bus
        day.hotel = upd.hotel
      } else {
        user.days.push({ ...upd })
      }
    }

    saveStatus.value = 'saved'
  } catch {
    saveStatus.value = 'error'
  }
  clearTimeout(saveTimeout)
  saveTimeout = setTimeout(() => {
    saveStatus.value = null
  }, 1500)
}

async function loadAssignments() {
  try {
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/travel-assignments`,
    )
    assignments.value = response.data || []

    // 기존 데이터에서 날짜 추출
    const dateSet = new Set()
    for (const user of assignments.value) {
      for (const day of user.days) {
        dateSet.add(day.date)
      }
    }
    if (dateSet.size > 0) {
      dates.value = [...dateSet].sort()
      activeDate.value = dates.value[0]
    }
  } catch (error) {
    console.error('Failed to load travel assignments:', error)
  }
}

onMounted(loadAssignments)
</script>
