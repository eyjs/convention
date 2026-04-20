<template>
  <SlideUpModal :is-open="!!props.schedule" @close="emit('close')">
    <template #header-title>
      <div v-if="props.schedule">
        <div class="text-xs font-medium text-[#0F6E56] mb-0.5">
          {{ props.schedule.startTime }}
          <template v-if="props.schedule.endTime && props.schedule.endTime !== props.schedule.startTime"> – {{ props.schedule.endTime }}</template>
        </div>
        <div class="text-base font-medium text-[#1a1a1a]">{{ props.schedule.title }}</div>
      </div>
    </template>

    <template #body>
      <div v-if="props.schedule" class="space-y-0">
        <!-- 장소 -->
        <div v-if="props.schedule.location" class="flex gap-2 py-2 border-b border-black/[0.05]">
          <span class="text-[11px] text-gray-400 w-7 flex-shrink-0 pt-0.5">장소</span>
          <div class="text-[13px] text-[#1a1a1a]">
            <a
              v-if="props.schedule.mapUrl"
              :href="props.schedule.mapUrl"
              target="_blank"
              rel="noopener noreferrer"
              class="text-[#1D9E75] no-underline"
            >{{ props.schedule.location }} →</a>
            <span v-else>{{ props.schedule.location }}</span>
          </div>
        </div>

        <!-- 안내 (설명) -->
        <div v-if="props.schedule.description" class="flex gap-2 py-2 border-b border-black/[0.05]">
          <span class="text-[11px] text-gray-400 w-7 flex-shrink-0 pt-0.5">안내</span>
          <div class="text-[13px] text-[#1a1a1a] leading-[1.6]">
            <QuillViewer :content="props.schedule.description" @image-clicked="openFullImage" />
          </div>
        </div>

        <!-- 이미지 갤러리 -->
        <div v-if="props.schedule.images?.length" class="py-3">
          <div :class="['grid gap-2', props.schedule.images.length === 1 ? 'grid-cols-1' : 'grid-cols-2']">
            <img
              v-for="img in props.schedule.images"
              :key="img.id"
              :src="img.imageUrl"
              class="w-full rounded-lg object-cover cursor-pointer hover:opacity-90 transition-opacity"
              :class="props.schedule.images.length === 1 ? 'max-h-64' : 'h-32'"
              @click="openFullImage(img.imageUrl)"
            />
          </div>
        </div>

        <!-- 내 자리 보기 버튼 -->
        <button
          v-if="props.schedule.seatingLayoutId"
          class="w-full py-3 mt-2 bg-gradient-to-r from-amber-500 to-orange-500 text-white rounded-xl font-semibold text-sm shadow hover:shadow-md active:scale-[0.98] transition-all flex items-center justify-center gap-2"
          @click="emit('go-to-seat', props.schedule.seatingLayoutId)"
        >
          내 자리 보기
        </button>

        <!-- 내 배정 정보 (목업: .m-my 보라 카드) -->
        <div v-if="scheduleBadges.length > 0" class="mt-3 bg-[#f3f1fb] rounded-[10px] p-3">
          <div class="text-[10px] font-medium text-[#534AB7] mb-2 tracking-wide">내 배정 정보</div>
          <div class="grid grid-cols-2 gap-1.5">
            <div
              v-for="(badge, bi) in scheduleBadges"
              :key="badge.key"
              class="bg-white rounded-lg px-2.5 py-2 border border-[rgba(83,74,183,0.13)]"
              :class="bi === scheduleBadges.length - 1 && scheduleBadges.length % 2 !== 0 ? 'col-span-2' : ''"
            >
              <div class="text-[10px] text-[#7F77DD] mb-0.5">{{ badge.key }}</div>
              <div class="text-sm font-medium text-[#3C3489]">{{ badge.value }}</div>
            </div>
          </div>
        </div>

        <!-- 참석자 보기 (관리자만) -->
        <div v-if="props.isAdmin && props.schedule.group && !props.schedule.isOptionTour" class="mt-3">
          <button
            class="w-full px-4 py-2.5 rounded-lg text-white font-medium transition-all hover:opacity-90 flex items-center justify-center space-x-2"
            :style="{ backgroundColor: props.brandColor }"
            @click="loadParticipants(props.schedule.scheduleTemplateId, props.schedule.group)"
          >
            <span>참석자 보기 ({{ props.schedule.participants }}명)</span>
          </button>
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
import { ref, computed } from 'vue'
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

// 해당 일정의 visibleAttributes로 배정 정보 필터링
const scheduleBadges = computed(() => {
  if (!props.schedule?.visibleAttributes || !props.attributes?.length) return []
  const keys = props.schedule.visibleAttributes.split(',').map((k) => k.trim()).filter(Boolean)
  return props.attributes.filter((a) => keys.includes(a.key))
})

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
