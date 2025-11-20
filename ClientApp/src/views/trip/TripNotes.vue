<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader :title="'체크리스트'" :subtitle="trip.title" :show-back="true" />
    
    <div class="max-w-2xl mx-auto px-4 py-4 pb-28">
      <!-- Tab Navigation -->
      <div class="flex justify-between items-center mb-4 border-b border-gray-200">
        <nav class="flex space-x-4" aria-label="Tabs">
          <button @click="activeTab = 'photos'"
                  :class="['px-3 py-2 font-medium text-sm rounded-t-lg', activeTab === 'photos' ? 'border-b-2 border-primary-500 text-primary-600' : 'text-gray-500 hover:text-gray-700']">
            사진첩
          </button>
          <button @click="activeTab = 'checklist'"
                  :class="['px-3 py-2 font-medium text-sm rounded-t-lg', activeTab === 'checklist' ? 'border-b-2 border-primary-500 text-primary-600' : 'text-gray-500 hover:text-gray-700']">
            체크리스트
          </button>
        </nav>
      </div>

      <!-- Tab Content -->
      <div v-if="activeTab === 'photos'">
        <div class="text-center py-16">
          <h2 class="text-xl font-bold text-gray-800">사진첩</h2>
          <p class="mt-2 text-gray-600">일자별로 업로드된 이미지를 표시합니다.</p>
        </div>
      </div>
      <div v-if="activeTab === 'checklist'">
        <div v-if="loading" class="text-center py-20">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 mx-auto"></div>
        </div>
        
        <div v-else class="space-y-4">
          <div v-for="(category, catIndex) in checklist" :key="category.id" class="bg-white rounded-xl shadow-sm">
            <div @click="toggleCategory(category.id)" class="flex justify-between items-center p-4 cursor-pointer">
              <h2 class="font-bold text-lg">{{ category.name }} <span class="text-gray-500 text-sm font-medium ml-2">({{ category.completedItemsCount }}/{{ category.totalItemsCount }})</span></h2>
               <div class="flex items-center gap-2">
                <button @click.stop="category.isEditing = !category.isEditing" class="text-sm font-medium text-gray-600 hover:text-gray-800 transition-colors">
                  {{ category.isEditing ? '완료' : '편집' }}
                </button>
                <button v-if="category.isEditing && !category.isDefault" @click.stop="deleteCategory(category.id)" class="p-1 text-red-500 hover:bg-red-100 rounded-full">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
                </button>
                <svg class="w-6 h-6 text-gray-400 transition-transform" :class="{'rotate-180': expandedCategories.includes(category.id)}" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
              </div>
            </div>

            <div v-if="expandedCategories.includes(category.id)">
              <div class="border-t border-gray-200 mt-4 pt-4"></div> <!-- Separator line -->
              <div class="px-4 pb-4 space-y-3">
                <div v-for="(item, itemIndex) in category.items" :key="item.id" @click="toggleItem(catIndex, itemIndex)" class="flex items-center gap-3 group cursor-pointer">
                  <div class="flex-shrink-0 w-5 h-5 rounded-full transition-all flex items-center justify-center"
                       :class="item.isChecked ? '' : 'border-2 border-dashed border-gray-300'">
                    <Check v-if="item.isChecked" class="w-5 h-5 text-primary-500" :stroke-width="3" />
                  </div>
                  <div class="flex-1" @click.stop="openItemEditModal(item)">
                    <p class="text-gray-800">{{ item.task }}</p>
                    <p v-if="item.description" class="text-xs text-gray-500">{{ item.description }}</p>
                  </div>
                  <button v-if="category.isEditing" @click.stop="deleteItem(category.id, item.id)" class="p-1 text-red-500 hover:bg-red-100 rounded-full transition-opacity">
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
                  </button>
                </div>
                
                <div class="flex items-center gap-3 cursor-pointer" @click.stop="promptNewItem(category.id)">
                   <div class="flex-shrink-0 w-5 h-5 rounded-full border-2 border-dashed border-primary-500 group-hover:border-primary-400"></div>
                   <p class="text-primary-500 font-medium">아이템 추가</p>
                </div>
              </div>
            </div>
          </div>
          <div class="flex justify-center mt-6">
            <button @click="promptNewCategory" class="w-1/2 py-3 bg-[#17B185] text-white rounded-xl font-semibold shadow-lg hover:bg-green-600 transition-all">
              카테고리 추가
            </button>
          </div>
        </div>
      </div>
    </div>

    <BottomNavigationBar v-if="tripId" :trip-id="tripId" :show="!uiStore.isModalOpen" />

    <!-- Generic Input Modal -->
    <SlideUpModal :is-open="isInputModalOpen" @close="closeInputModal">
      <template #header-title>{{ inputModalData.title }}</template>
      <template #body>
        <form @submit.prevent="handleInputConfirm">
          <label class="block text-sm font-medium text-gray-700 mb-1">{{ inputModalData.label }}</label>
          <input type="text" v-model="inputModalData.value" class="w-full px-3 py-2 border border-gray-300 rounded-lg" v-focus />
        </form>
      </template>
      <template #footer>
        <div class="flex gap-3 w-full">
          <button @click="closeInputModal" type="button" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold">취소</button>
          <button @click="handleInputConfirm" type="button" class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold">확인</button>
        </div>
      </template>
    </SlideUpModal>

    <!-- Item Edit Modal -->
    <SlideUpModal :is-open="isItemEditModalOpen" @close="closeItemEditModal">
        <template #header-title>항목 수정</template>
        <template #body>
            <form v-if="editingItem" id="item-edit-form" @submit.prevent="saveItemChanges" class="space-y-4">
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">항목</label>
                    <input type="text" v-model="editingItem.task" class="w-full px-3 py-2 border border-gray-300 rounded-lg" v-focus />
                </div>
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">메모</label>
                    <textarea v-model="editingItem.description" rows="4" class="w-full px-3 py-2 border border-gray-300 rounded-lg"></textarea>
                </div>
            </form>
        </template>
        <template #footer>
            <div class="flex gap-3 w-full">
                <button @click="closeItemEditModal" type="button" class="flex-1 py-3 px-4 bg-gray-100 text-gray-700 rounded-xl font-semibold">취소</button>
                <button type="submit" form="item-edit-form" class="flex-1 py-3 px-4 bg-primary-500 text-white rounded-xl font-semibold">저장</button>
            </div>
        </template>
    </SlideUpModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue';
