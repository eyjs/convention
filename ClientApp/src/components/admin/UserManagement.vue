<template>
  <div class="space-y-6">
    <AdminPageHeader title="회원 관리" :description="`전체 ${totalCount}명`" />

    <!-- Search and Filter -->
    <div class="flex items-center space-x-4">
      <input
        v-model.lazy="searchTerm"
        type="text"
        placeholder="이름, 아이디, 전화번호 검색..."
        class="block w-full px-3 py-2 bg-white border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm flex-grow"
      />
    </div>

    <!-- User Table -->
    <AdminTable
      :columns="tableColumns"
      :loading="loading"
      :empty="!loading && users.length === 0"
      :empty-icon="Users"
      empty-text="사용자가 없습니다."
    >
      <tr v-for="user in users" :key="user.id" class="hover:bg-gray-50">
        <td class="px-6 py-4 text-sm text-gray-500">{{ user.id }}</td>
        <td
          class="px-6 py-4 text-sm font-medium text-gray-900 whitespace-nowrap"
        >
          {{ user.name }}
        </td>
        <td class="px-6 py-4 text-sm text-gray-500">
          {{ user.loginId || '-' }}
        </td>
        <td class="px-6 py-4 text-sm text-gray-500">{{ user.phone }}</td>
        <td class="px-6 py-4">
          <AdminBadge
            :variant="
              user.role === 'Admin'
                ? 'info'
                : user.role === 'Guest'
                  ? 'neutral'
                  : 'success'
            "
          >
            {{ user.role }}
          </AdminBadge>
        </td>
        <td class="px-6 py-4">
          <AdminBadge :variant="user.isActive ? 'success' : 'danger'">
            {{ user.isActive ? '활성' : '비활성' }}
          </AdminBadge>
        </td>
        <td class="px-6 py-4 text-sm text-gray-500">
          {{ formatDate(user.createdAt) }}
        </td>
        <td class="px-6 py-4 text-sm text-gray-500">
          {{ formatDate(user.updatedAt) }}
        </td>
        <td class="px-6 py-4 text-right">
          <div class="flex justify-end space-x-2">
            <button
              class="text-sm text-primary-600 hover:underline"
              @click="() => openRoleModal(user)"
            >
              권한
            </button>
            <button
              class="text-sm text-red-600 hover:underline"
              @click="() => openStatusModal(user)"
            >
              상태
            </button>
          </div>
        </td>
      </tr>
    </AdminTable>

    <!-- Pagination -->
    <nav
      v-if="totalPages > 1"
      class="flex items-center justify-between pt-4"
      aria-label="Table navigation"
    >
      <span class="text-sm font-normal text-gray-500">
        총 <span class="font-semibold text-gray-900">{{ totalCount }}</span
        >명 중
        <span class="font-semibold text-gray-900"
          >{{ (page - 1) * pageSize + 1 }} -
          {{ Math.min(page * pageSize, totalCount) }}</span
        >
      </span>
      <ul class="inline-flex -space-x-px text-sm h-8">
        <li>
          <button
            :disabled="page === 1"
            class="flex items-center justify-center px-3 h-8 leading-tight text-gray-500 bg-white border border-gray-300 hover:bg-gray-100 hover:text-gray-700 rounded-l-lg"
            @click="changePage(page - 1)"
          >
            이전
          </button>
        </li>
        <li v-for="p in paginationRange" :key="p">
          <button
            :class="
              p === page
                ? 'z-10 flex items-center justify-center px-3 h-8 leading-tight text-primary-600 border border-primary-300 bg-primary-50 hover:bg-primary-100 hover:text-primary-700'
                : 'flex items-center justify-center px-3 h-8 leading-tight text-gray-500 bg-white border border-gray-300 hover:bg-gray-100 hover:text-gray-700'
            "
            @click="changePage(p)"
          >
            {{ p }}
          </button>
        </li>
        <li>
          <button
            :disabled="page === totalPages"
            class="flex items-center justify-center px-3 h-8 leading-tight text-gray-500 bg-white border border-gray-300 hover:bg-gray-100 hover:text-gray-700 rounded-r-lg"
            @click="changePage(page + 1)"
          >
            다음
          </button>
        </li>
      </ul>
    </nav>

    <!-- Role Modal -->
    <BaseModal :is-open="showRoleModal" max-width="md" @close="closeModal">
      <template #header>
        <h3 class="text-lg font-semibold">
          역할 변경: {{ selectedUser?.name }}
        </h3>
      </template>
      <template #body>
        <p class="mb-4">현재 역할: {{ selectedUser?.role }}</p>
        <select v-model="newRole" class="w-full px-3 py-2 border rounded-lg">
          <option value="Admin">Admin</option>
          <option value="User">User</option>
        </select>
      </template>
      <template #footer>
        <button
          class="px-4 py-2 border rounded-lg hover:bg-gray-50"
          @click="closeModal"
        >
          취소
        </button>
        <button
          class="px-4 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700"
          @click="updateRole"
        >
          변경
        </button>
      </template>
    </BaseModal>

    <!-- Status Modal -->
    <BaseModal :is-open="showStatusModal" max-width="md" @close="closeModal">
      <template #header>
        <h3 class="text-lg font-semibold">
          상태 변경: {{ selectedUser?.name }}
        </h3>
      </template>
      <template #body>
        <p class="mb-4">
          정말로 이 사용자를
          <span class="font-bold">{{
            selectedUser?.isActive ? '비활성' : '활성'
          }}</span>
          상태로 변경하시겠습니까?
        </p>
      </template>
      <template #footer>
        <button
          class="px-4 py-2 border rounded-lg hover:bg-gray-50"
          @click="closeModal"
        >
          취소
        </button>
        <button
          class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
          @click="updateStatus"
        >
          {{ selectedUser?.isActive ? '비활성화' : '활성화' }}
        </button>
      </template>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, computed } from 'vue'

