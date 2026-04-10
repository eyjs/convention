import { ref, onUnmounted } from 'vue'
import { fabric } from 'fabric'

let PIN_RADIUS = 10
let PIN_FONT = 8
const COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899', '#06b6d4', '#84cc16']

export function usePinCanvas(canvasElRef, containerRef, options = {}) {
  const { readonly = false, highlightUserId = null, onModified = () => {}, onPinSelect = () => {} } = options

  let canvas = null
  const zoom = ref(100)
  const cursorPos = ref({ x: 0, y: 0 })
  const mode = ref('select') // select | pin
  let _spaceHeld = false
  let _isPanning = false
  let _panLast = null
  let _isLoadingLayout = false
  let _lastGoodState = { pins: [], groups: [] }

  // ===== 초기화 =====
  function init() {
    const ct = containerRef.value
    if (!ct || !canvasElRef.value) return null
    canvas = new fabric.Canvas(canvasElRef.value, {
      width: ct.clientWidth,
      height: ct.clientHeight,
      backgroundColor: '#f9fafb',
      selection: !readonly,
      preserveObjectStacking: true,
    })

    canvas.on('mouse:wheel', _onWheel)
    canvas.on('mouse:move', _onMouseMove)
    canvas.on('mouse:down', _onMouseDown)
    canvas.on('mouse:up', _onMouseUp)
    canvas.on('selection:created', _onSelection)
    canvas.on('selection:updated', _onSelection)
    canvas.on('selection:cleared', () => onPinSelect(null))
    canvas.on('object:modified', () => { _syncAll(); onModified() })

    if (readonly) {
      canvas.selection = false
    }

    const ro = new ResizeObserver(() => {
      if (canvas && ct) canvas.setDimensions({ width: ct.clientWidth, height: ct.clientHeight })
    })
    ro.observe(ct)
    onUnmounted(() => ro.disconnect())

    return canvas
  }

  function dispose() { stopHighlight(); if (canvas) { canvas.dispose(); canvas = null } }

  // ===== 줌 =====
  function _onWheel(opt) {
    let z = canvas.getZoom() * (0.999 ** opt.e.deltaY)
    z = Math.min(5, Math.max(0.1, z))
    canvas.zoomToPoint({ x: opt.e.offsetX, y: opt.e.offsetY }, z)
    zoom.value = Math.round(z * 100)
    opt.e.preventDefault()
    opt.e.stopPropagation()
  }

  function setZoom(pct) {
    const z = Math.max(10, Math.min(500, pct)) / 100
    canvas.zoomToPoint({ x: canvas.width / 2, y: canvas.height / 2 }, z)
    zoom.value = Math.round(z * 100)
  }

  function zoomToFit() {
    if (!canvas || !canvas.backgroundImage) return
    const img = canvas.backgroundImage
    const z = Math.min(canvas.width / (img.width * img.scaleX), canvas.height / (img.height * img.scaleY), 2)
    canvas.setViewportTransform([z, 0, 0, z, 0, 0])
    zoom.value = Math.round(z * 100)
    canvas.requestRenderAll()
  }

  // ===== 마우스 =====
  function _onMouseMove(opt) {
    if (!canvas) return
    const p = canvas.getPointer(opt.e)
    cursorPos.value = { x: Math.round(p.x), y: Math.round(p.y) }
    if (_isPanning && _panLast) {
      const vpt = canvas.viewportTransform
      vpt[4] += opt.e.clientX - _panLast.x
      vpt[5] += opt.e.clientY - _panLast.y
      canvas.requestRenderAll()
      _panLast = { x: opt.e.clientX, y: opt.e.clientY }
    }
  }

  function _onMouseDown(opt) {
    if (_spaceHeld || mode.value === 'pan') {
      _isPanning = true
      _panLast = { x: opt.e.clientX, y: opt.e.clientY }
      canvas.selection = false
      canvas.defaultCursor = 'grabbing'
      return
    }
    // 핀 찍기 모드
    if (mode.value === 'pin' && !readonly) {
      if (!opt.target) {
        // 빈 영역 → 핀 추가
        const p = canvas.getPointer(opt.e)
        _addPin(Math.round(p.x), Math.round(p.y))
        onModified()
      }
      // 기존 핀 클릭 → 선택 모드로 전환하여 이동 가능
      if (opt.target && opt.target._pinId) {
        setMode('select')
        canvas.setActiveObject(opt.target)
      }
    }
  }

  function _onMouseUp() {
    if (_isPanning) {
      _isPanning = false
      _panLast = null
      if (!_spaceHeld && mode.value !== 'pan') {
        canvas.selection = !readonly
        canvas.defaultCursor = mode.value === 'pin' ? 'crosshair' : 'default'
      }
    }
  }

  function _onSelection(e) {
    const sel = (e.selected || []).find((o) => o._pinId)
    if (sel) onPinSelect(sel._pinData)
  }

  // ===== 핀 관리 =====
  function _addPin(x, y, data = null) {
    const id = data?.id || 'p_' + Date.now() + '_' + Math.floor(Math.random() * 1000)
    const group = data?.group || null
    const userId = data?.userId || null
    const userName = data?.userName || null
    const colorIdx = group ? _groupColorIndex(group) : 0
    const color = userId ? COLORS[colorIdx % COLORS.length] : '#d1d5db'
    const highlighted = highlightUserId && userId === highlightUserId

    const circle = new fabric.Circle({
      radius: PIN_RADIUS,
      fill: highlighted ? '#ef4444' : color,
      stroke: highlighted ? '#dc2626' : '#ffffff',
      strokeWidth: 2,
      shadow: new fabric.Shadow({ color: 'rgba(0,0,0,0.3)', blur: 4, offsetX: 1, offsetY: 1 }),
      originX: 'center',
      originY: 'center',
    })

    // Text 제거 — Fabric v5 + Chrome 호환 이슈 방지. 이름은 사이드패널에서 확인
    const pin = new fabric.Group([circle], {
      left: x,
      top: y,
      originX: 'center',
      originY: 'center',
      selectable: !readonly,
      hasControls: false,
      hasBorders: true,
      lockScalingX: true,
      lockScalingY: true,
      lockRotation: true,
      _pinId: id,
      _pinData: { id, x, y, group, userId, userName },
    })

    canvas.add(pin)
    return id
  }

  function addPinWithUser(x, y, user) {
    const id = _addPin(x, y, { id: 'p_' + Date.now() + '_' + Math.floor(Math.random() * 1000), x, y, group: null, userId: user.id, userName: user.name })
    if (canvas) canvas.requestRenderAll()
    return id
  }

  function removePin(id) {
    if (!canvas) return
    const obj = canvas.getObjects().find((o) => o._pinId === id)
    if (obj) { canvas.remove(obj); canvas.requestRenderAll(); onModified() }
  }

  function updatePin(id, updates) {
    if (!canvas) return
    const obj = canvas.getObjects().find((o) => o._pinId === id)
    if (!obj) return
    Object.assign(obj._pinData, updates)
    // 핀 리빌드
    const data = obj._pinData
    canvas.remove(obj)
    _addPin(data.x, data.y, data)
    canvas.requestRenderAll()
    onModified()
  }

  function assignUser(pinId, user) {
    updatePin(pinId, { userId: user.id, userName: user.name })
  }

  function clearUser(pinId) {
    updatePin(pinId, { userId: null, userName: null })
  }

  function setGroup(pinId, groupId) {
    updatePin(pinId, { group: groupId })
  }

  let _groupColors = {}
  let _groupIdx = 0
  function _groupColorIndex(group) {
    if (!_groupColors[group]) _groupColors[group] = _groupIdx++
    return _groupColors[group]
  }

  // ===== 삭제 =====
  function deleteSelected() {
    if (!canvas) return
    const active = canvas.getActiveObjects()
    active.forEach((o) => { if (o._pinId) canvas.remove(o) })
    canvas.discardActiveObject()
    canvas.requestRenderAll()
    _syncAll()
    onModified()
  }

  // ===== 배경 이미지 (이동/크기 조절 가능 오브젝트) =====
  let _bgObj = null
  const bgLocked = ref(true)

  function setBackground(url) {
    if (!canvas) return
    // 기존 배경 제거
    if (_bgObj) { canvas.remove(_bgObj); _bgObj = null }
    canvas.setBackgroundImage(null, canvas.requestRenderAll.bind(canvas))
    if (!url) { canvas.requestRenderAll(); return }

    fabric.Image.fromURL(url, (img) => {
      const maxDim = Math.max(img.width, img.height)
      const scale = maxDim > 4000 ? 4000 / maxDim : 1

      if (readonly) {
        // 사용자 뷰: 고정 배경
        img.set({ scaleX: scale, scaleY: scale, opacity: 1 })
        canvas.setBackgroundImage(img, () => { zoomToFit(); canvas.requestRenderAll() })
      } else {
        // 관리자: 이동/크기 조절 가능한 오브젝트
        img.set({
          scaleX: scale, scaleY: scale, opacity: 0.9,
          selectable: !bgLocked.value,
          evented: !bgLocked.value,
          hasControls: !bgLocked.value,
          lockMovementX: bgLocked.value,
          lockMovementY: bgLocked.value,
          _isBg: true,
        })
        _bgObj = img
        canvas.add(img)
        canvas.sendToBack(img) // 핀 아래로
        zoomToFit()
        canvas.requestRenderAll()
      }
    }, { crossOrigin: 'anonymous' })
  }

  function toggleBgLock() {
    bgLocked.value = !bgLocked.value
    if (_bgObj) {
      _bgObj.set({
        selectable: !bgLocked.value,
        evented: !bgLocked.value,
        hasControls: !bgLocked.value,
        lockMovementX: bgLocked.value,
        lockMovementY: bgLocked.value,
      })
      if (bgLocked.value) canvas.discardActiveObject()
      canvas.requestRenderAll()
    }
  }

  // ===== 직렬화 =====
  function toLayoutJSON() {
    if (!canvas) return _lastGoodState || { pins: [], groups: [] }
    const pins = []
    canvas.getObjects().forEach((o) => {
      if (!o._pinId || !o._pinData) return
      _syncObj(o)
      pins.push({ ...o._pinData })
    })
    // 그룹 추출
    const groupSet = new Map()
    pins.forEach((p) => {
      if (p.group && !groupSet.has(p.group)) {
        groupSet.set(p.group, { id: p.group, label: p.group, color: COLORS[_groupColorIndex(p.group) % COLORS.length] })
      }
    })
    const result = { pins, groups: [...groupSet.values()] }
    // 핀이 있으면 마지막 정상 상태 갱신 (빈 데이터로 덮어쓰기 방지)
    if (pins.length > 0) _lastGoodState = result
    return result
  }

  function loadLayoutJSON(data) {
    if (!canvas) return
    _isLoadingLayout = true
    try {
      // 기존 핀 제거
      canvas.getObjects().filter((o) => o._pinId).forEach((o) => canvas.remove(o))
      _groupColors = {}
      _groupIdx = 0

      // 그룹 색상 프리로드
      for (const g of data.groups || []) {
        _groupColors[g.id] = _groupIdx++
      }

      // 핀 생성
      for (const p of data.pins || []) {
        _addPin(p.x, p.y, p)
      }

      // 마지막 정상 상태 갱신
      if (data.pins && data.pins.length > 0) {
        _lastGoodState = { pins: [...data.pins], groups: [...(data.groups || [])] }
      }

      canvas.requestRenderAll()
    } catch (e) {
      console.error('loadLayoutJSON error:', e)
    } finally {
      _isLoadingLayout = false
    }
  }

  // 하위 호환: 기존 tables/decors/lines 구조도 변환
  function loadLegacyLayout(data) {
    const pins = []
    const groups = []
    if (data.tables) {
      for (const t of data.tables) {
        const gid = t.label || t.id
        groups.push({ id: gid, label: t.label || '테이블', color: COLORS[groups.length % COLORS.length] })
        for (const seat of t.seats || []) {
          const pos = _legacySeatPos(t, seat.index)
          pins.push({ id: `${t.id}_s${seat.index}`, x: t.x + pos.x, y: t.y + pos.y, group: gid, userId: seat.userId, userName: seat.userName })
        }
      }
    }
    return { pins, groups }
  }

  function _legacySeatPos(table, idx) {
    const w = table.width || 100, h = table.height || 100
    const total = table.seats?.length || 4
    if (table.type?.startsWith('round')) {
      const r = w / 2 + 20
      const a = (idx / total) * Math.PI * 2 - Math.PI / 2
      return { x: w / 2 + Math.cos(a) * r, y: h / 2 + Math.sin(a) * r }
    }
    const half = Math.ceil(total / 2)
    if (idx < half) return { x: ((idx + 0.5) / half) * w, y: -20 }
    return { x: ((idx - half + 0.5) / (total - half)) * w, y: h + 20 }
  }

  function _syncObj(o) {
    if (!o._pinData) return
    o._pinData.x = Math.round(o.left)
    o._pinData.y = Math.round(o.top)
  }

  function _syncAll() { if (canvas) canvas.getObjects().forEach(_syncObj) }

  // ===== 모드 전환 =====
  function setMode(m) {
    mode.value = m
    if (!canvas) return
    if (m === 'pin') {
      canvas.defaultCursor = 'crosshair'
      canvas.selection = false
      // 개별 핀은 드래그 가능하게 유지
      canvas.forEachObject((o) => { if (o._pinId) { o.selectable = true; o.evented = true } })
    } else if (m === 'pan') {
      canvas.defaultCursor = 'grab'
      canvas.selection = false
    } else {
      canvas.defaultCursor = 'default'
      canvas.selection = !readonly
    }
  }

  // ===== 내보내기 =====
  function downloadPNG(filename = 'seating-layout.png') {
    if (!canvas) return
    const url = canvas.toDataURL({ format: 'png', multiplier: 2 })
    const a = document.createElement('a'); a.href = url; a.download = filename; a.click()
  }

  // ===== 하이라이트 (펄스 애니메이션 + 자동 줌인) =====
  let _pulseInterval = null

  function highlightUser(uid) {
    if (!canvas || !uid) return
    const pin = canvas.getObjects().find((o) => o._pinData?.userId === uid)
    if (!pin) return

    // 자동 줌인 (본인 핀 중심)
    const z = 2.5
    canvas.setViewportTransform([
      z, 0, 0, z,
      canvas.width / 2 - pin.left * z,
      canvas.height / 2 - pin.top * z,
    ])
    zoom.value = Math.round(z * 100)

    // 펄스 링 애니메이션
    if (_pulseInterval) clearInterval(_pulseInterval)

    const pulseRing = new fabric.Circle({
      left: pin.left,
      top: pin.top,
      radius: PIN_RADIUS + 5,
      fill: 'transparent',
      stroke: '#ef4444',
      strokeWidth: 3,
      originX: 'center',
      originY: 'center',
      selectable: false,
      evented: false,
      opacity: 1,
    })
    canvas.add(pulseRing)

    // "여기입니다!" 라벨
    const label = new fabric.Text('● 내 자리', {
      left: pin.left,
      top: pin.top - 35,
      fontSize: 13,
      fontWeight: 'bold',
      fill: '#ef4444',
      originX: 'center',
      originY: 'center',
      selectable: false,
      evented: false,
      shadow: new fabric.Shadow({ color: 'white', blur: 6 }),
    })
    canvas.add(label)

    // 펄스 애니메이션 (확대→축소 반복)
    let expanding = true
    _pulseInterval = setInterval(() => {
      if (!canvas) { clearInterval(_pulseInterval); return }
      const r = pulseRing.radius
      if (expanding) {
        pulseRing.set({ radius: r + 0.5, opacity: Math.max(0.2, pulseRing.opacity - 0.02) })
        if (r >= PIN_RADIUS + 25) expanding = false
      } else {
        pulseRing.set({ radius: r - 0.5, opacity: Math.min(1, pulseRing.opacity + 0.02) })
        if (r <= PIN_RADIUS + 5) expanding = true
      }
      canvas.requestRenderAll()
    }, 30)

    canvas.requestRenderAll()
  }

  function stopHighlight() {
    if (_pulseInterval) { clearInterval(_pulseInterval); _pulseInterval = null }
  }

  // ===== 키보드 =====
  function handleKeyDown(e) {
    if (e.code === 'Space' && !e.repeat) { e.preventDefault(); _spaceHeld = true; if (canvas) canvas.defaultCursor = 'grab' }
  }
  function handleKeyUp(e) {
    if (e.code === 'Space') { _spaceHeld = false; _isPanning = false; _panLast = null; if (canvas) setMode(mode.value) }
  }

  // ===== 터치 (모바일 readonly) =====
  function setupTouch() {
    const el = containerRef.value
    if (!el) return
    let lastDist = 0, lastPan = null
    el.addEventListener('touchstart', (e) => {
      if (e.touches.length === 2) { const dx = e.touches[0].clientX - e.touches[1].clientX, dy = e.touches[0].clientY - e.touches[1].clientY; lastDist = Math.hypot(dx, dy) }
      else if (e.touches.length === 1) lastPan = { x: e.touches[0].clientX, y: e.touches[0].clientY }
    }, { passive: true })
    el.addEventListener('touchmove', (e) => {
      e.preventDefault()
      if (e.touches.length === 2 && lastDist) {
        const dx = e.touches[0].clientX - e.touches[1].clientX, dy = e.touches[0].clientY - e.touches[1].clientY
        const dist = Math.hypot(dx, dy)
        const z = canvas.getZoom() * (dist / lastDist)
        canvas.zoomToPoint({ x: (e.touches[0].clientX + e.touches[1].clientX) / 2, y: (e.touches[0].clientY + e.touches[1].clientY) / 2 }, Math.min(5, Math.max(0.1, z)))
        zoom.value = Math.round(canvas.getZoom() * 100); lastDist = dist
      } else if (e.touches.length === 1 && lastPan) {
        const vpt = canvas.viewportTransform
        vpt[4] += e.touches[0].clientX - lastPan.x; vpt[5] += e.touches[0].clientY - lastPan.y
        canvas.requestRenderAll(); lastPan = { x: e.touches[0].clientX, y: e.touches[0].clientY }
      }
    }, { passive: false })
    el.addEventListener('touchend', () => { lastDist = 0; lastPan = null }, { passive: true })
  }

  return {
    init, dispose, canvas: () => canvas,
    zoom, cursorPos, mode, setMode,
    setBackground, toggleBgLock, bgLocked, setZoom, zoomToFit,
    deleteSelected, downloadPNG,
    addPinWithUser, assignUser, clearUser, setGroup, removePin,
    toLayoutJSON, loadLayoutJSON, loadLegacyLayout,
    highlightUser, stopHighlight, setupTouch,
    handleKeyDown, handleKeyUp,
  }
}
