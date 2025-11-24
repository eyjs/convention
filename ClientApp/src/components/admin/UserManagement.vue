<template>
  <div class="space-y-6">
    <h2 class="text-2xl font-bold">회원 관리</h2>

    <!-- Search and Filter -->
    <div class="flex items-center space-x-4">
      <input
        v-model.lazy="searchTerm"
        type="text"
        placeholder="이름, 아이디, 전화번호 검색..."
        class="input flex-grow"
      />
    </div>

    <!-- User Table -->
    <div class="bg-white shadow rounded-lg overflow-x-auto">
      <table class="w-full text-sm text-left text-gray-500">
        <thead class="text-xs text-gray-700 uppercase bg-gray-50">
          <tr>
            <th scope="col" class="px-6 py-3">ID</th>
            <th scope="col" class="px-6 py-3">이름</th>
            <th scope="col" class="px-6 py-3">로그인 ID</th>
            <th scope="col" class="px-6 py-3">전화번호</th>
            <th scope="col" class="px-6 py-3">역할</th>
            <th scope="col" class="px-6 py-3">상태</th>
            <th scope="col" class="px-6 py-3">생성일</th>
            <th scope="col" class="px-6 py-3">수정일</th>
            <th scope="col" class="px-6 py-3">작업</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading" class="border-b">
            <td colspan="9" class="text-center p-6">
              <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600 mx-auto"></div>
            </td>
          </tr>
          <tr v-else-if="users.length === 0" class="border-b">
            <td colspan="9" class="text-center p-6 text-gray-500">
              사용자가 없습니다.
            </td>
          </tr>
          <tr v-for="user in users" :key="user.id" class="bg-white border-b hover:bg-gray-50">
            <td class="px-6 py-4">{{ user.id }}</td>
            <th scope="row" class="px-6 py-4 font-medium text-gray-900 whitespace-nowrap">{{ user.name }}</th>
            <td class="px-6 py-4">{{ user.loginId || '-' }}</td>
            <td class="px-6 py-4">{{ user.phone }}</td>
            <td class="px-6 py-4">
              <span
                :class="getRoleClass(user.role)"
                class="px-2 py-1 text-xs font-medium rounded-full"
              >{{ user.role }}</span>
            </td>
            <td class="px-6 py-4">
              <span
                :class="user.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
                class="px-2 py-1 text-xs font-medium rounded-full"
              >{{ user.isActive ? '활성' : '비활성' }}</span>
            </td>
            <td class="px-6 py-4">{{ formatDate(user.createdAt) }}</td>
            <td class="px-6 py-4">{{ formatDate(user.updatedAt) }}</td>
            <td class="px-6 py-4">
              <div class="flex space-x-2">
                <button @click="() => openRoleModal(user)" class="text-blue-600 hover:underline">권한</button>
                <button @click="() => openStatusModal(user)" class="text-red-600 hover:underline">상태</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <nav v-if="totalPages > 1" class="flex items-center justify-between pt-4" aria-label="Table navigation">
      <span class="text-sm font-normal text-gray-500">
        총 <span class="font-semibold text-gray-900">{{ totalCount }}</span>명 중
        <span class="font-semibold text-gray-900">{{ (page - 1) * pageSize + 1 }} - {{ Math.min(page * pageSize, totalCount) }}</span>
      </span>
      <ul class="inline-flex -space-x-px text-sm h-8">
        <li>
          <button @click="changePage(page - 1)" :disabled="page === 1" class="pagination-btn rounded-l-lg">이전</button>
        </li>
        <li v-for="p in paginationRange" :key="p">
          <button
            @click="changePage(p)"
            :class="{ 'pagination-btn-active': p === page, 'pagination-btn': p !== page }"
          >{{ p }}</button>
        </li>
        <li>
          <button @click="changePage(page + 1)" :disabled="page === totalPages" class="pagination-btn rounded-r-lg">다음</button>
        </li>
      </ul>
    </nav>
    
    <!-- Modals -->
    <div v-if="showRoleModal" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center">
      <div class="bg-white p-6 rounded-lg shadow-xl w-full max-w-md">
        <h3 class="text-lg font-bold mb-4">역할 변경: {{ selectedUser.name }}</h3>
        <p class="mb-4">현재 역할: {{ selectedUser.role }}</p>
        <select v-model="newRole" class="input w-full mb-4">
          <option value="Admin">Admin</option>
          <option value="User">User</option>
        </select>
        <div class="flex justify-end space-x-4">
          <button @click="closeModal" class="btn-secondary">취소</button>
          <button @click="updateRole" class="btn-primary">변경</button>
        </div>
      </div>
    </div>

    <div v-if="showStatusModal" class="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center">
      <div class="bg-white p-6 rounded-lg shadow-xl w-full max-w-md">
        <h3 class="text-lg font-bold mb-4">상태 변경: {{ selectedUser.name }}</h3>
        <p class="mb-4">정말로 이 사용자를 <span class="font-bold">{{ selectedUser.isActive ? '비활성' : '활성' }}</span> 상태로 변경하시겠습니까?</p>
        <div class="flex justify-end space-x-4">
          <button @click="closeModal" class="btn-secondary">취소</button>
          <button @click="updateStatus" class="btn-danger">{{ selectedUser.isActive ? '비활성화' : '활성화' }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, computed } from 'vue';
