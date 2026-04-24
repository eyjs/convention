<template>
  <div class="min-h-[60vh]">
    <!-- Header -->
    <div class="mb-6">
      <button
        class="inline-flex items-center gap-1.5 text-sm text-gray-500 dark:text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors mb-3"
        @click="emit('back')"
      >
        <ArrowLeft class="w-4 h-4" />
        <span>목록으로 돌아가기</span>
      </button>
      <div
        v-if="stats"
        class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2"
      >
        <div>
          <h1 class="text-xl font-bold text-gray-900 dark:text-gray-100">
            {{ stats.surveyTitle }}
          </h1>
          <p class="text-sm text-gray-500 dark:text-gray-400 mt-0.5">
            설문 통계 및 응답 관리
          </p>
        </div>
      </div>
      <div v-else>
        <h1 class="text-xl font-bold text-gray-900 dark:text-gray-100">
          설문 통계
        </h1>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3">
        <div
          class="w-8 h-8 border-2 border-indigo-200 border-t-indigo-600 rounded-full animate-spin"
        ></div>
        <p class="text-sm text-gray-500 dark:text-gray-400">
          통계를 불러오는 중...
        </p>
      </div>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex items-center justify-center py-20">
      <div class="text-center">
        <div
          class="w-12 h-12 rounded-full bg-red-50 dark:bg-red-900/20 flex items-center justify-center mx-auto mb-3"
        >
          <BarChart3 class="w-6 h-6 text-red-500" />
        </div>
        <p class="text-sm text-red-600 dark:text-red-400">{{ error }}</p>
      </div>
    </div>

    <div v-else-if="stats">
      <!-- Pill Tabs -->
      <div
        class="flex flex-wrap items-center gap-1 p-1 bg-gray-100 dark:bg-gray-800 rounded-xl mb-6 w-fit"
      >
        <button
          class="flex items-center gap-2 px-4 py-2 text-sm font-medium rounded-lg transition-all"
          :class="
            activeTab === 'summary'
              ? 'bg-white dark:bg-gray-700 text-indigo-600 dark:text-indigo-400 shadow-sm'
              : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300'
          "
          @click="activeTab = 'summary'"
        >
          <BarChart3 class="w-4 h-4" />
          통계 요약
        </button>
        <button
          class="flex items-center gap-2 px-4 py-2 text-sm font-medium rounded-lg transition-all"
          :class="
            activeTab === 'responses'
              ? 'bg-white dark:bg-gray-700 text-indigo-600 dark:text-indigo-400 shadow-sm'
              : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300'
          "
          @click="switchToResponses"
        >
          <MessageSquare class="w-4 h-4" />
          개별 응답
        </button>
      </div>

      <!-- 통계 요약 탭 -->
      <div v-if="activeTab === 'summary'">
        <!-- Summary Cards -->
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
          <div
            class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl p-5 flex items-center gap-4"
          >
            <div
              class="flex-shrink-0 w-11 h-11 rounded-lg bg-indigo-50 dark:bg-indigo-900/30 flex items-center justify-center"
            >
              <Users class="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
            </div>
            <div>
              <p
                class="text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wide"
              >
                총 응답 수
              </p>
              <p class="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {{ stats.totalResponses }}
              </p>
            </div>
          </div>

          <div
            class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl p-5 flex items-center gap-4"
          >
            <div
              class="flex-shrink-0 w-11 h-11 rounded-lg bg-emerald-50 dark:bg-emerald-900/30 flex items-center justify-center"
            >
              <MessageSquare
                class="w-5 h-5 text-emerald-600 dark:text-emerald-400"
              />
            </div>
            <div>
              <p
                class="text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wide"
              >
                질문 수
              </p>
              <p class="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {{ stats.questionStats.length }}
              </p>
            </div>
          </div>

          <div
            class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl p-5 flex items-center gap-4"
          >
            <div
              class="flex-shrink-0 w-11 h-11 rounded-lg bg-amber-50 dark:bg-amber-900/30 flex items-center justify-center"
            >
              <BarChart3 class="w-5 h-5 text-amber-600 dark:text-amber-400" />
            </div>
            <div>
              <p
                class="text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wide"
              >
                응답률
              </p>
              <p class="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {{ responseRate
                }}<span class="text-base font-medium text-gray-400 ml-0.5"
                  >%</span
                >
              </p>
            </div>
          </div>
        </div>

        <!-- Question Stats -->
        <div class="space-y-5">
          <div
            v-for="(qStats, qIndex) in stats.questionStats"
            :key="qStats.questionId"
            class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl overflow-hidden"
          >
            <!-- Question Header -->
            <div
              class="px-5 py-4 border-b border-gray-100 dark:border-gray-700/50 flex items-start gap-3"
            >
              <span
                class="flex-shrink-0 w-7 h-7 rounded-lg bg-indigo-100 dark:bg-indigo-900/40 text-indigo-600 dark:text-indigo-400 text-sm font-bold flex items-center justify-center mt-0.5"
              >
                {{ qIndex + 1 }}
              </span>
              <div>
                <h3
                  class="text-sm font-semibold text-gray-900 dark:text-gray-100 leading-snug"
                >
                  {{ qStats.questionText }}
                </h3>
                <span
                  class="inline-block mt-1 text-xs font-medium px-2 py-0.5 rounded-full bg-gray-100 dark:bg-gray-700 text-gray-500 dark:text-gray-400"
                >
                  {{ getQuestionTypeLabel(qStats.questionType) }}
                </span>
              </div>
            </div>

            <!-- Choice Answers -->
            <div v-if="isChoiceType(qStats.questionType)" class="px-5 py-4">
              <ul class="space-y-3">
                <li
                  v-for="(answer, aIdx) in getSortedAnswers(qStats)"
                  :key="answer.answer"
                >
                  <div class="flex justify-between items-center mb-1.5">
                    <div class="flex items-center gap-2">
                      <span
                        v-if="aIdx === 0 && answer.count > 0"
                        class="text-xs font-bold text-emerald-600 dark:text-emerald-400"
                      >
                        1위
                      </span>
                      <span class="text-sm text-gray-700 dark:text-gray-300">
                        {{ answer.answer }}
                      </span>
                    </div>
                    <span
                      class="text-sm font-semibold tabular-nums"
                      :class="
                        aIdx === 0 && answer.count > 0
                          ? 'text-emerald-600 dark:text-emerald-400'
                          : 'text-gray-500 dark:text-gray-400'
                      "
                    >
                      {{ answer.count }}명
                      <span
                        class="font-normal text-gray-400 dark:text-gray-500 ml-0.5"
                        >({{ getPercentage(qStats, answer.count) }}%)</span
                      >
                    </span>
                  </div>
                  <div
                    class="w-full bg-gray-100 dark:bg-gray-700 rounded-full h-2.5 overflow-hidden"
                  >
                    <div
                      class="h-full rounded-full transition-all duration-500 ease-out"
                      :class="getBarColor(aIdx, answer.count)"
                      :style="{
                        width: getPercentage(qStats, answer.count) + '%',
                      }"
                    ></div>
                  </div>
                </li>
              </ul>
            </div>

            <!-- Text Answers -->
            <div v-else class="px-5 py-4">
              <div
                v-if="qStats.answers.length === 0"
                class="text-center py-6 text-sm text-gray-400 dark:text-gray-500"
              >
                제출된 텍스트 답변이 없습니다.
              </div>
              <div v-else class="space-y-2 max-h-64 overflow-y-auto pr-1">
                <div
                  v-for="(answer, aIndex) in qStats.answers"
                  :key="aIndex"
                  class="flex items-start gap-3 p-3 bg-gray-50 dark:bg-gray-700/40 rounded-lg"
                >
                  <span
                    class="flex-shrink-0 w-5 h-5 rounded-full bg-gray-200 dark:bg-gray-600 text-gray-500 dark:text-gray-400 text-xs font-medium flex items-center justify-center mt-0.5"
                  >
                    {{ aIndex + 1 }}
                  </span>
                  <p
                    class="text-sm text-gray-700 dark:text-gray-300 leading-relaxed"
                  >
                    {{ answer.answer }}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 개별 응답 탭 -->
      <div v-if="activeTab === 'responses'">
        <!-- Controls -->
        <div
          class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 mb-5"
        >
          <p class="text-sm text-gray-500 dark:text-gray-400">
            총
            <span class="font-semibold text-gray-700 dark:text-gray-300">{{
              sortedResponses.length
            }}</span
            >건의 응답
          </p>
          <div class="flex flex-col sm:flex-row gap-2 w-full sm:w-auto">
            <div class="relative">
              <Search
                class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400"
              />
              <input
                v-model="responseSearch"
                type="text"
                placeholder="이름/그룹 검색..."
                class="pl-9 pr-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-200 text-sm w-full sm:w-52 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-colors"
              />
            </div>
            <!-- prettier-ignore -->
            <button
              class="inline-flex items-center justify-center gap-2 px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 text-sm font-medium whitespace-nowrap transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              :disabled="isExporting"
              @click="exportToExcel"
            >
              <Download class="w-4 h-4" />
              {{ isExporting ? '다운로드 중...' : '엑셀 다운로드' }}
            </button>
          </div>
        </div>

        <!-- View Toggle -->
        <div class="flex items-center gap-2 mb-4">
          <button
            class="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium rounded-lg transition-colors"
            :class="
              responseViewMode === 'table'
                ? 'bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400'
                : 'text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-700'
            "
            @click="responseViewMode = 'table'"
          >
            <FileSpreadsheet class="w-3.5 h-3.5" />
            테이블
          </button>
          <button
            class="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium rounded-lg transition-colors"
            :class="
              responseViewMode === 'card'
                ? 'bg-indigo-50 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400'
                : 'text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-700'
            "
            @click="responseViewMode = 'card'"
          >
            <Users class="w-3.5 h-3.5" />
            카드
          </button>
        </div>

        <!-- Loading -->
        <div
          v-if="responsesLoading"
          class="flex items-center justify-center py-16"
        >
          <div class="flex flex-col items-center gap-3">
            <div
              class="w-8 h-8 border-2 border-indigo-200 border-t-indigo-600 rounded-full animate-spin"
            ></div>
            <p class="text-sm text-gray-500">응답 목록을 불러오는 중...</p>
          </div>
        </div>

        <!-- Empty -->
        <div v-else-if="sortedResponses.length === 0" class="text-center py-16">
          <div
            class="w-12 h-12 rounded-full bg-gray-100 dark:bg-gray-700 flex items-center justify-center mx-auto mb-3"
          >
            <MessageSquare class="w-6 h-6 text-gray-400" />
          </div>
          <p class="text-sm text-gray-400 dark:text-gray-500">
            {{
              responseSearch
                ? '검색 결과가 없습니다.'
                : '제출된 응답이 없습니다.'
            }}
          </p>
        </div>

        <!-- Table View -->
        <div
          v-else-if="responseViewMode === 'table'"
          class="border border-gray-200 dark:border-gray-700 rounded-xl overflow-x-auto"
        >
          <table
            class="min-w-full divide-y divide-gray-200 dark:divide-gray-700"
          >
            <thead class="bg-gray-50 dark:bg-gray-800/80 sticky top-0 z-10">
              <tr>
                <th
                  class="px-5 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider cursor-pointer select-none hover:text-gray-700 dark:hover:text-gray-300 transition-colors"
                  @click="toggleSort('userName')"
                >
                  <span class="inline-flex items-center gap-1">
                    이름
                    <ChevronUp
                      v-if="sortField === 'userName' && sortAsc"
                      class="w-3.5 h-3.5"
                    />
                    <ChevronDown
                      v-else-if="sortField === 'userName' && !sortAsc"
                      class="w-3.5 h-3.5"
                    />
                  </span>
                </th>
                <th
                  class="px-5 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider cursor-pointer select-none hover:text-gray-700 dark:hover:text-gray-300 transition-colors"
                  @click="toggleSort('groupName')"
                >
                  <span class="inline-flex items-center gap-1">
                    그룹
                    <ChevronUp
                      v-if="sortField === 'groupName' && sortAsc"
                      class="w-3.5 h-3.5"
                    />
                    <ChevronDown
                      v-else-if="sortField === 'groupName' && !sortAsc"
                      class="w-3.5 h-3.5"
                    />
                  </span>
                </th>
                <th
                  class="px-5 py-3 text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider cursor-pointer select-none hover:text-gray-700 dark:hover:text-gray-300 transition-colors"
                  @click="toggleSort('submittedAt')"
                >
                  <span class="inline-flex items-center gap-1">
                    제출일
                    <ChevronUp
                      v-if="sortField === 'submittedAt' && sortAsc"
                      class="w-3.5 h-3.5"
                    />
                    <ChevronDown
                      v-else-if="sortField === 'submittedAt' && !sortAsc"
                      class="w-3.5 h-3.5"
                    />
                  </span>
                </th>
                <th
                  class="px-5 py-3 text-right text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider"
                >
                  상세
                </th>
              </tr>
            </thead>
            <tbody
              class="bg-white dark:bg-gray-800 divide-y divide-gray-100 dark:divide-gray-700/50"
            >
              <template v-for="resp in sortedResponses" :key="resp.responseId">
                <tr
                  class="hover:bg-gray-50 dark:hover:bg-gray-700/30 transition-colors"
                >
                  <td
                    class="px-5 py-3.5 whitespace-nowrap text-sm font-medium text-gray-900 dark:text-gray-200"
                  >
                    {{ resp.userName }}
                  </td>
                  <td
                    class="px-5 py-3.5 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400"
                  >
                    {{ resp.groupName || '-' }}
                  </td>
                  <td
                    class="px-5 py-3.5 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400"
                  >
                    {{ formatDateTime(resp.submittedAt) }}
                  </td>
                  <td class="px-5 py-3.5 whitespace-nowrap text-right">
                    <!-- prettier-ignore -->
                    <button
                      class="inline-flex items-center gap-1 text-sm font-medium transition-colors"
                      :class="expandedId === resp.responseId ? 'text-indigo-600 dark:text-indigo-400' : 'text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400'"
                      @click="toggleExpand(resp.responseId)"
                    >
                      <Eye v-if="expandedId !== resp.responseId" class="w-4 h-4" />
                      <EyeOff v-else class="w-4 h-4" />
                      {{ expandedId === resp.responseId ? '접기' : '보기' }}
                    </button>
                  </td>
                </tr>
                <!-- Expanded Row -->
                <tr v-if="expandedId === resp.responseId">
                  <td
                    colspan="4"
                    class="px-5 py-4 bg-gray-50/80 dark:bg-gray-900/30"
                  >
                    <div class="grid gap-3 sm:grid-cols-2">
                      <div
                        v-for="answer in resp.answers"
                        :key="answer.questionId"
                        class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-3"
                      >
                        <p
                          class="text-xs font-medium text-gray-500 dark:text-gray-400 mb-1 flex items-center gap-1.5"
                        >
                          {{ answer.questionText }}
                          <span
                            class="text-[10px] px-1.5 py-0.5 rounded bg-gray-100 dark:bg-gray-700 text-gray-400 dark:text-gray-500"
                          >
                            {{ getQuestionTypeLabel(answer.questionType) }}
                          </span>
                        </p>
                        <p class="text-sm text-gray-900 dark:text-gray-200">
                          <template
                            v-if="
                              answer.selectedOptions &&
                              answer.selectedOptions.length > 0
                            "
                          >
                            {{ answer.selectedOptions.join(', ') }}
                          </template>
                          <template v-else-if="answer.answerText">
                            {{ answer.answerText }}
                          </template>
                          <span
                            v-else
                            class="text-gray-300 dark:text-gray-600 italic"
                            >미응답</span
                          >
                        </p>
                      </div>
                    </div>
                  </td>
                </tr>
              </template>
            </tbody>
          </table>
        </div>

        <!-- Card View -->
        <div
          v-else-if="responseViewMode === 'card'"
          class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3"
        >
          <div
            v-for="resp in sortedResponses"
            :key="resp.responseId"
            class="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl overflow-hidden"
          >
            <!-- Card Header -->
            <div
              class="px-4 py-3 border-b border-gray-100 dark:border-gray-700/50 flex items-center justify-between"
            >
              <div>
                <p
                  class="text-sm font-semibold text-gray-900 dark:text-gray-100"
                >
                  {{ resp.userName }}
                </p>
                <p class="text-xs text-gray-400 dark:text-gray-500">
                  {{ resp.groupName || '그룹 없음' }} ·
                  {{ formatDateTime(resp.submittedAt) }}
                </p>
              </div>
              <!-- prettier-ignore -->
              <button
                class="p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors"
                @click="toggleExpand(resp.responseId)"
              >
                <ChevronUp v-if="expandedId === resp.responseId" class="w-4 h-4" />
                <ChevronDown v-else class="w-4 h-4" />
              </button>
            </div>

            <!-- Card Body (collapsed: first 2 answers preview) -->
            <div
              v-if="expandedId !== resp.responseId"
              class="px-4 py-3 space-y-2"
            >
              <div
                v-for="answer in resp.answers.slice(0, 2)"
                :key="answer.questionId"
              >
                <p
                  class="text-[11px] font-medium text-gray-400 dark:text-gray-500 truncate"
                >
                  {{ answer.questionText }}
                </p>
                <p class="text-xs text-gray-700 dark:text-gray-300 truncate">
                  <template
                    v-if="
                      answer.selectedOptions &&
                      answer.selectedOptions.length > 0
                    "
                  >
                    {{ answer.selectedOptions.join(', ') }}
                  </template>
                  <template v-else-if="answer.answerText">
                    {{ answer.answerText }}
                  </template>
                  <span v-else class="text-gray-300 dark:text-gray-600 italic"
                    >미응답</span
                  >
                </p>
              </div>
              <p
                v-if="resp.answers.length > 2"
                class="text-[11px] text-gray-400"
              >
                +{{ resp.answers.length - 2 }}개 더보기
              </p>
            </div>

            <!-- Card Body (expanded: all answers) -->
            <div
              v-else
              class="px-4 py-3 space-y-2.5 bg-gray-50/50 dark:bg-gray-900/20"
            >
              <div
                v-for="answer in resp.answers"
                :key="answer.questionId"
                class="border-l-2 border-indigo-300 dark:border-indigo-600 pl-3"
              >
                <p
                  class="text-[11px] font-medium text-gray-400 dark:text-gray-500"
                >
                  {{ answer.questionText }}
                  <span
                    class="text-[10px] ml-1 text-gray-300 dark:text-gray-600"
                    >{{ getQuestionTypeLabel(answer.questionType) }}</span
                  >
                </p>
                <p class="text-xs text-gray-800 dark:text-gray-200 mt-0.5">
                  <template
                    v-if="
                      answer.selectedOptions &&
                      answer.selectedOptions.length > 0
                    "
                  >
                    {{ answer.selectedOptions.join(', ') }}
                  </template>
                  <template v-else-if="answer.answerText">
                    {{ answer.answerText }}
                  </template>
                  <span v-else class="text-gray-300 dark:text-gray-600 italic"
                    >미응답</span
                  >
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- No Stats -->
    <div v-else class="flex items-center justify-center py-20">
      <div class="text-center">
        <div
          class="w-12 h-12 rounded-full bg-gray-100 dark:bg-gray-700 flex items-center justify-center mx-auto mb-3"
        >
          <BarChart3 class="w-6 h-6 text-gray-400" />
        </div>
        <p class="text-sm text-gray-500 dark:text-gray-400">
          이 설문에 대한 통계가 없습니다.
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import {
  ArrowLeft,
  Download,
  Search,
  ChevronDown,
  ChevronUp,
  Users,
  MessageSquare,
  BarChart3,
  FileSpreadsheet,
  Eye,
  EyeOff,
} from 'lucide-vue-next'
import api from '@/services/api'
import { formatDateTime } from '@/utils/date'
import { getQuestionTypeLabel, isChoiceType } from '@/constants/survey'

