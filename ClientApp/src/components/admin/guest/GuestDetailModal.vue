<template>
  <BaseModal :is-open="isOpen" max-width="4xl" @close="emit('close')">
    <template #header>
      <div class="flex items-center justify-between w-full">
        <h2 class="text-2xl font-bold">{{ guestDetail?.guestName }}</h2>
        <button
          class="px-3 py-1.5 text-sm bg-primary-50 text-primary-600 rounded-lg hover:bg-primary-100"
          @click="emit('edit', guestDetail)"
        >
          수정
        </button>
      </div>
    </template>
    <template #body>
      <div v-if="loading" class="text-center py-8">로딩 중...</div>
      <template v-else>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
          <div>
            <h3 class="font-semibold mb-3">기본 정보</h3>
            <dl class="space-y-2 text-sm">
              <div>
                <dt class="text-gray-500 inline">전화번호:</dt>
                <dd class="inline ml-2">{{ guestDetail?.telephone || '-' }}</dd>
              </div>
              <div>
                <dt class="text-gray-500 inline">주민번호:</dt>
                <dd class="inline ml-2">
                  {{ guestDetail?.residentNumber || '-' }}
                </dd>
              </div>
              <div>
                <dt class="text-gray-500 inline">소속:</dt>
                <dd class="inline ml-2">
                  {{ guestDetail?.affiliation || '-' }}
                </dd>
              </div>
              <div>
                <dt class="text-gray-500 inline">부서:</dt>
                <dd class="inline ml-2">{{ guestDetail?.corpPart || '-' }}</dd>
              </div>
              <div>
                <dt class="text-gray-500 inline">그룹:</dt>
                <dd class="inline ml-2">{{ guestDetail?.groupName || '-' }}</dd>
              </div>
              <div>
                <dt class="text-gray-500 inline">비고:</dt>
                <dd class="inline ml-2">{{ guestDetail?.remarks || '-' }}</dd>
              </div>
            </dl>
          </div>

          <div v-if="Object.keys(filteredAttributes()).length > 0">
            <h3 class="font-semibold mb-3">속성 정보</h3>
            <dl class="space-y-2 text-sm">
              <div
                v-for="(value, key) in filteredAttributes()"
                :key="key"
                class="flex items-center justify-between gap-2"
              >
                <div class="flex-1 min-w-0">
                  <dt class="text-gray-500 inline">{{ key }}:</dt>
                  <template v-if="editingAttrKey === key">
                    <input
                      v-model="editingAttrValue"
                      type="text"
                      class="inline ml-2 px-2 py-0.5 border rounded text-sm w-32 focus:ring-2 focus:ring-primary-500"
                      @keyup.enter="handleSaveAttribute(key)"
                      @keyup.escape="editingAttrKey = null"
                    />
                    <button
                      type="button"
                      class="text-xs text-primary-600 hover:text-primary-800 ml-1"
                      @click="handleSaveAttribute(key)"
                    >
                      저장
                    </button>
                    <button
                      type="button"
                      class="text-xs text-gray-400 hover:text-gray-600 ml-1"
                      @click="editingAttrKey = null"
                    >
                      취소
                    </button>
                  </template>
                  <dd v-else class="inline ml-2 font-medium">{{ value }}</dd>
                </div>
                <div
                  v-if="editingAttrKey !== key"
                  class="flex items-center gap-2 flex-shrink-0"
                >
                  <button
                    type="button"
                    class="text-xs text-blue-500 hover:text-blue-700 hover:underline"
                    @click="startEditAttribute(key, value)"
                  >
                    수정
                  </button>
                  <button
                    type="button"
                    class="text-xs text-red-500 hover:text-red-700 hover:underline"
                    @click="handleDeleteAttribute(key)"
                  >
                    삭제
                  </button>
                </div>
              </div>
            </dl>
          </div>
        </div>

        <!-- 여권 정보 -->
        <div
          v-if="guestDetail?.passport"
          class="mb-6 border rounded-lg p-4 bg-gray-50"
        >
          <h3 class="font-semibold mb-3">여권 정보</h3>
          <dl class="grid grid-cols-2 gap-2 text-sm">
            <div>
              <dt class="text-gray-500">영문명</dt>
              <dd class="font-medium">
                {{
                  guestDetail.passport.firstName ||
                  guestDetail.passport.lastName
                    ? `${guestDetail.passport.firstName || ''} ${guestDetail.passport.lastName || ''}`
                    : '-'
                }}
              </dd>
            </div>
            <div>
              <dt class="text-gray-500">여권번호</dt>
              <dd class="font-medium">
                {{ guestDetail.passport.passportNumber || '-' }}
              </dd>
            </div>
            <div>
              <dt class="text-gray-500">만료일</dt>
              <dd class="font-medium">
                {{ guestDetail.passport.passportExpiryDate || '-' }}
              </dd>
            </div>
            <div>
              <dt class="text-gray-500">여권사본</dt>
              <dd class="font-medium">
                <button
                  v-if="guestDetail.passport.passportImageUrl"
                  class="text-blue-600 hover:underline"
                  @click="showPassportImage"
                >
                  이미지 보기
                </button>
                <span v-else class="text-gray-400">미등록</span>
              </dd>
            </div>
            <div>
              <dt class="text-gray-500">검증 상태</dt>
              <dd>
                <span
                  class="px-2 py-0.5 rounded text-xs font-medium"
                  :class="
                    guestDetail.passport.passportVerified
                      ? 'bg-primary-100 text-primary-700'
                      : 'bg-gray-100 text-gray-500'
                  "
                >
                  {{
                    guestDetail.passport.passportVerified
                      ? '검증완료'
                      : '미검증'
                  }}
                </span>
              </dd>
            </div>
          </dl>
        </div>

        <!-- 옵션투어 -->
        <div
          v-if="guestDetail?.optionTours && guestDetail.optionTours.length > 0"
          class="mb-6"
        >
          <h3 class="font-semibold mb-3">옵션투어</h3>
          <div class="flex flex-wrap gap-2">
            <div
              v-for="tour in guestDetail.optionTours"
              :key="tour.optionTourId"
              class="px-3 py-2 bg-purple-50 border border-purple-200 rounded-lg text-sm"
            >
              <div class="font-medium text-purple-800">{{ tour.name }}</div>
              <div class="text-xs text-purple-600">
                {{ tour.date }} {{ tour.startTime
                }}{{ tour.endTime ? ` ~ ${tour.endTime}` : '' }}
              </div>
            </div>
          </div>
        </div>

        <div v-if="guestDetail?.schedules && guestDetail.schedules.length > 0">
          <h3 class="font-semibold mb-3">배정된 일정</h3>
          <div class="space-y-4">
            <div
              v-for="schedule in guestDetail.schedules"
              :key="schedule.scheduleTemplateId"
              class="border rounded-lg overflow-hidden"
            >
              <div class="bg-gray-50 px-4 py-3 border-b">
                <h4 class="font-semibold">{{ schedule.courseName }}</h4>
                <p v-if="schedule.description" class="text-sm text-gray-600">
                  {{ schedule.description }}
                </p>
              </div>
              <div class="p-4">
                <div class="space-y-3">
                  <div
                    v-for="item in schedule.items"
                    :key="item.id"
                    class="flex gap-3 p-3 bg-gray-50 rounded-lg"
                  >
                    <div class="flex-shrink-0 w-24 text-sm">
                      <div class="font-medium">
                        {{ formatDate(item.scheduleDate) }}
                      </div>
                      <div class="text-gray-600">{{ item.startTime }}</div>
                    </div>
                    <div class="flex-1">
                      <p class="font-medium">{{ item.title }}</p>
                      <p v-if="item.location" class="text-sm text-gray-500">
                        {{ item.location }}
                      </p>
                      <p
                        v-if="item.content"
                        class="text-sm text-gray-600 mt-1 whitespace-pre-wrap"
                      >
                        {{ item.content }}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div v-else class="text-center text-gray-500 py-8 border rounded-lg">
          배정된 일정이 없습니다
        </div>
      </template>
    </template>
  </BaseModal>
