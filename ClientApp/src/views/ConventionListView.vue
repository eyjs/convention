<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 헤더 -->
    <header class="bg-white shadow-sm sticky top-0 z-40">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center h-16">
          <h1 class="text-2xl font-bold text-gray-900">관리자 대시보드</h1>
          <button
            @click="handleLogout"
            class="flex items-center space-x-2 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
            </svg>
            <span>로그아웃</span>
          </button>
        </div>
        
        <!-- 탭 메뉴 -->
        <div class="border-t">
          <nav class="-mb-px flex space-x-8">
            <button
              v-for="tab in tabs"
              :key="tab.id"
              @click="activeTab = tab.id"
              :class="[
                'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
                activeTab === tab.id
                  ? 'border-primary-600 text-primary-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              ]"
            >
              <div class="flex items-center space-x-2">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="tab.icon" />
                </svg>
                <span>{{ tab.label }}</span>
              </div>
            </button>
          </nav>
        </div>
      </div>
    </header>

    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <!-- 행사 관리 탭 -->
      <div v-if="activeTab === 'conventions'">
        <div class="flex justify-between items-center mb-6">
          <div>
            <h2 class="text-xl font-bold text-gray-900">행사 관리</h2>
            <p class="text-sm text-gray-600 mt-1">전체 {{ conventions.length }}개 행사</p>
          </div>
          <button
            @click="showCreateModal = true"
            class="flex items-center space-x-2 px-4 py-2 bg-primary-600 hover:bg-primary-700 text-white rounded-lg transition-colors shadow-sm"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            <span>새 행사 만들기</span>
          </button>
        </div>

        <!-- 로딩 -->
        <div v-if="loading" class="text-center py-12">
          <div class="inline-block w-8 h-8 border-4 border-primary-600 border-t-transparent rounded-full animate-spin"></div>
          <p class="text-gray-600 mt-4">로딩 중...</p>
        </div>

        <!-- 행사 목록 -->
        <div v-else class="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          <div
            v-for="convention in conventions"
            :key="convention.id"
            class="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow overflow-hidden cursor-pointer"
            @click="goToConvention(convention.id)"
          >
            <div 
              class="h-32 relative"
              :style="{ background: `linear-gradient(135deg, ${convention.brandColor || '#6366f1'} 0%, ${adjustColor(convention.brandColor || '#6366f1', -20)} 100%)` }"
            >
              <div class="absolute top-3 right-3">
                <span v-if="convention.completeYn === 'Y'" class="px-2 py-1 bg-gray-800/50 text-white text-xs rounded-full">종료</span>
                <span v-else class="px-2 py-1 bg-green-500/80 text-white text-xs rounded-full">진행중</span>
              </div>
              <div class="absolute bottom-4 left-4 right-4">
                <h3 class="text-white font-bold text-lg truncate">{{ convention.title }}</h3>
              </div>
            </div>
            <div class="p-4">
              <div class="space-y-2 text-sm text-gray-600">
                <div class="flex items-center">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  {{ formatDate(convention.startDate) }} ~ {{ formatDate(convention.endDate) }}
                </div>
                <div class="flex items-center">
                  <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                  </svg>
                  참석자 {{ convention.guestCount }}명
                </div>
              </div>
              <div class="mt-4 pt-4 border-t flex justify-end space-x-2">
                <button @click.stop="editConvention(convention)" class="px-3 py-1 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-md">수정</button>
                <button @click.stop="completeConvention(convention.id)" 
                        :class="convention.completeYn === 'Y' ? 'bg-green-500 hover:bg-green-600' : 'bg-red-500 hover:bg-red-600'" 
                        class="px-3 py-1 text-sm font-medium text-white rounded-md">
                  {{ convention.completeYn === 'Y' ? '재개' : '종료' }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 챗봇 관리 탭 -->
      <div v-if="activeTab === 'chatbot'">
        <!-- 챗봇 통계 카드 -->
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
          <div class="bg-white rounded-lg shadow-sm p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-500">총 벡터 문서</p>
                <p class="text-2xl font-bold text-gray-900 mt-1">{{ chatbotStats.totalDocuments }}</p>
              </div>
              <div class="p-3 bg-blue-100 rounded-lg">
                <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-lg shadow-sm p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-500">활성 행사</p>
                <p class="text-2xl font-bold text-gray-900 mt-1">{{ chatbotStats.activeConventions }}</p>
              </div>
              <div class="p-3 bg-green-100 rounded-lg">
                <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-lg shadow-sm p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-500">총 참가자</p>
                <p class="text-2xl font-bold text-gray-900 mt-1">{{ chatbotStats.totalGuests }}</p>
              </div>
              <div class="p-3 bg-purple-100 rounded-lg">
                <svg class="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-lg shadow-sm p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-500">DB 크기</p>
                <p class="text-2xl font-bold text-gray-900 mt-1">{{ formatBytes(chatbotStats.dbSize) }}</p>
              </div>
              <div class="p-3 bg-orange-100 rounded-lg">
                <svg class="w-6 h-6 text-orange-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582-4 8-4s8 1.79 8 4" />
                </svg>
              </div>
            </div>
          </div>
        </div>

        <!-- 챗봇 하위 탭 -->
        <div class="bg-white rounded-lg shadow-sm">
          <div class="border-b border-gray-200">
            <nav class="flex -mb-px">
              <button v-for="tab in chatbotTabs"
                      :key="tab.id"
                      @click="activeChatbotTab = tab.id"
                      :class="[
                'px-6 py-3 text-sm font-medium border-b-2 transition-colors',
                activeChatbotTab === tab.id
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              ]">
                {{ tab.label }}
              </button>
            </nav>
          </div>

          <div class="p-6">
            <!-- LLM Provider 관리 -->
            <div v-show="activeChatbotTab === 'providers'">
              <LlmProviderManagement />
            </div>

            <!-- 전체 재색인 -->
            <div v-show="activeChatbotTab === 'reindex'" class="space-y-6">
              <div>
                <h3 class="text-lg font-semibold text-gray-900 mb-2">전체 데이터 재색인</h3>
                <p class="text-gray-600">시스템의 모든 정보를 다시 읽어 챗봇의 지식 베이스를 업데이트합니다.</p>
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
                  <button @click="handleFullReindex"
                          :disabled="reindexing"
                          class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 transition-colors">
                    <span v-if="reindexing">색인 중...</span>
                    <span v-else>재색인 시작</span>
                  </button>
                </div>
              </div>

              <div v-if="reindexResult" :class="[
                'p-4 rounded-lg',
                reindexResult.success ? 'bg-green-50 text-green-800' : 'bg-red-50 text-red-800'
              ]">
                {{ reindexResult.message }}
              </div>
            </div>

            <!-- 행사별 설정 -->
            <div v-show="activeChatbotTab === 'conventions'" class="space-y-4">
              <div class="flex items-center justify-between mb-4">
                <h3 class="text-lg font-semibold text-gray-900">행사별 챗봇 설정</h3>
                <input v-model="searchQuery"
                       type="text"
                       placeholder="행사 검색..."
                       class="px-4 py-2 border border-gray-300 rounded-lg w-64" />
              </div>

              <div class="space-y-3">
                <div v-for="convention in filteredChatbotConventions"
                     :key="convention.id"
                     class="border border-gray-200 rounded-lg p-4 hover:shadow-sm transition-shadow">
                  <div class="flex items-start justify-between">
                    <div class="flex-1">
                      <h4 class="font-medium text-gray-900">{{ convention.title }}</h4>
                      <p class="text-sm text-gray-500 mt-1">
                        {{ convention.startDate ? new Date(convention.startDate).toLocaleDateString('ko-KR') : '날짜 미정' }}
                        - 참가자 {{ convention.guestCount }}명
                      </p>
                      <div class="flex items-center gap-4 mt-2">
                        <span class="text-xs text-gray-600">벡터 문서: {{ convention.vectorCount }}개</span>
                        <span :class="[
                  'text-xs px-2 py-1 rounded',
                  convention.chatbotEnabled ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'
                ]">
                          {{ convention.chatbotEnabled ? '챗봇 활성' : '챗봇 비활성' }}
                        </span>
                      </div>
                    </div>
                    <div class="flex gap-2">
                      <button @click="showIndexedItems(convention.id)"
                              class="px-3 py-1 text-sm bg-purple-100 text-purple-700 rounded hover:bg-purple-200 transition-colors">
                        색인 상세
                      </button>
                      <button @click="reindexConvention(convention.id)"
                              class="px-3 py-1 text-sm bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition-colors">
                        재색인
                      </button>
                      <button @click="toggleChatbot(convention.id, !convention.chatbotEnabled)"
                              :class="[
                  'px-3 py-1 text-sm rounded transition-colors',
                  convention.chatbotEnabled
                    ? 'bg-red-100 text-red-700 hover:bg-red-200'
                    : 'bg-green-100 text-green-700 hover:bg-green-200'
                ]">
                        {{ convention.chatbotEnabled ? '비활성화' : '활성화' }}
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- 벡터 DB 통계 -->
            <div v-show="activeChatbotTab === 'vectors'" class="space-y-6">
              <h3 class="text-lg font-semibold text-gray-900">벡터 데이터베이스 상세</h3>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div class="border border-gray-200 rounded-lg p-4">
                  <h4 class="font-medium text-gray-700 mb-3">데이터 소스별 분포</h4>
                  <div class="space-y-2">
                    <div v-for="source in vectorStats.bySouce" :key="source.type" class="flex items-center justify-between">
                      <span class="text-sm text-gray-600">{{ source.label }}</span>
                      <div class="flex items-center gap-2">
                        <div class="w-24 bg-gray-200 rounded-full h-2">
                          <div class="bg-blue-600 h-2 rounded-full" :style="{ width: `${source.percentage}%` }"></div>
                        </div>
                        <span class="text-sm font-medium text-gray-900 w-12 text-right">{{ source.count }}</span>
                      </div>
                    </div>
                  </div>
                </div>

                <div class="border border-gray-200 rounded-lg p-4">
                  <h4 class="font-medium text-gray-700 mb-3">최근 색인 활동</h4>
                  <div class="space-y-2">
                    <div v-for="activity in recentActivities" :key="activity.id" class="text-sm">
                      <div class="flex items-center justify-between">
                        <span class="text-gray-600">{{ activity.action }}</span>
                        <span class="text-gray-400">{{ formatTimeAgo(activity.timestamp) }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- 시스템 로그 -->
            <div v-show="activeChatbotTab === 'logs'" class="space-y-4">
              <div class="flex items-center justify-between">
                <h3 class="text-lg font-semibold text-gray-900">시스템 로그</h3>
                <button @click="fetchLogs" class="px-4 py-2 bg-gray-100 hover:bg-gray-200 rounded-lg text-sm">
                  로그 새로고침
                </button>
              </div>

              <div class="bg-gray-900 rounded-lg p-4 h-96 overflow-y-auto font-mono text-sm">
                <div v-for="(log, index) in logs" :key="index" :class="[
                  'py-1',
                  log.level === 'error' ? 'text-red-400' :
                  log.level === 'warn' ? 'text-yellow-400' :
                  log.level === 'info' ? 'text-blue-400' :
                  'text-gray-400'
                ]">
                  <span class="text-gray-500">[{{ log.timestamp }}]</span>
                  <span class="ml-2">{{ log.message }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 회원 관리 탭 -->
      <div v-if="activeTab === 'users'">
        <div class="mb-6">
          <h2 class="text-xl font-bold text-gray-900">회원 관리</h2>
          <p class="text-sm text-gray-600 mt-1">전체 회원 조회 및 관리</p>
        </div>
        <div class="bg-white rounded-lg shadow p-6">
          <p class="text-gray-600">회원 관리 기능 구현 예정</p>
        </div>
      </div>

      <!-- 통계 탭 -->
      <div v-if="activeTab === 'statistics'">
        <div class="mb-6">
          <h2 class="text-xl font-bold text-gray-900">통계</h2>
          <p class="text-sm text-gray-600 mt-1">시스템 전체 통계 및 분석</p>
        </div>
        <div class="grid gap-6 md:grid-cols-2 lg:grid-cols-4 mb-6">
          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">전체 행사</p>
                <p class="text-2xl font-bold text-gray-900 mt-1">{{ conventions.length }}</p>
              </div>
              <div class="p-3 bg-blue-100 rounded-full">
                <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                </svg>
              </div>
            </div>
          </div>
          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">진행중 행사</p>
                <p class="text-2xl font-bold text-gray-900 mt-1">{{ activeConventions }}</p>
              </div>
              <div class="p-3 bg-green-100 rounded-full">
                <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
          </div>
          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">전체 참석자</p>
                <p class="text-2xl font-bold text-gray-900 mt-1">{{ totalGuests }}</p>
              </div>
              <div class="p-3 bg-purple-100 rounded-full">
                <svg class="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
              </div>
            </div>
          </div>
          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">전체 일정</p>
                <p class="text-2xl font-bold text-gray-900 mt-1">{{ totalSchedules }}</p>
              </div>
              <div class="p-3 bg-yellow-100 rounded-full">
                <svg class="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
            </div>
          </div>
        </div>
        <div class="bg-white rounded-lg shadow p-6">
          <p class="text-gray-600">상세 통계 기능 구현 예정</p>
        </div>
      </div>
    </div>
    <ConventionFormModal
      v-if="showCreateModal"
      :convention="editingConvention"
      @close="showCreateModal = false"
      @save="handleSaveConvention"
    />

    <!-- 색인 상세 정보 모달 -->
    <div v-if="showIndexDetailModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50" @click="showIndexDetailModal = false">
      <div class="bg-white rounded-lg p-6 w-full max-w-3xl max-h-[80vh] overflow-y-auto" @click.stop>
        <div class="flex items-center justify-between mb-6">
          <h3 class="text-xl font-bold text-gray-900">색인 상세 정보</h3>
          <button @click="showIndexDetailModal = false" class="text-gray-400 hover:text-gray-600">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <div v-if="loadingIndexDetail" class="text-center py-12">
          <div class="inline-block w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin"></div>
          <p class="mt-4 text-gray-600">로딩 중...</p>
        </div>

        <div v-else-if="indexedItemsDetail" class="space-y-6">
          <!-- 전체 벡터 수 -->
          <div class="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-lg p-6 border border-blue-200">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600 mb-1">총 벡터 문서</p>
                <p class="text-3xl font-bold text-blue-600">{{ indexedItemsDetail.vectorCount }}</p>
              </div>
              <div class="p-4 bg-white rounded-full shadow-sm">
                <svg class="w-8 h-8 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              </div>
            </div>
          </div>

          <!-- 색인 항목 상세 -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <!-- 행사 기본 정보 -->
            <div class="border rounded-lg p-4 hover:shadow-sm transition-shadow">
              <div class="flex items-center justify-between mb-3">
                <h4 class="font-semibold text-gray-900">행사 기본 정보</h4>
                <span :class="[
                  'px-2 py-1 text-xs rounded-full',
                  indexedItemsDetail.conventionInfo.indexed ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'
                ]">
                  {{ indexedItemsDetail.conventionInfo.indexed ? '✓ 색인됨' : '미색인' }}
                </span>
              </div>
              <div class="text-sm text-gray-600 space-y-1">
                <p><span class="font-medium">제목:</span> {{ indexedItemsDetail.conventionInfo.title }}</p>
                <p><span class="font-medium">기간:</span> {{ formatDate(indexedItemsDetail.conventionInfo.startDate) }} ~ {{ formatDate(indexedItemsDetail.conventionInfo.endDate) }}</p>
                <p><span class="font-medium">유형:</span> {{ indexedItemsDetail.conventionInfo.type }}</p>
              </div>
            </div>

            <!-- 참석자 통계 -->
            <div class="border rounded-lg p-4 hover:shadow-sm transition-shadow">
              <div class="flex items-center justify-between mb-3">
                <h4 class="font-semibold text-gray-900">참석자 통계</h4>
                <span :class="[
                  'px-2 py-1 text-xs rounded-full',
                  indexedItemsDetail.guestSummary.indexed ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'
                ]">
                  {{ indexedItemsDetail.guestSummary.indexed ? '✓ 색인됨' : '미색인' }}
                </span>
              </div>
              <div class="text-sm text-gray-600">
                <p class="text-2xl font-bold text-gray-900">{{ indexedItemsDetail.guestSummary.totalCount }}명</p>
                <p class="mt-1">부서별/소속별 통계 포함</p>
              </div>
            </div>

            <!-- 일정 템플릿 -->
            <div class="border rounded-lg p-4 hover:shadow-sm transition-shadow">
              <div class="flex items-center justify-between mb-3">
                <h4 class="font-semibold text-gray-900">일정 템플릿</h4>
                <span :class="[
                  'px-2 py-1 text-xs rounded-full',
                  indexedItemsDetail.schedules.indexed ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'
                ]">
                  {{ indexedItemsDetail.schedules.indexed ? '✓ 색인됨' : '미색인' }}
                </span>
              </div>
              <div class="text-sm text-gray-600 space-y-1">
                <p><span class="font-medium">템플릿 수:</span> {{ indexedItemsDetail.schedules.templateCount }}개</p>
                <p><span class="font-medium">일정 항목:</span> {{ indexedItemsDetail.schedules.itemCount }}개</p>
              </div>
            </div>

            <!-- 공지사항 -->
            <div class="border rounded-lg p-4 hover:shadow-sm transition-shadow">
              <div class="flex items-center justify-between mb-3">
                <h4 class="font-semibold text-gray-900">공지사항</h4>
                <span :class="[
                  'px-2 py-1 text-xs rounded-full',
                  indexedItemsDetail.notices.indexed ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'
                ]">
                  {{ indexedItemsDetail.notices.indexed ? '✓ 색인됨' : '미색인' }}
                </span>
              </div>
              <div class="text-sm text-gray-600">
                <p class="text-2xl font-bold text-gray-900">{{ indexedItemsDetail.notices.count }}개</p>
                <p class="mt-1">고정 공지 및 일반 공지</p>
              </div>
            </div>

            <!-- 액션 항목 -->
            <div class="border rounded-lg p-4 hover:shadow-sm transition-shadow">
              <div class="flex items-center justify-between mb-3">
                <h4 class="font-semibold text-gray-900">액션 항목</h4>
                <span :class="[
                  'px-2 py-1 text-xs rounded-full',
                  indexedItemsDetail.actions.indexed ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'
                ]">
                  {{ indexedItemsDetail.actions.indexed ? '✓ 색인됨' : '미색인' }}
                </span>
              </div>
              <div class="text-sm text-gray-600">
                <p class="text-2xl font-bold text-gray-900">{{ indexedItemsDetail.actions.count }}개</p>
                <p class="mt-1">활성 액션 및 할 일 목록</p>
              </div>
            </div>
          </div>

          <div class="flex justify-end pt-4 border-t">
            <button @click="showIndexDetailModal = false"
                    class="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700">
              닫기
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import apiClient from '@/services/api'
import { chatbotAdminAPI } from '@/services/chatbotAdminService'

