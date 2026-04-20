<template>
  <SlideUpModal :is-open="isOpen" z-index-class="z-[60]" @close="handleClose">
    <template #header-title>{{
      tripId ? '여행 정보 수정' : '여행 정보 입력'
    }}</template>
    <template #body>
      <form
        id="trip-info-form"
        class="space-y-4"
        @submit.prevent="saveTripInfo"
      >
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1"
            >여행 제목</label
          ><input v-model="tripData.title" type="text" class="w-full input" />
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1"
            >설명</label
          ><textarea
            v-model="tripData.description"
            rows="3"
            class="w-full input"
          ></textarea>
        </div>
        <div>
          <DateRangePicker v-model="dateRange" label="여행 기간" />
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1"
            >도시/국가</label
          >
          <CountryCitySearch v-model="countryCity" />
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1"
            >예산 (원)</label
          >
          <input
            v-model="tripData.budget"
            v-number-format
            type="text"
            step="1"
            min="0"
            placeholder="예산을 입력하세요 (선택사항)"
            class="w-full input"
          />
          <p class="text-xs text-gray-500 mt-1">
            여행 예산을 입력하면 대시보드에서 지출 현황을 추적할 수 있습니다.
          </p>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1"
            >커버 이미지</label
          >
          <div class="space-y-3">
            <div
              v-if="coverImagePreview"
              class="relative w-full h-48 rounded-lg overflow-hidden bg-gray-100"
            >
              <img
                loading="lazy"
                :src="coverImagePreview"
                alt="커버 이미지 미리보기"
                class="w-full h-full object-cover"
              />
              <button
                type="button"
                class="absolute top-2 right-2 p-2.5 bg-red-500 text-white rounded-full hover:bg-red-600 active:scale-95 transition-all shadow-lg"
                @click="removeCoverImage"
              >
                <svg
                  class="w-5 h-5"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M6 18L18 6M6 6l12 12"
                  />
                </svg>
              </button>
            </div>
            <label class="block">
              <input
                ref="coverImageInput"
                type="file"
                accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                class="hidden"
                @change="handleCoverImageChange"
              />
              <div
                class="flex items-center justify-center w-full py-4 px-4 border-2 border-dashed border-gray-300 rounded-lg cursor-pointer hover:border-primary-400 hover:bg-primary-50 transition-colors active:scale-95"
              >
                <div class="text-center">
                  <svg
                    class="mx-auto h-12 w-12 text-gray-400"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"
                    />
                  </svg>
                  <p class="mt-2 text-sm text-gray-600 font-medium">
                    이미지 선택
                  </p>
                  <p class="mt-1 text-xs text-gray-500">
                    JPG, PNG, GIF, WebP (최대 5MB)
                  </p>
                </div>
              </div>
            </label>
          </div>
        </div>
      </form>
    </template>
    <template #footer>
      <div class="space-y-3 w-full">
        <div class="flex gap-3 w-full">
          <button
            type="button"
            class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors"
            @click="handleClose"
          >
            취소
          </button>
          <button
            type="submit"
            form="trip-info-form"
            :disabled="isUploadingImage"
            class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ isUploadingImage ? '업로드 중...' : '저장' }}
          </button>
        </div>
        <button
          v-if="tripId"
          type="button"
          class="w-full py-3 px-4 bg-red-50 text-red-600 rounded-xl font-semibold hover:bg-red-100 active:bg-red-200 transition-colors border border-red-200"
          @click="deleteTrip"
        >
          여행 삭제
        </button>
      </div>
    </template>
  </SlideUpModal>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import DateRangePicker from '@/components/common/DateRangePicker.vue'
import CountryCitySearch from '@/components/common/CountryCitySearch.vue'
import apiClient from '@/services/api'
import { compressImage } from '@/utils/fileUpload'
import dayjs from 'dayjs'

const props = defineProps({
  isOpen: { type: Boolean, required: true },
  trip: { type: Object, required: true },
  tripId: { type: String, default: null },
  effectiveReadonly: { type: Boolean, default: false },
})

const emit = defineEmits(['close', 'saved', 'deleted'])

// Internal state
const tripData = ref({})
const countryCity = ref({ destination: '', countryCode: '' })
const coverImageInput = ref(null)
const coverImageFile = ref(null)
const coverImagePreview = ref(null)
const isUploadingImage = ref(false)

