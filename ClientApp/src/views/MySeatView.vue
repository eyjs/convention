<template>
  <div class="fixed inset-0 flex flex-col bg-gray-50">
    <!-- 상단 고정 -->
    <MainHeader title="내 자리" :show-back="true" />

    <div v-if="loading" class="flex-1 flex items-center justify-center text-gray-500">로딩 중...</div>
    <div v-else-if="!activeLayout" class="flex-1 flex items-center justify-center text-gray-400 px-4 text-center">배정된 자리가 없습니다.</div>

    <template v-else>
      <!-- 탭 -->
      <div v-if="layouts.length > 1" class="flex gap-2 px-4 py-2 overflow-x-auto flex-shrink-0 bg-white border-b">
        <button
          v-for="l in layouts" :key="l.id"
          class="px-3 py-1.5 rounded text-sm whitespace-nowrap"
          :class="activeId === l.id ? 'bg-blue-600 text-white' : 'bg-gray-100'"
          @click="loadDetail(l.id)"
        >{{ l.name }}</button>
      </div>

      <!-- 캔버스 영역 (이 영역만 줌/스크롤) -->
      <div class="flex-1 relative overflow-hidden bg-gray-200">
        <div
          ref="viewerRef"
          class="absolute inset-0 overflow-auto"
          style="-webkit-overflow-scrolling: touch;"
        >
          <div
            :style="{
              width: (editorW * viewZoom) + 'px',
              height: (editorH * viewZoom) + 'px',
              position: 'relative',
            }"
          >
            <img
              v-if="activeLayout.backgroundImageUrl"
              :src="activeLayout.backgroundImageUrl"
              :style="{
                width: (editorW * viewZoom) + 'px',
                height: (editorH * viewZoom) + 'px',
                display: 'block',
              }"
              @load="onImageLoad"
            />

            <!-- 핀 -->
            <div
              v-for="pin in (layoutData?.pins || []).filter(p => p.userId)"
              :key="pin.id"
              class="absolute"
              :style="{
                left: (pin.x * viewZoom) + 'px',
                top: (pin.y * viewZoom) + 'px',
                transform: 'translate(-50%, -50%)',
              }"
              @click.stop="showPinInfo(pin)"
            >
              <template v-if="pin.userId === userId">
                <div class="flex flex-col items-center">
                  <div
                    class="rounded-full bg-red-500 border-2 border-white shadow-lg flex items-center justify-center animate-bounce"
                    :style="{ width: pinSize + 'px', height: pinSize + 'px' }"
                  >
                    <span class="text-white font-bold" :style="{ fontSize: (pinSize * 0.4) + 'px' }">{{ pin.userName?.charAt(0) }}</span>
                  </div>
                  <span
                    class="mt-0.5 px-1 py-px bg-red-500 text-white font-bold rounded whitespace-nowrap shadow"
                    :style="{ fontSize: Math.max(7, pinSize * 0.35) + 'px' }"
                  >내 자리</span>
                </div>
              </template>
              <template v-else>
                <div
                  class="rounded-full bg-blue-500 border-2 border-white shadow flex items-center justify-center"
                  :style="{ width: pinSize + 'px', height: pinSize + 'px' }"
                >
                  <span class="text-white font-bold" :style="{ fontSize: (pinSize * 0.4) + 'px' }">{{ pin.userName?.charAt(0) }}</span>
                </div>
              </template>
            </div>
          </div>
        </div>

        <!-- 줌 버튼 (캔버스 위 고정) -->
        <div class="absolute top-3 right-3 flex flex-col gap-1.5 z-30">
          <button class="w-10 h-10 bg-white rounded-full shadow-lg text-xl font-bold active:bg-gray-100" @click="zoomIn">+</button>
          <button class="w-10 h-10 bg-white rounded-full shadow-lg text-sm font-bold active:bg-gray-100" @click="zoomFit">맞춤</button>
          <button class="w-10 h-10 bg-white rounded-full shadow-lg text-xl font-bold active:bg-gray-100" @click="zoomOut">−</button>
        </div>
      </div>

      <!-- 하단 고정 -->
      <div class="bg-white border-t px-4 py-3 flex items-center gap-3 flex-shrink-0 safe-area-bottom">
        <div class="w-10 h-10 rounded-full bg-red-500 flex items-center justify-center flex-shrink-0 animate-pulse">
          <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
        </div>
        <div class="flex-1 min-w-0">
          <p class="font-bold text-gray-900 truncate">{{ mySeatInfo?.group || activeLayout?.name || '좌석 배치도' }}</p>
          <p v-if="mySeatInfo?.companions?.length > 0" class="text-xs text-gray-500 truncate">
            같은 그룹: {{ mySeatInfo.companions.join(', ') }}
          </p>
          <p v-else-if="!mySeatInfo" class="text-xs text-gray-400">내 좌석이 배정되지 않았습니다</p>
        </div>
        <button
          v-if="mySeatInfo"
          class="px-3 py-2 bg-red-500 text-white text-xs rounded-full font-semibold shadow active:scale-95 flex-shrink-0"
          @click="scrollToMyPin"
        >
          내 자리로
        </button>
      </div>
    </template>

    <!-- 핀 정보 모달 -->
    <SlideUpModal :is-open="!!tappedPin" @close="tappedPin = null">
      <template #header-title>좌석 정보</template>
      <template #body>
        <div v-if="tappedPin" class="space-y-3">
          <div class="flex items-center gap-3">
            <div class="w-12 h-12 rounded-full bg-blue-500 flex items-center justify-center text-white text-lg font-bold">
              {{ tappedPin.userName?.charAt(0) || '?' }}
            </div>
            <div>
              <p class="font-bold text-gray-900 text-lg">{{ tappedPin.userName || '미배정' }}</p>
              <p v-if="tappedPin.group" class="text-sm text-gray-500">{{ tappedPin.group }}</p>
            </div>
          </div>
          <div v-if="sameGroupPins.length > 1" class="border-t pt-3">
            <p class="text-sm font-medium text-gray-700 mb-2">같은 그룹 ({{ sameGroupPins.length }}명)</p>
            <div class="space-y-1">
              <div v-for="p in sameGroupPins" :key="p.id" class="text-sm text-gray-600 flex items-center gap-2">
                <span class="w-6 h-6 rounded-full flex items-center justify-center text-white text-[10px] font-bold" :class="p.userId === userId ? 'bg-red-500' : 'bg-blue-500'">{{ p.userName?.charAt(0) }}</span>
                <span>{{ p.userName }}</span>
                <span v-if="p.userId === userId" class="text-red-500 text-xs">(나)</span>
              </div>
            </div>
          </div>
        </div>
      </template>
    </SlideUpModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import apiClient from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import MainHeader from '@/components/common/MainHeader.vue'
