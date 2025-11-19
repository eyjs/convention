<template>
  <Transition name="slide-up">
    <nav v-if="show" class="fixed bottom-0 left-0 right-0 z-40 bg-white shadow-[0_-4px_6px_-1px_rgb(0_0_0_/_0.1),_0_-2px_4px_-2px_rgb(0_0_0_/_0.1)] py-2">
      <div class="max-w-xl mx-auto flex justify-around items-center h-full">
        <router-link :to="`/trips/${tripId}`" class="flex flex-col items-center justify-center p-2 text-gray-500 hover:text-primary-600 transition-colors"
                     :class="{ 'text-primary-600 font-semibold': $route.path === `/trips/${tripId}` }">
          <Home class="w-6 h-6" />
          <span class="text-xs mt-1">홈</span>
        </router-link>
        <router-link :to="`/trips/${tripId}/itinerary`" class="flex flex-col items-center justify-center p-2 text-gray-500 hover:text-primary-600 transition-colors"
                     :class="{ 'text-primary-600 font-semibold': $route.path === `/trips/${tripId}/itinerary` }">
          <CalendarDays class="w-6 h-6" />
          <span class="text-xs mt-1">일정표</span>
        </router-link>
        <router-link :to="`/trips/${tripId}/expenses`" class="flex flex-col items-center justify-center p-2 text-gray-500 hover:text-primary-600 transition-colors"
                     :class="{ 'text-primary-600 font-semibold': $route.path === `/trips/${tripId}/expenses` }">
          <Receipt class="w-6 h-6" />
          <span class="text-xs mt-1">가계부</span>
        </router-link>
        <router-link :to="`/trips/${tripId}/notes`" class="flex flex-col items-center justify-center p-2 text-gray-500 hover:text-primary-600 transition-colors"
                     :class="{ 'text-primary-600 font-semibold': $route.path === `/trips/${tripId}/notes` }">
          <NotebookText class="w-6 h-6" />
          <span class="text-xs mt-1">노트</span>
        </router-link>
      </div>
    </nav>
  </Transition>
</template>

<script setup>
import { defineProps } from 'vue';
import { useRoute } from 'vue-router';
import { Home, CalendarDays, Receipt, NotebookText } from 'lucide-vue-next';

const props = defineProps({
  tripId: {
    type: [String, Number],
    required: true,
  },
  show: {
    type: Boolean,
    default: true,
  }
});

const route = useRoute();
</script>

<style scoped>
.slide-up-enter-active,
.slide-up-leave-active {
  transition: transform 0.3s ease-out;
}

.slide-up-enter-from,
.slide-up-leave-to {
  transform: translateY(100%);
}
</style>
