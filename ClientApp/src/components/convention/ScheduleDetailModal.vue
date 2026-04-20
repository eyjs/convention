<template>
  <SlideUpModal :is-open="!!props.schedule" @close="emit('close')">
    <template #header-title>
      <span v-if="props.schedule?.isOptionTour" class="flex items-center space-x-2">
        <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
          <path d="M10.894 2.553a1 1 0 00-1.788 0l-7 14a1 1 0 001.169 1.409l5-1.429A1 1 0 009 15.571V11a1 1 0 112 0v4.571a1 1 0 00.725.962l5 1.428a1 1 0 001.17-1.408l-7-14z" />
        </svg>
        <span>옵션투어 상세</span>
      </span>
      <span v-else>일정 상세</span>
    </template>

    <template #body>
      <div v-if="props.schedule" class="space-y-4">
        <!-- 상단 메타: 날짜 · 시간 · 장소 -->
        <div class="flex flex-wrap items-center gap-x-2 gap-y-1 text-xs text-gray-500">
          <span>{{ formatDate(props.schedule.date) }}</span>
          <span class="text-gray-300">·</span>
          <span class="font-medium" :style="{ color: props.brandColor }">
            {{ props.schedule.startTime }}
            <template v-if="props.schedule.endTime"> — {{ props.schedule.endTime }}</template>
          </span>
          <template v-if="props.schedule.location">
            <span class="text-gray-300">·</span>
            <span class="flex items-center gap-0.5">
              <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
              {{ props.schedule.location }}
            </span>
          </template>
          <span
            v-if="props.schedule.isOptionTour"
            class="px-1.5 py-0.5 rounded-md text-xs font-medium text-white"
            :style="{ backgroundColor: props.brandColor }"
          >
            옵션투어
          </span>
        </div>

        <!-- 타이틀 -->
        <h3 class="text-xl font-bold text-gray-900">{{ props.schedule.title }}</h3>

        <!-- 메인 컨텐츠 영역 -->
        <div
          v-if="props.schedule.description || props.schedule.images?.length"
          class="bg-gray-50 rounded-xl p-4 -mx-1"
        >
          <div v-if="props.schedule.description">
            <QuillViewer :content="props.schedule.description" @image-clicked="openFullImage" />
          </div>

          <!-- 이미지 갤러리 -->
          <div
            v-if="props.schedule.images?.length"
            :class="props.schedule.description ? 'mt-4 pt-4 border-t border-gray-200' : ''"
          >
            <div
              :class="[
                'grid gap-2',
                props.schedule.images.length === 1 ? 'grid-cols-1' : 'grid-cols-2',
              ]"
            >
              <img
                v-for="img in props.schedule.images"
                :key="img.id"
                :src="img.imageUrl"
                class="w-full rounded-lg object-cover cursor-pointer hover:opacity-90 transition-opacity"
                :class="props.schedule.images.length === 1 ? 'max-h-64' : 'h-40'"
                @click="openFullImage(img.imageUrl)"
              />
            </div>
          </div>
        </div>

        <!-- 내 자리 보기 버튼 -->
        <button
          v-if="props.schedule.seatingLayoutId"
          class="w-full py-3 bg-gradient-to-r from-amber-500 to-orange-500 text-white rounded-xl font-semibold text-sm shadow hover:shadow-md active:scale-[0.98] transition-all flex items-center justify-center gap-2"
          @click="emit('go-to-seat', props.schedule.seatingLayoutId)"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7" />
          </svg>
          내 자리 보기
        </button>

        <!-- 내 배정 정보 (attributes prop 기반) -->
        <div v-if="props.attributes?.length" class="pt-4 border-t mt-4">
          <div class="p-4 bg-blue-50 rounded-xl">
            <h4 class="text-sm font-semibold text-gray-700 mb-2">내 배정 정보</h4>
            <div class="flex flex-wrap gap-2">
              <span
                v-for="attr in props.attributes"
                :key="attr.key"
                class="px-2 py-1 bg-white rounded-lg text-xs text-gray-700 border border-blue-100 shadow-sm"
              >
                <span class="font-medium text-gray-500">{{ attr.key }}:</span>
                {{ attr.value }}
              </span>
            </div>
          </div>
        </div>

        <!-- 참여 그룹 & 참석자 보기 (관리자만, 옵션투어 제외) -->
        <div
          v-if="props.isAdmin && props.schedule.group && !props.schedule.isOptionTour"
          class="pt-4 border-t mt-6"
        >
          <div class="p-4 bg-gray-50 rounded-xl">
            <div class="flex items-center space-x-2 mb-3">
              <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
              <span class="text-sm font-medium text-gray-700">{{ props.schedule.group }}</span>
              <span class="text-sm text-gray-500">({{ props.schedule.participants }}명)</span>
            </div>
            <button
              class="w-full px-4 py-2.5 rounded-lg text-white font-medium transition-all hover:opacity-90 flex items-center justify-center space-x-2"
              :style="{ backgroundColor: props.brandColor }"
              @click="loadParticipants(props.schedule.scheduleTemplateId, props.schedule.group)"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
              <span>참석자 보기</span>
            </button>
          </div>
        </div>
      </div>
    </template>
  </SlideUpModal>

  <!-- 참석자 목록 모달 (중첩) -->
  <SlideUpModal :is-open="showParticipantsModal" z-index-class="z-[60]" @close="closeParticipantsModal">
    <template #header-title>{{ selectedGroupName }}</template>
    <template #body>
      <ParticipantList
        :participants="participants"
        :show-phone="props.isAdmin"
        :search-placeholder="props.isAdmin ? '이름, 소속, 연락처로 검색...' : '이름, 소속으로 검색...'"
      />
    </template>
  </SlideUpModal>

  <!-- 전체 이미지 보기 -->
  <div
    v-if="fullImageUrl"
    class="fixed inset-0 z-[100] bg-black/90 flex items-center justify-center"
    @click="fullImageUrl = null"
  >
    <img :src="fullImageUrl" class="max-w-full max-h-full object-contain" @click.stop />
    <button
      class="absolute top-4 right-4 w-10 h-10 bg-white/20 text-white rounded-full flex items-center justify-center text-xl hover:bg-white/30"
      @click="fullImageUrl = null"
    >
      &times;
    </button>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import apiClient from '@/services/api'
