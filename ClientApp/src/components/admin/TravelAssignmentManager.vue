<template>
  <div>
    <AdminPageHeader title="여행 배정 관리" class="mb-4" />

    <!-- 엑셀 업로드 + 날짜 관리 -->
    <div class="mb-4 p-4 bg-gray-50 rounded-lg space-y-3">
      <!-- 엑셀 업로드 -->
      <div class="flex flex-wrap items-center gap-2">
        <input
          ref="excelFileInput"
          type="file"
          accept=".xlsx"
          class="hidden"
          @change="handleExcelUpload"
        />
        <button
          class="px-4 py-2 bg-green-600 text-white rounded-lg text-sm hover:bg-green-700"
          :disabled="uploading"
          @click="$refs.excelFileInput.click()"
        >
          {{ uploading ? '업로드 중...' : '엑셀 업로드' }}
        </button>
        <span class="text-xs text-gray-400">
          시트명=날짜 (예: 2026-04-01 또는 4/1) · 행: 이름 | 전화번호 | 호차 |
          호텔 | 방번호 | 메모
        </span>
      </div>

      <!-- 업로드 결과 -->
      <div
        v-if="uploadResult"
        class="text-sm p-2 rounded"
        :class="
          uploadResult.success
            ? 'bg-green-50 text-green-800'
            : 'bg-red-50 text-red-800'
        "
      >
        <p>
          {{ uploadResult.sheetsProcessed }}개 시트,
          {{ uploadResult.usersMatched }}명 매칭
          <span v-if="uploadResult.usersNotFound > 0" class="text-red-600">
            ({{ uploadResult.usersNotFound }}명 미매칭)
          </span>
        </p>
        <p
          v-for="(w, i) in uploadResult.warnings?.slice(0, 5)"
          :key="i"
          class="text-xs text-orange-600"
        >
          {{ w }}
        </p>
      </div>

      <!-- 날짜 추가 -->
      <div class="flex flex-wrap items-center gap-2">
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
        <button
          v-if="activeDate"
          class="px-4 py-2 bg-red-50 text-red-600 rounded-lg text-sm hover:bg-red-100"
          @click="removeDate"
        >
          현재 날짜 삭제
        </button>
      </div>
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
    <div v-if="activeDate && assignments.length > 0">
      <!-- 상단 컨트롤 바 -->
      <div
        class="mb-3 p-3 bg-gray-50 rounded-lg flex flex-wrap items-center gap-3"
      >
        <!-- 그룹 필터 -->
        <div class="flex items-center gap-1.5">
          <span class="text-xs text-gray-500">그룹:</span>
          <select
            v-model="filterGroup"
            class="px-2 py-1.5 border border-gray-300 rounded text-sm"
          >
            <option value="">전체</option>
            <option v-for="g in groups" :key="g" :value="g">{{ g }}</option>
          </select>
        </div>

        <div class="w-px h-6 bg-gray-300"></div>

        <!-- 일괄 입력 -->
        <div class="flex items-center gap-1.5">
          <span class="text-xs text-gray-500">일괄:</span>
          <input
            v-model="bulkBus"
            type="text"
            placeholder="호차"
            class="px-2 py-1.5 border border-gray-300 rounded text-sm w-20"
          />
          <input
            v-model="bulkHotel"
            type="text"
            placeholder="호텔"
            class="px-2 py-1.5 border border-gray-300 rounded text-sm w-28"
          />
          <!-- prettier-ignore -->
          <button
            class="px-3 py-1.5 bg-blue-600 text-white rounded text-sm hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed"
            :disabled="!bulkBus && !bulkHotel"
            @click="applyBulk"
          >적용 ({{ filteredUsers.length }}명)</button>
        </div>

        <div class="w-px h-6 bg-gray-300"></div>

        <!-- 이전 날짜 복사 -->
        <button
          v-if="previousDate"
          class="px-3 py-1.5 bg-purple-50 text-purple-700 rounded text-sm hover:bg-purple-100"
          @click="copyFromPreviousDate"
        >
          {{ formatDateTab(previousDate) }} 복사
        </button>
      </div>

      <!-- 모바일 카드 편집 -->
      <div class="md:hidden space-y-2">
        <div
          v-for="(user, idx) in filteredUsers"
          :key="'m-' + user.userId"
          class="bg-white rounded-lg border p-3 space-y-2"
        >
          <div class="flex items-center justify-between">
            <span class="font-semibold text-gray-900 text-sm">{{ user.userName }}</span>
            <span class="text-xs text-gray-400">{{ user.groupName || '' }}</span>
          </div>
          <div class="grid grid-cols-2 gap-2">
            <div>
              <label class="text-[10px] text-gray-500">호차</label>
              <input :value="getDayValue(user, 'bus')" type="text" class="w-full px-2 py-1.5 border rounded text-sm" :placeholder="getAboveValue(idx, 'bus') || ''" @focus="onFocusEmpty($event, idx, 'bus')" @blur="updateField(user, 'bus', $event.target.value)" />
            </div>
            <div>
              <label class="text-[10px] text-gray-500">호텔</label>
              <input :value="getDayValue(user, 'hotel')" type="text" class="w-full px-2 py-1.5 border rounded text-sm" :placeholder="getAboveValue(idx, 'hotel') || ''" @focus="onFocusEmpty($event, idx, 'hotel')" @blur="updateField(user, 'hotel', $event.target.value)" />
            </div>
            <div>
              <label class="text-[10px] text-gray-500">방번호</label>
              <input :value="getDayValue(user, 'room')" type="text" class="w-full px-2 py-1.5 border rounded text-sm" placeholder="미배정" @blur="updateField(user, 'room', $event.target.value)" />
            </div>
            <div>
              <label class="text-[10px] text-gray-500">메모</label>
              <input :value="getDayValue(user, 'memo')" type="text" class="w-full px-2 py-1.5 border rounded text-sm" @blur="updateField(user, 'memo', $event.target.value)" />
            </div>
          </div>
        </div>
      </div>

      <!-- PC 테이블 -->
      <div class="hidden md:block overflow-x-auto border border-gray-200 rounded-lg">
        <table class="w-full text-sm">
          <thead class="bg-gray-50">
            <tr>
              <th
                class="px-3 py-2 text-left font-medium text-gray-600 w-24 sticky left-0 bg-gray-50"
              >
                이름
              </th>
              <th class="px-3 py-2 text-left font-medium text-gray-600 w-16">
                그룹
              </th>
              <th class="px-3 py-2 text-left font-medium text-gray-600 w-24">
                <span>호차</span>
              </th>
              <th class="px-3 py-2 text-left font-medium text-gray-600 w-32">
                <span>호텔</span>
              </th>
              <th class="px-3 py-2 text-left font-medium text-gray-600 w-20">
                방번호
              </th>
              <th class="px-3 py-2 text-left font-medium text-gray-600">
                메모
              </th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(user, idx) in filteredUsers"
              :key="user.userId"
              class="border-t border-gray-100 hover:bg-blue-50/30"
            >
              <td
                class="px-3 py-1 font-medium text-gray-900 text-xs sticky left-0 bg-white"
              >
                {{ user.userName }}
              </td>
              <td class="px-3 py-1 text-gray-400 text-xs">
                {{ user.groupName || '-' }}
              </td>
              <td class="px-2 py-0.5">
                <input
                  :value="getDayValue(user, 'bus')"
                  type="text"
                  class="w-full px-2 py-1 border border-gray-200 rounded text-sm focus:ring-1 focus:ring-primary-400 focus:border-primary-400"
                  :placeholder="getAboveValue(idx, 'bus') || ''"
                  @focus="onFocusEmpty($event, idx, 'bus')"
                  @blur="updateField(user, 'bus', $event.target.value)"
                />
              </td>
              <td class="px-2 py-0.5">
                <input
                  :value="getDayValue(user, 'hotel')"
                  type="text"
                  class="w-full px-2 py-1 border border-gray-200 rounded text-sm focus:ring-1 focus:ring-primary-400 focus:border-primary-400"
                  :placeholder="getAboveValue(idx, 'hotel') || ''"
                  @focus="onFocusEmpty($event, idx, 'hotel')"
                  @blur="updateField(user, 'hotel', $event.target.value)"
                />
              </td>
              <td class="px-2 py-0.5">
                <input
                  :value="getDayValue(user, 'room')"
                  type="text"
                  class="w-full px-2 py-1 border border-gray-200 rounded text-sm focus:ring-1 focus:ring-primary-400 focus:border-primary-400"
                  placeholder="미배정"
                  @blur="updateField(user, 'room', $event.target.value)"
                />
              </td>
              <td class="px-2 py-0.5">
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

      <p class="mt-2 text-xs text-gray-400">
        빈 셀 클릭 시 바로 위 값이 자동 입력됩니다 · Tab으로 다음 셀 이동
      </p>
    </div>

    <!-- 저장 상태 토스트 -->
    <Transition name="toast">
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
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import apiClient from '@/services/api'
import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'

