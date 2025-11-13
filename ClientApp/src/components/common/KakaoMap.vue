<template>
  <div ref="mapContainer" class="w-full h-full"></div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'

const props = defineProps({
  latitude: {
    type: Number,
    required: true
  },
  longitude: {
    type: Number,
    required: true
  }
})

const mapContainer = ref(null)
let map = null

onMounted(() => {
  if (window.kakao && window.kakao.maps) {
    initMap()
  } else {
    console.error('Kakao Maps API is not loaded.')
  }
})

watch(() => [props.latitude, props.longitude], () => {
  if (map) {
    const newPos = new window.kakao.maps.LatLng(props.latitude, props.longitude)
    map.setCenter(newPos)
    
    // Clear existing markers if any
    // For simplicity, we are creating a new marker every time.
    // A more robust solution would be to manage a marker instance.
    const marker = new window.kakao.maps.Marker({
      position: newPos
    })
    marker.setMap(map)
  }
})

function initMap() {
  const options = {
    center: new window.kakao.maps.LatLng(props.latitude, props.longitude),
    level: 3
  }
  map = new window.kakao.maps.Map(mapContainer.value, options)

  const marker = new window.kakao.maps.Marker({
    position: options.center
  })
  marker.setMap(map)
}
</script>