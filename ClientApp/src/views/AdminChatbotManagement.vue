<template>
  <div class="min-h-screen bg-gray-50">
    <header class="bg-white shadow-sm sticky top-0 z-40">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <div class="flex items-center">
            <button
              @click="router.push('/admin')"
              class="p-2 hover:bg-gray-100 rounded-lg"
            >
              <svg
                class="w-5 h-5 text-gray-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M10 19l-7-7m0 0l7-7m-7 7h18"
                />
              </svg>
            </button>
            <h1 class="text-xl sm:text-2xl font-bold text-gray-900 ml-2">
              챗봇 관리
            </h1>
          </div>
          <button
            @click="fetchAllData"
            class="px-4 py-2 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors"
          >
            <span class="flex items-center gap-2">
              <svg
                class="w-4 h-4"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
                />
              </svg>
              새로고침
            </span>
          </button>
        </div>
      </div>
    </header>

    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div class="bg-white rounded-lg shadow-sm p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-gray-500">총 벡터 문서</p>
              <p class="text-2xl font-bold text-gray-900 mt-1">
                {{ stats.totalDocuments }}
              </p>
            </div>
            <div class="p-3 bg-blue-100 rounded-lg">
              <svg
                class="w-6 h-6 text-blue-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                />
              </svg>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-lg shadow-sm p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-gray-500">활성 행사</p>
              <p class="text-2xl font-bold text-gray-900 mt-1">
                {{ stats.activeConventions }}
              </p>
            </div>
            <div class="p-3 bg-green-100 rounded-lg">
              <svg
                class="w-6 h-6 text-green-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-lg shadow-sm p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-gray-500">총 참가자</p>
              <p class="text-2xl font-bold text-gray-900 mt-1">
                {{ stats.totalGuests }}
              </p>
            </div>
            <div class="p-3 bg-purple-100 rounded-lg">
              <svg
                class="w-6 h-6 text-purple-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
                />
              </svg>
            </div>
          </div>
        </div>

        <div class="bg-white rounded-lg shadow-sm p-6">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-gray-500">DB 크기</p>
              <p class="text-2xl font-bold text-gray-900 mt-1">
                {{ formatBytes(stats.dbSize) }}
              </p>
            </div>
            <div class="p-3 bg-orange-100 rounded-lg">
              <svg
                class="w-6 h-6 text-orange-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582-4 8-4s8 1.79 8 4"
                />
              </svg>
            </div>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-sm">
        <div class="border-b border-gray-200">
          <nav class="flex -mb-px">
            <button
              v-for="tab in tabs"
              :key="tab.id"
              @click="activeTab = tab.id"
              :class="[
                'px-6 py-3 text-sm font-medium border-b-2 transition-colors',
                activeTab === tab.id
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300',
              ]"
            >
              {{ tab.label }}
            </button>
          </nav>
        </div>

        <div v-show="activeTab === 'providers'" class="p-6">
          <div class="space-y-6">
            <div class="bg-white rounded-lg shadow-sm p-6">
              <h3 class="text-lg font-semibold text-gray-900 mb-4">
                LLM Provider 관리
              </h3>
              <LlmProviderManagement />
            </div>
          </div>
        </div>

        <div v-show="activeTab === 'reindex'" class="p-6">
          <div class="space-y-6">
            <div>
              <h3 class="text-lg font-semibold text-gray-900 mb-2">
                전체 데이터 재색인
              </h3>
              <p class="text-gray-600">
                시스템의 모든 정보를 다시 읽어 챗봇의 지식 베이스를
                업데이트합니다.
              </p>
            </div>

            <div class="bg-gray-50 rounded-lg p-4">
              <div class="flex items-start justify-between">
                <div class="flex-1">
                  <h4 class="font-medium text-gray-900 mb-1">자동 색인 항목</h4>
                  <ul class="text-sm text-gray-600 space-y-1">
                    <li>• 모든 행사 정보 (제목, 날짜, 장소, 설명)</li>
                    <li>• 참가자 정보 (이름, 소속, 일정)</li>
                    <li>• 일정표 및 프로그램</li>
                    <li>• 공지사항 및 문서</li>
                  </ul>
                </div>
                <button
                  @click="handleFullReindex"
                  :disabled="reindexing"
                  class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 transition-colors"
                >
                  <span v-if="reindexing">색인 중...</span>
                  <span v-else>재색인 시작</span>
                </button>
              </div>
            </div>

            <div
              v-if="reindexResult"
              :class="[
                'p-4 rounded-lg',
                reindexResult.success
                  ? 'bg-green-50 text-green-800'
                  : 'bg-red-50 text-red-800',
              ]"
            >
              {{ reindexResult.message }}
            </div>
          </div>
        </div>

        <div v-show="activeTab === 'conventions'" class="p-6">
          <div class="space-y-4">
            <div class="flex items-center justify-between mb-4">
              <h3 class="text-lg font-semibold text-gray-900">
                행사별 챗봇 설정
              </h3>
              <input
                v-model="searchQuery"
                type="text"
                placeholder="행사 검색..."
                class="px-4 py-2 border border-gray-300 rounded-lg w-64"
              />
            </div>

            <div class="space-y-3">
              <div
                v-for="convention in filteredConventions"
                :key="convention.id"
                class="border border-gray-200 rounded-lg p-4 hover:shadow-sm transition-shadow"
              >
                <div class="flex items-start justify-between">
                  <div class="flex-1">
                    <h4 class="font-medium text-gray-900">
                      {{ convention.title }}
                    </h4>
                    <p class="text-sm text-gray-500 mt-1">
                      {{
                        convention.startDate
                          ? new Date(convention.startDate).toLocaleDateString(
                              'ko-KR',
                            )
                          : '날짜 미정'
                      }}
                      - 참가자 {{ convention.guestCount }}명
                    </p>
                    <div class="flex items-center gap-4 mt-2">
                      <span class="text-xs text-gray-600"
                        >벡터 문서: {{ convention.vectorCount }}개</span
                      >
                      <span
                        :class="[
                          'text-xs px-2 py-1 rounded',
                          convention.chatbotEnabled
                            ? 'bg-green-100 text-green-700'
                            : 'bg-gray-100 text-gray-600',
                        ]"
                      >
                        {{
                          convention.chatbotEnabled
                            ? '챗봇 활성'
                            : '챗봇 비활성'
                        }}
                      </span>
                    </div>
                  </div>
                  <div class="flex gap-2">
                    <button
                      @click="showIndexedItems(convention.id)"
                      class="px-3 py-1 text-sm bg-purple-100 text-purple-700 rounded hover:bg-purple-200 transition-colors"
                    >
                      색인 상세
                    </button>
                    <button
                      @click="reindexConvention(convention.id)"
                      class="px-3 py-1 text-sm bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition-colors"
                    >
                      재색인
                    </button>
                    <button
                      @click="
                        toggleChatbot(convention.id, !convention.chatbotEnabled)
                      "
                      :class="[
                        'px-3 py-1 text-sm rounded transition-colors',
                        convention.chatbotEnabled
                          ? 'bg-red-100 text-red-700 hover:bg-red-200'
                          : 'bg-green-100 text-green-700 hover:bg-green-200',
                      ]"
                    >
                      {{ convention.chatbotEnabled ? '비활성화' : '활성화' }}
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div v-show="activeTab === 'vectors'" class="p-6">
          <div class="space-y-6">
            <h3 class="text-lg font-semibold text-gray-900">
              벡터 데이터베이스 상세
            </h3>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div class="border border-gray-200 rounded-lg p-4">
                <h4 class="font-medium text-gray-700 mb-3">
                  데이터 소스별 분포
                </h4>
                <div class="space-y-2">
                  <div
                    v-for="source in vectorStats.bySouce"
                    :key="source.type"
                    class="flex items-center justify-between"
                  >
                    <span class="text-sm text-gray-600">{{
                      source.label
                    }}</span>
                    <div class="flex items-center gap-2">
                      <div class="w-24 bg-gray-200 rounded-full h-2">
                        <div
                          class="bg-blue-600 h-2 rounded-full"
                          :style="{ width: `${source.percentage}%` }"
                        ></div>
                      </div>
                      <span
                        class="text-sm font-medium text-gray-900 w-12 text-right"
                        >{{ source.count }}</span
                      >
                    </div>
                  </div>
                </div>
              </div>

              <div class="border border-gray-200 rounded-lg p-4">
                <h4 class="font-medium text-gray-700 mb-3">최근 색인 활동</h4>
                <div class="space-y-2">
                  <div
                    v-for="activity in recentActivities"
                    :key="activity.id"
                    class="text-sm"
                  >
                    <div class="flex items-center justify-between">
                      <span class="text-gray-600">{{ activity.action }}</span>
                      <span class="text-gray-400">{{
                        formatTimeAgo(activity.timestamp)
                      }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div v-show="activeTab === 'logs'" class="p-6">
          <div class="space-y-4">
            <div class="flex items-center justify-between">
              <h3 class="text-lg font-semibold text-gray-900">시스템 로그</h3>
              <button
                @click="fetchLogs"
                class="px-4 py-2 bg-gray-100 hover:bg-gray-200 rounded-lg text-sm"
              >
                로그 새로고침
              </button>
            </div>

            <div
              class="bg-gray-900 rounded-lg p-4 h-96 overflow-y-auto font-mono text-sm"
            >
              <div
                v-for="(log, index) in logs"
                :key="index"
                :class="[
                  'py-1',
                  log.level === 'error'
                    ? 'text-red-400'
                    : log.level === 'warn'
                      ? 'text-yellow-400'
                      : log.level === 'info'
                        ? 'text-blue-400'
                        : 'text-gray-400',
                ]"
              >
                <span class="text-gray-500">[{{ log.timestamp }}]</span>
                <span class="ml-2">{{ log.message }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 색인 상세 정보 모달 -->
      <div
        v-if="showIndexDetailModal"
        class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
        @click="showIndexDetailModal = false"
      >
        <div
          class="bg-white rounded-lg p-6 w-full max-w-3xl max-h-[80vh] overflow-y-auto"
          @click.stop
        >
          <div class="flex items-center justify-between mb-6">
            <h3 class="text-xl font-bold text-gray-900">색인 상세 정보</h3>
            <button
              @click="showIndexDetailModal = false"
              class="text-gray-400 hover:text-gray-600"
            >
              <svg
                class="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>
          </div>

          <div v-if="loadingIndexDetail" class="text-center py-12">
            <div
              class="inline-block w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin"
            ></div>
            <p class="mt-4 text-gray-600">로딩 중...</p>
          </div>

          <div v-else-if="indexedItemsDetail" class="space-y-6">
            <!-- 전체 벡터 수 -->
            <div
              class="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-lg p-6 border border-blue-200"
            >
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm text-gray-600 mb-1">총 벡터 문서</p>
                  <p class="text-3xl font-bold text-blue-600">
                    {{ indexedItemsDetail.vectorCount }}
                  </p>
                </div>
                <div class="p-4 bg-white rounded-full shadow-sm">
                  <svg
                    class="w-8 h-8 text-blue-600"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                    />
                  </svg>
                </div>
              </div>
            </div>

            <!-- 색인 항목 상세 -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <!-- 행사 기본 정보 -->
              <div
                class="border rounded-lg p-4 hover:shadow-sm transition-shadow"
              >
                <div class="flex items-center justify-between mb-3">
                  <h4 class="font-semibold text-gray-900">행사 기본 정보</h4>
                  <span
                    :class="[
                      'px-2 py-1 text-xs rounded-full',
                      indexedItemsDetail.conventionInfo.indexed
                        ? 'bg-green-100 text-green-700'
                        : 'bg-gray-100 text-gray-600',
                    ]"
                  >
                    {{
                      indexedItemsDetail.conventionInfo.indexed
                        ? '✓ 색인됨'
                        : '미색인'
                    }}
                  </span>
                </div>
                <div class="text-sm text-gray-600 space-y-1">
                  <p>
                    <span class="font-medium">제목:</span>
                    {{ indexedItemsDetail.conventionInfo.title }}
                  </p>
                  <p>
                    <span class="font-medium">기간:</span>
                    {{
                      formatDate(indexedItemsDetail.conventionInfo.startDate)
                    }}
                    ~
                    {{ formatDate(indexedItemsDetail.conventionInfo.endDate) }}
                  </p>
                  <p>
                    <span class="font-medium">유형:</span>
                    {{ indexedItemsDetail.conventionInfo.type }}
                  </p>
                </div>
              </div>

              <!-- 참석자 통계 -->
              <div
                class="border rounded-lg p-4 hover:shadow-sm transition-shadow"
              >
                <div class="flex items-center justify-between mb-3">
                  <h4 class="font-semibold text-gray-900">참석자 통계</h4>
                  <span
                    :class="[
                      'px-2 py-1 text-xs rounded-full',
                      indexedItemsDetail.guestSummary.indexed
                        ? 'bg-green-100 text-green-700'
                        : 'bg-gray-100 text-gray-600',
                    ]"
                  >
                    {{
                      indexedItemsDetail.guestSummary.indexed
                        ? '✓ 색인됨'
                        : '미색인'
                    }}
                  </span>
                </div>
                <div class="text-sm text-gray-600">
                  <p class="text-2xl font-bold text-gray-900">
                    {{ indexedItemsDetail.guestSummary.totalCount }}명
                  </p>
                  <p class="mt-1">부서별/소속별 통계 포함</p>
                </div>
              </div>

              <!-- 일정 템플릿 -->
              <div
                class="border rounded-lg p-4 hover:shadow-sm transition-shadow"
              >
                <div class="flex items-center justify-between mb-3">
                  <h4 class="font-semibold text-gray-900">일정 템플릿</h4>
                  <span
                    :class="[
                      'px-2 py-1 text-xs rounded-full',
                      indexedItemsDetail.schedules.indexed
                        ? 'bg-green-100 text-green-700'
                        : 'bg-gray-100 text-gray-600',
                    ]"
                  >
                    {{
                      indexedItemsDetail.schedules.indexed
                        ? '✓ 색인됨'
                        : '미색인'
                    }}
                  </span>
                </div>
                <div class="text-sm text-gray-600 space-y-1">
                  <p>
                    <span class="font-medium">템플릿 수:</span>
                    {{ indexedItemsDetail.schedules.templateCount }}개
                  </p>
                  <p>
                    <span class="font-medium">일정 항목:</span>
                    {{ indexedItemsDetail.schedules.itemCount }}개
                  </p>
                </div>
              </div>

              <!-- 공지사항 -->
              <div
                class="border rounded-lg p-4 hover:shadow-sm transition-shadow"
              >
                <div class="flex items-center justify-between mb-3">
                  <h4 class="font-semibold text-gray-900">공지사항</h4>
                  <span
                    :class="[
                      'px-2 py-1 text-xs rounded-full',
                      indexedItemsDetail.notices.indexed
                        ? 'bg-green-100 text-green-700'
                        : 'bg-gray-100 text-gray-600',
                    ]"
                  >
                    {{
                      indexedItemsDetail.notices.indexed ? '✓ 색인됨' : '미색인'
                    }}
                  </span>
                </div>
                <div class="text-sm text-gray-600">
                  <p class="text-2xl font-bold text-gray-900">
                    {{ indexedItemsDetail.notices.count }}개
                  </p>
                  <p class="mt-1">고정 공지 및 일반 공지</p>
                </div>
              </div>

              <!-- 액션 항목 -->
              <div
                class="border rounded-lg p-4 hover:shadow-sm transition-shadow"
              >
                <div class="flex items-center justify-between mb-3">
                  <h4 class="font-semibold text-gray-900">액션 항목</h4>
                  <span
                    :class="[
                      'px-2 py-1 text-xs rounded-full',
                      indexedItemsDetail.actions.indexed
                        ? 'bg-green-100 text-green-700'
                        : 'bg-gray-100 text-gray-600',
                    ]"
                  >
                    {{
                      indexedItemsDetail.actions.indexed ? '✓ 색인됨' : '미색인'
                    }}
                  </span>
                </div>
                <div class="text-sm text-gray-600">
                  <p class="text-2xl font-bold text-gray-900">
                    {{ indexedItemsDetail.actions.count }}개
                  </p>
                  <p class="mt-1">활성 액션 및 할 일 목록</p>
                </div>
              </div>
            </div>

            <div class="flex justify-end pt-4 border-t">
              <button
                @click="showIndexDetailModal = false"
                class="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700"
              >
                닫기
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
// [REFACTORED] apiClient 대신 chatbotAdminAPI를 import
import { chatbotAdminAPI } from '@/services/chatbotAdminService'
import LlmProviderManagement from '@/components/admin/LlmProviderManagement.vue'

