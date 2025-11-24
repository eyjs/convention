<template>
  <div class="p-10 bg-white" style="width: 210mm;">
    <!-- Header -->
    <header class="flex items-center justify-between pb-4 border-b-2 border-gray-800">
      <div>
        <h1 class="text-4xl font-bold text-gray-900">지출 레포트</h1>
      </div>
      <div class="text-right">
        <h2 class="text-2xl font-semibold text-primary-600">{{ trip.title }}</h2>
        <p class="text-sm text-gray-600">{{ trip.startDate }} ~ {{ trip.endDate }}</p>
      </div>
    </header>

    <main class="mt-8">
      <!-- Summary Section -->
      <section class="mb-10">
        <h3 class="text-xl font-semibold text-gray-800 mb-4">요약</h3>
        <div class="grid grid-cols-3 gap-4">
          <div class="p-4 bg-blue-50 rounded-xl">
            <p class="text-sm text-blue-800 font-semibold">총 예산</p>
            <p class="text-2xl font-bold text-blue-900">₩{{ (trip.budget || 0).toLocaleString() }}</p>
          </div>
          <div class="p-4 bg-red-50 rounded-xl">
            <p class="text-sm text-red-800 font-semibold">총 지출</p>
            <p class="text-2xl font-bold text-red-900">₩{{ totalExpenses.toLocaleString() }}</p>
          </div>
          <div class="p-4 rounded-xl" :class="{ 'bg-green-50': (trip.budget - totalExpenses) >= 0, 'bg-orange-50': (trip.budget - totalExpenses) < 0 }">
            <p class="text-sm font-semibold" :class="{ 'text-green-800': (trip.budget - totalExpenses) >= 0, 'text-orange-800': (trip.budget - totalExpenses) < 0 }">남은 예산</p>
            <p class="text-2xl font-bold" :class="{ 'text-green-900': (trip.budget - totalExpenses) >= 0, 'text-orange-900': (trip.budget - totalExpenses) < 0 }">
              ₩{{ (trip.budget - totalExpenses).toLocaleString() }}
            </p>
          </div>
        </div>
      </section>

      <!-- Chart & Category Summary -->
      <section class="mb-10 grid grid-cols-5 gap-8 items-center">
        <div class="col-span-2">
          <h3 class="text-xl font-semibold text-gray-800 mb-4">카테고리별 상세</h3>
          <table class="w-full text-sm text-left">
            <tbody>
              <tr v-for="cat in categoryStats" :key="cat.key" class="border-b">
                <td class="py-3 font-bold">{{ cat.name }}</td>
                <td class="py-3 text-right">₩{{ cat.total.toLocaleString() }}</td>
                <td class="py-3 text-right text-gray-500">{{ cat.count }}건</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div class="col-span-3">
            <VueApexCharts
              height="350"
              type="donut"
              :options="chartOptions"
              :series="chartSeries"
            />
        </div>
      </section>
      
      <!-- Daily Details -->
      <section class="mb-10">
        <h3 class="text-xl font-semibold text-gray-800 mb-4">일자별 지출</h3>
        <div v-for="day in dailyExpenses" :key="day.dayNumber" class="mb-6 break-inside-avoid">
            <h4 class="text-lg font-semibold bg-gray-100 p-3 rounded-t-lg flex justify-between">
              <span>Day {{ day.dayNumber }} ({{ formatDayDate(day.dayNumber) }})</span>
              <span>합계: ₩{{ day.total.toLocaleString() }}</span>
            </h4>
            <table class="w-full text-sm text-left text-gray-600">
                <thead class="text-xs text-gray-700 uppercase bg-gray-50">
                    <tr>
                        <th class="px-4 py-2 w-1/5">구분</th>
                        <th class="px-4 py-2 w-3/5">내용</th>
                        <th class="px-4 py-2 w-1/5 text-right">금액</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="item in day.items" :key="item.id" class="border-b hover:bg-gray-50">
                        <td class="px-4 py-2 font-medium">{{ item.type }}</td>
                        <td class="px-4 py-2">{{ item.description }}</td>
                        <td class="px-4 py-2 text-right">₩{{ item.amount.toLocaleString() }}</td>
                    </tr>
                </tbody>
            </table>
        </div>
      </section>

      <!-- Trip-wide Expenses -->
      <section class="break-inside-avoid" v-if="tripWideExpenses.items.length > 0">
          <h3 class="text-xl font-semibold text-gray-800 mb-4">전체 기간 지출</h3>
          <table class="w-full text-sm text-left text-gray-600">
              <thead class="text-xs text-gray-700 uppercase bg-gray-100">
                  <tr>
                      <th class="px-6 py-3">구분</th>
                      <th class="px-6 py-3 text-right">금액</th>
                  </tr>
              </thead>
              <tbody>
                  <tr v-for="item in tripWideExpenses.items" :key="item.category" class="border-b hover:bg-gray-50">
                      <td class="px-6 py-4 font-semibold">{{ item.category }}</td>
                      <td class="px-6 py-4 text-right">₩{{ item.amount.toLocaleString() }}</td>
                  </tr>
              </tbody>
              <tfoot class="font-bold text-gray-900 bg-gray-100">
                  <tr>
                      <td class="px-6 py-3 text-base">합계</td>
                      <td class="px-6 py-3 text-right text-base">₩{{ tripWideExpenses.total.toLocaleString() }}</td>
                  </tr>
              </tfoot>
          </table>
      </section>
    </main>

    <footer class="text-center text-xs text-gray-400 mt-10 pt-4 border-t">
      Generated on {{ new Date().toLocaleString() }}
    </footer>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import dayjs from 'dayjs';
