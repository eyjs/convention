<template>
  <div class="max-w-7xl mx-auto p-5 sm:p-8 font-sans">
    <div class="text-center mb-8">
      <h1 class="text-3xl sm:text-4xl font-bold text-gray-800 mb-2">🎯 참석자 속성 일괄 매핑</h1>
      <p class="text-base sm:text-lg text-gray-500">여러 참석자에게 속성을 한번에 설정할 수 있습니다</p>
    </div>

    <div class="bg-gray-50 p-6 rounded-xl shadow-sm mb-6">
      <div class="form-group">
        <label for="convention-select" class="block text-sm font-semibold text-gray-700 mb-2">행사 선택</label>
        <select id="convention-select" v-model="selectedConventionId" @change="loadAttributeDefinitions" class="w-full p-3 border border-gray-300 rounded-lg text-base focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition">
          <option :value="null">행사를 선택하세요</option>
          <option v-for="conv in conventions" :key="conv.id" :value="conv.id">{{ conv.title }}</option>
        </select>
      </div>
      <div v-if="attributeDefinitions.length > 0" class="mt-5 pt-5 border-t border-gray-200">
        <h3 class="text-base font-semibold text-gray-700 mb-3">설정 가능한 속성 목록</h3>
        <div class="flex flex-wrap gap-3">
          <span v-for="attr in attributeDefinitions" :key="attr.id" class="inline-flex items-center gap-2 bg-primary-50 text-primary-700 text-sm font-medium px-3 py-1.5 rounded-full border border-primary-200">
            {{ attr.attributeKey }}
            <span v-if="attr.isRequired" class="bg-red-500 text-white text-xs font-bold px-2 py-0.5 rounded-full">필수</span>
          </span>
        </div>
      </div>
    </div>

    <div v-if="selectedConventionId" class="flex flex-col sm:flex-row justify-between items-center bg-gray-100 p-4 rounded-lg mb-6 gap-4">
      <div class="flex items-center gap-3 flex-wrap justify-center">
        <button @click="toggleSelectAll" class="px-4 py-2 text-sm font-semibold rounded-lg transition" :class="allSelected ? 'bg-gray-600 text-white hover:bg-gray-700' : 'bg-white text-gray-700 border hover:bg-gray-50'">{{ allSelected ? '전체 선택 해제' : '전체 선택' }}</button>
        <span class="px-4 py-2 text-sm font-semibold bg-primary-600 text-white rounded-full">{{ selectedGuests.length }}명 선택됨</span>
      </div>
      <div class="flex items-center gap-3 w-full sm:w-auto">
        <input v-model="searchText" type="text" placeholder="이름, 회사, 부서로 검색..." class="w-full sm:w-64 px-4 py-2 border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-primary-500" />
        <button @click="openBulkEditModal" :disabled="selectedGuests.length === 0" class="px-4 py-2 text-sm font-semibold bg-primary-600 text-white rounded-lg hover:bg-primary-700 disabled:bg-gray-300 disabled:cursor-not-allowed flex-shrink-0">선택한 참석자 속성 설정</button>
      </div>
    </div>

    <div v-if="loading" class="text-center py-16 text-gray-500">
      <div class="inline-block border-4 border-gray-200 border-t-primary-600 rounded-full w-12 h-12 animate-spin mb-4"></div>
      <p>참석자 목록을 불러오는 중...</p>
    </div>

    <div v-if="!loading && filteredGuests.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5 max-h-[800px] overflow-y-auto p-4 bg-gray-50 rounded-lg">
      <div v-for="guest in filteredGuests" :key="guest.id" class="bg-white border-2 rounded-xl p-5 cursor-pointer transition-all duration-300 hover:shadow-lg hover:-translate-y-1" :class="isSelected(guest.id) ? 'border-primary-500 bg-primary-50 shadow-md' : 'border-gray-200'" @click="toggleSelection(guest.id)">
        <div class="flex items-start gap-4 mb-4">
          <input type="checkbox" :checked="isSelected(guest.id)" @click.stop="toggleSelection(guest.id)" class="w-5 h-5 mt-1 rounded text-primary-600 focus:ring-primary-500" />
          <div class="flex-1">
            <div class="text-lg font-bold text-gray-800 mb-1">{{ guest.guestName }}</div>
            <div class="text-sm text-gray-600 mb-1">{{ guest.corpName }} {{ guest.corpPart ? `/ ${guest.corpPart}` : '' }}</div>
            <div class="text-xs text-gray-400">{{ guest.telephone }}</div>
          </div>
        </div>
        <div v-if="Object.keys(guest.currentAttributes).length > 0" class="bg-gray-50 p-3 rounded-lg">
          <div class="text-xs font-semibold text-gray-500 mb-2 uppercase tracking-wider">현재 속성:</div>
          <div class="flex flex-wrap gap-2">
            <span v-for="(value, key) in guest.currentAttributes" :key="key" class="text-xs bg-white border px-2 py-1 rounded-md text-gray-700"><strong>{{ key }}:</strong> {{ value }}</span>
          </div>
        </div>
        <div v-else class="text-center p-3 text-xs text-gray-400 italic bg-gray-50 rounded-lg mt-3">
          속성 미설정
        </div>
      </div>
    </div>

    <div v-if="!loading && filteredGuests.length === 0" class="text-center py-20 text-base text-gray-500">
      <p v-if="!selectedConventionId">행사를 선택해주세요</p>
      <p v-else-if="searchText">검색 결과가 없습니다</p>
      <p v-else>등록된 참석자가 없습니다</p>
    </div>

    <!-- Modal -->
    <div v-if="showBulkEditModal" class="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4" @click="closeBulkEditModal">
      <div class="bg-white rounded-2xl shadow-xl max-w-3xl w-full max-h-[90vh] overflow-hidden flex flex-col" @click.stop>
        <div class="flex justify-between items-center p-6 border-b">
          <h2 class="text-2xl font-bold text-gray-800">참석자 속성 일괄 설정</h2>
          <button @click="closeBulkEditModal" class="text-3xl text-gray-400 hover:text-red-500 transition">&times;</button>
        </div>
        <div class="p-6 space-y-6 overflow-y-auto">
          <p class="bg-primary-50 border-l-4 border-primary-500 p-4 rounded-md text-gray-800"><strong class="font-bold text-primary-600 text-lg">{{ selectedGuests.length }}명</strong>의 참석자에게 속성을 설정합니다</p>
          <div class="space-y-4">
            <div v-for="definition in attributeDefinitions" :key="definition.id" class="form-group">
              <label class="block text-sm font-semibold text-gray-700 mb-2">{{ definition.attributeKey }}<span v-if="definition.isRequired" class="text-red-500 ml-1">*</span></label>
              <select v-if="definition.options && definition.options.length > 0" v-model="bulkAttributes[definition.attributeKey]" class="w-full p-3 border border-gray-300 rounded-lg text-base focus:ring-2 focus:ring-primary-500">
                <option value="">선택하세요</option>
                <option v-for="option in definition.options" :key="option" :value="option">{{ option }}</option>
              </select>
              <input v-else v-model="bulkAttributes[definition.attributeKey]" type="text" class="w-full p-3 border border-gray-300 rounded-lg text-base focus:ring-2 focus:ring-primary-500" :placeholder="`${definition.attributeKey}를 입력하세요`" />
            </div>
          </div>
          <div class="bg-gray-50 p-4 rounded-lg mt-6">
            <h3 class="text-base font-semibold text-gray-700 mb-3">미리보기</h3>
            <div class="space-y-3 max-h-40 overflow-y-auto">
              <div v-for="guestId in selectedGuests.slice(0, 5)" :key="guestId" class="bg-white p-3 rounded-md border">
                <strong class="block text-sm font-bold text-gray-800 mb-2">{{ getGuestName(guestId) }}</strong>
                <div class="flex flex-wrap gap-2">
                  <span v-for="(value, key) in bulkAttributes" :key="key" v-if="value" class="text-xs font-medium bg-primary-100 text-primary-800 px-2 py-1 rounded-md">{{ key }}: {{ value }}</span>
                </div>
              </div>
              <div v-if="selectedGuests.length > 5" class="text-center text-sm text-gray-500 italic p-2">... 외 {{ selectedGuests.length - 5 }}명</div>
            </div>
          </div>
        </div>
        <div class="flex justify-end gap-3 p-6 bg-gray-50 border-t">
          <button @click="closeBulkEditModal" class="px-5 py-2 text-sm font-semibold bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition">취소</button>
          <button @click="submitBulkAssign" class="px-5 py-2 text-sm font-semibold bg-green-600 text-white rounded-lg hover:bg-green-700 transition disabled:opacity-50" :disabled="submitting">{{ submitting ? '저장 중...' : '일괄 저장' }}</button>
        </div>
      </div>
    </div>

    <!-- Toast -->
    <div v-if="toast.show" class="fixed top-5 right-5 px-6 py-3 rounded-lg shadow-lg text-white font-semibold animate-slide-in" :class="toast.type === 'success' ? 'bg-green-500' : (toast.type === 'error' ? 'bg-red-500' : 'bg-yellow-500')">{{ toast.message }}</div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import apiClient from '@/services/api'; // apiClient 사용

