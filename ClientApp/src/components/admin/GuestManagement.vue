<template>
  <div>
    <div class="flex justify-between items-center mb-6">
      <h2 class="text-xl font-semibold">참석자 관리</h2>
      <button
        @click="showCreateModal = true"
        class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
      >
        + 참석자 추가
      </button>
    </div>

    <div v-if="loading" class="text-center py-8">로딩 중...</div>
    <div v-else-if="guests.length === 0" class="text-center py-8 bg-white rounded-lg shadow">
      <p class="text-gray-500">등록된 참석자가 없습니다</p>
      <button @click="showCreateModal = true" class="mt-4 px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700">
        첫 참석자 추가하기
      </button>
    </div>
    <div v-else class="bg-white rounded-lg shadow overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">이름</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">전화번호</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">부서</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">일정</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">속성</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">작업</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="guest in guests" :key="guest.id" class="hover:bg-gray-50 cursor-pointer" @click="showGuestDetail(guest.id)">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="font-medium text-gray-900">{{ guest.guestName }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ guest.telephone }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ guest.corpPart || '-' }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <span v-if="guest.scheduleTemplates.length === 0" class="text-gray-400">미배정</span>
                <div v-else class="flex flex-wrap gap-1">
                  <span v-for="st in guest.scheduleTemplates" :key="st.scheduleTemplateId" class="px-2 py-0.5 bg-blue-100 text-blue-800 rounded text-xs">
                    {{ st.courseName }}
                  </span>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <span v-if="guest.attributes.length === 0" class="text-gray-400">-</span>
                <span v-else class="text-gray-600">{{ guest.attributes.length }}개</span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium" @click.stop>
                <button @click="editGuest(guest)" class="text-primary-600 hover:text-primary-900 mr-3">수정</button>
                <button @click="deleteGuest(guest.id)" class="text-red-600 hover:text-red-900">삭제</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- 참석자 생성/수정 모달 -->
    <div v-if="showCreateModal || editingGuest" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-lg w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            {{ editingGuest ? '참석자 수정' : '참석자 추가' }}
          </h2>
          
          <div class="space-y-4">
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium mb-1">이름 *</label>
                <input v-model="guestForm.guestName" type="text" class="w-full px-3 py-2 border rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium mb-1">전화번호 *</label>
                <input v-model="guestForm.telephone" type="text" class="w-full px-3 py-2 border rounded-lg" />
              </div>
            </div>
            
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium mb-1">부서</label>
                <input v-model="guestForm.corpPart" type="text" class="w-full px-3 py-2 border rounded-lg" />
              </div>
              <div>
                <label class="block text-sm font-medium mb-1">주민등록번호</label>
                <input v-model="guestForm.residentNumber" type="text" placeholder="000000-0000000" class="w-full px-3 py-2 border rounded-lg" />
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium mb-1">소속</label>
              <input v-model="guestForm.affiliation" type="text" class="w-full px-3 py-2 border rounded-lg" />
            </div>

            <div>
              <label class="block text-sm font-medium mb-2">속성 정보</label>
              <div class="space-y-2">
                <div v-for="(attr, idx) in guestForm.attributeList" :key="idx" class="flex gap-2">
                  <input v-model="attr.key" placeholder="키 (예: 호차)" class="flex-1 px-3 py-2 border rounded-lg" />
                  <input v-model="attr.value" placeholder="값" class="flex-1 px-3 py-2 border rounded-lg" />
                  <button @click="guestForm.attributeList.splice(idx, 1)" class="px-3 py-2 text-red-600 hover:bg-red-50 rounded-lg">삭제</button>
                </div>
                <button @click="guestForm.attributeList.push({ key: '', value: '' })" class="w-full py-2 border-2 border-dashed rounded-lg text-sm text-gray-600 hover:bg-gray-50">
                  + 속성 추가
                </button>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium mb-2">일정 배정</label>
              <div v-if="availableTemplates.length === 0" class="text-sm text-gray-500 p-3 bg-gray-50 rounded">
                일정 템플릿이 없습니다. 먼저 일정 관리에서 템플릿을 생성하세요.
              </div>
              <div v-else class="space-y-2">
                <label v-for="template in availableTemplates" :key="template.id" class="flex items-center gap-2 p-2 border rounded hover:bg-gray-50">
                  <input type="checkbox" :value="template.id" v-model="guestForm.scheduleTemplateIds" class="rounded" />
                  <span class="font-medium">{{ template.courseName }}</span>
                  <span class="text-sm text-gray-500">{{ template.description }}</span>
                </label>
              </div>
            </div>
          </div>

          <div class="flex justify-end space-x-3 mt-6">
            <button @click="closeGuestModal" class="px-4 py-2 border rounded-lg hover:bg-gray-50">취소</button>
            <button @click="saveGuest" class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700">저장</button>
          </div>
        </div>
      </div>
    </div>

    <!-- 참석자 상세 모달 -->
    <div v-if="showDetailModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div class="bg-white rounded-lg w-full max-w-4xl max-h-[90vh] overflow-y-auto">
        <div class="p-6">
          <div class="flex justify-between items-start mb-6">
            <h2 class="text-2xl font-bold">{{ guestDetail?.guestName }}</h2>
            <button @click="closeDetailModal" class="text-gray-500 hover:text-gray-700">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
              </svg>
            </button>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
            <div>
              <h3 class="font-semibold mb-3">기본 정보</h3>
              <dl class="space-y-2 text-sm">
                <div><dt class="text-gray-500 inline">전화번호:</dt> <dd class="inline ml-2">{{ guestDetail?.telephone }}</dd></div>
                <div><dt class="text-gray-500 inline">부서:</dt> <dd class="inline ml-2">{{ guestDetail?.corpPart || '-' }}</dd></div>
                <div><dt class="text-gray-500 inline">주민번호:</dt> <dd class="inline ml-2">{{ guestDetail?.residentNumber || '-' }}</dd></div>
                <div><dt class="text-gray-500 inline">소속:</dt> <dd class="inline ml-2">{{ guestDetail?.affiliation || '-' }}</dd></div>
              </dl>
            </div>

            <div v-if="guestDetail?.attributes && Object.keys(guestDetail.attributes).length > 0">
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
              <div v-for="schedule in guestDetail.schedules" :key="schedule.scheduleTemplateId" class="border rounded-lg overflow-hidden">
                <div class="bg-gray-50 px-4 py-3 border-b">
                  <h4 class="font-semibold">{{ schedule.courseName }}</h4>
                  <p v-if="schedule.description" class="text-sm text-gray-600">{{ schedule.description }}</p>
                </div>
                <div class="p-4">
                  <div class="space-y-3">
                    <div v-for="item in schedule.items" :key="item.id" class="flex gap-3 p-3 bg-gray-50 rounded-lg">
                      <div class="flex-shrink-0 w-24 text-sm">
                        <div class="font-medium">{{ formatDate(item.scheduleDate) }}</div>
                        <div class="text-gray-600">{{ item.startTime }}</div>
                      </div>
                      <div class="flex-1">
                        <p class="font-medium">{{ item.title }}</p>
                        <p v-if="item.location" class="text-sm text-gray-500">📍 {{ item.location }}</p>
                        <p v-if="item.content" class="text-sm text-gray-600 mt-1 whitespace-pre-wrap">{{ item.content }}</p>
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
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import apiClient from '@/services/api'