import apiClient from '@/services/api'
import dayjs from 'dayjs'

import AdminPageHeader from '@/components/admin/ui/AdminPageHeader.vue'
import AdminTable from '@/components/admin/ui/AdminTable.vue'
import AdminBadge from '@/components/admin/ui/AdminBadge.vue'
import BaseModal from '@/components/common/BaseModal.vue'
import { Users } from 'lucide-vue-next'

const tableColumns = [
  { key: 'id', label: 'ID', width: '60px' },
  { key: 'name', label: '이름' },
  { key: 'loginId', label: '로그인 ID' },
  { key: 'phone', label: '전화번호' },
  { key: 'role', label: '역할' },
  { key: 'status', label: '상태' },
  { key: 'createdAt', label: '생성일' },
  { key: 'updatedAt', label: '수정일' },
  { key: 'actions', label: '작업', align: 'right' },
]

const users = ref([])
const loading = ref(false)
const searchTerm = ref('')
const page = ref(1)
const pageSize = ref(10)
const totalCount = ref(0)
const totalPages = ref(1)

const selectedUser = ref(null)
const showRoleModal = ref(false)
const showStatusModal = ref(false)
const newRole = ref('')

const fetchUsers = async () => {
  loading.value = true
  try {
    const response = await apiClient.get('/admin/users', {
      params: {
        searchTerm: searchTerm.value,
        page: page.value,
        pageSize: pageSize.value,
      },
    })
    users.value = response.data.items
    totalCount.value = response.data.totalCount
    totalPages.value = response.data.totalPages
  } catch (error) {
    console.error('Failed to fetch users:', error)
    alert('사용자 목록을 불러오는 데 실패했습니다.')
  } finally {
    loading.value = false
  }
}

watch(searchTerm, () => {
  page.value = 1
  fetchUsers()
})

const changePage = (newPage) => {
  if (newPage > 0 && newPage <= totalPages.value) {
    page.value = newPage
    fetchUsers()
  }
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  return dayjs(dateString).format('YYYY-MM-DD HH:mm')
}

const paginationRange = computed(() => {
  const range = []
  const total = totalPages.value
  const current = page.value
  const delta = 2
  const left = current - delta
  const right = current + delta + 1

  for (let i = 1; i <= total; i++) {
    if (i === 1 || i === total || (i >= left && i < right)) {
      range.push(i)
    }
  }

  const withDots = []
  let l
  for (const i of range) {
    if (l) {
      if (i - l === 2) {
        withDots.push(l + 1)
      } else if (i - l !== 1) {
        withDots.push('...')
      }
    }
    withDots.push(i)
    l = i
  }
  return withDots.filter((p) => p !== '...')
})

const closeModal = () => {
  showRoleModal.value = false
  showStatusModal.value = false
  selectedUser.value = null
}

const openRoleModal = (user) => {
  selectedUser.value = { ...user }
  newRole.value = user.role
  showRoleModal.value = true
}

const openStatusModal = (user) => {
  selectedUser.value = { ...user }
  showStatusModal.value = true
}

const updateRole = async () => {
  if (!selectedUser.value) return
  try {
    await apiClient.put(`/admin/users/${selectedUser.value.id}/role`, {
      role: newRole.value,
    })
    alert('역할이 성공적으로 변경되었습니다.')
    closeModal()
    fetchUsers()
  } catch (error) {
    console.error('Failed to update role:', error)
    alert('역할 변경에 실패했습니다.')
  }
}

const updateStatus = async () => {
  if (!selectedUser.value) return
  try {
    const newStatus = !selectedUser.value.isActive
    await apiClient.put(`/admin/users/${selectedUser.value.id}/status`, {
      isActive: newStatus,
    })
    alert('상태가 성공적으로 변경되었습니다.')
    closeModal()
    fetchUsers()
  } catch (error) {
    console.error('Failed to update status:', error)
    alert('상태 변경에 실패했습니다.')
  }
}

onMounted(fetchUsers)
</script>