const conventions = ref([]);
const selectedConventionId = ref(null);
const attributeDefinitions = ref([]);
const guests = ref([]);
const selectedGuests = ref([]);
const searchText = ref('');
const bulkAttributes = ref({});
const showBulkEditModal = ref(false);
const loading = ref(false);
const submitting = ref(false);
const toast = ref({ show: false, message: '', type: 'success' });

const filteredGuests = computed(() => {
  if (!searchText.value) return guests.value;
  const search = searchText.value.toLowerCase();
  return guests.value.filter(g =>
    g.guestName.toLowerCase().includes(search) ||
    (g.corpName && g.corpName.toLowerCase().includes(search)) ||
    (g.corpPart && g.corpPart.toLowerCase().includes(search))
  );
});

const allSelected = computed(() => 
    filteredGuests.value.length > 0 && 
    filteredGuests.value.every(g => selectedGuests.value.includes(g.id))
);

onMounted(async () => {
  await loadConventions();
});

async function loadConventions() {
  try {
    const response = await apiClient.get('/admin/conventions'); // API 경로 수정
    conventions.value = response.data;
  } catch (error) {
    showToast('행사 목록을 불러오는데 실패했습니다', 'error');
  }
}

async function loadAttributeDefinitions() {
  if (!selectedConventionId.value) return;
  try {
    const response = await apiClient.get(`/attributetemplate/conventions/${selectedConventionId.value}`); // API 경로 수정
    attributeDefinitions.value = response.data.map(def => ({ ...def, options: def.attributeValues ? JSON.parse(def.attributeValues) : [] }));
    await loadGuests();
  } catch (error) {
    showToast('속성 정의를 불러오는데 실패했습니다', 'error');
  }
}