const props = defineProps({
  conventionId: { type: Number, required: true }
})

const guests = ref([])
const availableTemplates = ref([])
const showCreateModal = ref(false)
const showDetailModal = ref(false)
const editingGuest = ref(null)
const guestDetail = ref(null)
const loading = ref(true)

const guestForm = ref({
  guestName: '',
  telephone: '',
  corpPart: '',
  residentNumber: '',
  affiliation: '',
  scheduleTemplateIds: [],
  attributeList: []
})

const loadGuests = async () => {
  loading.value = true
  try {
    const response = await apiClient.get(`/admin/conventions/${props.conventionId}/guests`)
    console.log('✅ Guests loaded:', response.data)
    guests.value = response.data
  } catch (error) {
    console.error('❌ Failed to load guests:', error)
    console.error('Error details:', error.response?.data)
  } finally {
    loading.value = false
  }
}

const loadTemplates = async () => {
  try {
    const response = await apiClient.get(`/admin/conventions/${props.conventionId}/schedule-templates`)
    console.log('✅ Templates loaded:', response.data)
    availableTemplates.value = response.data
  } catch (error) {
    console.error('❌ Failed to load templates:', error)
  }
}

const editGuest = (guest) => {
  editingGuest.value = guest
  guestForm.value = {
    guestName: guest.guestName,
    telephone: guest.telephone,
    corpPart: guest.corpPart || '',
    residentNumber: guest.residentNumber || '',
    affiliation: guest.affiliation || '',
    scheduleTemplateIds: guest.scheduleTemplates.map(st => st.scheduleTemplateId),
    attributeList: guest.attributes.map(a => ({ key: a.attributeKey, value: a.attributeValue }))
  }
}

