import { ref, watch, nextTick } from 'vue';

export function useKakaoMapSearch() {
  const mapContainer = ref(null);
  const searchTerm = ref('');
  const searchResults = ref([]);
  const loadingSearch = ref(false);

  let map = null;
  let places = null;
  let markers = [];
  let infowindow = null;
  let searchTimeout = null;

  // Initialize map and services
  function initMap() {
    if (!window.kakao || !window.kakao.maps || !mapContainer.value) {
      console.error('Kakao Maps API is not available or map container is not ready.');
      return;
    }
    const options = {
      center: new window.kakao.maps.LatLng(33.450701, 126.570667), // Default to Jeju
      level: 3,
    };
    map = new window.kakao.maps.Map(mapContainer.value, options);
    places = new window.kakao.maps.services.Places();
    infowindow = new window.kakao.maps.InfoWindow({ zIndex: 1 });
  }

  // Relayout map, useful when container size changes
  function relayoutMap() {
    if (map) {
      setTimeout(() => map.relayout(), 50);
    }
  }
  
  // Set map center
  function setMapCenter(lat, lng) {
    if (map && lat && lng) {
      const center = new window.kakao.maps.LatLng(lat, lng);
      map.setCenter(center);
    }
  }

  // Clear all markers from the map
  function clearMarkers() {
    markers.forEach(marker => marker.setMap(null));
    markers = [];
    if (infowindow) infowindow.close();
  }

  // Add a single marker to the map
  function addMarker(position, title) {
    const marker = new window.kakao.maps.Marker({
      map: map,
      position: position,
    });
    markers.push(marker);

    window.kakao.maps.event.addListener(marker, 'click', () => {
      infowindow.setContent(`<div style="padding:5px;font-size:12px;">${title}</div>`);
      infowindow.open(map, marker);
    });
  }

  // Display multiple places on the map
  function displayPlaces(placesData) {
    clearMarkers();
    if (!placesData || placesData.length === 0) return;

    const bounds = new window.kakao.maps.LatLngBounds();
    placesData.forEach(place => {
      const placePosition = new window.kakao.maps.LatLng(place.y, place.x);
      addMarker(placePosition, place.place_name);
      bounds.extend(placePosition);
    });
    if (map) map.setBounds(bounds);
  }

  // Callback for keyword search
  function placesSearchCB(data, status) {
    loadingSearch.value = false;
    if (status === window.kakao.maps.services.Status.OK) {
      searchResults.value = data;
      displayPlaces(data);
    } else {
      searchResults.value = [];
      clearMarkers();
    }
  }

  // Perform keyword search with debounce
  function searchPlacesByKeyword() {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
      if (searchTerm.value.length < 2) {
        searchResults.value = [];
        clearMarkers();
        return;
      }
      loadingSearch.value = true;
      if (places) {
        places.keywordSearch(searchTerm.value, placesSearchCB);
      } else {
        console.error('Kakao Places service not initialized.');
        loadingSearch.value = false;
      }
    }, 300);
  }
  
  function clearSearch() {
    searchTerm.value = '';
    searchResults.value = [];
    clearMarkers();
  }

  // Cleanup function to be called on component unmount
  function cleanup() {
    clearTimeout(searchTimeout);
    clearMarkers();
    map = null;
    places = null;
    infowindow = null;
  }

  return {
    mapContainer,
    searchTerm,
    searchResults,
    loadingSearch,
    initMap,
    relayoutMap,
    setMapCenter,
    searchPlacesByKeyword,
    clearSearch,
    cleanup,
  };
}
