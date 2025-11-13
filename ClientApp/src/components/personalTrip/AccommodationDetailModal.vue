<template>
  <SlideUpModal :is-open="isOpen" @close="closeModal">
    <template #header-title>숙소 상세</template>
    <template #body>
      <div v-if="accommodation" class="space-y-4">
        <h3 class="text-xl font-bold">{{ accommodation.name }}</h3>
        <p class="text-gray-600">{{ accommodation.address }}</p>
        <p v-if="accommodation.postalCode" class="text-gray-600">우편번호: {{ accommodation.postalCode }}</p>
        <p class="text-gray-800 font-medium">체크인: {{ formatDateTime(accommodation.checkInTime) }}</p>
        <p class="text-gray-800 font-medium">체크아웃: {{ formatDateTime(accommodation.checkOutTime) }}</p>
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
      <div class="flex justify-end gap-3">
        <button type="button" @click="editAccommodation" class="btn-secondary">수정</button>
        <button type="button" @click="closeModal" class="btn-primary">닫기</button>
      </div>
    </template>
  </SlideUpModal>
</template>

<script setup>
import { computed } from 'vue'
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
  }
})

const emit = defineEmits(['update:isOpen', 'edit'])

const closeModal = () => {
  emit('update:isOpen', false)
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
</script>

<style scoped>
.btn-primary {
  @apply px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700;
}
.btn-secondary {
  @apply px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100;
}
</style>
