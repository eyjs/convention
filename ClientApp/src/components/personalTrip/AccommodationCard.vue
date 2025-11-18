<template>
  <div class="bg-white rounded-2xl shadow-md p-5">
    <!-- Accommodation Info Card -->
    <div class="flex items-center gap-4 mb-4">
      <div class="flex-shrink-0 w-16 h-16 bg-gray-200 rounded-lg overflow-hidden flex items-center justify-center">
        <!-- Hotel Icon -->
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-8 h-8 text-gray-500">
          <path stroke-linecap="round" stroke-linejoin="round" d="M2.25 21h19.5m-19.5 0A2.25 2.25 0 0 0 5.25 18H19.5a2.25 2.25 0 0 0 2.25 2.25m-19.5 0A2.25 2.25 0 0 1 2.25 18V9.568m19.5 0a2.25 2.25 0 0 1 2.25 2.25V21M12 10.5v4.5m0-4.5H8.25m3.75 0h3.75M9 18h6" />
        </svg>
      </div>
      <div class="flex-1 min-w-0">
        <p class="font-bold text-lg text-gray-900 truncate">{{ accommodation.name }}</p>
        <p v-if="showTimeRemaining" class="text-xs mt-1" :class="timeRemainingColor">{{ timeRemaining }}</p>
      </div>
    </div>

    <!-- Check-in/Check-out -->
    <div class="grid grid-cols-2 gap-4 mb-4 border-t pt-4">
      <div>
        <p class="text-xs text-gray-500">체크인</p>
        <p class="font-semibold text-gray-800 mt-1">{{ formatDateTime(accommodation.checkInTime) }}</p>
      </div>
      <div>
        <p class="text-xs text-gray-500">체크아웃</p>
        <p class="font-semibold text-gray-800 mt-1">{{ formatDateTime(accommodation.checkOutTime) }}</p>
      </div>
    </div>

    <!-- Action Buttons -->
    <div v-if="showActions" class="flex gap-2">
      <button @click="$emit('select', accommodation)" class="flex-1 py-2.5 bg-gray-100 text-gray-700 rounded-lg font-semibold hover:bg-gray-200 transition-colors">
        숙소 상세
      </button>
      <button @click="copyAddressToClipboard(accommodation.address)" class="flex-1 py-2.5 bg-gray-100 text-gray-700 rounded-lg font-semibold hover:bg-gray-200 transition-colors">
        주소 복사
      </button>
      <button @click="$emit('delete', accommodation.id)" class="p-2.5 bg-red-100 text-red-600 rounded-lg font-semibold hover:bg-red-200 transition-colors">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import dayjs from 'dayjs';
import 'dayjs/locale/ko';

dayjs.locale('ko');

const props = defineProps({
  accommodation: {
    type: Object,
    required: true,
  },
  showActions: {
    type: Boolean,
    default: false,
  },
  showTimeRemaining: {
    type: Boolean,
    default: false,
  }
});

defineEmits(['select', 'delete']);

const timeRemaining = ref('');
const timeRemainingColor = ref('text-blue-600');
let intervalId = null;

const update_time_remaining = () => {
  if (!props.accommodation.checkInTime) {
    timeRemaining.value = '';
    return;
  }

  const now = dayjs();
  const checkIn = dayjs(props.accommodation.checkInTime);
  const checkOut = dayjs(props.accommodation.checkOutTime);

  if (now.isAfter(checkOut)) {
    timeRemaining.value = '숙박 완료';
    timeRemainingColor.value = 'text-gray-500';
    if (intervalId) clearInterval(intervalId);
    return;
  }
  
  if (now.isAfter(checkIn)) {
    timeRemaining.value = '숙박 중';
    timeRemainingColor.value = 'text-green-600';
    // The interval continues to check for checkout time
    return;
  }

  const diff = checkIn.diff(now);
  const duration = dayjs.duration(diff);

  const days = Math.floor(duration.asDays());
  const hours = duration.hours();
  const minutes = duration.minutes();

  let result = '';
  if (days > 0) result += `${days}일 `;
  if (hours > 0) result += `${hours}시간 `;
  if (minutes > 0) result += `${minutes}분 `;
  
  timeRemaining.value = result.trim() + ' 뒤 입실 가능';
  timeRemainingColor.value = 'text-blue-600';
};


onMounted(() => {
  if (props.showTimeRemaining) {
    update_time_remaining();
    intervalId = setInterval(update_time_remaining, 60000); // 1분마다 업데이트
  }
});

onUnmounted(() => {
  if (intervalId) {
    clearInterval(intervalId);
  }
});


const formatDateTime = (dateTime) => {
  if (!dateTime) return '';
  return dayjs(dateTime).format('MM.DD (ddd) HH:mm');
};

const copyAddressToClipboard = (address) => {
  if (!address) return;
  if (!navigator.clipboard) {
    fallbackCopyTextToClipboard(address);
    return;
  }
  navigator.clipboard.writeText(address).then(() => {
    alert('주소가 클립보드에 복사되었습니다.');
  }).catch(err => {
    console.error('navigator.clipboard를 사용한 복사 실패: ', err);
    fallbackCopyTextToClipboard(address);
  });
};

const fallbackCopyTextToClipboard = (text) => {
  const textArea = document.createElement("textarea");
  textArea.value = text;
  textArea.style.top = "0";
  textArea.style.left = "0";
  textArea.style.position = "fixed";
  document.body.appendChild(textArea);
  textArea.focus();
  textArea.select();
  try {
    const successful = document.execCommand('copy');
    if (successful) {
      alert('주소가 클립보드에 복사되었습니다.');
    } else {
      alert('주소 복사에 실패했습니다.');
    }
  } catch (err) {
    console.error('document.execCommand를 사용한 복사 실패: ', err);
    alert('주소 복사에 실패했습니다.');
  }
  document.body.removeChild(textArea);
};
</script>
