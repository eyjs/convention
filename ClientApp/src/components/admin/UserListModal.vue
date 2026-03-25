<template>
  <BaseModal :is-open="isOpen" @close="emit('close')">
    <template #header>
      <div class="flex items-center justify-between w-full">
        <span>{{ title }}</span>
        <span class="text-sm font-normal text-gray-500"
          >{{ filteredUsers.length }}명</span
        >
      </div>
    </template>
    <template #body>
      <div class="mb-3">
        <input
          v-model="search"
          type="text"
          placeholder="이름, 소속 검색..."
          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 text-sm focus:ring-indigo-500 focus:border-indigo-500"
        />
      </div>

      <div v-if="loading" class="text-center py-8 text-gray-500">
        불러오는 중...
      </div>

      <div
        v-else-if="filteredUsers.length === 0"
        class="text-center py-8 text-gray-400"
      >
        {{ search ? '검색 결과가 없습니다.' : '데이터가 없습니다.' }}
      </div>

      <div v-else class="max-h-[400px] overflow-y-auto">
        <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead class="bg-gray-50 dark:bg-gray-700 sticky top-0">
            <tr>
              <th
                class="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase"
              >
                이름
              </th>
              <th
                class="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase"
              >
                소속
              </th>
              <th
                class="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase"
              >
                연락처
              </th>
              <th
                v-if="showExtra"
                class="px-4 py-2 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase"
              >
                {{ extraLabel }}
              </th>
            </tr>
          </thead>
          <tbody
            class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700"
          >
            <tr
              v-for="user in filteredUsers"
              :key="user.id"
              class="hover:bg-gray-50 dark:hover:bg-gray-700/50"
            >
              <td class="px-4 py-2.5 text-sm text-gray-900 dark:text-gray-200">
                {{ user.name }}
              </td>
              <td class="px-4 py-2.5 text-sm text-gray-500 dark:text-gray-400">
                {{ user.corpName || '-' }}
              </td>
              <td class="px-4 py-2.5 text-sm text-gray-500 dark:text-gray-400">
                {{ user.phone || '-' }}
              </td>
              <td
                v-if="showExtra"
                class="px-4 py-2.5 text-sm text-gray-500 dark:text-gray-400"
              >
                {{ user[extraField] || '-' }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="flex justify-end mt-4">
        <button
          class="px-4 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300 text-sm font-medium"
          @click="emit('close')"
        >
          닫기
        </button>
      </div>
    </template>
  </BaseModal>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import BaseModal from '@/components/common/BaseModal.vue'

const props = defineProps({
  isOpen: { type: Boolean, required: true },
  title: { type: String, default: '명단' },
  users: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false },
  extraLabel: { type: String, default: '' },
  extraField: { type: String, default: '' },
})

const emit = defineEmits(['close'])

const search = ref('')

const showExtra = computed(() => !!props.extraLabel)

const filteredUsers = computed(() => {
  if (!search.value) return props.users
  const q = search.value.toLowerCase()
  return props.users.filter(
    (u) =>
      u.name?.toLowerCase().includes(q) ||
      u.corpName?.toLowerCase().includes(q) ||
      u.phone?.includes(q),
  )
})

watch(
  () => props.isOpen,
  (open) => {
    if (open) search.value = ''
  },
)
</script>
