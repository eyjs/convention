import { ref } from 'vue';

const isLoaded = ref(false);
const isLoading = ref(false);
const error = ref(null);

export function useGoogleMaps() {
  const loadScript = () => {
    if (isLoaded.value || isLoading.value) {
      return;
    }

    isLoading.value = true;
    const apiKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY;
    if (!apiKey) {
      const errorMessage = 'Google Maps API key is not configured. Please set VITE_GOOGLE_MAPS_API_KEY in your .env file.';
      console.error(errorMessage);
      error.value = errorMessage;
      isLoading.value = false;
      return;
    }

    const script = document.createElement('script');
    script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKey}&libraries=places&loading=async`;
    script.async = true;
    script.defer = true;
    document.head.appendChild(script);

    script.onload = () => {
      isLoaded.value = true;
      isLoading.value = false;
    };

    script.onerror = () => {
      error.value = 'Failed to load Google Maps script.';
      isLoading.value = false;
    };
  };

  return {
    isLoaded,
    isLoading,
    error,
    loadScript,
  };
}
