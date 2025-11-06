<template>
  <div>
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
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  participants: {
    type: Array,
    required: true,
    default: () => [],
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
