<template>
  <div v-if="result" class="mt-4">
    <div :class="[
      'p-4 rounded-md',
      result.success ? 'bg-green-50' : 'bg-red-50'
    ]">
      <p class="font-medium" :class="result.success ? 'text-green-800' : 'text-red-800'">
        {{ result.message }}
      </p>

      <!-- 참석자 업로드 결과 -->
      <div v-if="type === 'guests' && result.success && result.data" class="mt-2 text-sm text-green-700 space-y-1">
        <p>✓ 신규 생성: {{ result.data.created }}명</p>
        <p>✓ 정보 업데이트: {{ result.data.updated }}명</p>
        <p>✓ 총 처리: {{ result.data.total }}명</p>
      </div>

      <!-- 일정 업로드 결과 -->
      <div v-if="type === 'schedules' && result.success && result.data" class="mt-2 text-sm text-purple-700 space-y-1">
        <p>✓ 생성된 템플릿: {{ result.data.templates }}개</p>
        <div v-if="result.data.actions && result.data.actions.length > 0" class="mt-2">
          <p class="font-medium">생성된 일정:</p>
          <ul class="ml-4 mt-1 space-y-1">
            <li v-for="(action, idx) in result.data.actions" :key="idx" class="text-xs">
              • {{ action.actionType }}: {{ action.title }}
              <span class="text-purple-600">({{ formatDateTime(action.scheduleDateTime) }})</span>
            </li>
          </ul>
        </div>
      </div>

      <!-- 속성 업로드 결과 -->
      <div v-if="type === 'attributes' && result.success && result.data" class="mt-2 text-sm text-green-700">
        <div class="space-y-1">
          <p>✓ 처리된 참석자: {{ result.data.usersProcessed }}명</p>
          <p>✓ 신규 속성: {{ result.data.attributesCreated }}개</p>
          <p>✓ 업데이트 속성: {{ result.data.attributesUpdated }}개</p>
        </div>

        <!-- 통계 정보 -->
        <div v-if="result.data.statistics && Object.keys(result.data.statistics).length > 0" class="mt-4 p-3 bg-white rounded border border-green-200">
          <p class="font-medium text-green-900 mb-2">📊 속성별 통계</p>
          <div class="space-y-3">
            <div v-for="(values, key) in result.data.statistics" :key="key" class="text-xs">
              <p class="font-medium text-gray-700 mb-1">{{ key }}:</p>
              <div class="ml-3 space-y-1">
                <div v-for="(count, value) in values" :key="value" class="flex justify-between">
                  <span class="text-gray-600">{{ value }}</span>
                  <span class="text-green-600 font-medium">{{ count }}명</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 경고 메시지 -->
      <ul v-if="result.warnings && result.warnings.length" class="mt-3 text-sm text-yellow-700 bg-yellow-50 rounded p-2">
        <li v-for="(warning, idx) in result.warnings" :key="idx" class="flex items-start">
          <span class="mr-2">⚠</span>
          <span>{{ warning }}</span>
        </li>
      </ul>

      <!-- 에러 메시지 -->
      <ul v-if="result.errors && result.errors.length" class="mt-3 text-sm text-red-700 bg-red-50 rounded p-2">
        <li v-for="(error, idx) in result.errors" :key="idx" class="flex items-start">
          <span class="mr-2">✗</span>
          <span>{{ error }}</span>
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  result: {
    type: Object,
    required: true
  },
  type: {
    type: String,
    default: 'guests' // 'guests', 'schedules', 'attributes'
  }
})

const formatDateTime = (dateTimeStr) => {
  if (!dateTimeStr) return ''
  const date = new Date(dateTimeStr)
  return date.toLocaleString('ko-KR', {
    month: 'numeric',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>
