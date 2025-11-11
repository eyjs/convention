<!-- ClientApp/src/components/common/GlobalPopup.vue -->
<template>
  <component
    :is="currentModalComponent"
    :is-open="popupStore.isOpen"
    @close="popupStore.closePopup"
    :z-index-class="popupStore.options.zIndexClass || 'z-[50]'"
  >
    <template #header-title>{{ popupStore.title }}</template>
    <template #body>
      <div v-if="popupStore.component" class="p-4">
        <component :is="popupStore.component" v-bind="popupStore.props" />
      </div>
      <div v-else-if="popupStore.content" class="p-4 text-gray-700">
        {{ popupStore.content }}
      </div>
      <div v-else class="p-4 text-gray-500">
        <!-- 컴포넌트도 없고 content도 없을 때만 표시 -->
        팝업 내용이 없습니다.
      </div>
    </template>
  </component>
</template>

<script setup>
import { computed, defineAsyncComponent } from 'vue'
import { usePopupStore } from '@/stores/popup'

// 모달 컴포넌트들을 비동기적으로 임포트
const BaseModal = defineAsyncComponent(
  () => import('@/components/common/BaseModal.vue'),
)
const SlideUpModal = defineAsyncComponent(
  () => import('@/components/common/SlideUpModal.vue'),
)

const popupStore = usePopupStore()

// 현재 선택된 모달 컴포넌트를 동적으로 결정
const currentModalComponent = computed(() => {
  if (popupStore.modalType === 'BaseModal') {
    return BaseModal
  }
  return SlideUpModal // 기본값
})
</script>
