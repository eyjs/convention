<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader title="내 정보" :show-back="true" />

    <div class="max-w-2xl mx-auto py-6">
      <!-- 로딩 상태 -->
      <div v-if="loading" class="text-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
        <p class="mt-4 text-gray-600">로딩 중...</p>
      </div>

      <template v-else>
        <!-- 프로필 사진 -->
        <div class="px-4 py-6 flex flex-col items-center">
          <div class="relative">
            <template v-if="profile.profileImageUrl">
              <img :src="profile.profileImageUrl" alt="프로필 사진" class="h-24 w-24 rounded-full object-cover" />
            </template>
            <template v-else>
              <div class="h-24 w-24 rounded-full bg-gray-200 flex items-center justify-center">
                <svg class="h-16 w-16 text-gray-500" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M24 20.993V24H0v-2.997A14.977 14.977 0 0112.004 15c4.904 0 9.26 2.354 11.996 5.993zM16.002 8.999a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
              </div>
            </template>
            <label for="profile-photo-upload" class="absolute -bottom-1 -right-1 bg-white p-1.5 rounded-full shadow-md cursor-pointer hover:bg-gray-100">
              <Camera class="w-5 h-5 text-gray-600" />
              <input id="profile-photo-upload" type="file" class="hidden" @change="handleFileChange" accept="image/*" />
            </label>
          </div>
        </div>

        <!-- 정보 목록 -->
        <div class="bg-white rounded-lg shadow">
          <ul class="divide-y divide-gray-200">
            <li v-for="field in profileFields" :key="field.key" @click="field.isEditable ? openEditModal(field.key) : null" :class="{'cursor-pointer hover:bg-gray-50': field.isEditable}" class="px-4 py-4 flex justify-between items-center">
              <span class="font-medium text-gray-700">{{ field.label }}</span>
              <div class="flex items-center gap-2">
                <span class="text-gray-900">{{ field.value || '' }}</span>
                <ChevronRight v-if="field.isEditable" class="w-5 h-5 text-gray-400" />
              </div>
            </li>
          </ul>
        </div>
      </template>
    </div>

    <!-- 공통 수정 모달 사용 -->
    <SlideUpModal :is-open="isEditModalOpen" @close="closeEditModal">
      <template #header-title>{{ modalTitle }}</template>
      <template #body>
        <form id="edit-form" @submit.prevent="handleSave">
          <!-- 영문 이름 수정 -->
          <div v-if="editingField === 'englishName'" class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">영문 이름 (First Name)</label>
              <input v-model="tempData.firstName" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">영문 성 (Last Name)</label>
              <input v-model="tempData.lastName" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
            </div>
          </div>

          <!-- 비밀번호 변경 -->
          <div v-else-if="editingField === 'password'" class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">현재 비밀번호</label>
              <input v-model="tempData.currentPassword" type="password" required class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">새 비밀번호</label>
              <input v-model="tempData.newPassword" type="password" required minlength="6" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">새 비밀번호 확인</label>
              <input v-model="tempData.confirmPassword" type="password" required class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
            </div>
          </div>
          
          <!-- 기타 단일 필드 수정 -->
          <div v-else>
            <label class="block text-sm font-medium text-gray-700 mb-1">{{ modalTitle }}</label>
            <input :type="editingField === 'passportExpiryDate' ? 'date' : 'text'" v-model="tempData.value" class="w-full px-3 py-2 border border-gray-300 rounded-lg" />
          </div>
        </form>
      </template>
      <template #footer>
        <div class="flex justify-end gap-3">
          <button type="button" @click="closeEditModal" class="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100">취소</button>
          <button type="submit" form="edit-form" class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700">저장</button>
        </div>
      </template>
    </SlideUpModal>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { Camera, ChevronRight } from 'lucide-vue-next'
import MainHeader from '@/components/common/MainHeader.vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import apiClient from '@/services/api'

const loading = ref(true)
const profile = ref({})

const isEditModalOpen = ref(false)
const editingField = ref('')
const tempData = ref({})

