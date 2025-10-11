<template>
  <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
    <div class="bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
      <!-- 헤더 -->
      <div class="sticky top-0 bg-white border-b px-6 py-4">
        <div class="flex justify-between items-center">
          <h2 class="text-xl font-bold">{{ convention ? '행사 수정' : '새 행사 만들기' }}</h2>
          <button
            @click="$emit('close')"
            class="p-2 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </div>

      <!-- 폼 -->
      <form @submit.prevent="handleSubmit" class="p-6 space-y-6">
        <!-- 행사명 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">
            행사명 <span class="text-red-500">*</span>
          </label>
          <input
            v-model="form.title"
            type="text"
            required
            class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            placeholder="예: iFA STAR TOUR @ ROMA"
          />
        </div>

        <!-- 행사 유형 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">
            행사 유형 <span class="text-red-500">*</span>
          </label>
          <div class="grid grid-cols-2 gap-4">
            <label class="relative flex items-center p-4 border-2 rounded-lg cursor-pointer hover:border-primary-500 transition-colors"
              :class="{ 'border-primary-500 bg-primary-50': form.conventionType === 'DOMESTIC' }"
            >
              <input
                v-model="form.conventionType"
                type="radio"
                value="DOMESTIC"
                class="sr-only"
              />
              <div class="flex-1">
                <div class="font-medium">국내 행사</div>
                <div class="text-sm text-gray-500">국내에서 진행되는 행사</div>
              </div>
            </label>
            <label class="relative flex items-center p-4 border-2 rounded-lg cursor-pointer hover:border-primary-500 transition-colors"
              :class="{ 'border-primary-500 bg-primary-50': form.conventionType === 'OVERSEAS' }"
            >
              <input
                v-model="form.conventionType"
                type="radio"
                value="OVERSEAS"
                class="sr-only"
              />
              <div class="flex-1">
                <div class="font-medium">해외 행사</div>
                <div class="text-sm text-gray-500">해외에서 진행되는 행사</div>
              </div>
            </label>
          </div>
        </div>

        <!-- 기간 -->
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              시작일 <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.startDate"
              type="date"
              required
              class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              종료일 <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.endDate"
              type="date"
              required
              class="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            />
          </div>
        </div>

        <!-- 브랜드 컬러 -->
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-2">
            브랜드 컬러
          </label>
          <div class="flex items-center space-x-4">
            <input
              v-model="form.brandColor"
              type="color"
              class="w-16 h-10 rounded cursor-pointer"
            />
            <input
              v-model="form.brandColor"
              type="text"
              class="flex-1 px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
              placeholder="#6366f1"
            />
          </div>
          <p class="text-sm text-gray-500 mt-1">행사의 메인 컬러를 설정합니다</p>
        </div>

        <!-- 버튼 -->
        <div class="flex justify-end space-x-3 pt-4 border-t">
          <button
            type="button"
            @click="$emit('close')"
            class="px-4 py-2 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
          >
            취소
          </button>
          <button
            type="submit"
            :disabled="saving"
            class="px-4 py-2 bg-primary-600 hover:bg-primary-700 disabled:bg-gray-400 text-white rounded-lg transition-colors flex items-center space-x-2"
          >
            <svg v-if="saving" class="animate-spin w-4 h-4" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            <span>{{ saving ? '저장 중...' : (convention ? '수정' : '생성') }}</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  convention: Object
})

const emit = defineEmits(['close', 'save'])

const saving = ref(false)
const form = ref({
  title: '',
  conventionType: 'DOMESTIC',
  startDate: '',
  endDate: '',
  brandColor: '#6366f1',
  renderType: 'STANDARD',
  themePreset: 'default'
})

// 수정 모드일 때 기존 데이터 로드
watch(() => props.convention, (newVal) => {
  if (newVal) {
    form.value = {
      title: newVal.title,
      conventionType: newVal.conventionType,
      startDate: newVal.startDate ? newVal.startDate.split('T')[0] : '',
      endDate: newVal.endDate ? newVal.endDate.split('T')[0] : '',
      brandColor: newVal.brandColor || '#6366f1',
      renderType: newVal.renderType || 'STANDARD',
      themePreset: newVal.themePreset || 'default'
    }
  }
}, { immediate: true })

const handleSubmit = async () => {
  saving.value = true
  try {
    await emit('save', form.value)
  } finally {
    saving.value = false
  }
}
</script>
