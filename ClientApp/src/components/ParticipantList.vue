<template>
  <div
    class="fixed inset-0 bg-black bg-opacity-50 flex items-end sm:items-center justify-center p-0 sm:p-4"
    :class="zIndexClass"
    @click.self="$emit('close')"
  >
    <div
      class="bg-gray-50 rounded-t-2xl sm:rounded-lg shadow-xl w-full max-w-md max-h-[70vh] flex flex-col"
    >
      <!-- Header -->
      <div
        class="p-4 border-b flex justify-between items-center flex-shrink-0 bg-white sm:rounded-t-lg"
      >
        <h3 class="font-bold text-lg">
          {{ title }} ({{ participants.length }}명)
        </h3>
        <button
          @click="$emit('close')"
          class="p-2 -m-2 text-gray-500 hover:text-gray-800 rounded-full hover:bg-gray-100"
        >
          <svg
            class="w-6 h-6"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M6 18L18 6M6 6l12 12"
            ></path>
          </svg>
        </button>
      </div>

      <!-- Search Bar -->
      <div class="p-4 border-b flex-shrink-0 bg-white">
        <input
          v-model="searchQuery"
          type="text"
          :placeholder="searchPlaceholder"
          class="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2"
          :style="{ '--tw-ring-color': brandColor + '50' }"
        />
      </div>

      <!-- Participant List -->
      <div class="overflow-y-auto">
        <ul v-if="filteredParticipants.length > 0">
          <li
            v-for="participant in filteredParticipants"
            :key="participant.id || participant.name"
            class="p-4 flex items-center space-x-4 hover:bg-gray-100 transition-colors border-b border-gray-200"
          >
            <!-- Avatar Placeholder -->
            <div
              class="w-12 h-12 rounded-full text-white flex items-center justify-center flex-shrink-0"
              :style="{ backgroundColor: brandColor }"
            >
              <span class="text-xl font-bold">{{
                participant.name.charAt(0)
              }}</span>
            </div>
            <div class="flex-grow min-w-0">
              <p class="font-semibold text-gray-900 truncate">
                {{ participant.name }}
              </p>
              <p
                v-if="
                  participant.organization ||
                  participant.corpPart ||
                  participant.affiliation
                "
                class="text-sm text-gray-600 truncate"
              >
                {{
                  participant.organization ||
                  participant.corpPart ||
                  participant.affiliation
                }}
              </p>
              <p
                v-if="showPhone && participant.phone"
                class="text-sm text-gray-500 truncate"
              >
                {{ participant.phone }}
              </p>
            </div>
          </li>
        </ul>
        <div v-else class="text-center py-16 text-gray-500">
          <p>검색 결과가 없습니다.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  participants: {
    type: Array,
    required: true,
    default: () => [],
  },
  title: {
    type: String,
    default: '참여자 목록',
  },
  brandColor: {
    type: String,
    default: '#10b981',
  },
  showPhone: {
    type: Boolean,
    default: false,
  },
  searchPlaceholder: {
    type: String,
    default: '이름, 소속으로 검색...',
  },
  zIndexClass: {
    type: String,
    default: 'z-50',
  },
})

const searchQuery = ref('')

const filteredParticipants = computed(() => {
  if (!searchQuery.value) {
    return props.participants
  }
  const query = searchQuery.value.toLowerCase()
  return props.participants.filter((p) => {
    const nameMatch = p.name?.toLowerCase().includes(query)
    const orgMatch = (p.organization || p.corpPart || p.affiliation)
      ?.toLowerCase()
      .includes(query)
    const phoneMatch = props.showPhone && p.phone?.includes(query)

    return nameMatch || orgMatch || phoneMatch
  })
})
</script>
