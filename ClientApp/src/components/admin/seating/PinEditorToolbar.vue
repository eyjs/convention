<template>
  <div class="bg-white border-b px-3 py-1.5 flex items-center gap-2 text-sm print:hidden">
    <button class="p-1.5 hover:bg-gray-100 rounded" @click="$router.back()">
      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" /></svg>
    </button>
    <input :value="name" class="font-bold text-base border-b border-transparent focus:border-gray-300 outline-none w-40" @input="$emit('update:name', $event.target.value)" />

    <!-- 도구 -->
    <div class="flex items-center bg-gray-100 rounded-lg p-0.5 ml-4">
      <button
        :class="mode === 'select' ? 'bg-white shadow text-blue-600' : 'text-gray-500 hover:text-gray-700'"
        class="p-1.5 rounded-md transition-colors"
        title="선택 (V)"
        @click="$emit('mode', 'select')"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 15l-2 5L9 9l11 4-5 2zm0 0l5 5M7.188 2.239l.777 2.897M5.136 7.965l-2.898-.777M13.95 4.05l-2.122 2.122m-5.657 5.656l-2.12 2.122" /></svg>
      </button>
      <button
        :class="mode === 'pin' ? 'bg-white shadow text-red-600' : 'text-gray-500 hover:text-gray-700'"
        class="p-1.5 rounded-md transition-colors"
        title="핀 찍기 (P)"
        @click="$emit('mode', 'pin')"
      >
        <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24"><path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z" /></svg>
      </button>
      <button
        :class="mode === 'pan' ? 'bg-white shadow text-gray-800' : 'text-gray-500 hover:text-gray-700'"
        class="p-1.5 rounded-md transition-colors"
        title="이동 (H / Space)"
        @click="$emit('mode', 'pan')"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 11.5V14m0-2.5v-6a1.5 1.5 0 113 0m-3 6a1.5 1.5 0 00-3 0v2a7.5 7.5 0 0015 0v-5a1.5 1.5 0 00-3 0m-6-3V11m0-5.5v-1a1.5 1.5 0 013 0v1m0 0V11m0-5.5a1.5 1.5 0 013 0v3m0 0V11" /></svg>
      </button>
    </div>

    <!-- 배경 업로드 + 잠금 -->
    <label class="ml-2 px-3 py-1 border rounded cursor-pointer hover:bg-gray-50 text-xs">
      <input type="file" accept="image/*" class="hidden" @change="$emit('upload-bg', $event)" />
      📷 배경
    </label>
    <button
      v-if="hasBg"
      class="px-2 py-1 border rounded text-xs"
      :class="bgLocked ? 'text-gray-500' : 'text-red-600 border-red-300 bg-red-50'"
      :title="bgLocked ? '배경 잠금 해제 (이동/크기 조절)' : '배경 잠금'"
      @click="$emit('toggle-bg-lock')"
    >
      {{ bgLocked ? '🔒' : '🔓 이동 중' }}
    </button>

    <!-- Undo/Redo -->
    <div class="flex items-center gap-0.5 ml-2">
      <button :disabled="!canUndo" class="p-1.5 rounded hover:bg-gray-100 disabled:opacity-30" @click="$emit('undo')">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h10a4 4 0 014 4v2M3 10l4-4m-4 4l4 4" /></svg>
      </button>
      <button :disabled="!canRedo" class="p-1.5 rounded hover:bg-gray-100 disabled:opacity-30" @click="$emit('redo')">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 10H11a4 4 0 00-4 4v2M21 10l-4-4m4 4l-4 4" /></svg>
      </button>
    </div>

    <div class="flex-1"></div>

    <!-- 줌 -->
    <div class="flex items-center gap-1 text-xs text-gray-500">
      <button class="px-1.5 py-0.5 hover:bg-gray-100 rounded" @click="$emit('zoom', zoom - 25)">−</button>
      <button class="px-2 py-0.5 hover:bg-gray-100 rounded min-w-[3rem] text-center" @click="$emit('zoom-fit')">{{ zoom }}%</button>
      <button class="px-1.5 py-0.5 hover:bg-gray-100 rounded" @click="$emit('zoom', zoom + 25)">+</button>
    </div>

    <button
      class="px-3 py-1 rounded text-xs font-medium"
      :class="isDirty ? 'bg-blue-600 text-white hover:bg-blue-700' : 'border text-gray-400'"
      @click="$emit('save-now')"
    >
      💾 저장
    </button>
    <span class="text-xs text-gray-400 ml-1">{{ saveStatus }}</span>
    <button class="px-3 py-1 border rounded hover:bg-gray-50 text-xs" @click="$emit('export-png')">PNG</button>
  </div>
</template>

<script setup>
defineProps({ name: String, mode: String, zoom: Number, canUndo: Boolean, canRedo: Boolean, saveStatus: String, hasBg: Boolean, bgLocked: Boolean, isDirty: Boolean })
defineEmits(['update:name', 'mode', 'upload-bg', 'toggle-bg-lock', 'undo', 'redo', 'zoom', 'zoom-fit', 'export-png', 'save-now'])
</script>
