<template>
  <div v-if="loading" class="flex items-center justify-center py-20">
    <div
      class="w-10 h-10 border-2 border-primary-600 border-t-transparent rounded-full animate-spin"
    />
  </div>
  <router-view v-else :convention-id="conventionId" />
</template>

<script setup>
import { ref, computed, onMounted, inject, provide } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { conventionAPI } from '@/services/api'

const router = useRouter()
const route = useRoute()
const adminHeader = inject('adminHeader', null)

const conventionId = computed(() => {
  const id = parseInt(route.params.id)
  return isNaN(id) ? null : id
})
const convention = ref(null)
const loading = ref(true)

provide('convention', convention)
provide('conventionId', conventionId)

onMounted(async () => {
  if (!conventionId.value) {
    alert('행사 ID가 유효하지 않습니다.')
    router.push('/admin')
    return
  }

  try {
    const response = await conventionAPI.getConvention(conventionId.value)
    convention.value = response.data
    if (convention.value?.title) {
      adminHeader?.setSubtitle(convention.value.title)
    }
  } catch (error) {
    console.error('행사 정보 로드 실패:', error)
    alert('행사 정보를 불러오는데 실패했습니다.')
    router.push('/admin')
  } finally {
    loading.value = false
  }
})
</script>