const props = defineProps({
  surveyId: {
    type: Number,
    required: true,
  },
})

const emit = defineEmits(['back'])

const stats = ref(null)
const loading = ref(true)
const error = ref(null)
const activeTab = ref('summary')

// 개별 응답
const responses = ref([])
const responsesLoading = ref(false)
const expandedId = ref(null)
const isExporting = ref(false)
const responseViewMode = ref('table')

// 검색/정렬
const responseSearch = ref('')
const sortField = ref('submittedAt')
const sortAsc = ref(false)

const responseRate = computed(() => {
  if (!stats.value || !stats.value.questionStats.length) return 0
  const totalQuestions = stats.value.questionStats.length
  const totalResponses = stats.value.totalResponses
  if (totalResponses === 0) return 0
  // 응답이 있는 질문 비율
  const answeredQuestions = stats.value.questionStats.filter(
    (q) => q.answers && q.answers.length > 0,
  ).length
  return Math.round((answeredQuestions / totalQuestions) * 100)
})

const sortedResponses = computed(() => {
  let filtered = responses.value

  if (responseSearch.value) {
    const q = responseSearch.value.toLowerCase()
    filtered = filtered.filter(
      (r) =>
        r.userName?.toLowerCase().includes(q) ||
        r.groupName?.toLowerCase().includes(q),
    )
  }

  return [...filtered].sort((a, b) => {
    const field = sortField.value
    let aVal = a[field] ?? ''
    let bVal = b[field] ?? ''

    if (field === 'submittedAt') {
      aVal = new Date(aVal).getTime()
      bVal = new Date(bVal).getTime()
    } else {
      aVal = String(aVal).toLowerCase()
      bVal = String(bVal).toLowerCase()
    }

    if (aVal < bVal) return sortAsc.value ? -1 : 1
    if (aVal > bVal) return sortAsc.value ? 1 : -1
    return 0
  })
})