const router = useRouter()

// State
const activeTab = ref('providers')
const reindexing = ref(false)
const reindexResult = ref(null)
const searchQuery = ref('')

const stats = ref({
  totalDocuments: 0,
  activeConventions: 0,
  totalGuests: 0,
  dbSize: 0,
})

const conventions = ref([])
const vectorStats = ref({
  bySouce: [],
})
const recentActivities = ref([])
const logs = ref([])
const showIndexDetailModal = ref(false)
const loadingIndexDetail = ref(false)
const indexedItemsDetail = ref(null)

const tabs = [
  { id: 'providers', label: 'LLM Provider' },
  { id: 'reindex', label: '전체 재색인' },
  { id: 'conventions', label: '행사별 설정' },
  { id: 'vectors', label: '벡터 DB 통계' },
  { id: 'logs', label: '시스템 로그' },
]

// Computed
const filteredConventions = computed(() => {
  if (!searchQuery.value) return conventions.value
  const query = searchQuery.value.toLowerCase()
  return conventions.value.filter((c) => c.title.toLowerCase().includes(query))
})

// Methods
async function fetchAllData() {
  await Promise.all([
    fetchStats(),
    fetchConventions(),
    fetchVectorStats(),
    fetchRecentActivities(),
  ])
}

async function fetchStats() {
  try {
    // [REFACTORED]
    const response = await chatbotAdminAPI.getStatus()
    stats.value = response.data
  } catch (error) {
    console.error('Failed to fetch stats:', error)
  }
}

