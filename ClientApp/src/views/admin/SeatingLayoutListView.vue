<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader title="좌석 배치도" :show-back="true" :show-menu="false" />
    <div class="max-w-6xl mx-auto p-4">
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-xl font-bold text-gray-800">레이아웃 목록</h2>
        <button
          class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          @click="showCreate = true"
        >
          + 새 레이아웃
        </button>
      </div>

      <div v-if="loading" class="text-center py-12 text-gray-500">로딩 중...</div>
      <div v-else-if="layouts.length === 0" class="bg-white rounded p-12 text-center text-gray-400">
        아직 만든 레이아웃이 없습니다. "새 레이아웃"으로 시작하세요.
      </div>
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        <div
          v-for="l in layouts"
          :key="l.id"
          class="bg-white rounded-lg shadow hover:shadow-md transition cursor-pointer overflow-hidden"
          @click="goEditor(l.id)"
        >
          <div class="h-32 bg-gray-100 flex items-center justify-center">
            <img v-if="l.backgroundImageUrl" :src="l.backgroundImageUrl" class="object-cover w-full h-full" />
            <span v-else class="text-gray-300">미리보기 없음</span>
          </div>
          <div class="p-3">
            <h3 class="font-bold text-gray-900 truncate">{{ l.name }}</h3>
            <p v-if="l.description" class="text-xs text-gray-500 truncate">{{ l.description }}</p>
            <p class="text-xs text-gray-400 mt-1">
              테이블 {{ l.tableCount }}개 / 배정 {{ l.assignedCount }}명
            </p>
            <div class="flex gap-2 mt-2 text-xs">
              <button
                class="text-blue-600 hover:underline"
                @click.stop="duplicate(l.id)"
              >
                복제
              </button>
              <button
                class="text-red-500 hover:underline"
                @click.stop="remove(l.id)"
              >
                삭제
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 생성 모달 -->
    <BaseModal v-if="showCreate" :is-open="showCreate" max-width="md" @close="showCreate = false">
      <template #header><h3 class="text-lg font-bold">새 레이아웃</h3></template>
      <template #body>
        <div class="space-y-3">
          <div>
            <label class="text-xs text-gray-500">이름</label>
            <input v-model="newName" type="text" class="w-full border rounded px-3 py-2" placeholder="예: 환영만찬" />
          </div>
          <div>
            <label class="text-xs text-gray-500">설명 (선택)</label>
            <input v-model="newDesc" type="text" class="w-full border rounded px-3 py-2" />
          </div>
        </div>
      </template>
      <template #footer>
        <button class="px-4 py-2 border rounded" @click="showCreate = false">취소</button>
        <button class="px-4 py-2 bg-blue-600 text-white rounded" @click="create">생성</button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import apiClient from '@/services/api'
import MainHeader from '@/components/common/MainHeader.vue'
import BaseModal from '@/components/common/BaseModal.vue'

const route = useRoute()
const router = useRouter()
const conventionId = Number(route.params.id || route.params.conventionId)

const layouts = ref([])
const loading = ref(true)
const showCreate = ref(false)
const newName = ref('')
const newDesc = ref('')

async function load() {
  loading.value = true
  try {
    const res = await apiClient.get(`/admin/conventions/${conventionId}/seating-layouts`)
    layouts.value = res.data
  } finally {
    loading.value = false
  }
}

async function create() {
  if (!newName.value.trim()) return alert('이름을 입력해주세요.')
  const res = await apiClient.post(`/admin/conventions/${conventionId}/seating-layouts`, {
    name: newName.value,
    description: newDesc.value || null,
  })
  showCreate.value = false
  newName.value = ''
  newDesc.value = ''
  goEditor(res.data.id)
}

async function duplicate(id) {
  await apiClient.post(`/admin/seating-layouts/${id}/duplicate`)
  load()
}

async function remove(id) {
  if (!confirm('정말 삭제하시겠습니까?')) return
  await apiClient.delete(`/admin/seating-layouts/${id}`)
  load()
}

function goEditor(layoutId) {
  router.push(`/admin/conventions/${conventionId}/seating/${layoutId}`)
}

onMounted(load)
</script>
