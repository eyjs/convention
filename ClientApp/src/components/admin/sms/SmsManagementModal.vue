<template>
  <div class="fixed inset-0 z-50 overflow-y-auto" aria-labelledby="modal-title" role="dialog" aria-modal="true">
    <div class="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
      <div class="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" aria-hidden="true" @click="$emit('close')"></div>
      <span class="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>

      <div class="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-2xl sm:w-full">
        
        <!-- Header -->
        <div class="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4 border-b">
          <h3 class="text-lg leading-6 font-medium text-gray-900">
            문자 발송 관리 (단계: {{ step }})
          </h3>
        </div>

        <!-- Body -->
        <div class="p-6 bg-gray-50 min-h-[400px]">
          
          <!-- Step 1: 템플릿 목록 -->
          <div v-if="step === 1">
            <h1 class="text-2xl font-bold mb-4">Step 1: 템플릿 목록</h1>
            <p>이 화면이 보인다면 Step 1은 정상입니다.</p>
            <div class="mt-6">
              <button @click="step = 2" class="px-4 py-2 bg-blue-500 text-white rounded mr-2">Step 2 (에디터)로 이동</button>
              <button @click="step = 3" class="px-4 py-2 bg-green-500 text-white rounded">Step 3 (수신자)로 이동</button>
            </div>
          </div>

          <!-- Step 2: 템플릿 에디터 -->
          <div v-if="step === 2">
            <h1 class="text-2xl font-bold mb-4">Step 2: 템플릿 에디터</h1>
            <p>이 화면이 보인다면 Step 2는 정상입니다.</p>
            <div class="mt-6">
              <button @click="step = 1" class="px-4 py-2 bg-gray-500 text-white rounded">Step 1 (목록)으로 돌아가기</button>
            </div>
          </div>

          <!-- Step 3: 수신자 선택 -->
          <div v-if="step === 3">
            <h1 class="text-2xl font-bold mb-4">Step 3: 수신자 선택</h1>
            <p>이 화면이 보인다면 Step 3은 정상입니다.</p>
            <div class="mt-6">
              <button @click="step = 1" class="px-4 py-2 bg-gray-500 text-white rounded">Step 1 (목록)으로 돌아가기</button>
            </div>
          </div>

        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({ conventionId: Number })
const emit = defineEmits(['close'])

const step = ref(1)

</script>