function getSortedAnswers(qStats) {
  return [...qStats.answers].sort((a, b) => b.count - a.count)
}

function getBarColor(index, count) {
  if (count === 0) return 'bg-gray-300 dark:bg-gray-600'
  if (index === 0) return 'bg-emerald-500'
  if (index === 1) return 'bg-indigo-500'
  if (index === 2) return 'bg-indigo-400'
  return 'bg-indigo-300 dark:bg-indigo-600'
}

function toggleSort(field) {
  if (sortField.value === field) {
    sortAsc.value = !sortAsc.value
  } else {
    sortField.value = field
    sortAsc.value = true
  }
}

async function fetchSurveyStats() {
  loading.value = true
  error.value = null
  try {
    const response = await api.get(`/surveys/${props.surveyId}/stats`)
    stats.value = response.data
  } catch {
    error.value = '설문 통계를 불러오는데 실패했습니다.'
  } finally {
    loading.value = false
  }
}

async function fetchResponses() {
  responsesLoading.value = true
  try {
    const response = await api.get(`/surveys/${props.surveyId}/responses`)
    responses.value = response.data
  } catch {
    error.value = '개별 응답을 불러오는데 실패했습니다.'
  } finally {
    responsesLoading.value = false
  }
}

function switchToResponses() {
  activeTab.value = 'responses'
  fetchResponses()
}

function toggleExpand(responseId) {
  expandedId.value = expandedId.value === responseId ? null : responseId
}

async function exportToExcel() {
  isExporting.value = true
  try {
    const response = await api.get(
      `/surveys/${props.surveyId}/responses/export`,
      { responseType: 'blob' },
    )
    const url = window.URL.createObjectURL(new Blob([response.data]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `survey_${props.surveyId}_responses.xlsx`)
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)
  } catch {
    error.value = '엑셀 다운로드에 실패했습니다.'
  } finally {
    isExporting.value = false
  }
}

onMounted(() => {
  fetchSurveyStats()
})

watch(
  () => props.surveyId,
  () => {
    stats.value = null
    responses.value = []
    expandedId.value = null
    activeTab.value = 'summary'
    fetchSurveyStats()
  },
)

function getPercentage(questionStats, count) {
  if (stats.value.totalResponses === 0) {
    return 0
  }
  if (questionStats.questionType === 'MULTIPLE_CHOICE') {
    const totalVotes = questionStats.answers.reduce(
      (sum, ans) => sum + ans.count,
      0,
    )
    return totalVotes === 0 ? 0 : Math.round((count / totalVotes) * 100)
  }
  return Math.round((count / stats.value.totalResponses) * 100)
}
</script>