import { useRoute } from 'vue-router';
import MainHeader from '@/components/common/MainHeader.vue';
import BottomNavigationBar from '@/components/common/BottomNavigationBar.vue';
import SlideUpModal from '@/components/common/SlideUpModal.vue';
import { useUIStore } from '@/stores/ui';
import apiClient from '@/services/api';
import { Check } from 'lucide-vue-next';

const uiStore = useUIStore();
const route = useRoute();
const tripId = computed(() => route.params.id);
const activeTab = ref('checklist');

const loading = ref(true);
const trip = ref({});
const checklist = ref([]);
const expandedCategories = ref([]);

// Modal for text input (Category/Item creation)
const isInputModalOpen = ref(false);
const inputModalData = ref({ title: '', label: '', value: '', callback: null });

// Modal for editing an item's task and description
const isItemEditModalOpen = ref(false);
const editingItem = ref(null);


const vFocus = {
  mounted: (el) => el.focus()
}

onMounted(async () => {
  await fetchTripData();
  if (checklist.value.length > 0) {
    // Expand default categories by default
    checklist.value.forEach(c => {
      if(c.isDefault) expandedCategories.value.push(c.id);
    })
  }
});

async function fetchTripData() {
  loading.value = true;
  try {
    const response = await apiClient.get(`/personal-trips/${tripId.value}`);
    trip.value = response.data;
    checklist.value = response.data.checklistCategories.map(cat => ({
      ...cat,
      isEditing: ref(false) // Add isEditing state to each category
    }));
  } catch (error) {
    console.error("Failed to fetch trip data:", error);
    alert('데이터를 불러오는데 실패했습니다.');
  } finally {
    loading.value = false;
  }
}

function toggleCategory(categoryId) {
  const index = expandedCategories.value.indexOf(categoryId);
  if (index > -1) {
    expandedCategories.value.splice(index, 1);
  } else {
    expandedCategories.value.push(categoryId);
  }
}