import SlideUpModal from '@/components/common/SlideUpModal.vue'
import QuillViewer from '@/components/common/QuillViewer.vue'
import ParticipantList from '@/components/ParticipantList.vue'

const props = defineProps({
  schedule: { type: Object, default: null },
  brandColor: { type: String, default: '#10b981' },
  isAdmin: { type: Boolean, default: false },
  attributes: { type: Array, default: () => [] },
})

const emit = defineEmits(['close', 'go-to-seat'])

// 내부 상태
const fullImageUrl = ref(null)
const showParticipantsModal = ref(false)
const participants = ref([])
const selectedGroupName = ref('')

function formatDate(dateStr) {
  if (!dateStr) return ''
  const [year, month, day] = dateStr.split('-').map(Number)
  const date = new Date(year, month - 1, day)
  const days = ['일요일', '월요일', '화요일', '수요일', '목요일', '금요일', '토요일']
  return `${year}년 ${month}월 ${day}일 ${days[date.getDay()]}`
}

function openFullImage(url) {
  fullImageUrl.value = url
}

async function loadParticipants(scheduleTemplateId, groupName) {
  selectedGroupName.value = groupName || '참여 그룹'
  showParticipantsModal.value = true
  try {
    const response = await apiClient.get(`/user-schedules/participants/${scheduleTemplateId}`)
    participants.value = response.data.participants || []
  } catch (error) {
    console.error('Failed to load participants:', error)
    participants.value = []
  }
}

function closeParticipantsModal() {
  showParticipantsModal.value = false
  participants.value = []
  selectedGroupName.value = ''
}
</script>
