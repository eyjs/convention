<template>
  <SlideUpModal :is-open="isOpen" @close="close">
    <template #header-title>여행 공유</template>
    <template #body>
      <div class="space-y-6">
        <div class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
          <span class="font-medium text-gray-800">공유 링크 활성화</span>
          <label class="relative inline-flex items-center cursor-pointer">
            <input type="checkbox" :checked="isShared" @change="onToggle" class="sr-only peer">
            <div class="w-11 h-6 bg-gray-200 rounded-full peer peer-focus:ring-4 peer-focus:ring-blue-300 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-0.5 after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-600"></div>
          </label>
        </div>

        <div v-if="isShared" class="space-y-2 animate-fade-in">
          <label class="text-sm font-medium text-gray-700">공유 URL</label>
          <div class="flex gap-2">
            <input
              type="text"
              :value="shareUrl"
              readonly
              class="input flex-1 bg-gray-100 cursor-pointer"
              @click="copyToClipboard"
            />
            <button @click="copyToClipboard" class="px-4 py-2 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors">
              복사
            </button>
          </div>
          <p class="text-xs text-gray-500">이 링크를 가진 사람은 누구나 여행 일정을 볼 수 있습니다.</p>
        </div>
        
        <div v-else class="text-center py-4 bg-gray-50 rounded-lg">
            <p class="text-gray-600">공유가 비활성화되어 있습니다.</p>
        </div>
      </div>
    </template>
  </SlideUpModal>
</template>

<script setup>
import { ref } from 'vue';
import SlideUpModal from '@/components/common/SlideUpModal.vue';

const props = defineProps({
  isOpen: Boolean,
  isShared: Boolean,
  shareUrl: String,
});

const emit = defineEmits(['close', 'update:isShared']);

const close = () => emit('close');

const onToggle = (event) => {
  emit('update:isShared', event.target.checked);
};

const copyToClipboard = () => {
  if (!props.shareUrl) return;

  // navigator.clipboard가 없거나 비-보안 컨텍스트일 경우를 위한 폴백
  if (!navigator.clipboard) {
    fallbackCopyTextToClipboard(props.shareUrl);
    return;
  }

  navigator.clipboard.writeText(props.shareUrl).then(() => {
    alert('공유 링크가 클립보드에 복사되었습니다.');
  }).catch(err => {
    console.error('navigator.clipboard를 사용한 복사 실패: ', err);
    // 실패 시 폴백 방법 시도
    fallbackCopyTextToClipboard(props.shareUrl);
  });
};

const fallbackCopyTextToClipboard = (text) => {
  const textArea = document.createElement("textarea");
  textArea.value = text;
  
  // 화면에 보이지 않도록 스타일 설정
  textArea.style.top = "0";
  textArea.style.left = "0";
  textArea.style.position = "fixed";

  document.body.appendChild(textArea);
  textArea.focus();
  textArea.select();

  try {
    const successful = document.execCommand('copy');
    if (successful) {
      alert('공유 링크가 클립보드에 복사되었습니다.');
    } else {
      alert('링크 복사에 실패했습니다.');
    }
  } catch (err) {
    console.error('document.execCommand를 사용한 복사 실패: ', err);
    alert('링크 복사에 실패했습니다.');
  }

  document.body.removeChild(textArea);
};
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.3s ease-out;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
