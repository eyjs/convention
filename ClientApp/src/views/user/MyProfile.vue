<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader title="내 정보" :show-back="true" />

    <div class="max-w-2xl mx-auto py-6">
      <!-- 로딩 상태 -->
      <div v-if="loading" class="text-center py-12">
        <div
          class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"
        ></div>
        <p class="mt-4 text-gray-600">로딩 중...</p>
      </div>

      <template v-else>
        <!-- 프로필 사진 -->
        <div class="px-4 py-6 flex flex-col items-center">
          <div class="relative">
            <img
              v-if="profile.profileImageUrl"
              loading="lazy"
              :src="profile.profileImageUrl"
              alt="프로필 사진"
              class="h-24 w-24 rounded-full object-cover"
            />
            <div
              v-else
              class="h-24 w-24 rounded-full bg-gray-200 flex items-center justify-center"
            >
              <svg
                class="h-16 w-16 text-gray-500"
                fill="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  d="M24 20.993V24H0v-2.997A14.977 14.977 0 0112.004 15c4.904 0 9.26 2.354 11.996 5.993zM16.002 8.999a4 4 0 11-8 0 4 4 0 018 0z"
                />
              </svg>
            </div>
            <label
              for="profile-photo-upload"
              class="absolute -bottom-1 -right-1 bg-white p-1.5 rounded-full shadow-md cursor-pointer hover:bg-gray-100"
            >
              <Camera class="w-5 h-5 text-gray-600" />
              <input
                id="profile-photo-upload"
                type="file"
                class="hidden"
                accept="image/*"
                @change="handleFileChange"
              />
            </label>
          </div>
        </div>

        <!-- 기본 정보 목록 -->
        <div class="bg-white rounded-lg shadow">
          <ul class="divide-y divide-gray-200">
            <template v-for="field in profileFields" :key="field.key">
              <li
                v-if="!field.editable"
                class="px-4 py-4 flex justify-between items-center"
              >
                <span class="font-medium text-gray-700">{{ field.label }}</span>
                <span class="text-gray-500">{{ field.value || '-' }}</span>
              </li>
              <button
                v-else
                class="w-full text-left px-4 py-4 flex justify-between items-center hover:bg-gray-50 transition-colors"
                @click="openEditFieldModal(field)"
              >
                <span class="font-medium text-gray-700">{{ field.label }}</span>
                <div class="flex items-center gap-2">
                  <span class="text-gray-900">{{ field.value || '-' }}</span>
                  <ChevronRight class="w-5 h-5 text-gray-400" />
                </div>
              </button>
            </template>
          </ul>
        </div>

        <!-- 여권 정보 섹션 -->
        <div class="mt-6">
          <div class="px-4 flex items-center justify-between mb-2">
            <h2 class="text-lg font-semibold text-gray-800">여권 정보</h2>
            <span
              v-if="profile.passportVerified"
              class="px-2.5 py-1 text-xs font-semibold rounded-full bg-green-100 text-green-700"
            >
              승인완료
            </span>
          </div>
          <!-- 승인 상태 안내 -->
          <div
            v-if="profile.passportVerified"
            class="mx-4 mb-2 px-3 py-2 bg-green-50 border border-green-200 rounded-lg"
          >
            <p class="text-xs text-green-700">
              여권 정보가 관리자에 의해 승인되었습니다. 수정이 필요한 경우
              관리자에게 문의해주세요.
            </p>
          </div>
          <div class="bg-white rounded-lg shadow">
            <ul class="divide-y divide-gray-200">
              <template v-for="field in passportFields" :key="field.key">
                <!-- 수정 가능 (미승인) -->
                <button
                  v-if="field.editable"
                  class="w-full text-left px-4 py-4 flex justify-between items-center hover:bg-gray-50 transition-colors"
                  @click="openEditFieldModal(field)"
                >
                  <span class="font-medium text-gray-700">{{
                    field.label
                  }}</span>
                  <div class="flex items-center gap-2">
                    <span class="text-gray-900">{{ field.value || '-' }}</span>
                    <ChevronRight class="w-5 h-5 text-gray-400" />
                  </div>
                </button>
                <!-- 수정 불가 (승인됨) -->
                <li v-else class="px-4 py-4 flex justify-between items-center">
                  <span class="font-medium text-gray-700">{{
                    field.label
                  }}</span>
                  <div class="flex items-center gap-2">
                    <span class="text-gray-900">{{ field.value || '-' }}</span>
                    <Lock class="w-4 h-4 text-green-500" />
                  </div>
                </li>
              </template>
              <!-- 여권 이미지 필드 -->
              <li class="px-4 py-4 flex justify-between items-center">
                <span class="font-medium text-gray-700">여권 이미지</span>
                <div v-if="profile.passportImageUrl">
                  <button
                    class="text-sm font-medium text-blue-600 hover:underline"
                    @click="showPassportImage"
                  >
                    여권 이미지 확인
                  </button>
                </div>
                <div v-else-if="!profile.passportVerified">
                  <label
                    for="passport-image-upload"
                    class="text-sm font-medium text-blue-600 hover:underline cursor-pointer"
                  >
                    <span>업로드</span>
                    <input
                      id="passport-image-upload"
                      type="file"
                      class="hidden"
                      accept="image/*"
                      @change="handlePassportImageUpload"
                    />
                  </label>
                </div>
                <div v-else>
                  <span class="text-sm text-gray-400">-</span>
                </div>
              </li>
            </ul>
          </div>
        </div>

        <!-- 비밀번호 변경 버튼 -->
        <div class="px-4 py-4 mt-2">
          <button
            class="w-full py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors"
            @click="openPasswordChangeModal"
          >
            비밀번호 변경
          </button>
        </div>
      </template>
    </div>

    <!-- 필드 편집 모달 -->
    <EditFieldPopup
      :is-open="isEditFieldModalOpen"
      :title="`${editingField.label} 수정`"
      :label="editingField.label"
      :value="editingField.value"
      :type="editingField.type"
      :field-key="editingField.key"
      @close="closeEditFieldModal"
      @save="handleFieldSave"
    />

    <!-- 비밀번호 변경 모달 -->
    <SlideUpModal :is-open="isPasswordModalOpen" @close="closePasswordModal">
      <template #header-title>비밀번호 변경</template>
      <template #body>
        <form
          id="password-form"
          class="space-y-4"
          @submit.prevent="handlePasswordSave"
        >
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1"
              >현재 비밀번호</label
            >
            <input
              v-model="tempPasswordData.currentPassword"
              type="password"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1"
              >새 비밀번호</label
            >
            <input
              v-model="tempPasswordData.newPassword"
              type="password"
              required
              minlength="6"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1"
              >새 비밀번호 확인</label
            >
            <input
              v-model="tempPasswordData.confirmPassword"
              type="password"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </form>
      </template>
      <template #footer>
        <div class="flex gap-3 w-full">
          <button
            type="button"
            class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors"
            @click="closePasswordModal"
          >
            취소
          </button>
          <button
            type="submit"
            form="password-form"
            class="flex-1 py-3 px-4 bg-gradient-to-r from-cyan-500 to-blue-600 text-white rounded-xl font-semibold hover:shadow-lg active:scale-95 transition-all"
          >
            저장
          </button>
        </div>
      </template>
    </SlideUpModal>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { Camera, ChevronRight, Lock } from 'lucide-vue-next'
