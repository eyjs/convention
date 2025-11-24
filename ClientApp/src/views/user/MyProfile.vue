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

        <!-- 프로필 편집 버튼 -->
        <div class="px-4 py-4">
          <button @click="openProfileEditModal" class="w-full py-3 px-4 bg-gradient-to-r from-cyan-500 to-blue-600 text-white rounded-xl font-semibold hover:shadow-lg active:scale-95 transition-all">
            프로필 편집
          </button>
        </div>

        <!-- 정보 목록 -->
        <div class="bg-white rounded-lg shadow">
          <ul class="divide-y divide-gray-200">
            <li v-for="field in profileFields" :key="field.key" class="px-4 py-4 flex justify-between items-center">
              <span class="font-medium text-gray-700">{{ field.label }}</span>
              <div class="flex items-center gap-2">
                <span class="text-gray-900">{{ field.value || '-' }}</span>
              </div>
            </li>
          </ul>
        </div>

        <!-- 비밀번호 변경 버튼 -->
        <div class="px-4 py-4">
          <button @click="openPasswordChangeModal" class="w-full py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors">
            비밀번호 변경
          </button>
        </div>
      </template>
    </div>

    <!-- 프로필 편집 모달 -->
    <SlideUpModal :is-open="isProfileEditModalOpen" @close="closeProfileEditModal">
      <template #header-title>프로필 편집</template>
      <template #body>
          <form id="profile-edit-form" @submit.prevent="handleProfileSave" class="space-y-4">

              <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">휴대폰 번호</label>
                  <input v-model="tempProfileData.phone" type="tel" class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
              </div>
              <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">소속</label>
                  <input v-model="tempProfileData.affiliation" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
              </div>
              <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">영문 이름 (First Name)</label>
                  <input v-model="tempProfileData.firstName" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
              </div>
              <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">영문 성 (Last Name)</label>
                  <input v-model="tempProfileData.lastName" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
              </div>
              <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">여권 번호</label>
                  <input v-model="tempProfileData.passportNumber" type="text" class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
              </div>
              <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">여권 만료일</label>
                  <input v-model="tempProfileData.passportExpiryDate" type="date" class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
              </div>
          </form>
      </template>
      <template #footer>
        <div class="flex gap-3 w-full">
          <button type="button" @click="closeProfileEditModal" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors">취소</button>
          <button type="submit" form="profile-edit-form" class="flex-1 py-3 px-4 bg-gradient-to-r from-cyan-500 to-blue-600 text-white rounded-xl font-semibold hover:shadow-lg active:scale-95 transition-all">저장</button>
        </div>
      </template>
    </SlideUpModal>

    <!-- 비밀번호 변경 모달 -->
    <SlideUpModal :is-open="isPasswordModalOpen" @close="closePasswordModal">
      <template #header-title>비밀번호 변경</template>
      <template #body>
        <form id="password-form" @submit.prevent="handlePasswordSave" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">현재 비밀번호</label>
            <input v-model="tempPasswordData.currentPassword" type="password" required class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">새 비밀번호</label>
            <input v-model="tempPasswordData.newPassword" type="password" required minlength="6" class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">새 비밀번호 확인</label>
            <input v-model="tempPasswordData.confirmPassword" type="password" required class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500" />
          </div>
        </form>
      </template>
      <template #footer>
        <div class="flex gap-3 w-full">
          <button type="button" @click="closePasswordModal" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors">취소</button>
          <button type="submit" form="password-form" class="flex-1 py-3 px-4 bg-gradient-to-r from-cyan-500 to-blue-600 text-white rounded-xl font-semibold hover:shadow-lg active:scale-95 transition-all">저장</button>
        </div>
      </template>
    </SlideUpModal>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { Camera } from 'lucide-vue-next'
import MainHeader from '@/components/common/MainHeader.vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import apiClient from '@/services/api'

const loading = ref(true)
const profile = ref({})

const isProfileEditModalOpen = ref(false)
const isPasswordModalOpen = ref(false)
const tempProfileData = ref({})
const tempPasswordData = ref({})

const profileFields = computed(() => {
  if (!profile.value) return []
  return [
    { key: 'loginId', label: '아이디', value: profile.value.loginId },
    { key: 'name', label: '이름', value: profile.value.name },
    { key: 'englishName', label: '영문 이름', value: `${profile.value.firstName || ''} ${profile.value.lastName || ''}`.trim() },
    { key: 'phone', label: '휴대폰 번호', value: profile.value.phone },
    { key: 'email', label: '이메일', value: profile.value.email },
    { key: 'affiliation', label: '소속', value: profile.value.affiliation },
    { key: 'passportNumber', label: '여권 번호', value: profile.value.passportNumber },
    { key: 'passportExpiryDate', label: '여권 만료일', value: profile.value.passportExpiryDate },
    ...(profile.value.corpName ? [{ key: 'corpName', label: '회사명', value: profile.value.corpName }] : []),
    ...(profile.value.corpPart ? [{ key: 'corpPart', label: '부서', value: profile.value.corpPart }] : []),
  ]
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

// 프로필 편집 모달 열기
function openProfileEditModal() {
  tempProfileData.value = {
    firstName: profile.value.firstName || '',
    lastName: profile.value.lastName || '',
    phone: profile.value.phone || '',
    affiliation: profile.value.affiliation || '',
    passportNumber: profile.value.passportNumber || '',
    passportExpiryDate: profile.value.passportExpiryDate || ''
  }
  isProfileEditModalOpen.value = true
}

// 프로필 편집 모달 닫기
function closeProfileEditModal() {
  isProfileEditModalOpen.value = false
  tempProfileData.value = {}
}

// 비밀번호 변경 모달 열기
function openPasswordChangeModal() {
  tempPasswordData.value = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  }
  isPasswordModalOpen.value = true
}

// 비밀번호 변경 모달 닫기
function closePasswordModal() {
  isPasswordModalOpen.value = false
  tempPasswordData.value = {}
}

// 프로필 저장
async function handleProfileSave() {
  const payload = {
    firstName: tempProfileData.value.firstName,
    lastName: tempProfileData.value.lastName,
    phone: tempProfileData.value.phone,
    affiliation: tempProfileData.value.affiliation,
    passportNumber: tempProfileData.value.passportNumber,
    passportExpiryDate: tempProfileData.value.passportExpiryDate,
  }

  try {
    await apiClient.put('/users/profile', payload)
    alert('프로필이 수정되었습니다.')
    closeProfileEditModal()
    await loadProfile() // 데이터 새로고침
  } catch (error) {
    console.error('프로필 수정 실패:', error)
    alert(error.response?.data?.message || '프로필 수정에 실패했습니다.')
  }
}

// 비밀번호 저장
async function handlePasswordSave() {
  if (tempPasswordData.value.newPassword !== tempPasswordData.value.confirmPassword) {
    alert('새 비밀번호가 일치하지 않습니다.')
    return
  }

  try {
    await apiClient.put('/users/password', tempPasswordData.value)
    alert('비밀번호가 변경되었습니다.')
    closePasswordModal()
  } catch (error) {
    console.error('비밀번호 변경 실패:', error)
    alert(error.response?.data?.message || '비밀번호 변경에 실패했습니다.')
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