import LlmProviderManagement from '@/components/admin/LlmProviderManagement.vue'
import ConventionFormModal from '@/components/admin/ConventionFormModal.vue'

const router = useRouter()
const authStore = useAuthStore()

const activeTab = ref('conventions')
const conventions = ref([])
const loading = ref(false)
const showCreateModal = ref(false)
const editingConvention = ref(null)

// 챗봇 관리 관련 상태
const activeChatbotTab = ref('providers')
const chatbotStats = ref({
  totalDocuments: 0,
  activeConventions: 0,
  totalGuests: 0,
  dbSize: 0
})
const chatbotConventions = ref([])
const vectorStats = ref({
  bySouce: []
})
const recentActivities = ref([])
const logs = ref([])
const reindexing = ref(false)
const reindexResult = ref(null)
const searchQuery = ref('')
const showIndexDetailModal = ref(false)
const loadingIndexDetail = ref(false)
const indexedItemsDetail = ref(null)

const chatbotTabs = [
  { id: 'providers', label: 'LLM Provider' },
  { id: 'reindex', label: '전체 재색인' },
  { id: 'conventions', label: '행사별 설정' },
  { id: 'vectors', label: '벡터 DB 통계' },
  { id: 'logs', label: '시스템 로그' }
]

async function handleSaveConvention(conventionData) {
  try {
    if (editingConvention.value) {
      // 수정
      await apiClient.put(`/conventions/${editingConvention.value.id}`, conventionData)
    } else {
      // 생성
      await apiClient.post('/conventions', conventionData)
    }
    showCreateModal.value = false
    editingConvention.value = null
    await loadConventions()
  } catch (error) {
    console.error('Failed to save convention:', error)
    alert('행사 저장에 실패했습니다.')
  }
}