async function loadGuests() {
  loading.value = true;
  try {
    const response = await apiClient.get(`/admin/conventions/${selectedConventionId.value}/guests`); // API 경로 수정
    guests.value = response.data.map(g => ({...g, currentAttributes: g.attributes.reduce((acc, attr) => {acc[attr.attributeKey] = attr.attributeValue; return acc}, {}) }));
    selectedGuests.value = [];
  } catch (error) {
    showToast('참석자 목록을 불러오는데 실패했습니다', 'error');
  } finally {
    loading.value = false;
  }
}

function toggleSelection(guestId) {
  const index = selectedGuests.value.indexOf(guestId);
  if (index > -1) {
    selectedGuests.value.splice(index, 1);
  } else {
    selectedGuests.value.push(guestId);
  }
}

const isSelected = (guestId) => selectedGuests.value.includes(guestId);

function toggleSelectAll() {
  if (allSelected.value) {
    selectedGuests.value = [];
  } else {
    selectedGuests.value = filteredGuests.value.map(g => g.id);
  }
}

function openBulkEditModal() {
  if (selectedGuests.value.length === 0) {
    showToast('참석자를 선택해주세요', 'warning');
    return;
  }
  bulkAttributes.value = {};
  attributeDefinitions.value.forEach(def => { bulkAttributes.value[def.attributeKey] = ''; });
  showBulkEditModal.value = true;
}

function closeBulkEditModal() {
  showBulkEditModal.value = false;
  bulkAttributes.value = {};
}

function getGuestName(guestId) {
  const guest = guests.value.find(g => g.id === guestId);
  return guest ? guest.guestName : '';
}

async function submitBulkAssign() {
  const requiredAttrs = attributeDefinitions.value.filter(def => def.isRequired);
  for (const attr of requiredAttrs) {
    if (!bulkAttributes.value[attr.attributeKey]) {
      showToast(`${attr.attributeKey}는 필수 항목입니다`, 'warning');
      return;
    }
  }
  const filteredAttributes = Object.entries(bulkAttributes.value).filter(([_, value]) => value !== '').reduce((acc, [key, value]) => { acc[key] = value; return acc; }, {});
  if (Object.keys(filteredAttributes).length === 0) {
    showToast('최소 하나의 속성을 입력해주세요', 'warning');
    return;
  }
  submitting.value = true;
  try {
    const payload = { conventionId: selectedConventionId.value, guestMappings: selectedGuests.value.map(guestId => ({ guestId, attributes: filteredAttributes })) };
    const response = await apiClient.post('/guest/bulk-assign-attributes', payload);
    if (response.data.success) {
      showToast(response.data.message, 'success');
      closeBulkEditModal();
      await loadGuests();
      selectedGuests.value = [];
    } else {
      showToast(response.data.message || '알 수 없는 오류 발생', 'error');
    }
  } catch (error) {
    showToast(error.response?.data?.message || '속성 할당에 실패했습니다', 'error');
  } finally {
    submitting.value = false;
  }
}

function showToast(message, type = 'success') {
  toast.value = { show: true, message, type };
  setTimeout(() => { toast.value.show = false; }, 3000);
}
</script>

<style>
@keyframes slideIn {
  from { transform: translateX(100%); opacity: 0; }
  to { transform: translateX(0); opacity: 1; }
}
.animate-slide-in {
  animation: slideIn 0.3s ease-out;
}
</style>