import apiClient from '@/services/api';
import dayjs from 'dayjs';

const users = ref([]);
const loading = ref(false);
const searchTerm = ref('');
const page = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);
const totalPages = ref(1);

const selectedUser = ref(null);
const showRoleModal = ref(false);
const showStatusModal = ref(false);
const newRole = ref('');

const fetchUsers = async () => {
  loading.value = true;
  try {
    const response = await apiClient.get('/admin/users', {
      params: {
        searchTerm: searchTerm.value,
        page: page.value,
        pageSize: pageSize.value,
      },
    });
    users.value = response.data.items;
    totalCount.value = response.data.totalCount;
    totalPages.value = response.data.totalPages;
  } catch (error) {
    console.error('Failed to fetch users:', error);
    alert('사용자 목록을 불러오는 데 실패했습니다.');
  } finally {
    loading.value = false;
  }
};

watch(searchTerm, () => {
  page.value = 1;
  fetchUsers();
});

const changePage = (newPage) => {
  if (newPage > 0 && newPage <= totalPages.value) {
    page.value = newPage;
    fetchUsers();
  }
};

const formatDate = (dateString) => {
  if (!dateString) return '-';
  return dayjs(dateString).format('YYYY-MM-DD HH:mm');
};

const getRoleClass = (role) => {
  switch (role) {
    case 'Admin': return 'bg-purple-100 text-purple-800';
    case 'User': return 'bg-blue-100 text-blue-800';
    case 'Guest': return 'bg-gray-100 text-gray-800';
    default: return 'bg-gray-100 text-gray-800';
  }
};

const paginationRange = computed(() => {
    const range = [];
    const total = totalPages.value;
    const current = page.value;
    const delta = 2;
    const left = current - delta;
    const right = current + delta + 1;

    for (let i = 1; i <= total; i++) {
        if (i === 1 || i === total || (i >= left && i < right)) {
            range.push(i);
        }
    }

    const withDots = [];
    let l;
    for (const i of range) {
        if (l) {
            if (i - l === 2) {
                withDots.push(l + 1);
            } else if (i - l !== 1) {
                withDots.push('...');
            }
        }
        withDots.push(i);
        l = i;
    }
    return withDots.filter(p => p !== '...');
});


const closeModal = () => {
  showRoleModal.value = false;
  showStatusModal.value = false;
  selectedUser.value = null;
};

const openRoleModal = (user) => {
  selectedUser.value = { ...user };
  newRole.value = user.role;
  showRoleModal.value = true;
};

const openStatusModal = (user) => {
  selectedUser.value = { ...user };
  showStatusModal.value = true;
};

const updateRole = async () => {
  if (!selectedUser.value) return;
  try {
    await apiClient.put(`/admin/users/${selectedUser.value.id}/role`, { role: newRole.value });
    alert('역할이 성공적으로 변경되었습니다.');
    closeModal();
    fetchUsers();
  } catch (error) {
    console.error('Failed to update role:', error);
    alert('역할 변경에 실패했습니다.');
  }
};

const updateStatus = async () => {
  if (!selectedUser.value) return;
  try {
    const newStatus = !selectedUser.value.isActive;
    await apiClient.put(`/admin/users/${selectedUser.value.id}/status`, { isActive: newStatus });
    alert('상태가 성공적으로 변경되었습니다.');
    closeModal();
    fetchUsers();
  } catch (error) {
    console.error('Failed to update status:', error);
    alert('상태 변경에 실패했습니다.');
  }
};

onMounted(fetchUsers);
</script>

<style scoped>
.input {
  @apply block w-full px-3 py-2 bg-white border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm;
}
.btn-primary {
    @apply px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500;
}
.btn-secondary {
    @apply px-4 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500;
}
.btn-danger {
    @apply px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500;
}
.pagination-btn {
    @apply flex items-center justify-center px-3 h-8 leading-tight text-gray-500 bg-white border border-gray-300 hover:bg-gray-100 hover:text-gray-700;
}
.pagination-btn-active {
    @apply z-10 flex items-center justify-center px-3 h-8 leading-tight text-primary-600 border border-primary-300 bg-primary-50 hover:bg-primary-100 hover:text-primary-700;
}
</style>
