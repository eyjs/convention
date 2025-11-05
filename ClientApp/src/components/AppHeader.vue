<template>
  <header class="bg-white shadow-sm safe-area-top sticky top-0 z-50">
    <div class="flex items-center justify-between px-4 py-3">
      <!-- iFA 로고 -->
      <div class="flex items-center">
        <h1 class="text-xl font-bold text-gray-800">iFA</h1>
      </div>

      <!-- ADMIN 버튼 -->
      <button
        class="bg-gray-700 text-white px-4 py-2 rounded-full text-sm font-medium touch-feedback"
        @click="goToAdmin"
      >
        ADMIN
      </button>
    </div>
  </header>
</template>

<script setup>
const goToAdmin = () => {
  // 관리자 페이지로 이동 또는 모달 표시
  window.open('/admin', '_blank')
}
</script>

<style scoped>
/* 추가 스타일이 필요한 경우 */
</style>