function editConvention(convention) {
  editingConvention.value = { ...convention };
  showCreateModal.value = true;
}

async function completeConvention(conventionId) {
  if (!confirm('행사를 종료 처리하시겠습니까?')) return;

  try {
    await apiClient.post(`/conventions/${conventionId}/complete`);
    await loadConventions();
  } catch (error) {
    console.error('Failed to complete convention:', error);
    alert('행사 종료 처리에 실패했습니다.');
  }
}

const tabs = [
  {
    id: 'conventions',
    label: '행사 관리',
    icon: 'M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10'
  },
  {
    id: 'chatbot',
    label: '챗봇 관리',
    icon: 'M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z'
  },
  {
    id: 'users',
    label: '회원 관리',
    icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z'
  },
  {
    id: 'statistics',
    label: '통계',
    icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z'
  }
]

const activeConventions = computed(() => {
  return conventions.value.filter(c => c.completeYn === 'N').length
})

const totalGuests = computed(() => {
  return conventions.value.reduce((sum, c) => sum + (c.guestCount || 0), 0)
})

const totalSchedules = computed(() => {
  return conventions.value.reduce((sum, c) => sum + (c.scheduleCount || 0), 0)
})

const handleLogout = async () => {
  if (confirm('로그아웃하시겠습니까?')) {
    await authStore.logout()
    router.push('/login')
  }
}

