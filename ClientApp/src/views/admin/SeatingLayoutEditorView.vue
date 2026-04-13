<template>
  <div class="h-screen h-dvh flex flex-col bg-gray-100 overflow-hidden">
    <!-- 상단 바 -->
    <div class="bg-white border-b px-3 py-1.5 flex items-center gap-2 text-sm print:hidden flex-shrink-0">
      <button class="p-1.5 hover:bg-gray-100 rounded" @click="$router.back()">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" /></svg>
      </button>
      <input v-if="layout" v-model="layout.name" class="font-bold text-base border-b border-transparent focus:border-gray-300 outline-none w-32 sm:w-48" @input="markDirty" />

      <!-- 도구 -->
      <div class="flex items-center gap-1 ml-2">
        <button class="px-2 py-1.5 rounded text-xs font-medium bg-blue-600 text-white" @click="tc.addTable('circle')">⊕ 원형</button>
        <button class="px-2 py-1.5 rounded text-xs font-medium bg-blue-600 text-white" @click="tc.addTable('rect')">⊞ 사각</button>
      </div>

      <div class="flex-1"></div>

      <!-- 엑셀 -->
      <button class="px-2 py-1.5 border rounded text-xs hover:bg-gray-50" @click="downloadMembers">📥 다운로드</button>
      <label class="px-2 py-1.5 border rounded text-xs hover:bg-gray-50 cursor-pointer">
        📤 업로드
        <input type="file" accept=".xlsx" class="hidden" @change="uploadMembers" />
      </label>

      <!-- 배경 -->
      <label class="px-2 py-1.5 border rounded text-xs hover:bg-gray-50 cursor-pointer">
        📷
        <input type="file" accept="image/*" class="hidden" @change="uploadBg" />
      </label>

      <!-- 저장 -->
      <button class="px-2 py-1.5 rounded text-xs font-medium" :class="isDirty ? 'bg-blue-600 text-white' : 'border text-gray-400'" @click="saveNow">💾</button>
      <span class="text-xs text-gray-400 hidden sm:inline">{{ saveStatus }}</span>
    </div>

    <div v-if="!layout" class="flex-1 flex items-center justify-center text-gray-400">로딩 중...</div>

    <div v-else class="flex-1 flex overflow-hidden">
      <!-- 캔버스 -->
      <div ref="containerRef" class="flex-1 overflow-hidden relative">
        <canvas ref="canvasEl"></canvas>
      </div>

      <!-- 우측 패널 -->
      <div class="w-72 sm:w-80 flex-shrink-0 print:hidden bg-white border-l overflow-y-auto">
        <div class="p-3 space-y-3">
          <!-- 선택된 테이블 -->
          <div v-if="selectedTable">
            <div class="flex items-center justify-between mb-2">
              <h3 class="font-bold text-gray-900">{{ selectedTable.number }}번 테이블</h3>
              <button class="text-xs text-red-500 hover:underline" @click="tc.deleteSelected(); markDirty()">삭제</button>
            </div>
            <div class="mb-2">
              <label class="text-xs text-gray-500">번호</label>
              <input v-model="selectedTable.number" type="text" class="w-full border rounded px-2 py-1.5 text-sm" @input="markDirty" />
            </div>

            <!-- 멤버 -->
            <div class="border-t pt-2">
              <h4 class="text-sm font-medium text-gray-700 mb-1">멤버 ({{ selectedTable.members?.length || 0 }}명)</h4>
              <div class="space-y-1 max-h-48 overflow-y-auto">
                <div v-for="m in selectedTable.members || []" :key="m.userId" class="flex items-center justify-between py-1 px-2 bg-gray-50 rounded text-sm">
                  <span>{{ m.name }}</span>
                  <div class="flex gap-1">
                    <button class="text-xs text-blue-500" @click="startMove(m)">이동</button>
                    <button class="text-xs text-red-400" @click="tc.removeMember(selectedTable.number, m.userId); markDirty()">×</button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div v-else class="text-gray-400 text-center text-sm py-8">테이블을 선택하세요</div>

          <!-- 전체 테이블 목록 -->
          <div class="border-t pt-3">
            <h4 class="text-sm font-medium text-gray-700 mb-2">전체 테이블</h4>
            <div class="space-y-1 max-h-60 overflow-y-auto">
              <div v-for="t in allTables" :key="t.id" class="flex items-center justify-between py-1.5 px-2 rounded text-sm hover:bg-gray-50 cursor-pointer" @click="selectTableByNumber(t.number)">
                <span class="font-medium">{{ t.number }}번</span>
                <span class="text-xs text-gray-500">{{ t.members?.length || 0 }}명</span>
              </div>
            </div>
          </div>

          <!-- 배정 현황 -->
          <div class="border-t pt-3">
            <div class="flex justify-between text-sm">
              <span class="text-gray-600">배정</span>
              <span class="font-bold">{{ assignedCount }}/{{ totalGuests }}명</span>
            </div>
            <div class="h-2 bg-gray-100 rounded-full mt-1 overflow-hidden">
              <div class="h-full bg-blue-500 rounded-full" :style="{ width: totalGuests ? `${(assignedCount / totalGuests) * 100}%` : '0%' }"></div>
            </div>
          </div>

          <!-- 미배정 참석자 (웹 배정) -->
          <div class="border-t pt-3">
            <h4 class="text-sm font-medium text-gray-700 mb-2">미배정 ({{ unassignedGuests.length }}명)</h4>
            <input v-model="guestSearch" type="text" placeholder="이름 검색..." class="w-full border rounded px-2 py-1.5 text-sm mb-2" />
            <div class="max-h-48 overflow-y-auto space-y-0.5">
              <div
                v-for="g in filteredUnassigned"
                :key="g.id"
                class="flex items-center justify-between py-1.5 px-2 rounded text-sm hover:bg-blue-50 cursor-pointer"
                @click="startWebAssign(g)"
              >
                <span class="font-medium truncate">{{ g.name }}</span>
                <span class="text-xs text-gray-400">{{ g.corpPart || g.groupName || '' }}</span>
              </div>
              <div v-if="filteredUnassigned.length === 0" class="text-xs text-gray-400 text-center py-3">
                {{ guestSearch ? '검색 결과 없음' : '모두 배정 완료' }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 웹 배정 모달 (사람 → 테이블 선택) -->
    <SlideUpModal :is-open="!!assigningGuest" @close="assigningGuest = null">
      <template #header-title>{{ assigningGuest?.name }} → 어느 테이블?</template>
      <template #body>
        <div class="space-y-1">
          <button
            v-for="t in allTables" :key="t.id"
            class="w-full text-left py-3 px-4 rounded-lg hover:bg-blue-50 flex items-center justify-between"
            @click="doWebAssign(t.number)"
          >
            <span class="font-medium">{{ t.number }}번 테이블</span>
            <span class="text-sm text-gray-500">{{ t.members?.length || 0 }}명</span>
          </button>
        </div>
      </template>
    </SlideUpModal>

    <!-- 멤버 이동 모달 -->
    <SlideUpModal :is-open="!!movingMember" @close="movingMember = null">
      <template #header-title>{{ movingMember?.name }} → 어디로?</template>
      <template #body>
        <div class="space-y-1">
          <button
            v-for="t in allTables" :key="t.id"
            class="w-full text-left py-3 px-4 rounded-lg hover:bg-blue-50 flex items-center justify-between"
            :class="t.number === selectedTable?.number ? 'bg-gray-100 text-gray-400' : ''"
            :disabled="t.number === selectedTable?.number"
            @click="doMove(t.number)"
          >
            <span class="font-medium">{{ t.number }}번 테이블</span>
            <span class="text-sm text-gray-500">{{ t.members?.length || 0 }}명</span>
          </button>
        </div>
      </template>
    </SlideUpModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useRoute, onBeforeRouteLeave } from 'vue-router'