</template>

<script setup>
import { ref, watch } from 'vue'
import 'viewerjs/dist/viewer.css'
import { api as viewerApi } from 'v-viewer'
import apiClient from '@/services/api'
import BaseModal from '@/components/common/BaseModal.vue'

const props = defineProps({
  isOpen: { type: Boolean, required: true },
  conventionId: { type: Number, required: true },
  guestId: { type: Number, default: null },
})

const emit = defineEmits(['close', 'edit'])

const guestDetail = ref(null)
const loading = ref(false)

const editingAttrKey = ref(null)
const editingAttrValue = ref('')

const HIDDEN_ATTRIBUTES = ['travel_info']

const filteredAttributes = () => {
  if (!guestDetail.value?.attributes) return {}
  return Object.fromEntries(
    Object.entries(guestDetail.value.attributes).filter(
      ([key]) => !HIDDEN_ATTRIBUTES.includes(key),
    ),
  )
}

const formatDate = (dateStr) => {
  const date = new Date(dateStr)
  return `${date.getMonth() + 1}/${date.getDate()}`
}

function showPassportImage() {
  if (guestDetail.value?.passport?.passportImageUrl) {
    viewerApi({
      images: [guestDetail.value.passport.passportImageUrl],
    })
  }
}

function startEditAttribute(key, value) {
  editingAttrKey.value = key
  editingAttrValue.value = value
}

async function handleSaveAttribute(key) {
  if (!props.guestId) return
  try {
    await apiClient.put(
      `/admin/guests/${props.guestId}/attributes/${encodeURIComponent(key)}`,
      { value: editingAttrValue.value },
    )
    if (guestDetail.value?.attributes) {
      guestDetail.value = {
        ...guestDetail.value,
        attributes: {
          ...guestDetail.value.attributes,
          [key]: editingAttrValue.value,
        },
      }
    }
    editingAttrKey.value = null
  } catch (e) {
    alert(e.response?.data?.message || '속성 수정에 실패했습니다.')
  }
}

async function handleDeleteAttribute(key) {
  if (!props.guestId) return
  if (!confirm(`속성 '${key}'을(를) 삭제하시겠습니까?`)) return
  try {
    await apiClient.delete(
      `/admin/guests/${props.guestId}/attributes/${encodeURIComponent(key)}`,
    )
    // 로컬 상태 업데이트
    if (guestDetail.value?.attributes) {
      const next = { ...guestDetail.value.attributes }
      delete next[key]
      guestDetail.value = { ...guestDetail.value, attributes: next }
    }
  } catch (e) {
    alert(e.response?.data?.message || '속성 삭제에 실패했습니다.')
  }
}

watch(
  () => props.guestId,
  async (newId) => {
    if (newId) {
      loading.value = true
      try {
        const response = await apiClient.get(`/admin/guests/${newId}/detail`)
        guestDetail.value = response.data
      } catch (error) {
        console.error('Failed to load guest detail:', error)
      } finally {
        loading.value = false
      }
    } else {
      guestDetail.value = null
    }
  },
)
</script>