async function fetchConventions() {
  try {
    // [REFACTORED]
    const response = await chatbotAdminAPI.getConventions()
    conventions.value = response.data
  } catch (error) {
    console.error('Failed to fetch conventions:', error)
  }
}

async function fetchVectorStats() {
  try {
    // [REFACTORED]
    const response = await chatbotAdminAPI.getVectorStats()
    vectorStats.value = response.data
  } catch (error) {
    console.error('Failed to fetch vector stats:', error)
  }
}

async function fetchRecentActivities() {
  try {
    // [REFACTORED]
    const response = await chatbotAdminAPI.getRecentActivities()
    recentActivities.value = response.data
  } catch (error) {
    console.error('Failed to fetch recent activities:', error)
  }
}

async function fetchLogs() {
  try {
    // [REFACTORED]
    const response = await chatbotAdminAPI.getLogs()
    logs.value = response.data
  } catch (error) {
    console.error('Failed to fetch logs:', error)
  }
}

async function handleFullReindex() {
  if (!confirm('모든 데이터를 다시 색인하시겠습니까?')) return

  reindexing.value = true
  reindexResult.value = null

  try {
    // [REFACTORED]
    const response = await chatbotAdminAPI.reindexAll()
    reindexResult.value = {
      success: true,
      message: response.data.message || '재색인이 완료되었습니다.',
    }
    await fetchStats()
  } catch (error) {
    reindexResult.value = {
      success: false,
      message: error.response?.data?.message || '재색인에 실패했습니다.',
    }
  } finally {
    reindexing.value = false
  }
}