import VueApexCharts from 'vue3-apexcharts';

const props = defineProps({
  trip: { type: Object, required: true },
});

const getFlightAmount = (flight) => {
  if (flight.category === '렌트카' || flight.category === '자가용') {
    return (flight.rentalCost || 0) + (flight.fuelCost || 0) + (flight.tollFee || 0) + (flight.parkingFee || 0);
  }
  return flight.amount || 0;
};

const totalExpenses = computed(() => {
  let total = 0;
  if (props.trip.itineraryItems) total += props.trip.itineraryItems.reduce((sum, item) => sum + (item.expenseAmount || 0), 0);
  if (props.trip.accommodations) total += props.trip.accommodations.reduce((sum, acc) => sum + (acc.expenseAmount || 0), 0);
  if (props.trip.flights) total += props.trip.flights.reduce((sum, f) => sum + getFlightAmount(f), 0);
  return total;
});

const categoryStats = computed(() => {
  const itineraryItems = props.trip.itineraryItems?.filter(i => i.expenseAmount > 0) || [];
  const accommodationItems = props.trip.accommodations?.filter(a => a.expenseAmount > 0) || [];
  const transportationItems = props.trip.flights?.filter(f => getFlightAmount(f) > 0) || [];
  return [
    { name: '일정', key: 'itinerary', total: itineraryItems.reduce((sum, i) => sum + i.expenseAmount, 0), count: itineraryItems.length },
    { name: '숙소', key: 'accommodation', total: accommodationItems.reduce((sum, a) => sum + (a.expenseAmount || 0), 0), count: accommodationItems.length },
    { name: '교통', key: 'transportation', total: transportationItems.reduce((sum, f) => sum + getFlightAmount(f), 0), count: transportationItems.length },
  ].sort((a, b) => b.total - a.total);
});

const dailyExpenses = computed(() => {
  if (!props.trip.startDate) return [];
  const days = new Map();
  const tripStartDate = dayjs(props.trip.startDate);
  const numDays = dayjs(props.trip.endDate).diff(tripStartDate, 'day') + 1;

  for (let i = 1; i <= numDays; i++) {
    days.set(i, { dayNumber: i, items: [], total: 0 });
  }

  props.trip.itineraryItems?.forEach(item => {
    if (item.expenseAmount > 0 && item.dayNumber) {
      const day = days.get(item.dayNumber);
      if (day) {
        day.items.push({ id: `it-${item.id}`, type: '일정', description: item.locationName, amount: item.expenseAmount });
        day.total += item.expenseAmount;
      }
    }
  });

  props.trip.flights?.filter(f => f.category === '택시' && f.itineraryItemId).forEach(taxi => {
    const itinerary = props.trip.itineraryItems?.find(i => i.id === taxi.itineraryItemId);
    if (itinerary && itinerary.dayNumber) {
      const day = days.get(itinerary.dayNumber);
      if (day) {
        const amount = getFlightAmount(taxi);
        day.items.push({ id: `fl-${taxi.id}`, type: '교통(택시)', description: `${taxi.departureLocation || ''} → ${taxi.arrivalLocation || ''}`, amount });
        day.total += amount;
      }
    }
  });

  return Array.from(days.values()).filter(day => day.items.length > 0);
});

const tripWideExpenses = computed(() => {
  const items = [];
  const categories = ['항공편', '기차', '버스', '렌트카', '자가용'];
  categories.forEach(category => {
    const categoryFlights = props.trip.flights?.filter(f => f.category === category && !f.itineraryItemId) || [];
    const total = categoryFlights.reduce((sum, f) => sum + getFlightAmount(f), 0);
    if (total > 0) items.push({ category, amount: total });
  });

  const accommodationTotal = props.trip.accommodations?.reduce((sum, acc) => sum + (acc.expenseAmount || 0), 0) || 0;
  if (accommodationTotal > 0) {
    items.push({ category: '숙소', amount: accommodationTotal });
  }
  return { items, total: items.reduce((sum, item) => sum + item.amount, 0) };
});

const formatDayDate = (dayNumber) => {
  if (!props.trip.startDate) return '';
  const date = dayjs(props.trip.startDate).add(dayNumber - 1, 'day');
  return `${date.format('M/D')}(${['일', '월', '화', '수', '목', '금', '토'][date.day()]})`;
};

const chartSeries = computed(() => categoryStats.value.filter(c => c.total > 0).map(c => c.total));
const chartOptions = computed(() => ({
    chart: { type: 'donut', fontFamily: 'inherit', animations: { enabled: false } },
    labels: categoryStats.value.filter(c => c.total > 0).map(c => c.name),
    colors: ['#3B82F6', '#EF4444', '#10B981', '#F59E0B', '#8B5CF6'],
    legend: { show: false },
    dataLabels: { enabled: true, formatter: (val, opts) => `${opts.w.globals.labels[opts.seriesIndex]} ${val.toFixed(1)}%`, style: { fontSize: '12px', fontWeight: 'bold' } },
    plotOptions: { pie: { donut: { labels: { show: true, total: { show: true, label: '총 지출', formatter: () => `₩${totalExpenses.value.toLocaleString('ko-KR')}` } } } } },
    tooltip: { enabled: false },
}));

</script>

<style>
@import url('https://cdn.jsdelivr.net/npm/tailwindcss@2.2.19/dist/tailwind.min.css');
@import url('https://fonts.googleapis.com/css2?family=Noto+Sans+KR:wght@400;500;700&display=swap');
body {
    font-family: 'Noto Sans KR', sans-serif;
}
.break-inside-avoid {
    break-inside: avoid;
}
</style>