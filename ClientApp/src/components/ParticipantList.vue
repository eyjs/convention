<template>
  <div class="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center p-4" @click.self="$emit('close')">
    <div class="bg-white rounded-lg shadow-xl w-full max-w-md max-h-[80vh] flex flex-col">
      <div class="p-4 border-b flex justify-between items-center">
        <h3 class="font-bold text-lg">참여자 목록</h3>
        <button @click="$emit('close')" class="p-2 -m-2">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
        </button>
      </div>
      <div class="p-4 overflow-y-auto">
        <ul class="divide-y divide-gray-200">
          <li v-for="(participant, index) in participants" :key="index" class="py-3 flex items-center">
            <div class="flex-grow">
              <p class="font-semibold">{{ participant.name }}</p>
              <p class="text-sm text-gray-600">{{ participant.affiliation }}</p>
            </div>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  participants: {
    type: Array,
    required: true,
    default: () => [],
  },
});
</script>