async function reindexConvention(conventionId) {
  try {
    // [REFACTORED]
    await chatbotAdminAPI.reindexConvention(conventionId)
    alert('행사 재색인이 시작되었습니다.')
    await fetchConventions()
  } catch (error) {
    alert('재색인에 실패했습니다: ' + error.message)
  }
}

async function toggleChatbot(conventionId, enabled) {
  try {
    // [REFACTORED]
    await chatbotAdminAPI.toggleChatbot(conventionId, enabled)
    await fetchConventions()
  } catch (error) {
    alert('설정 변경에 실패했습니다: ' + error.message)
  }
}

async function showIndexedItems(conventionId) {
  showIndexDetailModal.value = true
  loadingIndexDetail.value = true
  indexedItemsDetail.value = null

  try {
    const response = await chatbotAdminAPI.getIndexedItems(conventionId)
    indexedItemsDetail.value = response.data
  } catch (error) {
    console.error('Failed to fetch indexed items:', error)
    alert('색인 상세 정보를 불러오는데 실패했습니다: ' + error.message)
    showIndexDetailModal.value = false
  } finally {
    loadingIndexDetail.value = false
  }
}

function formatDate(dateString) {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleDateString('ko-KR')
}

function formatBytes(bytes) {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i]
}

function formatTimeAgo(timestamp) {
  const diff = Date.now() - new Date(timestamp).getTime()
  const minutes = Math.floor(diff / 60000)
  const hours = Math.floor(minutes / 60)
  const days = Math.floor(hours / 24)

  if (days > 0) return `${days}일 전`
  if (hours > 0) return `${hours}시간 전`
  if (minutes > 0) return `${minutes}분 전`
  return '방금 전'
}

onMounted(() => {
  fetchAllData()
  fetchLogs()
})
</script>