import 'viewerjs/dist/viewer.css'
import { api as viewerApi } from 'v-viewer'
import MainHeader from '@/components/common/MainHeader.vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import EditFieldPopup from '@/components/common/EditFieldPopup.vue'
import apiClient, { userAPI } from '@/services/api'
import { compressImage } from '@/utils/fileUpload'

const loading = ref(true)
const profile = ref({})

const isPasswordModalOpen = ref(false)
const tempPasswordData = ref({})
const isEditFieldModalOpen = ref(false)
const editingField = ref({})

const allFields = computed(() => {
  if (!profile.value) return []
  const p = profile.value
  return [
    {
      key: 'loginId',
      label: '아이디',
      value: p.loginId,
      editable: false,
      group: 'profile',
    },
    {
      key: 'name',
      label: '이름',
      value: p.name,
      editable: false,
      group: 'profile',
    },
    {
      key: 'phone',
      label: '휴대폰 번호',
      value: p.phone,
      editable: true,
      type: 'tel',
      group: 'profile',
    },
    {
      key: 'email',
      label: '이메일',
      value: p.email,
      editable: true,
      type: 'email',
      group: 'profile',
    },
    {
      key: 'affiliation',
      label: '소속',
      value: p.affiliation,
      editable: true,
      type: 'text',
      group: 'profile',
    },
    ...(p.corpName
      ? [
          {
            key: 'corpName',
            label: '회사명',
            value: p.corpName,
            editable: false,
            group: 'profile',
          },
        ]
      : []),
    ...(p.corpPart
      ? [
          {
            key: 'corpPart',
            label: '부서',
            value: p.corpPart,
            editable: false,
            group: 'profile',
          },
        ]
      : []),

    {
      key: 'firstName',
      label: '영문 이름 (First Name)',
      value: p.firstName,
      editable: !p.passportVerified,
      type: 'text',
      group: 'passport',
    },
    {
      key: 'lastName',
      label: '영문 성 (Last Name)',
      value: p.lastName,
      editable: !p.passportVerified,
      type: 'text',
      group: 'passport',
    },
    {
      key: 'passportNumber',
      label: '여권 번호',
      value: p.passportNumber,
      editable: !p.passportVerified,
      type: 'text',
      group: 'passport',
    },
    {
      key: 'passportExpiryDate',
      label: '여권 만료일',
      value: p.passportExpiryDate,
      editable: !p.passportVerified,
      type: 'date',
      group: 'passport',
    },
  ]
})

