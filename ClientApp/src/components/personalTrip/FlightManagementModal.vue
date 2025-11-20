<template>
  <SlideUpModal :is-open="isOpen" @close="close">
    <template #header-title>교통 관리</template>
    <template #body>
      <div class="space-y-6">
        <!-- 카테고리별 섹션 -->
        <div v-for="category in categories" :key="category.name" class="space-y-2">
          <div class="flex items-center justify-between">
            <h3 class="text-sm font-semibold text-gray-700 flex items-center gap-2">
              <component :is="category.icon" class="w-4 h-4" :class="category.color" />
              {{ category.name }}
            </h3>
            <button @click="onAddWithCategory(category.name)" class="text-xs px-3 py-1.5 rounded-lg font-medium transition-all" :class="category.buttonClass">
              + {{ category.name }} 추가
            </button>
          </div>

          <div v-if="getFlightsByCategory(category.name).length > 0" class="space-y-2">
            <div v-for="flight in getFlightsByCategory(category.name)" :key="flight.id" class="border rounded-lg p-3 bg-gray-50">
              <div class="flex justify-between items-start">
                <div class="flex-1 min-w-0">
                  <p class="font-semibold text-gray-800">{{ flight.category }}</p>

                  <!-- 항공편: 예약번호 표시 -->
                  <template v-if="flight.category === '항공편'">
                    <p v-if="flight.bookingReference" class="text-xs text-gray-500 mt-0.5">예약번호: {{ flight.bookingReference }}</p>
                  </template>

                  <!-- 렌트카/자가용: 세부 비용 표시 -->
                  <template v-if="flight.category === '렌트카' || flight.category === '자가용'">
                    <div class="text-xs text-gray-500 mt-1 space-y-0.5">
                      <p v-if="flight.rentalCost">렌트: ₩{{ formatAmount(flight.rentalCost) }}</p>
                      <p v-if="flight.fuelCost">주유: ₩{{ formatAmount(flight.fuelCost) }}</p>
                      <p v-if="flight.tollFee">톨비: ₩{{ formatAmount(flight.tollFee) }}</p>
                      <p v-if="flight.parkingFee">주차: ₩{{ formatAmount(flight.parkingFee) }}</p>
                    </div>
                  </template>

                  <p class="text-sm font-medium text-primary-600 mt-1.5">₩{{ formatAmount(flight.amount || 0) }}</p>
                </div>
                <div class="flex-shrink-0 flex gap-2 ml-4">
                  <button @click="onEdit(flight)" class="p-2 text-primary-600 hover:bg-primary-100 rounded-full transition-colors">
                    <PencilIcon class="w-4 h-4" />
                  </button>
                  <button @click="onDelete(flight.id)" class="p-2 text-red-600 hover:bg-red-100 rounded-full transition-colors">
                    <Trash2Icon class="w-4 h-4" />
                  </button>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="text-center py-4 bg-gray-50 rounded-lg border border-dashed border-gray-300">
            <p class="text-xs text-gray-400">등록된 {{ category.name }} 없음</p>
          </div>
        </div>
      </div>
    </template>
    <template #footer>
      <button type="button" @click="close" class="w-full py-3 bg-gray-100 text-gray-700 rounded-xl font-semibold hover:bg-gray-200 active:bg-gray-300 transition-colors">
        닫기
      </button>
    </template>
  </SlideUpModal>
</template>

<script setup>
import SlideUpModal from '@/components/common/SlideUpModal.vue';
import { PencilIcon, Trash2Icon, PlaneIcon, TrainIcon, BusIcon, CarIcon } from 'lucide-vue-next';
import dayjs from 'dayjs';
import { h } from 'vue';

const props = defineProps({
  isOpen: Boolean,
  flights: {
    type: Array,
    default: () => [],
  },
});

const emit = defineEmits(['close', 'add', 'edit', 'delete']);

const categories = [
  {
    name: '항공편',
    icon: PlaneIcon,
    color: 'text-blue-600',
    buttonClass: 'bg-blue-50 text-blue-600 hover:bg-blue-100'
  },
  {
    name: '기차',
    icon: TrainIcon,
    color: 'text-green-600',
    buttonClass: 'bg-green-50 text-green-600 hover:bg-green-100'
  },
  {
    name: '버스',
    icon: BusIcon,
    color: 'text-orange-600',
    buttonClass: 'bg-orange-50 text-orange-600 hover:bg-orange-100'
  },
  {
    name: '택시',
    icon: CarIcon,
    color: 'text-purple-600',
    buttonClass: 'bg-purple-50 text-purple-600 hover:bg-purple-100'
  },
  {
    name: '렌트카',
    icon: CarIcon,
    color: 'text-pink-600',
    buttonClass: 'bg-pink-50 text-pink-600 hover:bg-pink-100'
  },
  {
    name: '자가용',
    icon: CarIcon,
    color: 'text-indigo-600',
    buttonClass: 'bg-indigo-50 text-indigo-600 hover:bg-indigo-100'
  },
];

const close = () => emit('close');
const onAddWithCategory = (category) => emit('add', category);
const onEdit = (flight) => emit('edit', flight);
const onDelete = (flightId) => emit('delete', flightId);

const getFlightsByCategory = (category) => {
  if (!props.flights) return [];
  return props.flights.filter(f => f.category === category);
};

const formatDateTime = (dateTime) => {
  if (!dateTime) return '';
  return dayjs(dateTime).format('YYYY-MM-DD HH:mm');
};

const formatDate = (date) => {
  if (!date) return '';
  return dayjs(date).format('YYYY-MM-DD');
};

const formatAmount = (amount) => {
  if (!amount) return '0';
  return amount.toLocaleString('ko-KR');
};
</script>