const closeGuestModal = () => {
  showCreateModal.value = false
  editingGuest.value = null
  guestForm.value = {
    guestName: '',
    telephone: '',
    corpPart: '',
    residentNumber: '',
    affiliation: '',
    scheduleTemplateIds: [],
    attributeList: []
  }
}

const saveGuest = async () => {
  try {
    const attributes = {}
    guestForm.value.attributeList.forEach(attr => {
      if (attr.key && attr.value) {
        attributes[attr.key] = attr.value
      }
    })

    const data = {
      guestName: guestForm.value.guestName,
      telephone: guestForm.value.telephone,
      corpPart: guestForm.value.corpPart,
      residentNumber: guestForm.value.residentNumber,
      affiliation: guestForm.value.affiliation,
      attributes: Object.keys(attributes).length > 0 ? attributes : null
    }

    let guestId
    if (editingGuest.value) {
      await apiClient.put(`/admin/guests/${editingGuest.value.id}`, data)
      guestId = editingGuest.value.id
    } else {
      const response = await apiClient.post(`/admin/conventions/${props.conventionId}/guests`, data)
      guestId = response.data.id
    }

    // 일정 배정
    await apiClient.post(`/admin/guests/${guestId}/schedules`, {
      scheduleTemplateIds: guestForm.value.scheduleTemplateIds
    })

    await loadGuests()
    closeGuestModal()
  } catch (error) {
    console.error('Failed to save guest:', error)
    alert('저장 실패: ' + (error.response?.data?.message || error.message))
  }
}

const deleteGuest = async (id) => {
  if (!confirm('참석자를 삭제하시겠습니까?')) return

  try {
    await apiClient.delete(`/admin/guests/${id}`)
    await loadGuests()
  } catch (error) {
    console.error('Failed to delete guest:', error)
    alert('삭제 실패')
  }
}

const showGuestDetail = async (guestId) => {
  try {
    const response = await apiClient.get(`/admin/guests/${guestId}/detail`)
    console.log('✅ Guest detail loaded:', response.data)
    guestDetail.value = response.data
    showDetailModal.value = true
  } catch (error) {
    console.error('❌ Failed to load guest detail:', error)
  }
}

const closeDetailModal = () => {
  showDetailModal.value = false
  guestDetail.value = null
}

const formatDate = (dateStr) => {
  const date = new Date(dateStr)
  return `${date.getMonth() + 1}/${date.getDate()}`
}

onMounted(() => {
  loadGuests()
  loadTemplates()
})
</script>