const profileFields = computed(() =>
  allFields.value.filter((f) => f.group === 'profile'),
)
const passportFields = computed(() =>
  allFields.value.filter((f) => f.group === 'passport'),
)

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

function openEditFieldModal(field) {
  if (!field.editable) return
  editingField.value = { ...field }
  isEditFieldModalOpen.value = true
}

function closeEditFieldModal() {
  isEditFieldModalOpen.value = false
  editingField.value = {}
}

async function handleFieldSave({ key, value }) {
  try {
    await userAPI.updateProfileField(key, value)
    alert('정보가 수정되었습니다.')
    closeEditFieldModal()
    await loadProfile()
  } catch (error) {
    console.error('프로필 필드 수정 실패:', error)
    alert(error.response?.data?.message || '정보 수정에 실패했습니다.')
  }
}

function openPasswordChangeModal() {
  tempPasswordData.value = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  }
  isPasswordModalOpen.value = true
}

function closePasswordModal() {
  isPasswordModalOpen.value = false
  tempPasswordData.value = {}
}

async function handlePasswordSave() {
  if (
    tempPasswordData.value.newPassword !==
    tempPasswordData.value.confirmPassword
  ) {
    alert('새 비밀번호가 일치하지 않습니다.')
    return
  }
  try {
    await apiClient.put('/users/password', tempPasswordData.value)
    // 기본 비밀번호 경고 플래그 정리
    sessionStorage.removeItem('defaultPasswordLogin')
    // 계정별 dismiss 플래그 제거 (향후 다시 1111로 리셋될 경우 대비)
    const uid = (await apiClient.get('/users/profile').catch(() => null))?.data
      ?.id
    if (uid) localStorage.removeItem(`defaultPasswordDismissed:${uid}`)
    alert('비밀번호가 변경되었습니다.')
    closePasswordModal()
  } catch (error) {
    console.error('비밀번호 변경 실패:', error)
    alert(error.response?.data?.message || '비밀번호 변경에 실패했습니다.')
  }
}

async function handleFileChange(event) {
  const file = event.target.files[0]
  if (!file) return

  const formData = new FormData()
  formData.append('file', await compressImage(file))

  try {
    const response = await apiClient.post('/users/profile/photo', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
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

async function handlePassportImageUpload(event) {
  const file = event.target.files[0]
  if (!file) return
  try {
    const response = await userAPI.uploadPassportImage(file)
    profile.value.passportImageUrl = response.data.passportImageUrl
    alert('여권 사진이 업로드되었습니다.')
  } catch (error) {
    console.error('여권 사진 업로드 실패:', error)
    alert(error.response?.data?.message || '사진 업로드에 실패했습니다.')
  } finally {
    event.target.value = ''
  }
}

function showPassportImage() {
  if (profile.value.passportImageUrl) {
    viewerApi({
      images: [profile.value.passportImageUrl],
    })
  }
}

onMounted(loadProfile)
</script>