import apiClient from '@/services/api'
import { useAutoSave } from '@/composables/useAutoSave'
import { useTableCanvas } from '@/components/admin/seating/useTableCanvas'
import SlideUpModal from '@/components/common/SlideUpModal.vue'

const route = useRoute()
const layoutId = Number(route.params.layoutId)
const conventionId = Number(route.params.id || route.params.conventionId)

const layout = ref(null)
const canvasEl = ref(null)
const containerRef = ref(null)
const guests = ref([])
const selectedTable = ref(null)
const movingMember = ref(null)
const assigningGuest = ref(null)
const guestSearch = ref('')
const isDirty = ref(false)

const tc = useTableCanvas(canvasEl, containerRef, {
  onModified: markDirty,
  onTableSelect: (t) => { selectedTable.value = t },
})

const allTables = computed(() => tc.toLayoutJSON().tables || [])
const assignedCount = computed(() => allTables.value.reduce((s, t) => s + (t.members?.length || 0), 0))
const totalGuests = computed(() => guests.value.length)

const assignedUserIds = computed(() => {
  const set = new Set()
  allTables.value.forEach(t => t.members?.forEach(m => set.add(m.userId)))
  return set
})
const unassignedGuests = computed(() => guests.value.filter(g => !assignedUserIds.value.has(g.id)))
const filteredUnassigned = computed(() => {
  const q = guestSearch.value.trim().toLowerCase()
  const list = unassignedGuests.value
  if (!q) return list.slice(0, 100)
  return list.filter(g => g.name?.toLowerCase().includes(q) || g.corpPart?.toLowerCase().includes(q)).slice(0, 100)
})

// 자동 저장
const autoSave = useAutoSave(async () => {
  const json = tc.toLayoutJSON()
  await apiClient.put(`/admin/seating-layouts/${layoutId}`, { name: layout.value.name, layoutJson: JSON.stringify(json) })
  isDirty.value = false
}, { delay: 1500 })

