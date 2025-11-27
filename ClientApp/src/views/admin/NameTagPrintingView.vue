<template>
  <div class="min-h-screen bg-gray-50">
    <AdminHeader title="명찰 일괄 인쇄" show-back-button />

    <!-- Main Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="bg-white p-8 rounded-lg shadow-md">
        <div class="max-w-xl mx-auto">
          <h2 class="text-lg font-semibold text-gray-800 mb-2">행사 선택</h2>
          <p class="text-sm text-gray-600 mb-4">
            명찰을 인쇄할 행사를 선택해주세요. 행사 선택 후 엑셀 파일을 업로드할 수 있습니다.
          </p>

          <select v-model="selectedConventionId" class="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition">
            <option disabled :value="null">-- 행사를 선택하세요 --</option>
            <option v-for="convention in conventions" :key="convention.id" :value="convention.id">
              {{ convention.title }} ({{ formatDate(convention.startDate) }})
            </option>
          </select>
        </div>
      </div>

      <div v-if="selectedConventionId" class="mt-8">
        <TablePrint :convention-id="selectedConventionId" />
      </div>
       <div v-else class="mt-8 text-center bg-white p-12 rounded-lg shadow-md">
        <div class="inline-block p-4 bg-gray-100 rounded-full">
            <svg class="w-10 h-10 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"></path></svg>
        </div>
        <h3 class="mt-4 text-lg font-semibold text-gray-700">행사를 먼저 선택해주세요.</h3>
        <p class="mt-1 text-sm text-gray-500">상단의 드롭다운 메뉴에서 행사를 선택하면 인쇄 작업이 활성화됩니다.</p>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import apiClient from '@/services/api';
import AdminHeader from '@/components/admin/AdminHeader.vue';
import TablePrint from '@/components/admin/TablePrint.vue';

const router = useRouter();
const conventions = ref([]);
const selectedConventionId = ref(null);

async function loadConventions() {
  try {
    const response = await apiClient.get('/conventions');
    conventions.value = response.data.filter(c => c.completeYn !== 'Y'); // 진행중인 행사만
  } catch (error) {
    console.error('Failed to load conventions:', error);
    alert('행사 목록을 불러오는데 실패했습니다.');
  }
}

function formatDate(dateString) {
  if (!dateString) return '';
  return new Date(dateString).toLocaleDateString('ko-KR');
}

onMounted(loadConventions);
</script>
