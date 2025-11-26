<template>
  <SlideUpModal :is-open="isOpen" @close="closeModal">
    <template #header-title>숙소 상세</template>
    <template #body>
      <div v-if="accommodation" class="space-y-4">
        <div class="flex items-center justify-between gap-2">
          <h3 class="text-xl font-bold flex-1">{{ accommodation.name }}</h3>
          <button type="button" @click="copyAddressToClipboard" class="p-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors flex-shrink-0" title="주소 복사">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z" /></svg>
          </button>
        </div>
        <p class="text-gray-600">{{ accommodation.address }}</p>
        <p v-if="accommodation.postalCode" class="text-gray-600">우편번호: {{ accommodation.postalCode }}</p>
        <p class="text-gray-800 font-medium">체크인: {{ formatDateTime(accommodation.checkInTime) }}</p>
        <p class="text-gray-800 font-medium">체크아웃: {{ formatDateTime(accommodation.checkOutTime) }}</p>
        <p v-if="accommodation.expenseAmount" class="text-gray-800 font-medium">비용: ₩{{ accommodation.expenseAmount.toLocaleString() }}</p> <!-- Added this line -->
        <p v-if="accommodation.bookingReference" class="text-gray-700">예약 번호: {{ accommodation.bookingReference }}</p>
        <p v-if="accommodation.contactNumber" class="text-gray-700">연락처: {{ accommodation.contactNumber }}</p>
        <p v-if="accommodation.notes" class="whitespace-pre-wrap text-gray-700">{{ accommodation.notes }}</p>

        <div class="h-48 w-full rounded-lg mt-4">
          <KakaoMap
            v-if="isDomestic && accommodation.latitude && accommodation.longitude"
            :latitude="accommodation.latitude"
            :longitude="accommodation.longitude"
          />
          <GoogleMapPlaceholder v-else-if="!isDomestic && accommodation.latitude" />
        </div>
      </div>
    </template>
    <template #footer>
      <div class="flex gap-3 w-full">
        <button type="button" @click="closeModal" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors">닫기</button>
        <button v-if="showEdit" type="button" @click="editAccommodation" class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold hover:bg-primary-600 active:scale-95 transition-all">수정</button>
      </div>
    </template>
  </SlideUpModal>
</template>

<script setup>
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import KakaoMap from '@/components/common/KakaoMap.vue'
import GoogleMapPlaceholder from '@/components/common/GoogleMapPlaceholder.vue'
import { format } from 'date-fns'

const props = defineProps({
  isOpen: {
    type: Boolean,
    default: false
  },
  accommodation: {
    type: Object,
    default: null
  },
  isDomestic: {
    type: Boolean,
    default: false
  },
  showEdit: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits(['close', 'edit'])

const closeModal = () => {
  emit('close')
}

const editAccommodation = () => {
  emit('edit', props.accommodation)
  closeModal()
}

const formatDateTime = (dateTimeString) => {
  if (!dateTimeString) return 'N/A'
  try {
    // Assuming dateTimeString is in ISO format or similar that Date can parse
    const date = new Date(dateTimeString)
    return format(date, 'yyyy년 MM월 dd일 HH시 mm분')
  } catch (e) {
    console.error('Error formatting date:', e)
    return dateTimeString // Return original if parsing fails
  }
}

const copyAddressToClipboard = () => {
  if (!props.accommodation?.address) {
    alert('주소 정보가 없습니다.')
    return
  }

  const address = props.accommodation.address
  if (!navigator.clipboard) {
    fallbackCopyTextToClipboard(address)
    return
  }
  navigator.clipboard.writeText(address).then(() => {
    alert('주소가 클립보드에 복사되었습니다.')
  }).catch(err => {
    console.error('navigator.clipboard를 사용한 복사 실패: ', err)
    fallbackCopyTextToClipboard(address)
  })
}

const fallbackCopyTextToClipboard = (text) => {
  const textArea = document.createElement("textarea")
  textArea.value = text
  textArea.style.top = "0"
  textArea.style.left = "0"
  textArea.style.position = "fixed"
  document.body.appendChild(textArea)
  textArea.focus()
  textArea.select()
  try {
    const successful = document.execCommand('copy')
    if (successful) {
      alert('주소가 클립보드에 복사되었습니다.')
    } else {
      alert('주소 복사에 실패했습니다.')
    }
  } catch (err) {
    console.error('document.execCommand를 사용한 복사 실패: ', err)
    alert('주소 복사에 실패했습니다.')
  }
  document.body.removeChild(textArea)
}

const openNavigation = () => {
  if (!props.accommodation) {
    alert('숙소 정보가 없습니다.')
    return
  }

  const acc = props.accommodation
  let url = ''

  if (acc.latitude && acc.longitude) {
    // 카카오맵 좌표 기반 길찾기
    url = `https://map.kakao.com/link/to/${encodeURIComponent(acc.name)},${acc.latitude},${acc.longitude}`
  } else if (acc.address) {
    // 카카오맵 주소 기반 검색
    url = `https://map.kakao.com/link/search/${encodeURIComponent(acc.address)}`
  } else {
    alert('주소 정보가 없습니다.')
    return
  }

  window.open(url, '_blank')
}
</script>