const props = defineProps({
  conventionId: { type: Number, required: true },
})

const assignments = ref([])
const dates = ref([])
const activeDate = ref('')
const newDate = ref('')
const filterGroup = ref('')
const bulkBus = ref('')
const bulkHotel = ref('')
const saveStatus = ref(null)
const uploading = ref(false)
const uploadResult = ref(null)
let saveTimeout = null

const groups = computed(() => {
  const set = new Set(
    assignments.value.filter((a) => a.groupName).map((a) => a.groupName),
  )
  return [...set].sort()
})

const filteredUsers = computed(() => {
  let list = assignments.value
  if (filterGroup.value) {
    list = list.filter((a) => a.groupName === filterGroup.value)
  }
  return list
})

const previousDate = computed(() => {
  const idx = dates.value.indexOf(activeDate.value)
  return idx > 0 ? dates.value[idx - 1] : null
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

// 바로 위 행의 값 가져오기 (빈 셀 placeholder + 자동 채우기용)
function getAboveValue(currentIdx, field) {
  if (currentIdx === 0) return ''
  const aboveUser = filteredUsers.value[currentIdx - 1]
  return getDayValue(aboveUser, field)
}

// 빈 셀에 포커스하면 위 셀 값 자동 입력
function onFocusEmpty(event, idx, field) {
  if (event.target.value) return
  const aboveVal = getAboveValue(idx, field)
  if (aboveVal) {
    event.target.value = aboveVal
  }
}

function addDate() {
  if (!newDate.value || dates.value.includes(newDate.value)) return
  dates.value.push(newDate.value)
  dates.value.sort()
  activeDate.value = newDate.value
  newDate.value = ''
}

async function removeDate() {
  if (!activeDate.value) return
  if (!confirm(`${formatDateTab(activeDate.value)} 날짜를 삭제할까요?`)) return

  const dateToRemove = activeDate.value
  try {
    await apiClient.delete(
      `/admin/conventions/${props.conventionId}/travel-assignments/dates/${dateToRemove}`,
    )
    // 프론트엔드 상태에서도 제거
    dates.value = dates.value.filter((d) => d !== dateToRemove)
    assignments.value.forEach((user) => {
      user.days = user.days.filter((d) => d.date !== dateToRemove)
    })
    activeDate.value = dates.value[0] || ''
  } catch (error) {
    console.error('Failed to remove date:', error)
    alert('날짜 삭제에 실패했습니다.')
  }
}

// 이전 날짜의 호차/호텔/방번호/메모를 현재 날짜로 복사
async function copyFromPreviousDate() {
  if (!previousDate.value) return
  if (
    !confirm(`${formatDateTab(previousDate.value)}의 배정 정보를 복사할까요?`)
  )
    return

  const updates = []
  for (const user of filteredUsers.value) {
    const prevDay = user.days.find((d) => d.date === previousDate.value)
    if (!prevDay) continue

    updates.push({
      userId: user.userId,
      date: activeDate.value,
      bus: prevDay.bus,
      hotel: prevDay.hotel,
      room: prevDay.room,
      memo: prevDay.memo,
    })
  }

  if (updates.length === 0) return
  await applyBulkUpdates(updates)
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
  const updates = filteredUsers.value.map((user) => ({
    userId: user.userId,
    date: activeDate.value,
    bus: bulkBus.value || getDayValue(user, 'bus') || null,
    hotel: bulkHotel.value || getDayValue(user, 'hotel') || null,
    room: getDayValue(user, 'room') || null,
    memo: getDayValue(user, 'memo') || null,
  }))

  await applyBulkUpdates(updates)
}

async function applyBulkUpdates(updates) {
  saveStatus.value = 'saving'
  try {
    await apiClient.put(
      `/admin/conventions/${props.conventionId}/travel-assignments/bulk`,
      updates,
    )

    for (const upd of updates) {
      const user = assignments.value.find((a) => a.userId === upd.userId)
      if (!user) continue
      const day = user.days.find((d) => d.date === upd.date)
      if (day) {
        Object.assign(day, upd)
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

async function handleExcelUpload(event) {
  const file = event.target.files[0]
  if (!file) return

  uploading.value = true
  uploadResult.value = null

  const formData = new FormData()
  formData.append('file', file)

  try {
    const response = await apiClient.post(
      `/admin/conventions/${props.conventionId}/travel-assignments/upload`,
      formData,
      { headers: { 'Content-Type': 'multipart/form-data' } },
    )
    uploadResult.value = response.data
    await loadAssignments()
  } catch (error) {
    uploadResult.value = {
      success: false,
      warnings: [error.response?.data?.error || error.message || '업로드 실패'],
    }
  } finally {
    uploading.value = false
    event.target.value = ''
  }
}

async function loadAssignments() {
  try {
    const response = await apiClient.get(
      `/admin/conventions/${props.conventionId}/travel-assignments`,
    )
    assignments.value = response.data || []

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

<style scoped>
.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}
.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateY(10px);
}
</style>