const dateRange = computed({
  get() {
    return {
      start: tripData.value.startDate
        ? new Date(tripData.value.startDate)
        : null,
      end: tripData.value.endDate ? new Date(tripData.value.endDate) : null,
    }
  },
  set(newRange) {
    if (!newRange) {
      tripData.value.startDate = null
      tripData.value.endDate = null
      return
    }
    tripData.value.startDate = newRange.start
      ? dayjs(newRange.start).format('YYYY-MM-DD')
      : null
    tripData.value.endDate = newRange.end
      ? dayjs(newRange.end).format('YYYY-MM-DD')
      : null
  },
})

// 모달이 열릴 때 trip 데이터로 초기화
watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal) {
      tripData.value = { ...props.trip }
      countryCity.value = {
        destination: props.trip.destination,
        countryCode: props.trip.countryCode,
      }
      if (props.trip.coverImageUrl) {
        coverImagePreview.value = props.trip.coverImageUrl
      } else {
        coverImagePreview.value = null
      }
      coverImageFile.value = null
    }
  },
)

function handleClose() {
  emit('close')
}

function handleCoverImageChange(event) {
  const file = event.target.files?.[0]
  if (!file) return

  // 파일 크기 체크 (5MB)
  if (file.size > 5 * 1024 * 1024) {
    alert('파일 크기는 5MB를 초과할 수 없습니다.')
    if (coverImageInput.value) {
      coverImageInput.value.value = ''
    }
    return
  }

  // 파일 타입 체크
  const allowedTypes = [
    'image/jpeg',
    'image/jpg',
    'image/png',
    'image/gif',
    'image/webp',
  ]
  if (!allowedTypes.includes(file.type)) {
    alert('지원하지 않는 파일 형식입니다. (JPG, PNG, GIF, WebP만 가능)')
    if (coverImageInput.value) {
      coverImageInput.value.value = ''
    }
    return
  }

  coverImageFile.value = file

  const reader = new FileReader()
  reader.onload = (e) => {
    coverImagePreview.value = e.target.result
  }
  reader.onerror = () => {
    alert('이미지 파일을 읽는 데 실패했습니다.')
  }
  reader.readAsDataURL(file)
}

function removeCoverImage() {
  coverImageFile.value = null
  coverImagePreview.value = null
  tripData.value.coverImageUrl = null
  if (coverImageInput.value) {
    coverImageInput.value.value = ''
  }
}

async function uploadCoverImage() {
  if (!coverImageFile.value) return null

  const formData = new FormData()
  formData.append('file', await compressImage(coverImageFile.value))

  try {
    isUploadingImage.value = true
    const response = await apiClient.post(
      '/personal-trips/upload-cover-image',
      formData,
      {
        headers: { 'Content-Type': 'multipart/form-data' },
      },
    )
    return response.data.url
  } catch (error) {
    console.error('Failed to upload cover image:', error)
    alert(
      `이미지 업로드에 실패했습니다.\n${error.response?.data?.message || error.message}`,
    )
    return null
  } finally {
    isUploadingImage.value = false
  }
}

async function saveTripInfo() {
  if (props.effectiveReadonly) return
  tripData.value.destination = countryCity.value.destination
  tripData.value.countryCode = countryCity.value.countryCode

  // 예산 값에서 콤마 제거하고 숫자로 변환
  if (tripData.value.budget) {
    const budgetStr = String(tripData.value.budget).replace(/,/g, '')
    tripData.value.budget = budgetStr ? Number(budgetStr) : null
  }

  try {
    // 새 이미지 파일이 있으면 업로드
    if (coverImageFile.value) {
      const imageUrl = await uploadCoverImage()
      if (imageUrl) {
        tripData.value.coverImageUrl = imageUrl
      }
    }

    if (!props.tripId) {
      // 새 여행 생성
      const response = await apiClient.post('/personal-trips', tripData.value)
      emit('saved', {
        isNew: true,
        tripId: response.data.id,
        data: response.data,
      })
    } else {
      // 기존 여행 수정
      const response = await apiClient.put(
        `/personal-trips/${props.tripId}`,
        tripData.value,
      )
      emit('saved', { isNew: false, tripId: props.tripId, data: response.data })
    }
  } catch (error) {
    console.error('Failed to save trip info:', error)
    alert('저장에 실패했습니다.')
  }
}

async function deleteTrip() {
  if (props.effectiveReadonly) return
  if (!props.tripId) return

  if (
    !confirm(
      '정말 이 여행을 삭제하시겠습니까?\n삭제된 여행은 목록에서 보이지 않게 됩니다.',
    )
  )
    return

  try {
    await apiClient.delete(`/personal-trips/${props.tripId}`)
    emit('deleted')
  } catch (error) {
    console.error('Failed to delete trip:', error)
    alert('삭제에 실패했습니다.')
  }
}
</script>