async function loadConventions() {
  loading.value = true
  try {
    const response = await apiClient.get('/conventions')
    conventions.value = response.data
  } catch (error) {
    console.error('Failed to load conventions:', error)
  } finally {
    loading.value = false
  }
}

function goToConvention(conventionId) {
  router.push(`/admin/conventions/${conventionId}`)
}

function formatDate(dateString) {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('ko-KR', { year: 'numeric', month: '2-digit', day: '2-digit' })
}

function adjustColor(color, amount) {
  if (!color) return '#555'
  const num = parseInt(color.replace('#', ''), 16)
  const r = Math.max(0, Math.min(255, (num >> 16) + amount))
  const g = Math.max(0, Math.min(255, ((num >> 8) & 0x00FF) + amount))
  const b = Math.max(0, Math.min(255, (num & 0x0000FF) + amount))
  return '#' + ((r << 16) | (g << 8) | b).toString(16).padStart(6, '0')
}

// 챗봇 관련 함수들
const filteredChatbotConventions = computed(() => {
  if (!searchQuery.value) return chatbotConventions.value
  const query = searchQuery.value.toLowerCase()
  return chatbotConventions.value.filter(c =>
    c.title.toLowerCase().includes(query)
  )
})

async function fetchChatbotStats() {
  try {
    const response = await chatbotAdminAPI.getStatus()
    chatbotStats.value = response.data
  } catch (error) {
    console.error('Failed to fetch chatbot stats:', error)
  }
}

