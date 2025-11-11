<!-- ClientApp/src/components/custom/MyInfoComponent.vue -->
<template>
  <div class="space-y-4 p-4">
    <h3 class="text-xl font-bold text-gray-900">내 정보 상세</h3>
    <p class="text-gray-700">
      이 팝업은 백엔드 설정에 따라 동적으로 로드된 컴포넌트입니다.
    </p>
    <div v-if="id">
      <p><strong>전달받은 ID (TargetId):</strong> {{ id }}</p>
      <p class="text-sm text-gray-500">
        (이 ID를 사용하여 API 호출 등으로 실제 데이터를 가져올 수 있습니다.)
      </p>
    </div>
    <div v-else>
      <p class="text-red-500">전달받은 ID (TargetId)가 없습니다.</p>
    </div>

    <div class="mt-4 p-3 bg-blue-50 rounded-lg">
      <p class="font-semibold text-blue-800">현재 로그인 사용자 정보 (AuthStore에서 가져옴):</p>
      <p v-if="authStore.user">
        <strong>사용자 ID:</strong> {{ authStore.user.id }}<br/>
        <strong>사용자 이름:</strong> {{ authStore.user.name }}
      </p>
      <p v-else class="text-red-500">로그인 사용자 정보를 가져올 수 없습니다.</p>
    </div>

    <button
      @click="closePopup"
      class="mt-4 w-full px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
    >
      닫기
    </button>
  </div>
</template>

<script setup>
import { defineProps } from 'vue';
import { usePopupStore } from '@/stores/popup';
import { useAuthStore } from '@/stores/auth'; // AuthStore 임포트

const props = defineProps({
  id: { // targetId가 이 props로 전달됩니다.
    type: Number,
    default: null,
  },
});

const popupStore = usePopupStore();
const authStore = useAuthStore(); // AuthStore 인스턴스 생성

const closePopup = () => {
  popupStore.closePopup();
};
</script>
