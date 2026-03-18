<template>
  <div class="bg-white rounded-lg shadow-sm p-6">
    <h3 class="text-lg font-bold text-gray-900 mb-4">데이터베이스 관리</h3>

    <!-- 연결 상태 -->
    <div v-if="status" class="mb-6 p-4 rounded-lg border" :class="statusClass">
      <div class="flex items-center space-x-2 mb-2">
        <svg
          v-if="status.canConnect"
          class="w-5 h-5 text-green-600"
          fill="currentColor"
          viewBox="0 0 20 20"
        >
          <path
            fill-rule="evenodd"
            d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
            clip-rule="evenodd"
          />
        </svg>
        <svg
          v-else
          class="w-5 h-5 text-red-600"
          fill="currentColor"
          viewBox="0 0 20 20"
        >
          <path
            fill-rule="evenodd"
            d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
            clip-rule="evenodd"
          />
        </svg>
        <span class="font-semibold">{{
          status.canConnect ? 'DB 연결 성공' : 'DB 연결 실패'
        }}</span>
      </div>

      <div class="text-sm space-y-1">
        <div>
          <strong>데이터베이스:</strong>
          {{ status.databaseExists ? '존재함' : '없음' }}
        </div>
        <div>
          <strong>적용된 Migration:</strong>
          {{ status.appliedMigrations?.length || 0 }}개
        </div>
        <div>
          <strong>대기 중인 Migration:</strong>
          {{ status.pendingMigrations?.length || 0 }}개
        </div>
        <div v-if="status.needsMigration" class="text-orange-600 font-semibold">
          ⚠️ Migration 필요
        </div>
        <div v-if="status.error" class="text-red-600 mt-2">
          {{ status.error }}
        </div>
      </div>
    </div>

    <!-- Migration 상세 분석 결과 -->
    <div v-if="analyses.length > 0" class="mb-6 space-y-4">
      <div class="flex items-center justify-between">
        <h4 class="font-bold text-gray-900">Migration 상세 분석</h4>
        <button
          class="text-sm text-blue-600 hover:text-blue-700 flex items-center space-x-1"
          @click="showAnalysisDetails = !showAnalysisDetails"
        >
          <span>{{ showAnalysisDetails ? '접기' : '펼치기' }}</span>
          <svg
            class="w-4 h-4"
            :class="{ 'rotate-180': showAnalysisDetails }"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M19 9l-7 7-7-7"
            />
          </svg>
        </button>
      </div>

      <Transition name="slide-fade">
        <div v-if="showAnalysisDetails" class="space-y-4">
          <div
            v-for="analysis in analyses"
            :key="analysis.migrationId"
            class="border rounded-lg p-4"
            :class="getRiskBorderClass(analysis.riskLevel)"
          >
            <!-- Migration 헤더 -->
            <div class="flex items-start justify-between mb-3">
              <div>
                <h5 class="font-bold text-gray-900">
                  {{ analysis.migrationName }}
                </h5>
                <p class="text-xs text-gray-500 mt-1">
                  {{ analysis.migrationId }}
                </p>
              </div>
              <div class="flex items-center space-x-2">
                <span
                  class="px-3 py-1 rounded-full text-sm font-bold"
                  :class="getRiskBadgeClass(analysis.riskLevel)"
                >
                  {{ analysis.overallRisk }}
                </span>
                <span
                  v-if="analysis.requiresBackup"
                  class="px-2 py-1 bg-yellow-100 text-yellow-800 text-xs rounded"
                  >백업 필요</span
                >
                <span
                  v-if="analysis.requiresReview"
                  class="px-2 py-1 bg-red-100 text-red-800 text-xs rounded"
                  >수동 검토</span
                >
              </div>
            </div>

            <!-- 위험 항목 목록 -->
            <div
              v-if="analysis.risks && analysis.risks.length > 0"
              class="mb-3"
            >
              <h6 class="text-sm font-semibold text-gray-700 mb-2">
                🔍 감지된 변경 사항:
              </h6>
              <div class="space-y-2">
                <div
                  v-for="(risk, idx) in analysis.risks"
                  :key="idx"
                  class="bg-gray-50 rounded p-3 border-l-4"
                  :class="getRiskBorderColorClass(risk.levelValue)"
                >
                  <div class="flex items-start justify-between">
                    <div class="flex-1">
                      <div class="flex items-center space-x-2 mb-1">
                        <span
                          class="text-xs font-bold px-2 py-0.5 rounded"
                          :class="getRiskLevelBadge(risk.levelValue)"
                          >{{ risk.level }}</span
                        >
                        <span class="text-xs font-semibold text-gray-700">{{
                          risk.type
                        }}</span>
                        <span v-if="risk.table" class="text-xs text-gray-500"
                          >테이블: {{ risk.table }}</span
                        >
                        <span v-if="risk.column" class="text-xs text-gray-500"
                          >컬럼: {{ risk.column }}</span
                        >
                      </div>
                      <p class="text-sm text-gray-900 mb-1">
                        {{ risk.description }}
                      </p>
                      <p class="text-xs text-gray-600 mb-1">
                        <strong>영향:</strong> {{ risk.impact }}
                      </p>
                      <p v-if="risk.mitigation" class="text-xs text-blue-600">
                        <strong>조치:</strong> {{ risk.mitigation }}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- 영향받는 테이블 -->
            <div
              v-if="
                analysis.affectedTables && analysis.affectedTables.length > 0
              "
              class="mb-3"
            >
              <h6 class="text-sm font-semibold text-gray-700 mb-2">
                📊 영향받는 테이블:
              </h6>
              <div class="bg-blue-50 border border-blue-200 rounded p-3">
                <table class="w-full text-sm">
                  <thead>
                    <tr class="border-b border-blue-300">
                      <th class="text-left py-1 text-gray-700">테이블</th>
                      <th class="text-left py-1 text-gray-700">현재 행 수</th>
                      <th class="text-left py-1 text-gray-700">작업</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr
                      v-for="table in analysis.affectedTables"
                      :key="table.tableName"
                      class="border-b border-blue-100 last:border-0"
                    >
                      <td class="py-1 font-semibold">{{ table.tableName }}</td>
                      <td class="py-1">
                        {{
                          table.currentRowCount >= 0
                            ? table.currentRowCount.toLocaleString() + ' 행'
                            : '알 수 없음'
                        }}
                      </td>
                      <td class="py-1 text-xs text-gray-600">
                        {{ table.operation }}
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <!-- 권장 사항 -->
            <div v-if="analysis.recommendation" class="bg-gray-100 rounded p-3">
              <h6 class="text-sm font-semibold text-gray-700 mb-2">
                💡 권장 사항:
              </h6>
              <p class="text-sm text-gray-800 whitespace-pre-line">
                {{ analysis.recommendation }}
              </p>
            </div>
          </div>
        </div>
      </Transition>
    </div>

    <!-- 액션 버튼 -->
    <div class="flex flex-wrap gap-3 mb-6">
      <button
        :disabled="loading"
        class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 transition-colors flex items-center space-x-2"
        @click="checkStatus"
      >
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
        <span v-if="loading">확인 중...</span>
        <span v-else>상태 확인</span>
      </button>

      <button
        v-if="status?.needsMigration"
        :disabled="loading"
        class="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:bg-gray-400 transition-colors flex items-center space-x-2"
        @click="analyzeMigrations"
      >
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
            d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"
          />
        </svg>
        <span v-if="loading">분석 중...</span>
        <span v-else>위험도 분석</span>
      </button>

      <button
        v-if="status?.needsMigration && !hasCriticalRisk"
        :disabled="loading"
        class="px-4 py-2 bg-orange-600 text-white rounded-lg hover:bg-orange-700 disabled:bg-gray-400 transition-colors"
        @click="dryRunMigration"
      >
        <span v-if="loading">확인 중...</span>
        <span v-else>미리보기 (Dry Run)</span>
      </button>

      <button
        v-if="status?.needsMigration"
        :disabled="loading || (hasCriticalRisk && !userApprovedCritical)"
        :class="[
          'px-4 py-2 rounded-lg transition-colors flex items-center space-x-2',
          hasCriticalRisk
            ? 'bg-red-600 text-white hover:bg-red-700 disabled:bg-gray-400'
            : 'bg-green-600 text-white hover:bg-green-700 disabled:bg-gray-400',
        ]"
        @click="confirmMigration"
      >
        <svg
          v-if="hasCriticalRisk"
          class="w-4 h-4"
          fill="currentColor"
          viewBox="0 0 20 20"
        >
          <path
            fill-rule="evenodd"
            d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
            clip-rule="evenodd"
          />
        </svg>
        <span v-if="loading">실행 중...</span>
        <span v-else-if="hasCriticalRisk">⚠️⚠️ 위험 - Migration 실행</span>
        <span v-else>Migration 실행</span>
      </button>

      <button
        :disabled="loading"
        class="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 disabled:bg-gray-400 transition-colors"
        @click="testConnection"
      >
        연결 테스트
      </button>
    </div>

    <!-- Critical Risk 경고 -->
    <div
      v-if="hasCriticalRisk && !userApprovedCritical"
      class="mb-6 bg-red-50 border-2 border-red-500 rounded-lg p-4"
    >
      <div class="flex items-start space-x-3">
        <svg
          class="w-6 h-6 text-red-600 flex-shrink-0 mt-0.5"
          fill="currentColor"
          viewBox="0 0 20 20"
        >
          <path
            fill-rule="evenodd"
            d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
            clip-rule="evenodd"
          />
        </svg>
        <div class="flex-1">
          <h5 class="font-bold text-red-900 mb-2">
            ⚠️⚠️ 매우 위험한 Migration 감지!
          </h5>
          <p class="text-sm text-red-800 mb-3">
            데이터 손실 위험이 있는 작업(DROP COLUMN, DROP TABLE 등)이 포함되어
            있습니다.
          </p>
          <label class="flex items-center space-x-2 cursor-pointer">
            <input
              v-model="userApprovedCritical"
              type="checkbox"
              class="w-4 h-4 text-red-600 focus:ring-red-500"
            />
            <span class="text-sm font-semibold text-red-900">
              위험을 이해했으며, 전체 DB 백업을 완료했습니다.
            </span>
          </label>
        </div>
      </div>
    </div>

    <!-- 결과 로그 -->
    <div v-if="logs.length > 0" class="mt-6">
      <div class="flex items-center justify-between mb-2">
        <h4 class="font-semibold text-gray-900">실행 로그</h4>
        <button
          class="text-xs text-gray-500 hover:text-gray-700"
          @click="logs = []"
        >
          지우기
        </button>
      </div>
      <div
        class="bg-gray-900 text-green-400 rounded-lg p-4 font-mono text-xs max-h-64 overflow-y-auto"
      >
        <div
          v-for="(log, index) in logs"
          :key="index"
          class="mb-1"
          :class="getLogClass(log.type)"
        >
          <span class="text-gray-500">{{ log.time }}</span> {{ log.message }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import apiClient from '@/services/api'

const status = ref(null)
const analyses = ref([])
const loading = ref(false)
const logs = ref([])
const showAnalysisDetails = ref(true)
const userApprovedCritical = ref(false)

const statusClass = computed(() => {
  if (!status.value) return 'border-gray-300'
  if (!status.value.canConnect) return 'border-red-300 bg-red-50'
  if (status.value.needsMigration) return 'border-orange-300 bg-orange-50'
  return 'border-green-300 bg-green-50'
})

const hasCriticalRisk = computed(() => {
  return analyses.value.some((a) => a.riskLevel >= 4) // Critical = 4
})

function getRiskBorderClass(riskLevel) {
  switch (riskLevel) {
    case 4:
      return 'border-red-500 bg-red-50'
    case 3:
      return 'border-orange-500 bg-orange-50'
    case 2:
      return 'border-yellow-500 bg-yellow-50'
    case 1:
      return 'border-blue-500 bg-blue-50'
    default:
      return 'border-green-500 bg-green-50'
  }
}

function getRiskBadgeClass(riskLevel) {
  switch (riskLevel) {
    case 4:
      return 'bg-red-600 text-white'
    case 3:
      return 'bg-orange-600 text-white'
    case 2:
      return 'bg-yellow-600 text-white'
    case 1:
      return 'bg-blue-600 text-white'
    default:
      return 'bg-green-600 text-white'
  }
}

function getRiskBorderColorClass(levelValue) {
  switch (levelValue) {
    case 4:
      return 'border-red-500'
    case 3:
      return 'border-orange-500'
    case 2:
      return 'border-yellow-500'
    case 1:
      return 'border-blue-500'
    default:
      return 'border-green-500'
  }
}

function getRiskLevelBadge(levelValue) {
  switch (levelValue) {
    case 4:
      return 'bg-red-100 text-red-800'
    case 3:
      return 'bg-orange-100 text-orange-800'
    case 2:
      return 'bg-yellow-100 text-yellow-800'
    case 1:
      return 'bg-blue-100 text-blue-800'
    default:
      return 'bg-green-100 text-green-800'
  }
}

function getLogClass(type) {
  switch (type) {
    case 'ERROR':
      return 'text-red-400'
    case 'WARN':
      return 'text-yellow-400'
    case 'SUCCESS':
      return 'text-green-400'
    default:
      return 'text-green-400'
  }
}

function addLog(message, type = 'INFO') {
  const now = new Date()
  logs.value.push({
    time: `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}:${now.getSeconds().toString().padStart(2, '0')}`,
    message,
    type,
  })
}

async function checkStatus() {
  loading.value = true
  addLog('[INFO] 데이터베이스 상태 확인 중...')

  try {
    const response = await apiClient.get('/admin/database/status')
    status.value = response.data
    addLog(
      `[SUCCESS] 상태 확인 완료: ${response.data.canConnect ? '연결 성공' : '연결 실패'}`,
      'SUCCESS',
    )

    if (response.data.needsMigration) {
      addLog(
        `[WARN] ${response.data.pendingMigrations.length}개의 Migration이 대기 중입니다.`,
        'WARN',
      )
      // 자동으로 분석 실행
      await analyzeMigrations()
    } else if (response.data.canConnect) {
      addLog('[SUCCESS] 데이터베이스가 최신 상태입니다.', 'SUCCESS')
    }
  } catch (error) {
    addLog(
      `[ERROR] 상태 확인 실패: ${error.response?.data?.error || error.message}`,
      'ERROR',
    )
    console.error('Failed to check database status:', error)
  } finally {
    loading.value = false
  }
}

async function analyzeMigrations() {
  loading.value = true
  addLog('[INFO] Migration 위험도 분석 중...')

  try {
    const response = await apiClient.get('/admin/database/analyze-migrations')
    analyses.value = response.data.analyses || []

    if (analyses.value.length > 0) {
      addLog(
        `[SUCCESS] ${analyses.value.length}개의 Migration 분석 완료`,
        'SUCCESS',
      )

      const criticalCount = analyses.value.filter(
        (a) => a.riskLevel >= 4,
      ).length
      const highCount = analyses.value.filter((a) => a.riskLevel === 3).length

      if (criticalCount > 0) {
        addLog(
          `[WARN] ⚠️⚠️ ${criticalCount}개의 매우 위험한 Migration 감지!`,
          'WARN',
        )
      }
      if (highCount > 0) {
        addLog(`[WARN] ⚠️ ${highCount}개의 주의 필요 Migration 감지`, 'WARN')
      }
    } else {
      addLog('[INFO] 분석할 Migration이 없습니다.')
    }
  } catch (error) {
    addLog(
      `[ERROR] 분석 실패: ${error.response?.data?.detail || error.message}`,
      'ERROR',
    )
  } finally {
    loading.value = false
  }
}

async function testConnection() {
  loading.value = true
  addLog('[INFO] 데이터베이스 연결 테스트 중...')

  try {
    const response = await apiClient.get('/admin/database/test-connection')
    addLog(`[SUCCESS] ${response.data.message}`, 'SUCCESS')
    addLog(`[INFO] Provider: ${response.data.provider}`)
  } catch (error) {
    addLog(
      `[ERROR] 연결 테스트 실패: ${error.response?.data?.detail || error.message}`,
      'ERROR',
    )
  } finally {
    loading.value = false
  }
}

async function dryRunMigration() {
  loading.value = true
  addLog('[INFO] Dry Run 모드로 Migration 확인 중...')

  try {
    const response = await apiClient.post('/admin/database/migrate?dryRun=true')
    addLog('[INFO] Dry Run 완료 (실제 실행하지 않음)')
    addLog(
      `[INFO] 적용 예정 Migration: ${response.data.pendingMigrations?.join(', ')}`,
    )
  } catch (error) {
    addLog(
      `[ERROR] Dry Run 실패: ${error.response?.data?.detail || error.message}`,
      'ERROR',
    )
  } finally {
    loading.value = false
  }
}

async function confirmMigration() {
  const confirmMessage = hasCriticalRisk.value
    ? `⚠️⚠️ 매우 위험한 Migration 실행 경고!\n\n` +
      `데이터 손실 위험이 있는 작업이 포함되어 있습니다.\n\n` +
      `적용될 Migration: ${status.value.pendingMigrations.length}개\n\n` +
      `⚠️ 이 작업은 되돌릴 수 없습니다!\n\n` +
      `정말로 실행하시겠습니까?`
    : `⚠️ Migration 실행 확인\n\n` +
      `적용될 Migration: ${status.value.pendingMigrations.length}개\n\n` +
      `계속하시겠습니까?`

  const confirmed = confirm(confirmMessage)

  if (!confirmed) {
    addLog('[CANCEL] Migration 실행이 취소되었습니다.')
    return
  }

  loading.value = true
  addLog('[WARN] Migration 실행 시작...', 'WARN')

  try {
    const response = await apiClient.post(
      '/admin/database/migrate?dryRun=false',
    )
    addLog(`[SUCCESS] ${response.data.message}`, 'SUCCESS')
    addLog(
      `[INFO] 실행된 Migration: ${response.data.executedMigrations?.join(', ')}`,
    )
    addLog(`[INFO] 소요 시간: ${response.data.duration}`)

    // 상태 다시 확인
    userApprovedCritical.value = false
    analyses.value = []
    await checkStatus()
  } catch (error) {
    addLog(
      `[ERROR] Migration 실패: ${error.response?.data?.detail || error.message}`,
      'ERROR',
    )
    console.error('Migration failed:', error)
    alert(`Migration 실패:\n${error.response?.data?.detail || error.message}`)
  } finally {
    loading.value = false
  }
}

// 초기 상태 확인
checkStatus()
</script>

<style scoped>
.slide-fade-enter-active {
  transition: all 0.3s ease-out;
}

.slide-fade-leave-active {
  transition: all 0.2s cubic-bezier(1, 0.5, 0.8, 1);
}

.slide-fade-enter-from,
.slide-fade-leave-to {
  transform: translateY(-10px);
  opacity: 0;
}

/* 스크롤바 스타일링 */
.overflow-y-auto::-webkit-scrollbar {
  width: 8px;
}
.overflow-y-auto::-webkit-scrollbar-track {
  background: #1f2937;
}
.overflow-y-auto::-webkit-scrollbar-thumb {
  background: #4b5563;
  border-radius: 4px;
}
.overflow-y-auto::-webkit-scrollbar-thumb:hover {
  background: #6b7280;
}
</style>
