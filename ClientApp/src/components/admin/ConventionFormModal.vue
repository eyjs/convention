<template>
  <BaseModal :is-open="true" @close="$emit('close')" max-width="2xl">
    <template #header>
      <h2 class="text-xl font-bold">
        {{ convention ? '행사 수정' : '새 행사 만들기' }}
      </h2>
    </template>
    <template #body>
      <form @submit.prevent="handleSubmit" class="space-y-6">
        <!-- 행사명 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">
            행사명 <span class="text-red-500">*</span>
          </label>
          <input
            v-model="form.title"
            type="text"
            required
            class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            placeholder="예: iFA STAR TOUR @ ROMA"
          />
        </div>

        <!-- 행사 유형 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">
            행사 유형 <span class="text-red-500">*</span>
          </label>
          <div class="grid grid-cols-2 gap-4">
            <label
              class="relative flex items-center p-4 border-2 rounded-lg cursor-pointer hover:border-primary-500 transition-colors"
              :class="{
                'border-primary-500 bg-primary-50':
                  form.conventionType === 'DOMESTIC',
              }"
            >
              <input
                v-model="form.conventionType"
                type="radio"
                value="DOMESTIC"
                class="sr-only"
              />
              <div class="flex-1">
                <div class="font-medium">국내 행사</div>
                <div class="text-sm text-gray-500">국내에서 진행되는 행사</div>
              </div>
            </label>
            <label
              class="relative flex items-center p-4 border-2 rounded-lg cursor-pointer hover:border-primary-500 transition-colors"
              :class="{
                'border-primary-500 bg-primary-50':
                  form.conventionType === 'OVERSEAS',
              }"
            >
              <input
                v-model="form.conventionType"
                type="radio"
                value="OVERSEAS"
                class="sr-only"
              />
              <div class="flex-1">
                <div class="font-medium">해외 행사</div>
                <div class="text-sm text-gray-500">해외에서 진행되는 행사</div>
              </div>
            </label>
          </div>
        </div>

        <!-- 기간 -->
        <div>
          <DateRangePicker v-model="dateRange" label="기간" />
        </div>

        <!-- 커버 이미지 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">커버 이미지</label>
          <div class="space-y-3">
            <div v-if="coverImagePreview" class="relative w-full h-48 rounded-lg overflow-hidden bg-gray-100">
              <img :src="coverImagePreview" alt="커버 이미지 미리보기" class="w-full h-full object-cover" />
              <button type="button" @click="removeCoverImage" class="absolute top-2 right-2 p-2.5 bg-red-500 text-white rounded-full hover:bg-red-600 active:scale-95 transition-all shadow-lg">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
              </button>
            </div>
            <label class="block">
              <input type="file" ref="coverImageInput" @change="handleCoverImageChange" accept="image/jpeg,image/jpg,image/png,image/gif,image/webp" class="hidden" />
              <div class="flex items-center justify-center w-full py-4 px-4 border-2 border-dashed border-gray-300 rounded-lg cursor-pointer hover:border-blue-400 hover:bg-blue-50 transition-colors active:scale-95">
                <div class="text-center">
                  <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                  <p class="mt-2 text-sm text-gray-600 font-medium">이미지 선택</p>
                  <p class="mt-1 text-xs text-gray-500">JPG, PNG, GIF, WebP (최대 5MB)</p>
                </div>
              </div>
            </label>
          </div>
        </div>

        <!-- 브랜드 컬러 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">
            브랜드 컬러
          </label>
          <div class="flex items-center space-x-4 mb-2">
            <input
              v-model="form.brandColor"
              type="color"
              class="w-16 h-10 rounded cursor-pointer"
              :disabled="useDefaultColor"
            />
            <input
              v-model="form.brandColor"
              type="text"
              class="flex-1 px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              placeholder="#6366f1"
              :disabled="useDefaultColor"
            />
          </div>
          <label class="flex items-center space-x-2 cursor-pointer">
            <input
              v-model="useDefaultColor"
              type="checkbox"
              class="w-4 h-4 text-[#17B185] border-gray-300 rounded focus:ring-[#17B185]"
            />
            <span class="text-sm text-gray-700">기본색상 사용 (#17B185)</span>
          </label>
          <p class="text-sm text-gray-500 mt-1">
            행사의 메인 컬러를 설정합니다
          </p>
        </div>
      </form>
    </template>
    <template #footer>
      <button
        type="button"
        @click="$emit('close')"
        class="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
      >
        취소
      </button>
      <button
        @click="handleSubmit"
        :disabled="saving || isUploadingImage"
        class="px-4 py-2 bg-primary-600 hover:bg-primary-700 disabled:bg-gray-400 text-white rounded-lg transition-colors flex items-center space-x-2"
      >
        <svg
          v-if="saving || isUploadingImage"
          class="animate-spin w-4 h-4"
          fill="none"
          viewBox="0 0 24 24"
        >
          <circle
            class="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            stroke-width="4"
          ></circle>
          <path
            class="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          ></path>
        </svg>
        <span>{{ isUploadingImage ? '업로드 중...' : saving ? '저장 중...' : convention ? '수정' : '생성' }}</span>
      </button>
    </template>
  </BaseModal>
</template>

<script setup>
import { ref, watch, computed } from 'vue'
import BaseModal from '@/components/common/BaseModal.vue'
import DateRangePicker from '@/components/common/DateRangePicker.vue'
import apiClient from '@/services/api'
import dayjs from 'dayjs'

const props = defineProps({
  convention: Object,
})

const emit = defineEmits(['close', 'save'])

const saving = ref(false)
const useDefaultColor = ref(false)
const DEFAULT_BRAND_COLOR = '#17B185'

const form = ref({
  title: '',
  conventionType: 'DOMESTIC',
  startDate: '',
  endDate: '',
  brandColor: '#6366f1',
  conventionImg: null,
  renderType: 'STANDARD',
  themePreset: 'default',
})

// Image upload state
const coverImageInput = ref(null)
const coverImageFile = ref(null)
const coverImagePreview = ref(null)
const isUploadingImage = ref(false)

const dateRange = computed({
  get() {
    return {
      start: form.value.startDate ? new Date(form.value.startDate) : null,
      end: form.value.endDate ? new Date(form.value.endDate) : null,
    }
  },
  set(newRange) {
    if (!newRange) {
      form.value.startDate = null
      form.value.endDate = null
      return
    }
    form.value.startDate = newRange.start ? dayjs(newRange.start).format('YYYY-MM-DD') : null
    form.value.endDate = newRange.end ? dayjs(newRange.end).format('YYYY-MM-DD') : null
  },
})

// 기본색상 체크박스 변경 시 브랜드 컬러 자동 설정
watch(useDefaultColor, (isDefault) => {
  if (isDefault) {
    form.value.brandColor = DEFAULT_BRAND_COLOR
  }
})

// 수정 모드일 때 기존 데이터 로드
watch(
  () => props.convention,
  (newVal) => {
    if (newVal) {
      const brandColor = newVal.brandColor || '#6366f1'
      form.value = {
        title: newVal.title,
        conventionType: newVal.conventionType,
        startDate: newVal.startDate ? newVal.startDate.split('T')[0] : '',
        endDate: newVal.endDate ? newVal.endDate.split('T')[0] : '',
        brandColor: brandColor,
        conventionImg: newVal.conventionImg,
        renderType: newVal.renderType || 'STANDARD',
        themePreset: newVal.themePreset || 'default',
      }
      // 기본색상이면 체크박스 체크
      useDefaultColor.value = brandColor === DEFAULT_BRAND_COLOR
      // 이미지 미리보기 설정
      if (newVal.conventionImg) {
        coverImagePreview.value = newVal.conventionImg
      }
    }
  },
  { immediate: true },
)

function handleCoverImageChange(event) {
  const file = event.target.files?.[0]
  if (!file) return

  if (file.size > 5 * 1024 * 1024) {
    alert('파일 크기는 5MB를 초과할 수 없습니다.')
    return
  }
  
  coverImageFile.value = file
  const reader = new FileReader()
  reader.onload = (e) => {
    coverImagePreview.value = e.target.result
  }
  reader.readAsDataURL(file)
}

function removeCoverImage() {
  coverImageFile.value = null
  coverImagePreview.value = null
  form.value.conventionImg = null
  if (coverImageInput.value) {
    coverImageInput.value.value = ''
  }
}

async function uploadConventionCoverImage() {
  if (!coverImageFile.value) return null

  const formData = new FormData()
  formData.append('file', coverImageFile.value)
  isUploadingImage.value = true

  try {
    const response = await apiClient.post('/admin/conventions/upload-cover-image', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return response.data.url
  } catch (error) {
    console.error('Failed to upload cover image:', error)
    alert('커버 이미지 업로드에 실패했습니다.')
    return null
  } finally {
    isUploadingImage.value = false
  }
}

const handleSubmit = async () => {
  saving.value = true
  try {
    if (coverImageFile.value) {
      const imageUrl = await uploadConventionCoverImage()
      if (imageUrl) {
        form.value.conventionImg = imageUrl
      } else {
        // 업로드 실패 시 저장 중단
        saving.value = false
        return
      }
    }
    await emit('save', form.value)
  } finally {
    saving.value = false
  }
}
</script>
