<template>
  <div class="fixed inset-0 flex flex-col bg-gray-50">
    <MainHeader title="내 자리" :show-back="true" />

    <div v-if="loading" class="flex-1 flex items-center justify-center text-gray-500">로딩 중...</div>
    <div v-else-if="!activeLayout" class="flex-1 flex items-center justify-center text-gray-400 px-4 text-center">배정된 자리가 없습니다.</div>

    <template v-else>
      <div v-if="layouts.length > 1" class="flex gap-2 px-4 py-2 overflow-x-auto flex-shrink-0 bg-white border-b">
        <button v-for="l in layouts" :key="l.id" class="px-3 py-1.5 rounded text-sm whitespace-nowrap" :class="activeId === l.id ? 'bg-blue-600 text-white' : 'bg-gray-100'" @click="loadDetail(l.id)">{{ l.name }}</button>
      </div>

      <!-- 도식도 -->
      <div class="flex-1 relative overflow-hidden bg-gray-200">
        <div ref="viewerRef" class="absolute inset-0 overflow-auto overscroll-contain" style="-webkit-overflow-scrolling: touch;">
          <div :style="{ width: (imgW * viewZoom) + 'px', height: (imgH * viewZoom) + 'px', position: 'relative' }">
            <img v-if="activeLayout.backgroundImageUrl" :src="activeLayout.backgroundImageUrl" :style="{ width: (imgW * viewZoom) + 'px', height: (imgH * viewZoom) + 'px', display: 'block' }" @load="onImageLoad" />

            <!-- 테이블 마커 -->
            <div
              v-for="t in layoutData?.tables || []"
              :key="t.id"
              class="absolute flex items-center justify-center cursor-pointer transition-transform"
              :style="{
                left: (t.x * viewZoom) + 'px', top: (t.y * viewZoom) + 'px',
                width: ((t.width || 80) * viewZoom) + 'px', height: ((t.height || 80) * viewZoom) + 'px',
                transform: 'translate(-50%, -50%)',
              }"
              @click="showTableMembers(t)"
            >
              <div
                class="w-full h-full flex items-center justify-center shadow-lg border-2 border-white"
                :class="[
                  t.shape === 'rect' ? 'rounded-lg' : 'rounded-full',
                  t.number === myTableNumber ? 'animate-bounce' : '',
                ]"
                :style="{ backgroundColor: t.number === myTableNumber ? '#ef4444' : (t.color || '#3b82f6'), opacity: 0.9 }"
              >
                <span class="text-white font-bold drop-shadow" :style="{ fontSize: Math.max(12, 20 * viewZoom) + 'px' }">{{ t.number }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- 줌 -->
        <div class="absolute top-3 right-3 flex flex-col gap-1.5 z-30">
          <button class="w-10 h-10 bg-white rounded-full shadow-lg text-xl font-bold active:bg-gray-100" @click="viewZoom = Math.min(5, +(viewZoom + 0.3).toFixed(1))">+</button>
          <button class="w-10 h-10 bg-white rounded-full shadow-lg text-sm font-bold active:bg-gray-100" @click="zoomFit">맞춤</button>
          <button class="w-10 h-10 bg-white rounded-full shadow-lg text-xl font-bold active:bg-gray-100" @click="viewZoom = Math.max(0.2, +(viewZoom - 0.3).toFixed(1))">−</button>
        </div>
      </div>

      <!-- 하단 -->
      <div class="bg-white border-t px-4 py-3 flex items-center gap-3 flex-shrink-0 safe-area-bottom">
        <div class="w-10 h-10 rounded-full bg-red-500 flex items-center justify-center flex-shrink-0 animate-pulse">
          <span class="text-white font-bold text-lg">{{ myTableNumber || '?' }}</span>
        </div>
        <div class="flex-1 min-w-0">
          <p class="font-bold text-gray-900">{{ myTableNumber ? myTableNumber + '번 테이블' : '미배정' }}</p>
          <p v-if="myTableMembers.length > 0" class="text-xs text-gray-500 truncate">{{ myTableMembers.join(', ') }}</p>
        </div>
        <button v-if="myTableNumber" class="px-3 py-2 bg-red-500 text-white text-xs rounded-full font-semibold shadow active:scale-95 flex-shrink-0" @click="scrollToMyTable">내 자리로</button>
      </div>
    </template>

    <!-- 테이블 멤버 모달 -->
    <SlideUpModal :is-open="!!tappedTable" @close="tappedTable = null">
      <template #header-title>{{ tappedTable?.number }}번 테이블 ({{ tappedTable?.members?.length || 0 }}명)</template>
      <template #body>
        <div v-if="tappedTable" class="space-y-1">
          <div v-for="m in tappedTable.members || []" :key="m.userId" class="flex items-center gap-3 py-2 px-3 rounded-lg" :class="m.userId === userId ? 'bg-red-50' : 'hover:bg-gray-50'">
            <span class="w-8 h-8 rounded-full flex items-center justify-center text-white text-sm font-bold" :class="m.userId === userId ? 'bg-red-500' : 'bg-blue-500'">{{ m.name?.charAt(0) }}</span>
            <span class="font-medium">{{ m.name }}</span>
            <span v-if="m.userId === userId" class="text-red-500 text-xs ml-auto">(나)</span>
          </div>
          <div v-if="!tappedTable.members?.length" class="text-center text-gray-400 py-4">배정된 사람이 없습니다</div>
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
const fitZoomVal = ref(1)
const tappedTable = ref(null)
const imgW = ref(1200)
const imgH = ref(800)

const myTableNumber = computed(() => {
  if (!layoutData.value?.tables) return null
  for (const t of layoutData.value.tables) {
    if (t.members?.some(m => m.userId === userId.value)) return t.number
  }
  return null
})

const myTableMembers = computed(() => {
  if (!myTableNumber.value || !layoutData.value?.tables) return []
  const t = layoutData.value.tables.find(t => t.number === myTableNumber.value)
  return t?.members?.filter(m => m.userId !== userId.value).map(m => m.name) || []
})

const myTablePos = computed(() => {
  if (!myTableNumber.value || !layoutData.value?.tables) return null
  const t = layoutData.value.tables.find(t => t.number === myTableNumber.value)
  return t ? { x: t.x, y: t.y } : null
})

function showTableMembers(t) { tappedTable.value = t }

function zoomFit() {
  viewZoom.value = fitZoomVal.value
  if (viewerRef.value) viewerRef.value.scrollTo({ left: 0, top: 0, behavior: 'smooth' })
}

function scrollToMyTable() {
  if (!myTablePos.value || !viewerRef.value) return
  const el = viewerRef.value
  el.scrollTo({
    left: myTablePos.value.x * viewZoom.value - el.clientWidth / 2,
    top: myTablePos.value.y * viewZoom.value - el.clientHeight / 2,
    behavior: 'smooth',
  })
}

async function load() {
  loading.value = true
  try {
    const param = route.query.layout
    if (param) await loadDetail(Number(param))
    try { const res = await apiClient.get(`/seating-layouts/my/${conventionId}`); layouts.value = res.data || [] } catch { layouts.value = [] }
    if (!param && layouts.value.length > 0) await loadDetail(layouts.value[0].id)
  } finally { loading.value = false }
}

async function loadDetail(id) {
  activeId.value = id
  try {
    const res = await apiClient.get(`/seating-layouts/${id}/view`)
    activeLayout.value = res.data
    const parsed = JSON.parse(res.data.layoutJson || '{"tables":[]}')
    layoutData.value = parsed.tables ? parsed : { tables: [] }
  } catch { activeLayout.value = null; layoutData.value = { tables: [] } }
}

function onImageLoad(e) {
  imgW.value = e.target.naturalWidth || 1200
  imgH.value = e.target.naturalHeight || 800
  const m = Math.max(imgW.value, imgH.value)
  const s = m > 4000 ? 4000 / m : 1
  imgW.value *= s; imgH.value *= s
  if (viewerRef.value) {
    const fw = viewerRef.value.clientWidth / imgW.value
    const fh = viewerRef.value.clientHeight / imgH.value
    const z = +Math.min(fw, fh, 1).toFixed(2)
    viewZoom.value = z; fitZoomVal.value = z
  }
  setTimeout(scrollToMyTable, 500)
}

// 핀치줌
onMounted(() => {
  load()
  setTimeout(() => {
    const el = viewerRef.value; if (!el) return
    let lastDist = 0, lastZoom = 1
    el.addEventListener('touchstart', e => { if (e.touches.length === 2) { e.preventDefault(); const dx = e.touches[0].clientX - e.touches[1].clientX, dy = e.touches[0].clientY - e.touches[1].clientY; lastDist = Math.hypot(dx, dy); lastZoom = viewZoom.value } }, { passive: false })
    el.addEventListener('touchmove', e => { if (e.touches.length === 2 && lastDist) { e.preventDefault(); const dx = e.touches[0].clientX - e.touches[1].clientX, dy = e.touches[0].clientY - e.touches[1].clientY; viewZoom.value = Math.min(5, Math.max(0.2, +(lastZoom * Math.hypot(dx, dy) / lastDist).toFixed(2))) } }, { passive: false })
    el.addEventListener('touchend', () => { lastDist = 0 }, { passive: true })
  }, 1000)
})
</script>

<style scoped>
.safe-area-bottom { padding-bottom: max(12px, env(safe-area-inset-bottom)); }
</style>
