<template>
  <div class="p-4 sm:p-6 lg:p-8">
    <div class="sm:flex sm:items-center">
      <div class="sm:flex-auto">
        <h1 class="text-xl font-semibold text-gray-900">폼 빌더</h1>
        <p class="mt-2 text-sm text-gray-700">
          사용자로부터 데이터를 수집하기 위한 동적 폼을 생성하고 관리합니다.
        </p>
      </div>
      <div class="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
        <button
          @click="createNewForm"
          type="button"
          class="inline-flex items-center justify-center rounded-md border border-transparent bg-primary-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:ring-offset-2 sm:w-auto"
        >
          새 폼 만들기
        </button>
      </div>
    </div>
    <div class="mt-8 flex flex-col">
      <div class="-my-2 -mx-4 overflow-x-auto sm:-mx-6 lg:-mx-8">
        <div class="inline-block min-w-full py-2 align-middle md:px-6 lg:px-8">
          <div v-if="loading" class="flex justify-center items-center py-10">
            <div
              class="w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"
            ></div>
          </div>
          <div
            v-else-if="forms.length === 0"
            class="text-center py-10 border-2 border-dashed border-gray-300 rounded-lg"
          >
            <p class="text-gray-500">생성된 폼이 없습니다.</p>
            <p class="mt-2 text-sm text-gray-500">
              '새 폼 만들기'를 클릭하여 첫 폼을 생성해보세요.
            </p>
          </div>
          <div
            v-else
            class="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg"
          >
            <table class="min-w-full divide-y divide-gray-300">
              <thead class="bg-gray-50">
                <tr>
                  <th
                    scope="col"
                    class="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6"
                  >
                    ID
                  </th>
                  <th
                    scope="col"
                    class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900"
                  >
                    이름
                  </th>
                  <th
                    scope="col"
                    class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900"
                  >
                    설명
                  </th>
                  <th
                    scope="col"
                    class="px-3 py-3.5 text-left text-sm font-semibold text-gray-900"
                  >
                    수정일
                  </th>
                  <th scope="col" class="relative py-3.5 pl-3 pr-4 sm:pr-6">
                    <span class="sr-only">Edit</span>
                  </th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-200 bg-white">
                <tr v-for="form in forms" :key="form.id">
                  <td
                    class="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6"
                  >
                    {{ form.id }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    {{ form.name }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    {{ form.description }}
                  </td>
                  <td class="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                    {{ formatDateTime(form.updatedAt) }}
                  </td>
                  <td
                    class="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6"
                  >
                    <button
                      @click="editForm(form.id)"
                      class="text-primary-600 hover:text-primary-900"
                    >
                      편집
                    </button>
                    <button
                      @click="confirmDelete(form.id)"
                      class="ml-4 text-red-600 hover:text-red-900"
                    >
                      삭제
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import formBuilderService from '@/services/formBuilderService'

const router = useRouter()
const route = useRoute()
const forms = ref([])
const loading = ref(false)
const conventionId = ref(null)

async function fetchForms() {
  if (!conventionId.value) {
    alert('컨벤션 ID를 찾을 수 없습니다.')
    return
  }
  loading.value = true
  try {
    const response = await formBuilderService.getFormDefinitions(
      conventionId.value,
    )
    forms.value = response.data
  } catch (error) {
    console.error('Failed to fetch forms:', error)
    alert('폼 목록을 불러오는 데 실패했습니다.')
  } finally {
    loading.value = false
  }
}

function createNewForm() {
  router.push({
    name: 'FormBuilderEdit',
    params: { conventionId: conventionId.value, id: 'new' },
  })
}

function editForm(id) {
  router.push({
    name: 'FormBuilderEdit',
    params: { conventionId: conventionId.value, id },
  })
}

async function confirmDelete(id) {
  if (confirm(`ID ${id} 폼을 정말로 삭제하시겠습니까?`)) {
    try {
      await formBuilderService.deleteFormDefinition(conventionId.value, id)
      alert('폼이 삭제되었습니다.')
      await fetchForms() // 목록 새로고침
    } catch (error) {
      console.error('Failed to delete form:', error)
      alert('폼 삭제에 실패했습니다.')
    }
  }
}

function formatDateTime(dateString) {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleString('ko-KR')
}

onMounted(() => {
  conventionId.value = parseInt(route.params.conventionId, 10)
  fetchForms()
})
</script>
