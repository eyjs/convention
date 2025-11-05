<template>
  <div
    v-if="questions.length > 0"
    class="px-6 py-6 bg-white border-b border-gray-200"
  >
    <div class="max-w-3xl mx-auto space-y-3">
      <button
        v-for="(question, index) in visibleQuestions"
        :key="index"
        @click="handleQuestionClick(question)"
        class="w-full text-left px-5 py-3.5 bg-white rounded-2xl border border-gray-200 hover:border-gray-300 hover:bg-gray-50 transition-all group"
      >
        <div class="flex items-center justify-between">
          <span class="text-sm text-gray-700 flex-1">
            {{ question }}
          </span>
          <svg
            class="w-4 h-4 text-gray-400 group-hover:text-gray-600 flex-shrink-0 ml-3 transition-transform group-hover:translate-x-0.5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M9 5l7 7-7 7"
            />
          </svg>
        </div>
      </button>
    </div>

    <button
      v-if="questions.length > maxVisible && !showAll"
      @click="showAll = true"
      class="w-full mt-3 py-2 text-sm text-gray-600 hover:text-gray-900 font-medium transition-colors"
    >
      더 보기 ({{ questions.length - maxVisible }}개)
    </button>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  questions: {
    type: Array,
    default: () => [],
  },
  maxVisible: {
    type: Number,
    default: 4,
  },
})

const emit = defineEmits(['select'])

const showAll = ref(false)

const visibleQuestions = computed(() => {
  if (showAll.value || props.questions.length <= props.maxVisible) {
    return props.questions
  }
  return props.questions.slice(0, props.maxVisible)
})

function handleQuestionClick(question) {
  emit('select', question)
}
</script>
