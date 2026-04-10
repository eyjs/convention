<template>
  <div class="h-screen flex flex-col bg-gray-100">
    <PinEditorToolbar
      :name="layout?.name || ''"
      :mode="pc.mode.value"
      :zoom="pc.zoom.value"
      :can-undo="history.canUndo.value"
      :can-redo="history.canRedo.value"
      :save-status="saveStatus"
      :has-bg="!!layout?.backgroundImageUrl"
      :bg-locked="pc.bgLocked.value"
      @toggle-bg-lock="pc.toggleBgLock"
      @update:name="(v) => { if (layout) { layout.name = v; markDirty() } }"
      @mode="pc.setMode"
      @upload-bg="uploadBg"
      @undo="undo"
      @redo="redo"
      @zoom="pc.setZoom"
      @zoom-fit="pc.zoomToFit"
      @export-png="pc.downloadPNG"
    />

    <div v-if="!layout" class="flex-1 flex items-center justify-center text-gray-400">로딩 중...</div>

    <div v-else class="flex-1 flex overflow-hidden">
      <!-- 캔버스 -->
      <div ref="containerRef" class="flex-1 overflow-hidden relative">
        <canvas ref="canvasEl"></canvas>
      </div>

      <!-- 우측 패널 -->
      <div class="w-72 flex-shrink-0 print:hidden">
        <PinPropertyPanel
          :selected-pin="selectedPin"
          :guests="guests"
          :assigned-user-ids="assignedUserIds"
          @assign="openAssign"
          @clear-user="(id) => { pc.clearUser(id); markDirty() }"
          @set-group="({ pinId, group }) => { pc.setGroup(pinId, group); markDirty() }"
          @delete="() => { pc.deleteSelected(); markDirty() }"
          @quick-assign="quickAssign"
        />
      </div>
    </div>

    <!-- 하단 상태바 -->
    <div class="bg-white border-t px-4 py-1 flex items-center gap-4 text-xs text-gray-500 print:hidden">
      <span>줌: {{ pc.zoom.value }}%</span>
      <span class="text-gray-300">|</span>
      <span>핀: {{ pinCount }}개</span>
      <span class="text-gray-300">|</span>
      <span>배정: {{ assignedCount }}/{{ guests.length }}명</span>
    </div>

    <AssignAttendeeModal
      :is-open="assignModal.open"
      :guests="guests"
      :assigned-user-ids="assignedUserIds"
      @close="assignModal.open = false"
      @select="onAssignSelect"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, reactive, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import apiClient from '@/services/api'
import { useAutoSave } from '@/composables/useAutoSave'
import { useHistory } from '@/composables/useHistory'
import { usePinCanvas } from '@/components/admin/seating/usePinCanvas'
import PinEditorToolbar from '@/components/admin/seating/PinEditorToolbar.vue'
import PinPropertyPanel from '@/components/admin/seating/PinPropertyPanel.vue'
import AssignAttendeeModal from '@/components/admin/seating/AssignAttendeeModal.vue'

const route = useRoute()
const layoutId = Number(route.params.layoutId)
const conventionId = Number(route.params.id || route.params.conventionId)

const layout = ref(null)
const canvasEl = ref(null)
const containerRef = ref(null)
const guests = ref([])
const selectedPin = ref(null)
const assignModal = reactive({ open: false, pinId: null })
const history = useHistory(50)

const pc = usePinCanvas(canvasEl, containerRef, {
  onModified: markDirty,
  onPinSelect: (pin) => { selectedPin.value = pin },
})

const dirtyCounter = ref(0) // 변경 시마다 증가 → computed 재평가 트리거
const assignedUserIds = computed(() => {
  dirtyCounter.value // 의존성 등록
  const set = new Set()
  const data = pc.toLayoutJSON()
  data.pins.forEach((p) => { if (p.userId) set.add(p.userId) })
  return set
})
const assignedCount = computed(() => { dirtyCounter.value; return assignedUserIds.value.size })
const pinCount = computed(() => { dirtyCounter.value; return pc.toLayoutJSON().pins.length })

// 자동 저장
const autoSave = useAutoSave(async () => {
  const json = pc.toLayoutJSON()
  await apiClient.put(`/admin/seating-layouts/${layoutId}`, {
    name: layout.value.name,
    layoutJson: JSON.stringify(json),
  })
}, { delay: 3000 })