import SlideUpModal from '@/components/common/SlideUpModal.vue'

const route = useRoute()
const authStore = useAuthStore()
const conventionId = Number(route.params.conventionId)
const userId = computed(() => authStore.user?.id)

const layouts = ref([])
const activeLayout = ref(null)
const activeId = ref(null)
const layoutData = ref(null)
const loading = ref(true)
const viewerRef = ref(null)
const viewZoom = ref(1)
const tappedPin = ref(null)
const imgNaturalW = ref(1200)
const imgNaturalH = ref(800)

const editorW = computed(() => {
  const m = Math.max(imgNaturalW.value, imgNaturalH.value)
  return imgNaturalW.value * (m > 4000 ? 4000 / m : 1)
})
const editorH = computed(() => {
  const m = Math.max(imgNaturalW.value, imgNaturalH.value)
  return imgNaturalH.value * (m > 4000 ? 4000 / m : 1)
})

// 핀 크기 (줌에 비례)
const pinSize = computed(() => Math.max(16, Math.round(24 * viewZoom.value)))

const mySeatInfo = computed(() => {
  if (!layoutData.value?.pins) return null
  const myPin = layoutData.value.pins.find((p) => p.userId === userId.value)
  if (!myPin) return null
  const companions = layoutData.value.pins
    .filter((p) => p.group && p.group === myPin.group && p.userId && p.userId !== userId.value)
    .map((p) => p.userName)
  return { group: myPin.group, companions, x: myPin.x, y: myPin.y }
})

const sameGroupPins = computed(() => {
  if (!tappedPin.value?.group || !layoutData.value?.pins) return []
  return layoutData.value.pins.filter((p) => p.group === tappedPin.value.group && p.userId)
})

function showPinInfo(pin) { tappedPin.value = pin }

const fitZoom = ref(1) // 초기 화면 맞춤 줌 저장

function zoomIn() { viewZoom.value = Math.min(5, +(viewZoom.value + 0.3).toFixed(1)) }
function zoomOut() { viewZoom.value = Math.max(0.2, +(viewZoom.value - 0.3).toFixed(1)) }
function zoomFit() {
  viewZoom.value = fitZoom.value
  // 초기 상태로 스크롤 리셋
  if (viewerRef.value) viewerRef.value.scrollTo({ left: 0, top: 0, behavior: 'smooth' })
}

function scrollToMyPin() {
  if (!mySeatInfo.value || !viewerRef.value) return
  const el = viewerRef.value
  el.scrollTo({
    left: mySeatInfo.value.x * viewZoom.value - el.clientWidth / 2,
    top: mySeatInfo.value.y * viewZoom.value - el.clientHeight / 2,
    behavior: 'smooth',
  })
}

async function load() {
  loading.value = true
  try {
    const param = route.query.layout
    if (param) await loadDetail(Number(param))
    try {
      const res = await apiClient.get(`/seating-layouts/my/${conventionId}`)
      layouts.value = res.data || []
    } catch { layouts.value = [] }
    if (!param && layouts.value.length > 0) await loadDetail(layouts.value[0].id)
  } finally { loading.value = false }
}

async function loadDetail(id) {
  activeId.value = id
  try {
    const res = await apiClient.get(`/seating-layouts/${id}/view`)
    activeLayout.value = res.data
    const parsed = JSON.parse(res.data.layoutJson || '{}')
    layoutData.value = parsed.pins ? parsed : { pins: [], groups: [] }
  } catch {
    activeLayout.value = null
    layoutData.value = { pins: [], groups: [] }
  }
}

function onImageLoad(e) {
  imgNaturalW.value = e.target.naturalWidth || 1200
  imgNaturalH.value = e.target.naturalHeight || 800
  // 화면에 맞추기 + 초기 줌 저장
  if (viewerRef.value) {
    const fitW = viewerRef.value.clientWidth / editorW.value
    const fitH = viewerRef.value.clientHeight / editorH.value
    const z = +Math.min(fitW, fitH, 1).toFixed(2)
    viewZoom.value = z
    fitZoom.value = z
  }
  setTimeout(scrollToMyPin, 500)
}

onMounted(load)
</script>

<style scoped>
.safe-area-bottom { padding-bottom: max(12px, env(safe-area-inset-bottom)); }
</style>
