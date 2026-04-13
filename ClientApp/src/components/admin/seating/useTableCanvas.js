import { ref, onUnmounted } from 'vue'
import { fabric } from 'fabric'

const COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899', '#06b6d4', '#84cc16']

export function useTableCanvas(canvasElRef, containerRef, options = {}) {
  const { readonly = false, highlightTableNumber = null, onModified = () => {}, onTableSelect = () => {} } = options

  let canvas = null
  const zoom = ref(100)
  let _spaceHeld = false, _isPanning = false, _panLast = null

  function init() {
    const ct = containerRef.value
    if (!ct || !canvasElRef.value) return null
    canvas = new fabric.Canvas(canvasElRef.value, {
      width: ct.clientWidth, height: ct.clientHeight,
      backgroundColor: '#f9fafb', selection: !readonly, preserveObjectStacking: true,
    })
    canvas.on('mouse:wheel', _onWheel)
    canvas.on('mouse:move', _onMove)
    canvas.on('mouse:down', _onDown)
    canvas.on('mouse:up', _onUp)
    canvas.on('object:modified', () => { _syncAll(); onModified() })
    canvas.on('selection:created', (e) => _selectTable(e.selected))
    canvas.on('selection:updated', (e) => _selectTable(e.selected))
    canvas.on('selection:cleared', () => onTableSelect(null))
    if (readonly) canvas.selection = false
    const ro = new ResizeObserver(() => { if (canvas && ct) canvas.setDimensions({ width: ct.clientWidth, height: ct.clientHeight }) })
    ro.observe(ct)
    onUnmounted(() => ro.disconnect())
    return canvas
  }

  function dispose() { if (canvas) { canvas.dispose(); canvas = null } }

  // 줌
  function _onWheel(opt) {
    let z = canvas.getZoom() * (0.999 ** opt.e.deltaY)
    z = Math.min(5, Math.max(0.1, z))
    canvas.zoomToPoint({ x: opt.e.offsetX, y: opt.e.offsetY }, z)
    zoom.value = Math.round(z * 100)
    opt.e.preventDefault(); opt.e.stopPropagation()
  }
  function setZoom(pct) {
    const z = Math.max(10, Math.min(500, pct)) / 100
    canvas.zoomToPoint({ x: canvas.width / 2, y: canvas.height / 2 }, z)
    zoom.value = Math.round(z * 100)
  }
  function zoomToFit() {
    if (!canvas) return
    const bg = canvas.backgroundImage
    if (bg) {
      const z = Math.min(canvas.width / (bg.width * bg.scaleX), canvas.height / (bg.height * bg.scaleY), 2)
      canvas.setViewportTransform([z, 0, 0, z, 0, 0])
      zoom.value = Math.round(z * 100)
    }
    canvas.requestRenderAll()
  }

  // 팬
  function _onMove(opt) {
    if (_isPanning && _panLast) {
      const vpt = canvas.viewportTransform
      vpt[4] += opt.e.clientX - _panLast.x; vpt[5] += opt.e.clientY - _panLast.y
      canvas.requestRenderAll()
      _panLast = { x: opt.e.clientX, y: opt.e.clientY }
    }
  }
  function _onDown(opt) {
    if (_spaceHeld) { _isPanning = true; _panLast = { x: opt.e.clientX, y: opt.e.clientY }; canvas.selection = false; canvas.defaultCursor = 'grabbing' }
  }
  function _onUp() {
    if (_isPanning) { _isPanning = false; _panLast = null; if (!_spaceHeld) { canvas.selection = !readonly; canvas.defaultCursor = 'default' } }
  }

  function _selectTable(selected) {
    const obj = (selected || []).find(o => o._tableData)
    if (obj) onTableSelect(obj._tableData)
  }

  // 테이블 추가
  function addTable(shape = 'circle') {
    const id = 't_' + Date.now()
    const num = String((_getMaxNumber() + 1))
    const data = { id, number: num, label: num + '번', shape, x: 200, y: 200, width: 80, height: 80, color: COLORS[parseInt(num) % COLORS.length], members: [] }
    _buildTableMarker(data)
    canvas.requestRenderAll()
    onModified()
  }

  function _getMaxNumber() {
    let max = 0
    if (canvas) canvas.getObjects().forEach(o => { if (o._tableData) { const n = parseInt(o._tableData.number); if (!isNaN(n) && n > max) max = n } })
    return max
  }

  function _buildTableMarker(data) {
    const isHighlighted = highlightTableNumber && data.number === highlightTableNumber
    const fill = isHighlighted ? '#ef4444' : (data.color || '#3b82f6')
    const w = data.width || 80, h = data.height || 80

    let shape
    if (data.shape === 'rect') {
      shape = new fabric.Rect({ width: w, height: h, fill, opacity: 0.85, stroke: '#fff', strokeWidth: 2, rx: 8, ry: 8, originX: 'center', originY: 'center' })
    } else {
      shape = new fabric.Circle({ radius: w / 2, fill, opacity: 0.85, stroke: '#fff', strokeWidth: 2, originX: 'center', originY: 'center' })
    }

    const label = new fabric.Text(data.number, { fontSize: 22, fontWeight: 'bold', fill: '#ffffff', originX: 'center', originY: 'center', shadow: new fabric.Shadow({ color: 'rgba(0,0,0,0.3)', blur: 2 }) })
    const count = new fabric.Text(data.members?.length ? `${data.members.length}명` : '', { fontSize: 10, fill: '#ffffffcc', originX: 'center', originY: 'center', top: 18 })

    const group = new fabric.Group([shape, label, count], {
      left: data.x || 200, top: data.y || 200,
      selectable: !readonly, hasControls: !readonly, lockRotation: true,
      _tableData: data,
    })
    if (isHighlighted && readonly) {
      // 바운스 효과를 위한 shadow
      group.set({ shadow: new fabric.Shadow({ color: '#ef4444', blur: 20, offsetX: 0, offsetY: 0 }) })
    }
    canvas.add(group)
    return group
  }

  function deleteSelected() {
    if (!canvas) return
    canvas.getActiveObjects().forEach(o => { if (o._tableData) canvas.remove(o) })
    canvas.discardActiveObject(); canvas.requestRenderAll(); _syncAll(); onModified()
  }

  // 배경
  function setBackground(url) {
    if (!canvas) return
    if (!url) { canvas.setBackgroundImage(null, canvas.requestRenderAll.bind(canvas)); return }
    fabric.Image.fromURL(url, (img) => {
      const maxDim = Math.max(img.width, img.height)
      const scale = maxDim > 4000 ? 4000 / maxDim : 1
      img.set({ scaleX: scale, scaleY: scale, opacity: readonly ? 1 : 0.85 })
      canvas.setBackgroundImage(img, () => { zoomToFit(); canvas.requestRenderAll() })
    }, { crossOrigin: 'anonymous' })
  }

  // 직렬화
  function toLayoutJSON() {
    if (!canvas) return { tables: [] }
    const tables = []
    canvas.getObjects().forEach(o => {
      if (!o._tableData) return
      _syncObj(o)
      tables.push({ ...o._tableData })
    })
    return { tables }
  }

  function loadLayoutJSON(data) {
    if (!canvas) return
    canvas.getObjects().filter(o => o._tableData).forEach(o => canvas.remove(o))
    for (const t of data.tables || []) _buildTableMarker(t)
    if (readonly) canvas.forEachObject(o => { o.selectable = false; o.evented = !readonly || !!o._tableData })
    canvas.requestRenderAll()
  }

  function _syncObj(o) {
    if (!o._tableData) return
    o._tableData.x = Math.round(o.left); o._tableData.y = Math.round(o.top)
    if (o.scaleX && o.scaleX !== 1) {
      o._tableData.width = Math.round((o._tableData.width || 80) * o.scaleX)
      o._tableData.height = Math.round((o._tableData.height || 80) * o.scaleY)
    }
  }
  function _syncAll() { if (canvas) canvas.getObjects().forEach(_syncObj) }

  // 멤버 이동
  function moveMember(userId, fromTableNum, toTableNum) {
    const data = toLayoutJSON()
    const from = data.tables.find(t => t.number === fromTableNum)
    const to = data.tables.find(t => t.number === toTableNum)
    if (!from || !to) return false
    const idx = from.members.findIndex(m => m.userId === userId)
    if (idx < 0) return false
    const member = from.members.splice(idx, 1)[0]
    to.members.push(member)
    loadLayoutJSON(data)
    onModified()
    return true
  }

  function addMemberToTable(tableNum, user) {
    const data = toLayoutJSON()
    const table = data.tables.find(t => t.number === tableNum)
    if (!table) return false
    if (table.members.some(m => m.userId === user.id)) return false
    table.members.push({ userId: user.id, name: user.name })
    loadLayoutJSON(data)
    onModified()
    return true
  }

  function removeMember(tableNum, userId) {
    const data = toLayoutJSON()
    const table = data.tables.find(t => t.number === tableNum)
    if (!table) return false
    table.members = table.members.filter(m => m.userId !== userId)
    loadLayoutJSON(data)
    onModified()
    return true
  }

  // 키보드
  function handleKeyDown(e) { if (e.code === 'Space' && !e.repeat) { e.preventDefault(); _spaceHeld = true; if (canvas) canvas.defaultCursor = 'grab' } }
  function handleKeyUp(e) { if (e.code === 'Space') { _spaceHeld = false; _isPanning = false; _panLast = null; if (canvas) { canvas.defaultCursor = 'default'; canvas.selection = !readonly } } }

  // 터치
  function setupTouch() {
    const el = containerRef.value; if (!el) return
    let lastDist = 0, lastPan = null
    el.addEventListener('touchstart', e => {
      if (e.touches.length === 2) { const dx = e.touches[0].clientX - e.touches[1].clientX, dy = e.touches[0].clientY - e.touches[1].clientY; lastDist = Math.hypot(dx, dy) }
      else if (e.touches.length === 1) lastPan = { x: e.touches[0].clientX, y: e.touches[0].clientY }
    }, { passive: true })
    el.addEventListener('touchmove', e => {
      e.preventDefault()
      if (e.touches.length === 2 && lastDist) {
        const dx = e.touches[0].clientX - e.touches[1].clientX, dy = e.touches[0].clientY - e.touches[1].clientY, dist = Math.hypot(dx, dy)
        const z = canvas.getZoom() * (dist / lastDist)
        canvas.zoomToPoint({ x: (e.touches[0].clientX + e.touches[1].clientX) / 2, y: (e.touches[0].clientY + e.touches[1].clientY) / 2 }, Math.min(5, Math.max(0.1, z)))
        zoom.value = Math.round(canvas.getZoom() * 100); lastDist = dist
      } else if (e.touches.length === 1 && lastPan) {
        const vpt = canvas.viewportTransform; vpt[4] += e.touches[0].clientX - lastPan.x; vpt[5] += e.touches[0].clientY - lastPan.y
        canvas.requestRenderAll(); lastPan = { x: e.touches[0].clientX, y: e.touches[0].clientY }
      }
    }, { passive: false })
    el.addEventListener('touchend', () => { lastDist = 0; lastPan = null }, { passive: true })
  }

  function downloadPNG(fn = 'seating.png') {
    if (!canvas) return; const url = canvas.toDataURL({ format: 'png', multiplier: 2 })
    const a = document.createElement('a'); a.href = url; a.download = fn; a.click()
  }

  return {
    init, dispose, canvas: () => canvas, zoom, setZoom, zoomToFit,
    addTable, deleteSelected, setBackground, downloadPNG,
    toLayoutJSON, loadLayoutJSON,
    moveMember, addMemberToTable, removeMember,
    handleKeyDown, handleKeyUp, setupTouch,
  }
}