const saveStatus = computed(() => {
  if (autoSave.saving.value) return '저장 중...'
  if (autoSave.lastSavedAt.value) return autoSave.lastSavedAt.value.toLocaleTimeString('ko-KR', { hour: '2-digit', minute: '2-digit' }) + ' 저장됨'
  return ''
})

let historyTimer = null
function markDirty() {
  dirtyCounter.value++
  autoSave.trigger()
  if (historyTimer) clearTimeout(historyTimer)
  historyTimer = setTimeout(() => history.push(pc.toLayoutJSON()), 500)
}

function undo() {
  const snap = history.undo()
  if (snap) { pc.loadLayoutJSON(snap); autoSave.trigger() }
}
function redo() {
  const snap = history.redo()
  if (snap) { pc.loadLayoutJSON(snap); autoSave.trigger() }
}

// 배경
async function uploadBg(e) {
  const file = e.target.files[0]
  if (!file) return
  const fd = new FormData()
  fd.append('file', file)
  const res = await apiClient.post(`/admin/seating-layouts/${layoutId}/background`, fd, { headers: { 'Content-Type': 'multipart/form-data' } })
  layout.value.backgroundImageUrl = res.data.backgroundImageUrl
  pc.setBackground(res.data.backgroundImageUrl)
  e.target.value = ''
}

// 좌석 배정
function openAssign(pin) {
  assignModal.pinId = pin.id
  assignModal.open = true
}

function onAssignSelect(guest) {
  if (!assignModal.pinId) return
  pc.assignUser(assignModal.pinId, guest)
  assignModal.open = false
  selectedPin.value = null
  markDirty()
}

function quickAssign(guest) {
  // 선택된 핀이 있고 미배정이면 바로 배정
  if (selectedPin.value && !selectedPin.value.userId) {
    pc.assignUser(selectedPin.value.id, guest)
    selectedPin.value = null
    markDirty()
  }
}

// 키보드
function onKeyDown(e) {
  if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') return
  pc.handleKeyDown(e)
  if (e.key === 'Delete' || e.key === 'Backspace') { pc.deleteSelected(); markDirty() }
  else if (e.ctrlKey && e.key === 'z' && !e.shiftKey) { e.preventDefault(); undo() }
  else if ((e.ctrlKey && e.key === 'y') || (e.ctrlKey && e.shiftKey && e.key === 'Z')) { e.preventDefault(); redo() }
  else if (e.key === 'v' && !e.ctrlKey) pc.setMode('select')
  else if (e.key === 'p' && !e.ctrlKey) pc.setMode('pin')
  else if (e.key === 'h' && !e.ctrlKey) pc.setMode('pan')
  else if (e.key === 'Escape') pc.setMode('select')
}

function onKeyUp(e) { pc.handleKeyUp(e) }

onMounted(async () => {
  const res = await apiClient.get(`/admin/seating-layouts/${layoutId}`)
  layout.value = res.data

  try {
    const r2 = await apiClient.get(`/admin/conventions/${conventionId}/guests`)
    guests.value = Array.isArray(r2.data) ? r2.data : []
  } catch { guests.value = [] }

  await nextTick()
  await new Promise((r) => setTimeout(r, 100))
  pc.init()

  if (layout.value.backgroundImageUrl) {
    pc.setBackground(layout.value.backgroundImageUrl)
  }

  // 데이터 로드
  try {
    const parsed = JSON.parse(layout.value.layoutJson || '{}')
    // 새 구조 (pins) 또는 레거시 (tables)
    if (parsed.pins) {
      pc.loadLayoutJSON(parsed)
    } else if (parsed.tables) {
      const converted = pc.loadLegacyLayout(parsed)
      pc.loadLayoutJSON(converted)
    }
    history.push(pc.toLayoutJSON())
  } catch {}

  window.addEventListener('keydown', onKeyDown)
  window.addEventListener('keyup', onKeyUp)
})

onUnmounted(() => {
  pc.dispose()
  window.removeEventListener('keydown', onKeyDown)
  window.removeEventListener('keyup', onKeyUp)
})
</script>

<style>
@media print { body * { visibility: hidden !important; } .canvas-container, .canvas-container * { visibility: visible !important; } .canvas-container { position: absolute !important; left: 0 !important; top: 0 !important; } }
</style>