const saveStatus = computed(() => {
  if (autoSave.saving.value) return '저장 중...'
  if (autoSave.lastSavedAt.value) return autoSave.lastSavedAt.value.toLocaleTimeString('ko-KR', { hour: '2-digit', minute: '2-digit' }) + ' 저장됨'
  return ''
})

function markDirty() { isDirty.value = true; autoSave.trigger() }

async function saveNow() {
  if (!layout.value) return
  const json = tc.toLayoutJSON()
  await apiClient.put(`/admin/seating-layouts/${layoutId}`, { name: layout.value.name, layoutJson: JSON.stringify(json) })
  isDirty.value = false
}

// 배경
async function uploadBg(e) {
  const file = e.target.files[0]; if (!file) return
  const fd = new FormData(); fd.append('file', file)
  const res = await apiClient.post(`/admin/seating-layouts/${layoutId}/background`, fd, { headers: { 'Content-Type': 'multipart/form-data' } })
  layout.value.backgroundImageUrl = res.data.backgroundImageUrl
  tc.setBackground(res.data.backgroundImageUrl)
  e.target.value = ''
}

// 엑셀
async function downloadMembers() {
  const res = await apiClient.get(`/admin/seating-layouts/${layoutId}/members/download`, { responseType: 'blob' })
  const url = URL.createObjectURL(new Blob([res.data]))
  const a = document.createElement('a'); a.href = url; a.download = `좌석배정.xlsx`; a.click(); URL.revokeObjectURL(url)
}

async function uploadMembers(e) {
  const file = e.target.files[0]; if (!file) return
  const fd = new FormData(); fd.append('file', file)
  try {
    const res = await apiClient.post(`/admin/seating-layouts/${layoutId}/members/upload`, fd, { headers: { 'Content-Type': 'multipart/form-data' } })
    alert(`${res.data.tableCount}개 테이블에 ${res.data.matched}명 배정 완료` + (res.data.unmatched > 0 ? ` (${res.data.unmatched}명 매칭 실패)` : ''))
    // 데이터 리로드
    const layoutRes = await apiClient.get(`/admin/seating-layouts/${layoutId}`)
    layout.value = layoutRes.data
    tc.loadLayoutJSON(JSON.parse(layout.value.layoutJson || '{"tables":[]}'))
  } catch (err) {
    alert(err.response?.data?.message || '업로드 실패')
  }
  e.target.value = ''
}

// 웹 배정 (미배정 → 테이블 선택)
function startWebAssign(guest) { assigningGuest.value = guest }
function doWebAssign(tableNum) {
  if (!assigningGuest.value) return
  tc.addMemberToTable(tableNum, assigningGuest.value)
  assigningGuest.value = null
  markDirty()
}

// 멤버 이동
function startMove(member) { movingMember.value = member }
function doMove(toNum) {
  if (!movingMember.value || !selectedTable.value) return
  tc.moveMember(movingMember.value.userId, selectedTable.value.number, toNum)
  movingMember.value = null
  markDirty()
}

function selectTableByNumber(num) {
  // 캔버스에서 해당 테이블 선택
  const cvs = tc.canvas()
  if (!cvs) return
  const obj = cvs.getObjects().find(o => o._tableData?.number === num)
  if (obj) { cvs.setActiveObject(obj); cvs.requestRenderAll() }
}

// 키보드
function onKeyDown(e) {
  if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') return
  tc.handleKeyDown(e)
  if (e.key === 'Delete') { tc.deleteSelected(); markDirty() }
  else if (e.ctrlKey && e.key === 's') { e.preventDefault(); saveNow() }
}
function onKeyUp(e) { tc.handleKeyUp(e) }

onBeforeRouteLeave(async (to, from, next) => { if (isDirty.value) await saveNow(); next() })

onMounted(async () => {
  const res = await apiClient.get(`/admin/seating-layouts/${layoutId}`)
  layout.value = res.data
  try { const r2 = await apiClient.get(`/admin/conventions/${conventionId}/guests`); guests.value = Array.isArray(r2.data) ? r2.data : [] } catch { guests.value = [] }

  await nextTick(); await new Promise(r => setTimeout(r, 100))
  tc.init()
  if (layout.value.backgroundImageUrl) tc.setBackground(layout.value.backgroundImageUrl)
  try {
    const parsed = JSON.parse(layout.value.layoutJson || '{"tables":[]}')
    tc.loadLayoutJSON(parsed.tables ? parsed : { tables: [] })
  } catch {}

  window.addEventListener('keydown', onKeyDown)
  window.addEventListener('keyup', onKeyUp)
})
onUnmounted(() => { tc.dispose(); window.removeEventListener('keydown', onKeyDown); window.removeEventListener('keyup', onKeyUp) })
</script>
