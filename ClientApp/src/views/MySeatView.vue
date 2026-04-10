<template>
  <div class="min-h-screen bg-gray-50">
    <MainHeader title="내 자리" :show-back="true" />
    <div class="max-w-4xl mx-auto p-4">
      <div v-if="loading" class="text-center py-12 text-gray-500">로딩 중...</div>
      <div v-else-if="layouts.length === 0 && !activeLayout" class="bg-white rounded p-8 text-center text-gray-400">배정된 자리가 없습니다.</div>
      <div v-else>
        <div v-if="layouts.length > 1" class="flex gap-2 mb-3 overflow-x-auto">
          <button
            v-for="l in layouts" :key="l.id"
            class="px-3 py-1.5 rounded text-sm whitespace-nowrap"
            :class="activeId === l.id ? 'bg-blue-600 text-white' : 'bg-white border'"
            @click="loadDetail(l.id)"
          >{{ l.name }}</button>
        </div>

        <div v-if="activeLayout" class="bg-white rounded-lg shadow overflow-hidden relative">
          <!-- 이미지 + 핀 오버레이 (Fabric 없이 순수 HTML) -->
          <div
            ref="viewerRef"
            class="overflow-auto overscroll-contain"
            style="height: 75vh; -webkit-overflow-scrolling: touch;"
          >
            <div
              class="relative inline-block min-w-full"
              :style="{ transform: `scale(${viewZoom})`, transformOrigin: '0 0' }"
            >
              <!-- 배경 이미지 -->
              <img
                v-if="activeLayout.backgroundImageUrl"
                :src="activeLayout.backgroundImageUrl"
                class="block max-w-none"
                @load="onImageLoad"
              />
              <div
                v-else
                class="bg-gray-100"
                style="width: 1200px; height: 800px;"
              ></div>

              <!-- 핀 마커 오버레이 -->
              <div
                v-for="pin in layoutData?.pins || []"
                :key="pin.id"
                class="absolute flex flex-col items-center"
                :style="{ left: pin.x + 'px', top: pin.y + 'px', transform: 'translate(-50%, -50%)' }"
              >
                <div
                  class="rounded-full flex items-center justify-center text-white text-[9px] font-bold shadow-md border-2 border-white"
                  :class="[
                    pin.userId === userId ? 'w-8 h-8 bg-red-500 animate-bounce' : 'w-6 h-6',
                    pin.userId ? (pin.userId === userId ? '' : 'bg-blue-500') : 'bg-gray-300'
                  ]"
                >
                  {{ pin.userName ? pin.userName.substring(0, 1) : '' }}
                </div>
                <!-- 본인 자리 라벨 -->
                <span
                  v-if="pin.userId === userId"
                  class="mt-0.5 px-1.5 py-0.5 bg-red-500 text-white text-[10px] font-bold rounded whitespace-nowrap shadow"
                >
                  내 자리
                </span>
                <!-- 다른 사람 이름 -->
                <span
                  v-else-if="pin.userName"
                  class="mt-0.5 text-[8px] text-gray-600 whitespace-nowrap"
                >
                  {{ pin.userName.substring(0, 3) }}
                </span>
              </div>
            </div>
          </div>

          <!-- 하단 플로팅 -->
          <div
            v-if="mySeatInfo"
            class="sticky bottom-0 bg-white/95 backdrop-blur border-t px-4 py-3 flex items-center gap-3"
          >
            <div class="w-10 h-10 rounded-full bg-red-500 flex items-center justify-center flex-shrink-0 animate-pulse">
              <svg class="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
            </div>
            <div class="flex-1">
              <p class="font-bold text-gray-900">{{ mySeatInfo.group || '내 자리' }}</p>
              <p v-if="mySeatInfo.companions.length > 0" class="text-xs text-gray-500">
                같은 그룹: {{ mySeatInfo.companions.join(', ') }}
              </p>
            </div>
            <button
              class="px-3 py-1.5 bg-red-500 text-white text-xs rounded-full font-medium shadow hover:bg-red-600 active:scale-95 transition-all"
              @click="scrollToMyPin"
            >
              내 자리로
            </button>
          </div>

          <!-- 줌 컨트롤 -->
          <div class="absolute top-3 right-3 flex flex-col gap-1 z-10">
            <button class="w-8 h-8 bg-white/90 rounded shadow text-lg font-bold" @click="viewZoom = Math.min(3, viewZoom + 0.25)">+</button>
            <button class="w-8 h-8 bg-white/90 rounded shadow text-lg font-bold" @click="viewZoom = Math.max(0.5, viewZoom - 0.25)">−</button>
            <button class="w-8 h-8 bg-white/90 rounded shadow text-[10px]" @click="viewZoom = 1">1:1</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import apiClient from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import MainHeader from '@/components/common/MainHeader.vue'

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

const mySeatInfo = computed(() => {
  if (!layoutData.value?.pins) return null
  const myPin = layoutData.value.pins.find((p) => p.userId === userId.value)
  if (!myPin) return null
  const companions = layoutData.value.pins
    .filter((p) => p.group && p.group === myPin.group && p.userId && p.userId !== userId.value)
    .map((p) => p.userName)
  return { group: myPin.group, companions, x: myPin.x, y: myPin.y }
})

async function load() {
  loading.value = true
  try {
    const res = await apiClient.get(`/seating-layouts/my/${conventionId}`)
    layouts.value = res.data
    const param = route.query.layout
    if (param) await loadDetail(Number(param))
    else if (layouts.value.length > 0) await loadDetail(layouts.value[0].id)
  } finally { loading.value = false }
}

async function loadDetail(id) {
  activeId.value = id
  const res = await apiClient.get(`/seating-layouts/${id}/view`)
  activeLayout.value = res.data
  try {
    const parsed = JSON.parse(res.data.layoutJson || '{}')
    if (parsed.pins) layoutData.value = parsed
    else layoutData.value = { pins: [], groups: [] }
  } catch { layoutData.value = { pins: [], groups: [] } }

  // 본인 핀으로 자동 스크롤
  await nextTick()
  setTimeout(scrollToMyPin, 500)
}

function onImageLoad() {
  // 이미지 로드 완료 후 자동 스크롤
  setTimeout(scrollToMyPin, 200)
}

function scrollToMyPin() {
  if (!mySeatInfo.value || !viewerRef.value) return
  const { x, y } = mySeatInfo.value
  const el = viewerRef.value
  const z = viewZoom.value
  el.scrollLeft = x * z - el.clientWidth / 2
  el.scrollTop = y * z - el.clientHeight / 2
}

onMounted(load)
</script>