async function fetchChatbotConventions() {
  try {
    const response = await chatbotAdminAPI.getConventions()
    chatbotConventions.value = response.data
  } catch (error) {
    console.error('Failed to fetch chatbot conventions:', error)
  }
}

async function fetchVectorStats() {
  try {
    const response = await chatbotAdminAPI.getVectorStats()
    vectorStats.value = response.data
  } catch (error) {
    console.error('Failed to fetch vector stats:', error)
  }
}

async function fetchRecentActivities() {
  try {
    const response = await chatbotAdminAPI.getRecentActivities()
    recentActivities.value = response.data
  } catch (error) {
    console.error('Failed to fetch recent activities:', error)
  }
}

async function fetchLogs() {
  try {
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
    const response = await chatbotAdminAPI.reindexAll()
    reindexResult.value = {
      success: true,
      message: response.data.message || '재색인이 완료되었습니다.'
    }
    await fetchChatbotStats()
    await fetchChatbotConventions()
  } catch (error) {
    reindexResult.value = {
      success: false,
      message: error.response?.data?.message || '재색인에 실패했습니다.'
    }
  } finally {
    reindexing.value = false
  }
}

async function reindexConvention(conventionId) {
  try {
    await chatbotAdminAPI.reindexConvention(conventionId)
    alert('행사 재색인이 시작되었습니다.')
    await fetchChatbotConventions()
  } catch (error) {
    alert('재색인에 실패했습니다: ' + error.message)
  }
}

async function toggleChatbot(conventionId, enabled) {
  try {
    await chatbotAdminAPI.toggleChatbot(conventionId, enabled)
    await fetchChatbotConventions()
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

function formatBytes(bytes) {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
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

// 챗봇 탭 활성화 시 데이터 로드
watch(activeTab, async (newTab) => {
  if (newTab === 'chatbot') {
    await fetchChatbotStats()
    await fetchChatbotConventions()
    await fetchVectorStats()
    await fetchRecentActivities()
    await fetchLogs()
  }
})

onMounted(() => {
  loadConventions()
})
</script>
