<template>
  <div
    :class="[
      'flex items-start space-x-3 group',
      message.role === 'user' ? 'flex-row-reverse space-x-reverse' : 'flex-row',
    ]"
  >
    <div
      v-if="message.role === 'assistant'"
      class="flex-shrink-0 w-8 h-8 rounded-full bg-gradient-to-br from-emerald-400 to-cyan-500 flex items-center justify-center shadow-sm"
    >
      <svg
        class="w-5 h-5 text-white"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z"
        />
      </svg>
    </div>

    <div
      v-else
      class="flex-shrink-0 w-8 h-8 rounded-full bg-gradient-to-br from-purple-500 to-pink-500 flex items-center justify-center shadow-sm"
    >
      <svg class="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 20 20">
        <path
          fill-rule="evenodd"
          d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z"
          clip-rule="evenodd"
        />
      </svg>
    </div>

    <div class="flex-1 max-w-3xl">
      <div
        :class="[
          'rounded-2xl px-5 py-3.5',
          message.role === 'user'
            ? 'bg-gray-100 text-gray-900'
            : message.isError
              ? 'bg-red-50 text-red-700 border border-red-200'
              : 'bg-white text-gray-900',
        ]"
      >
        <div class="prose prose-sm max-w-none">
          <div class="whitespace-pre-wrap break-words leading-7">
            {{ message.content }}
          </div>
        </div>

        <div
          v-if="
            message.role === 'assistant' &&
            message.llmProvider &&
            !message.isError
          "
          class="mt-3 pt-3 border-t border-gray-200 flex items-center text-xs text-gray-500"
        >
          <svg
            class="w-3.5 h-3.5 mr-1.5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M13 10V3L4 14h7v7l9-11h-7z"
            />
          </svg>
          {{ message.llmProvider }}
        </div>

        <div
          v-if="message.sources && message.sources.length > 0"
          class="mt-3 pt-3 border-t border-gray-200"
        >
          <button
            @click="showSources = !showSources"
            class="flex items-center text-xs font-medium text-gray-600 hover:text-gray-900 transition-colors"
          >
            <svg
              class="w-4 h-4 mr-1.5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
            출처 {{ message.sources.length }}개
            <svg
              :class="[
                'w-3.5 h-3.5 ml-1 transition-transform',
                showSources ? 'rotate-180' : '',
              ]"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M19 9l-7 7-7-7"
              />
            </svg>
          </button>

          <div v-if="showSources" class="mt-2.5 space-y-2">
            <div
              v-for="(source, index) in message.sources"
              :key="index"
              class="p-3 bg-gray-50 rounded-xl border border-gray-200"
            >
              <div class="flex items-center justify-between mb-1.5">
                <span class="text-xs font-medium text-gray-700">
                  {{ source.conventionTitle || '행사 정보' }}
                </span>
                <span class="text-xs text-gray-500">
                  {{ Math.round(source.similarity * 100) }}%
                </span>
              </div>
              <div class="text-xs text-gray-600 leading-relaxed">
                {{ source.content }}
              </div>
            </div>
          </div>
        </div>
      </div>

      <div
        class="mt-1.5 px-1 text-xs text-gray-400 flex items-center space-x-3"
        :class="message.role === 'user' ? 'justify-end' : 'justify-start'"
      >
        <span>{{ formatTime(message.timestamp) }}</span>
        <button
          v-if="message.role === 'assistant'"
          class="opacity-0 group-hover:opacity-100 transition-opacity hover:text-gray-600"
          title="복사"
          @click="copyToClipboard"
        >
          <svg
            class="w-3.5 h-3.5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"
            />
          </svg>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import dayjs from 'dayjs'

const props = defineProps({
  message: {
    type: Object,
    required: true,
  },
})

const showSources = ref(false)

function formatTime(timestamp) {
  if (!timestamp) return ''

  const date = dayjs(timestamp)
  const now = dayjs()

  if (date.isSame(now, 'day')) {
    return date.format('A h:mm')
  }

  if (date.isSame(now.subtract(1, 'day'), 'day')) {
    return `어제 ${date.format('A h:mm')}`
  }

  return date.format('M월 D일 A h:mm')
}

function copyToClipboard() {
  navigator.clipboard.writeText(props.message.content)
}
</script>

<style scoped>
.prose {
  color: inherit;
}

.prose p {
  margin: 0;
}
</style>
