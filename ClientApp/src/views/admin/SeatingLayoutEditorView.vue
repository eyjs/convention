<template>
  <div class="h-screen h-dvh flex flex-col bg-gray-100 overflow-hidden">
    <!-- 상단 바 -->
    <div class="bg-white border-b px-3 py-1.5 flex items-center gap-2 text-sm print:hidden flex-shrink-0">
      <button class="p-1.5 hover:bg-gray-100 rounded" @click="$router.back()">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" /></svg>
      </button>
      <input v-if="layout" v-model="layout.name" class="font-bold text-base border-b border-transparent focus:border-gray-300 outline-none w-32 sm:w-48" @input="markDirty" />

      <!-- 테이블 추가 (PC만) -->
      <div class="hidden md:flex items-center gap-1 ml-2">
        <button class="px-2.5 py-1.5 rounded text-xs font-medium bg-blue-600 text-white hover:bg-blue-700" @click="tc.addTable('circle')">○ 원형 추가</button>
        <button class="px-2.5 py-1.5 rounded text-xs font-medium bg-blue-600 text-white hover:bg-blue-700" @click="tc.addTable('rect')">□ 사각 추가</button>
      </div>

      <div class="flex-1"></div>

      <!-- 액션 버튼 (통일 스타일) -->
      <button class="h-9 px-3 border rounded-lg text-xs hover:bg-gray-50 inline-flex items-center gap-1.5" @click="downloadMembers">
        <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
        <span class="hidden sm:inline">다운로드</span>
      </button>
      <label class="h-9 px-3 border rounded-lg text-xs hover:bg-gray-50 cursor-pointer inline-flex items-center gap-1.5">
        <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" /></svg>
        <span class="hidden sm:inline">업로드</span>
        <input type="file" accept=".xlsx" class="hidden" @change="uploadMembers" />
      </label>
      <label class="h-9 px-3 border rounded-lg text-xs hover:bg-gray-50 cursor-pointer inline-flex items-center gap-1.5">
        <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
        <span class="hidden sm:inline">배경</span>
        <input type="file" accept="image/*" class="hidden" @change="uploadBg" />
      </label>
      <button class="h-9 px-3 rounded-lg text-xs font-medium inline-flex items-center gap-1.5" :class="isDirty ? 'bg-blue-600 text-white' : 'border text-gray-400'" @click="saveNow">
        <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4" /></svg>
        <span class="hidden sm:inline">저장</span>
      </button>
      <span class="text-xs text-gray-400 hidden lg:inline">{{ saveStatus }}</span>
    </div>

    <div v-if="!layout" class="flex-1 flex items-center justify-center text-gray-400">로딩 중...</div>

    <div v-else class="flex-1 flex overflow-hidden">
      <!-- ===== 모바일: 카드 리스트 ===== -->
      <div class="md:hidden flex-1 overflow-y-auto p-3 space-y-3">
        <!-- 배정 현황 -->
        <div class="bg-white rounded-lg p-3 shadow-sm">
          <div class="flex justify-between text-sm mb-1">
            <span class="text-gray-600">배정 현황</span>
            <span class="font-bold">{{ assignedCount }}/{{ totalGuests }}명</span>
          </div>
          <div class="h-2 bg-gray-100 rounded-full overflow-hidden">
            <div class="h-full bg-blue-500 rounded-full" :style="{ width: totalGuests ? `${(assignedCount / totalGuests) * 100}%` : '0%' }"></div>
          </div>
        </div>

        <!-- 테이블 카드 -->
        <div
          v-for="t in allTables" :key="'mc-' + t.id"
          class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden"
        >
          <div class="px-4 py-3 flex items-center justify-between border-b border-gray-50">
            <div class="flex items-center gap-2">
              <span class="w-8 h-8 rounded-full bg-gray-100 flex items-center justify-center font-bold text-gray-800 text-sm">{{ t.number }}</span>
              <span class="font-semibold text-gray-900">{{ t.number }}번 테이블</span>
            </div>
            <span class="text-xs text-gray-500">{{ t.members?.length || 0 }}명</span>
          </div>
          <div class="px-4 py-2">
            <div v-if="!t.members?.length" class="text-xs text-gray-400 py-2">배정된 사람 없음</div>
            <div v-for="m in t.members || []" :key="m.userId" class="flex items-center justify-between py-1.5">
              <span class="text-sm text-gray-800">{{ m.name }}</span>
              <div class="flex gap-2">
                <button class="text-xs text-blue-500 min-h-[32px] px-2" @click="startMoveFromCard(t, m)">이동</button>
                <button class="text-xs text-red-400 min-h-[32px] px-2" @click="removeMemberFromCard(t.number, m.userId)">삭제</button>
              </div>
            </div>
          </div>
          <button class="w-full py-2 text-xs text-blue-600 border-t border-gray-50 hover:bg-blue-50" @click="addMemberToCard(t)">+ 사람 추가</button>
        </div>

        <!-- 미배정 -->
        <div v-if="unassignedGuests.length > 0" class="bg-amber-50 rounded-xl p-3">
          <h4 class="text-sm font-semibold text-amber-800 mb-2">미배정 ({{ unassignedGuests.length }}명)</h4>
          <div class="flex flex-wrap gap-1">
            <span v-for="g in unassignedGuests.slice(0, 20)" :key="g.id" class="px-2 py-1 bg-white rounded text-xs text-gray-700 shadow-sm">{{ g.name }}</span>
            <span v-if="unassignedGuests.length > 20" class="px-2 py-1 text-xs text-amber-600">+{{ unassignedGuests.length - 20 }}명</span>
          </div>
        </div>
      </div>

      <!-- ===== PC: 캔버스 + 우측 패널 ===== -->
      <div ref="containerRef" class="hidden md:block flex-1 overflow-hidden relative">
        <canvas ref="canvasEl"></canvas>
      </div>

      <!-- 우측 패널 (PC만) -->
      <div class="hidden md:block w-72 sm:w-80 flex-shrink-0 print:hidden bg-white border-l overflow-y-auto">
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

const allTables = computed(() => tc.layoutState.value.tables || [])
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

// 웹 배정
function startWebAssign(guest) {
  // 테이블 선택된 상태면 바로 배정
  if (selectedTable.value) {
    tc.addMemberToTable(selectedTable.value.number, guest)
    markDirty()
    return
  }
  // 테이블 미선택이면 모달
  assigningGuest.value = guest
}
function doWebAssign(tableNum) {
  const guest = assigningGuest.value || (cardEditTable.value ? {} : null)
  if (assigningGuest.value?.id) {
    tc.addMemberToTable(tableNum, assigningGuest.value)
  }
  assigningGuest.value = null
  cardEditTable.value = null
  markDirty()
}

// 모바일 카드 전용
const cardEditTable = ref(null)

function startMoveFromCard(table, member) {
  cardEditTable.value = table
  selectedTable.value = table
  movingMember.value = member
}

function removeMemberFromCard(tableNum, userId) {
  tc.removeMember(tableNum, userId)
  markDirty()
}

function addMemberToCard(table) {
  cardEditTable.value = table
  selectedTable.value = table
  assigningGuest.value = {} // 트리거만 (모달에서 선택)
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
