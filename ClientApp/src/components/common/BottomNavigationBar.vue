<template>
  <Transition name="slide-up">
    <nav v-if="show" class="fixed bottom-0 left-0 right-0 z-40 bg-white shadow-[0_-4px_6px_-1px_rgb(0_0_0_/_0.1),_0_-2px_4px_-2px_rgb(0_0_0_/_0.1)] py-2">
      <div class="max-w-xl mx-auto flex justify-around items-center h-full">
        <router-link :to="homeUrl" class="nav-link">
          <Home class="w-6 h-6" />
          <span class="text-xs mt-1">홈</span>
        </router-link>
        <router-link :to="itineraryUrl" class="nav-link">
          <CalendarDays class="w-6 h-6" />
          <span class="text-xs mt-1">일정표</span>
        </router-link>
        <router-link :to="expensesUrl" class="nav-link">
          <Receipt class="w-6 h-6" />
          <span class="text-xs mt-1">가계부</span>
        </router-link>
        <router-link :to="notesUrl" class="nav-link">
          <NotebookText class="w-6 h-6" />
          <span class="text-xs mt-1">노트</span>
        </router-link>
        <router-link :to="transportationUrl" class="nav-link">
          <Car class="w-6 h-6" />
          <span class="text-xs mt-1">교통편</span>
        </router-link>
      </div>
    </nav>
  </Transition>
</template>

<script setup>
import { computed } from 'vue';
import { Home, CalendarDays, Receipt, NotebookText, Car } from 'lucide-vue-next';

const props = defineProps({
  tripId: {
    type: [String, Number],
    required: true,
  },
  shareToken: {
    type: String,
    default: null
  },
  show: {
    type: Boolean,
    default: true,
  }
});

// A helper function to create safe URLs.
const createUrl = (path) => {
  // Check if both tripId and shareToken are missing or undefined
  if ((!props.tripId || props.tripId === 'undefined') && (!props.shareToken || props.shareToken === 'undefined')) {
    return '#'; // Return a non-functional link if IDs are missing or undefined
  }

  if (props.shareToken && props.shareToken !== 'undefined') {
    return `/trips/share/${props.shareToken}${path}`;
  }

  if (props.tripId && props.tripId !== 'undefined') {
    return `/trips/${props.tripId}${path}`;
  }

  return '#'; // Fallback
};

// Computed URLs based on share mode
const homeUrl = computed(() => createUrl(''));
const itineraryUrl = computed(() => createUrl('/itinerary'));
const expensesUrl = computed(() => createUrl('/expenses'));
const notesUrl = computed(() => createUrl('/notes'));
const transportationUrl = computed(() => createUrl('/transportation'));

</script>

<style scoped>
.nav-link {
  @apply flex flex-col items-center justify-center p-2 text-gray-500 hover:text-primary-600 transition-colors;
}

.router-link-exact-active {
  @apply text-primary-600 font-semibold;
}

.slide-up-enter-active,
.slide-up-leave-active {
  transition: transform 0.3s ease-out;
}

.slide-up-enter-from,
.slide-up-leave-to {
  transform: translateY(100%);
}
</style>