async function toggleItem(catIndex, itemIndex) {
    if (isEditMode.value) return;

    const item = checklist.value[catIndex].items[itemIndex];
    item.isChecked = !item.isChecked;
    
    // Optimistic update for the count
    const category = checklist.value[catIndex];
    if(item.isChecked) {
        category.completedItemsCount++;
    } else {
        category.completedItemsCount--;
    }

    try {
        await apiClient.put(`/personal-trips/checklist-items/${item.id}`, {
            task: item.task,
            description: item.description,
            isChecked: item.isChecked,
            order: item.order
        });
    } catch (error) {
        console.error("Failed to update item:", error);
        alert('항목 상태 변경에 실패했습니다.');
        // Revert on failure
        item.isChecked = !item.isChecked;
        if(item.isChecked) {
            category.completedItemsCount++;
        } else {
            category.completedItemsCount--;
        }
    }
}

// --- Item Edit Modal ---
function openItemEditModal(item) {
    // Need to clone the object to avoid reactive changes before saving
    editingItem.value = { ...item };
    isItemEditModalOpen.value = true;
}

function closeItemEditModal() {
    isItemEditModalOpen.value = false;
    editingItem.value = null;
}

async function saveItemChanges() {
    if (!editingItem.value) return;

    const itemToSave = editingItem.value;

    try {
        const response = await apiClient.put(`/personal-trips/checklist-items/${itemToSave.id}`, {
            task: itemToSave.task,
            description: itemToSave.description,
            isChecked: itemToSave.isChecked,
            order: itemToSave.order
        });
        
        // Find and update the original item in the reactive list
        for (const category of checklist.value) {
            const itemIndex = category.items.findIndex(i => i.id === itemToSave.id);
            if (itemIndex !== -1) {
                category.items[itemIndex] = response.data;
                break;
            }
        }
        closeItemEditModal();

    } catch (error) {
        console.error("Failed to save item:", error);
        alert('항목 저장에 실패했습니다.');
    }
}


// --- Generic Input Modal for Creation ---
function openInputModal(title, label, callback) {
  inputModalData.value = {
    title,
    label,
    value: '',
    callback
  };
  isInputModalOpen.value = true;
}

function closeInputModal() {
  isInputModalOpen.value = false;
}

function handleInputConfirm() {
  if (inputModalData.value.callback) {
    inputModalData.value.callback(inputModalData.value.value);
  }
  closeInputModal();
}

function promptNewCategory() {
  openInputModal('새 카테고리 추가', '카테고리 이름', async (name) => {
    if (name && name.trim()) {
      try {
        const response = await apiClient.post(`/personal-trips/${tripId.value}/checklist-categories`, { name: name.trim() });
        // After adding, refetch to get correct counts
        await fetchTripData();
        await nextTick();
        expandedCategories.value.push(response.data.id);
      } catch (error) {
        console.error("Failed to add category:", error);
        alert('카테고리 추가에 실패했습니다.');
      }
    }
  });
}

async function deleteCategory(categoryId) {
    if (!confirm("이 카테고리와 모든 항목을 삭제하시겠습니까?")) return;
    try {
        await apiClient.delete(`/personal-trips/checklist-categories/${categoryId}`);
        checklist.value = checklist.value.filter(c => c.id !== categoryId);
    } catch (error) {
        console.error("Failed to delete category:", error);
        alert('카테고리 삭제에 실패했습니다.');
    }
}

function promptNewItem(categoryId) {
    openInputModal('새 항목 추가', '항목 내용', async (task) => {
        if (task && task.trim()) {
            try {
                await apiClient.post(`/personal-trips/checklist-items`, {
                    checklistCategoryId: categoryId,
                    task: task.trim(),
                    description: ''
                });
                // Refetch to get correct counts and new item
                await fetchTripData();
            } catch (error) {
                console.error("Failed to add item:", error);
                alert('항목 추가에 실패했습니다.');
            }
        }
    });
}

async function deleteItem(categoryId, itemId) {
    if (!confirm("이 항목을 삭제하시겠습니까?")) return;
    try {
        await apiClient.delete(`/personal-trips/checklist-items/${itemId}`);
        const category = checklist.value.find(c => c.id === categoryId);
        if (category) {
            category.items = category.items.filter(i => i.id !== itemId);
            // Update counts
            category.totalItemsCount--;
            if (category.items.find(i => i.id === itemId)?.isChecked) {
                category.completedItemsCount--;
            }
        }
    }
    catch (error) {
        console.error("Failed to delete item:", error);
        alert('항목 삭제에 실패했습니다.');
    }
}

</script>
<style scoped>
.input-text-edit {
  @apply w-full border-b-2 border-primary-500 outline-none;
}
</style>