const profileFields = computed(() => {
  if (!profile.value) return []
  return [
    { key: 'loginId', label: '아이디', value: profile.value.loginId, isEditable: false },
    { key: 'name', label: '이름', value: profile.value.name, isEditable: false },
    { key: 'englishName', label: '영문 이름', value: `${profile.value.firstName || ''} ${profile.value.lastName || ''}`.trim(), isEditable: true },
    { key: 'phone', label: '휴대폰 번호', value: profile.value.phone, isEditable: true },
    { key: 'email', label: '이메일', value: profile.value.email, isEditable: false },
    { key: 'affiliation', label: '소속', value: profile.value.affiliation, isEditable: true },
    { key: 'passportNumber', label: '여권 번호', value: profile.value.passportNumber, isEditable: true },
    { key: 'passportExpiryDate', label: '여권 만료일', value: profile.value.passportExpiryDate, isEditable: true },
    ...(profile.value.corpName ? [{ key: 'corpName', label: '회사명', value: profile.value.corpName, isEditable: false }] : []),
    ...(profile.value.corpPart ? [{ key: 'corpPart', label: '부서', value: profile.value.corpPart, isEditable: false }] : []),
    { key: 'password', label: '비밀번호 변경', value: '', isEditable: true },
  ]
})

const modalTitle = computed(() => {
  switch (editingField.value) {
    case 'englishName': return '영문 이름 수정'
    case 'phone': return '휴대폰 번호 수정'
    case 'affiliation': return '소속 수정'
    case 'passportNumber': return '여권 번호 수정'
    case 'passportExpiryDate': return '여권 만료일 수정'
    case 'password': return '비밀번호 변경'
    default: return '정보 수정'
  }
})

// 내 정보 로드
async function loadProfile() {
  loading.value = true
  try {
    const response = await apiClient.get('/users/profile')
    profile.value = response.data
  } catch (error) {
    console.error('프로필 로드 실패:', error)
    alert('프로필을 불러올 수 없습니다.')
  } finally {
    loading.value = false
  }
}

// 수정 모달 열기
function openEditModal(fieldKey) {
  editingField.value = fieldKey
  if (fieldKey === 'englishName') {
    tempData.value = { firstName: profile.value.firstName, lastName: profile.value.lastName }
  } else if (fieldKey === 'password') {
    tempData.value = { currentPassword: '', newPassword: '', confirmPassword: '' }
  } else {
    tempData.value = { value: profile.value[fieldKey] }
  }
  isEditModalOpen.value = true
}

// 수정 모달 닫기
function closeEditModal() {
  isEditModalOpen.value = false
  editingField.value = ''
  tempData.value = {}
}

// 변경사항 저장
async function handleSave() {
  let endpoint = '/users/profile'
  let payload = {}

  if (editingField.value === 'password') {
    if (tempData.value.newPassword !== tempData.value.confirmPassword) {
      alert('새 비밀번호가 일치하지 않습니다.')
      return
    }
    endpoint = '/users/password'
    payload = tempData.value
  } else {
    payload = {
      phone: profile.value.phone,
      firstName: profile.value.firstName,
      lastName: profile.value.lastName,
      passportNumber: profile.value.passportNumber,
      passportExpiryDate: profile.value.passportExpiryDate,
      affiliation: profile.value.affiliation,
    }

    if (editingField.value === 'englishName') {
      payload.firstName = tempData.value.firstName
      payload.lastName = tempData.value.lastName
    } else {
      payload[editingField.value] = tempData.value.value
    }
  }
  
  try {
    await apiClient.put(endpoint, payload)
    alert('정보가 수정되었습니다.')
    closeEditModal()
    await loadProfile() // 데이터 새로고침
  } catch (error) {
    console.error('정보 수정 실패:', error)
    alert(error.response?.data?.message || '정보 수정에 실패했습니다.')
  }
}

// 프로필 사진 변경 핸들러
async function handleFileChange(event) {
  const file = event.target.files[0]
  if (!file) return

  const formData = new FormData()
  formData.append('file', file)

  try {
    const response = await apiClient.post('/users/profile/photo', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    profile.value.profileImageUrl = response.data.profileImageUrl
    alert('프로필 사진이 변경되었습니다.')
  } catch (error) {
    console.error('프로필 사진 업로드 실패:', error)
    alert(error.response?.data?.message || '사진 업로드에 실패했습니다.')
  } finally {
    event.target.value = ''
  }
}

onMounted(loadProfile)
</script>
