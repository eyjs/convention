<template>
  <div
    v-if="loading"
    class="min-h-screen min-h-dvh flex items-center justify-center"
  >
    <div
      class="inline-block w-8 h-8 border-4 border-t-transparent rounded-full animate-spin"
      :style="{ borderColor: brandColor, borderTopColor: 'transparent' }"
    ></div>
  </div>
  <div
    v-else-if="convention"
    class="min-h-screen min-h-dvh bg-gradient-to-br from-gray-50 to-gray-100"
  >
    <!-- 헤더 배너 -->
    <div class="relative h-48 overflow-hidden" :style="headerGradientStyle">
      <!-- 배경 이미지 (있는 경우) -->
      <div
        v-if="convention.conventionImg"
        class="absolute inset-0 bg-cover bg-center"
        :style="{ backgroundImage: `url(${convention.conventionImg})` }"
      ></div>
      <!-- 어두운 오버레이 (배경 이미지가 있을 때 텍스트 가독성 확보) -->
      <div
        v-if="convention.conventionImg"
        class="absolute inset-0 bg-black/40"
      ></div>
      <!-- 배경 패턴 (배경 이미지가 없을 때만) -->
      <div v-else class="absolute inset-0 opacity-10">
        <div
          class="absolute top-0 left-0 w-64 h-64 bg-white rounded-full -translate-x-32 -translate-y-32"
        ></div>
        <div
          class="absolute bottom-0 right-0 w-96 h-96 bg-white rounded-full translate-x-48 translate-y-48"
        ></div>
      </div>

      <!-- 공통 헤더 사용 (오버레이) -->
      <div class="absolute top-0 left-0 right-0 z-20">
        <MainHeader title="" :transparent="true" />
      </div>

      <div class="relative h-full flex flex-col justify-center px-6 text-white">
        <h1 class="text-2xl font-bold mb-2">{{ convention.title }}</h1>
        <div class="flex items-center space-x-2 text-sm text-white/90">
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
              d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
            />
          </svg>
          <span>{{ formattedConventionPeriod }}</span>
        </div>
        <div class="flex items-center space-x-2 text-sm text-white/90 mt-1">
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
              d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
            />
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
            />
          </svg>
          <span>{{ convention.location }}</span>
        </div>

        <!-- D-Day 뱃지 -->
        <div
          class="mt-4 inline-flex items-center px-3 py-1.5 bg-white/20 backdrop-blur-sm rounded-full text-sm font-bold"
        >
          <svg
            class="w-4 h-4 mr-1.5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
          D-{{ dDay }}
        </div>
      </div>
    </div>

    <!-- GLOBAL_ROOT_POPUP: 팝업 공지 (화면에 보이지 않고 팝업만 트리거) -->
    <DynamicActionRenderer
      v-if="globalPopupActions.length > 0"
      :features="globalPopupActions"
    />

    <!-- 여행 가이드 링크 -->
    <div v-if="hasTravelGuide" class="px-4 pt-4">
      <router-link
        :to="`/conventions/${conventionId}/travel-guide`"
        class="flex items-center gap-3 bg-white rounded-xl shadow-sm p-3 hover:shadow-md transition-shadow"
      >
        <span class="text-lg">🧳</span>
        <span class="text-sm font-medium text-gray-700">여행 가이드</span>
        <span class="text-xs text-gray-400">긴급연락처 · 집결지 · 캘린더</span>
        <span class="ml-auto text-xs text-primary-500 font-medium">→</span>
      </router-link>
    </div>

    <!-- HOME_SUB_HEADER 위치: 헤더 배너 바로 아래 -->
    <div v-if="subHeaderActions.length > 0" class="px-4 pt-4">
      <DynamicActionRenderer :features="subHeaderActions" />
    </div>

    <!-- 메인 컨텐츠 -->
    <div class="px-4 pt-10 space-y-6 -mt-8">
      <!-- 내 정보 (대시보드 최상단) -->
      <div v-if="myInfo" class="bg-white rounded-2xl shadow-lg overflow-hidden">
        <!-- 탭 헤더 (내정보 / 동반자1 / 동반자2 ...) -->
        <div class="flex border-b overflow-x-auto">
          <button
            class="px-4 py-3 text-sm font-medium whitespace-nowrap transition-colors relative"
            :class="
              infoTab === 'me'
                ? 'text-gray-900'
                : 'text-gray-400 hover:text-gray-600'
            "
            @click="infoTab = 'me'"
          >
            내 정보
            <div
              v-if="infoTab === 'me'"
              class="absolute bottom-0 left-0 right-0 h-0.5"
              :style="{ backgroundColor: brandColor }"
            ></div>
          </button>
          <button
            v-for="(comp, idx) in myInfo.companions"
            :key="comp.id"
            class="px-4 py-3 text-sm font-medium whitespace-nowrap transition-colors relative"
            :class="
              infoTab === 'comp-' + comp.id
                ? 'text-gray-900'
                : 'text-gray-400 hover:text-gray-600'
            "
            @click="infoTab = 'comp-' + comp.id"
          >
            {{ comp.name || '동반자' + (idx + 1) }}
            <div
              v-if="infoTab === 'comp-' + comp.id"
              class="absolute bottom-0 left-0 right-0 h-0.5"
              :style="{ backgroundColor: brandColor }"
            ></div>
          </button>
        </div>

        <!-- 내 정보 탭 -->
        <div v-if="infoTab === 'me'" class="p-5">
          <!-- 프로필 요약 -->
          <div class="flex items-center gap-3 mb-4 pb-4 border-b">
            <div
              class="w-11 h-11 rounded-full flex items-center justify-center text-white font-bold flex-shrink-0"
              :style="{ backgroundColor: brandColor }"
            >
              {{ myInfo.profile.name?.charAt(0) }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="font-bold text-gray-900">{{ myInfo.profile.name }}</p>
              <p class="text-sm text-gray-500 truncate">
                {{
                  myInfo.profile.corpName || myInfo.profile.affiliation || ''
                }}
                <span v-if="myInfo.profile.corpPart">
                  · {{ myInfo.profile.corpPart }}
                </span>
              </p>
            </div>
          </div>

          <!-- 배정 정보 태그 -->
          <div
            v-if="
              myInfo.scheduleCourses.length > 0 || myInfo.attributes.length > 0
            "
            class="flex flex-wrap gap-2 mb-4"
          >
            <span
              v-for="course in myInfo.scheduleCourses"
              :key="'course-' + course.id"
              class="px-2.5 py-1 text-xs font-medium rounded-md"
              :style="{
                backgroundColor: brandColor + '15',
                color: brandColor,
              }"
            >
              {{ course.courseName }}
            </span>
            <span
              v-for="attr in myInfo.attributes"
              :key="attr.key"
              class="px-2.5 py-1 text-xs font-medium rounded-md bg-gray-100 text-gray-700"
            >
              {{ attr.key }}: {{ attr.value }}
            </span>
          </div>

          <!-- 준비 상태 체크리스트 -->
          <div class="space-y-0 divide-y divide-gray-100">
            <!-- 여권 -->
            <!-- prettier-ignore -->
            <button
              class="w-full flex items-center justify-between py-3 text-left hover:bg-gray-50 -mx-1 px-1 rounded-lg transition-colors"
              @click="showPassportModal = true"
            >
              <div class="flex items-center gap-2.5">
                <span
                  class="w-5 h-5 rounded-full flex items-center justify-center flex-shrink-0 text-xs"
                  :class="
                    myInfo.passport.verified
                      ? 'bg-green-500 text-white'
                      : myInfo.passport.hasNumber && myInfo.passport.hasImage
                        ? 'bg-yellow-400 text-white'
                        : 'bg-gray-200 text-gray-400'
                  "
                >
                  {{ myInfo.passport.verified ? '✓' : myInfo.passport.hasNumber && myInfo.passport.hasImage ? '!' : '—' }}
                </span>
                <span class="text-sm text-gray-800">여권 정보</span>
              </div>
              <div class="flex items-center gap-1.5">
                <span
                  class="text-xs font-medium px-2 py-0.5 rounded-md"
                  :class="
                    myInfo.passport.verified
                      ? 'bg-green-50 text-green-700'
                      : myInfo.passport.hasNumber && myInfo.passport.hasImage
                        ? 'bg-yellow-50 text-yellow-700'
                        : 'bg-red-50 text-red-600'
                  "
                >
                  {{
                    myInfo.passport.verified
                      ? '승인완료'
                      : myInfo.passport.hasNumber && myInfo.passport.hasImage
                        ? '승인대기'
                        : '미입력'
                  }}
                </span>
                <svg
                  class="w-4 h-4 text-gray-300"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M9 5l7 7-7 7"
                  />
                </svg>
              </div>
            </button>

            <!-- 동반자 정보 -->
            <div
              v-if="myInfo.companions.length > 0"
              class="flex items-center justify-between py-3"
            >
              <div class="flex items-center gap-2.5">
                <span
                  class="w-5 h-5 rounded-full flex items-center justify-center flex-shrink-0 text-xs"
                  :class="
                    myInfo.companions.every((c) => c.passport.verified)
                      ? 'bg-green-500 text-white'
                      : 'bg-gray-200 text-gray-400'
                  "
                >
                  {{
                    myInfo.companions.every((c) => c.passport.verified)
                      ? '✓'
                      : '—'
                  }}
                </span>
                <span class="text-sm text-gray-800"
                  >동반자 정보 ({{ myInfo.companions.length }}명)</span
                >
              </div>
              <span
                class="text-xs font-medium px-2 py-0.5 rounded-md"
                :class="
                  myInfo.companions.every((c) => c.passport.verified)
                    ? 'bg-green-50 text-green-700'
                    : 'bg-red-50 text-red-600'
                "
              >
                {{
                  myInfo.companions.every((c) => c.passport.verified)
                    ? '완료'
                    : '미완료'
                }}
              </span>
            </div>

            <!-- 옵션투어 -->
            <button
              v-if="myInfo.optionTours.length > 0"
              class="w-full flex items-center justify-between py-3 text-left hover:bg-gray-50 -mx-1 px-1 rounded-lg transition-colors"
              @click="navigateTo('/schedule')"
            >
              <div class="flex items-center gap-2.5">
                <span
                  class="w-5 h-5 rounded-full flex items-center justify-center flex-shrink-0 text-xs bg-green-500 text-white"
                >
                  ✓
                </span>
                <span class="text-sm text-gray-800">옵션투어</span>
              </div>
              <div class="flex items-center gap-1.5">
                <span
                  class="text-xs font-medium px-2 py-0.5 rounded-md bg-green-50 text-green-700"
                >
                  {{ myInfo.optionTours.length }}개
                </span>
                <svg
                  class="w-4 h-4 text-gray-300"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M9 5l7 7-7 7"
                  />
                </svg>
              </div>
            </button>
          </div>
        </div>

        <!-- 동반자 탭 -->
        <div
          v-for="comp in myInfo.companions"
          v-show="infoTab === 'comp-' + comp.id"
          :key="'tab-' + comp.id"
          class="p-5"
        >
          <div class="flex items-center gap-3 mb-4 pb-4 border-b">
            <div
              class="w-11 h-11 rounded-full flex items-center justify-center text-white font-bold flex-shrink-0 bg-gray-400"
            >
              {{ comp.name?.charAt(0) }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="font-bold text-gray-900">{{ comp.name }}</p>
              <p class="text-sm text-gray-500">{{ comp.relationType }}</p>
            </div>
          </div>

          <div class="space-y-0 divide-y divide-gray-100">
            <!-- 동반자 여권 -->
            <div class="flex items-center justify-between py-3">
              <div class="flex items-center gap-2.5">
                <span
                  class="w-5 h-5 rounded-full flex items-center justify-center flex-shrink-0 text-xs"
                  :class="
                    comp.passport.verified
                      ? 'bg-green-500 text-white'
                      : comp.passport.hasNumber
                        ? 'bg-yellow-400 text-white'
                        : 'bg-gray-200 text-gray-400'
                  "
                >
                  {{
                    comp.passport.verified
                      ? '✓'
                      : comp.passport.hasNumber
                        ? '!'
                        : '—'
                  }}
                </span>
                <span class="text-sm text-gray-800">여권 정보</span>
              </div>
              <span
                class="text-xs font-medium px-2 py-0.5 rounded-md"
                :class="
                  comp.passport.verified
                    ? 'bg-green-50 text-green-700'
                    : comp.passport.hasNumber
                      ? 'bg-yellow-50 text-yellow-700'
                      : 'bg-red-50 text-red-600'
                "
              >
                {{
                  comp.passport.verified
                    ? '승인완료'
                    : comp.passport.hasNumber
                      ? '승인대기'
                      : '미입력'
                }}
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- 여권 상세 모달 -->
      <BaseModal
        :is-open="showPassportModal"
        max-width="sm"
        @close="showPassportModal = false"
      >
        <template #header>
          <h3 class="text-lg font-bold text-gray-900">여권 정보</h3>
        </template>
        <template #body>
          <div v-if="myInfo" class="space-y-4">
            <div class="space-y-3">
              <div class="flex justify-between">
                <span class="text-sm text-gray-500">영문 성</span>
                <span class="text-sm font-medium text-gray-900">{{
                  myInfo.profile.passportLastName || '-'
                }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-sm text-gray-500">영문 이름</span>
                <span class="text-sm font-medium text-gray-900">{{
                  myInfo.profile.passportFirstName || '-'
                }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-sm text-gray-500">여권 번호</span>
                <span class="text-sm font-medium text-gray-900">{{
                  myInfo.profile.passportNumber || '-'
                }}</span>
              </div>
              <div class="flex justify-between">
                <span class="text-sm text-gray-500">만료일</span>
                <span class="text-sm font-medium text-gray-900">{{
                  myInfo.profile.passportExpiryDate || '-'
                }}</span>
              </div>
              <div class="flex justify-between items-center">
                <span class="text-sm text-gray-500">승인 상태</span>
                <span
                  class="text-xs font-medium px-2 py-0.5 rounded-md"
                  :class="
                    myInfo.passport.verified
                      ? 'bg-green-50 text-green-700'
                      : myInfo.passport.hasNumber
                        ? 'bg-yellow-50 text-yellow-700'
                        : 'bg-red-50 text-red-600'
                  "
                >
                  {{
                    myInfo.passport.verified
                      ? '승인완료'
                      : myInfo.passport.hasNumber
                        ? '승인대기'
                        : '미입력'
                  }}
                </span>
              </div>
            </div>
            <div v-if="myInfo.profile.passportImageUrl" class="pt-2 border-t">
              <p class="text-sm text-gray-500 mb-2">여권 이미지</p>
              <img
                :src="myInfo.profile.passportImageUrl"
                alt="여권 이미지"
                class="w-full rounded-lg border"
              />
            </div>
            <!-- prettier-ignore -->
            <button
              class="w-full py-2.5 text-sm font-medium rounded-lg transition-colors"
              :style="{ backgroundColor: brandColor + '15', color: brandColor }"
              @click="showPassportModal = false; router.push('/my-profile')"
            >
              여권 정보 수정
            </button>
          </div>
        </template>
      </BaseModal>

      <!-- HOME_CONTENT_TOP 위치: 동적 액션 -->
      <DynamicActionRenderer
        v-if="contentTopActions.length > 0"
        :features="contentTopActions"
      />

      <!-- 필수 제출 사항 체크리스트 -->
      <ChecklistProgress
        v-if="checklistStatus && checklistStatus.totalItems > 0"
        :checklist="checklistStatus"
        :brand-color="brandColor"
      />

      <!-- 설문조사 (미완료만 표시, 모두 완료 시 숨김) -->
      <div
        v-if="pendingSurveys.length > 0"
        class="bg-white rounded-2xl shadow-lg p-5"
      >
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center gap-2">
            <div
              class="w-8 h-8 rounded-lg flex items-center justify-center"
              :style="{
                backgroundColor: brandColor + '15',
              }"
            >
              <svg
                class="w-4.5 h-4.5"
                :style="{ color: brandColor }"
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
            </div>
            <div>
              <h2 class="text-lg font-bold text-gray-900">설문조사</h2>
              <p class="text-xs text-gray-500">
                {{ pendingSurveys.length }}개 참여 대기
              </p>
            </div>
          </div>
        </div>
        <div class="space-y-2.5">
          <button
            v-for="survey in pendingSurveys"
            :key="'survey-' + survey.id"
            class="w-full flex items-center justify-between p-3.5 rounded-xl border-2 border-gray-200 hover:shadow-sm transition-all text-left group"
            :class="'hover:border-blue-500 hover:bg-blue-50'"
            @click="navigateTo(`/surveys/${survey.id}`)"
          >
            <div class="flex items-center gap-3">
              <div
                class="w-6 h-6 rounded-full flex items-center justify-center bg-gray-200 group-hover:bg-blue-200 transition-colors flex-shrink-0"
              >
                <svg
                  class="w-3.5 h-3.5 text-gray-400 group-hover:text-blue-500"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M15 15l-2 5L9 9l11 4-5 2zm0 0l5 5M7.188 2.239l.777 2.897M5.136 7.965l-2.898-.777M13.95 4.05l-2.122 2.122m-5.657 5.656l-2.12 2.122"
                  />
                </svg>
              </div>
              <div>
                <p class="text-sm font-medium text-gray-900">
                  {{ survey.title }}
                </p>
                <p
                  v-if="survey.description"
                  class="text-xs text-gray-500 mt-0.5 line-clamp-1"
                >
                  {{ survey.description }}
                </p>
              </div>
            </div>
            <div class="flex items-center gap-1.5 flex-shrink-0">
              <span
                class="text-xs font-medium px-2 py-0.5 rounded-md bg-red-50 text-red-600"
              >
                미완료
              </span>
              <svg
                class="w-4 h-4 text-gray-300"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M9 5l7 7-7 7"
                />
              </svg>
            </div>
          </button>
        </div>
      </div>

      <!-- 공지사항 -->
      <div class="bg-white rounded-2xl shadow-lg p-5">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-bold text-gray-900">공지사항</h2>
          <button
            v-if="remainingNoticesCount > 0"
            class="text-sm text-primary-600 font-medium flex items-center"
            @click="navigateTo('/notices')"
          >
            +{{ remainingNoticesCount }}개 더보기
            <svg
              class="w-4 h-4 ml-0.5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9 5l7 7-7 7"
              />
            </svg>
          </button>
          <button
            v-else-if="recentNotices.length > 0"
            class="text-sm text-primary-600 font-medium flex items-center"
            @click="navigateTo('/notices')"
          >
            전체보기
            <svg
              class="w-4 h-4 ml-0.5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9 5l7 7-7 7"
              />
            </svg>
          </button>
        </div>

        <div v-if="recentNotices.length > 0" class="space-y-3">
          <div
            v-for="notice in recentNotices"
            :key="notice.id"
            class="bg-gradient-to-br from-white to-gray-50 rounded-xl shadow-md hover:shadow-lg transition-all p-4 cursor-pointer border border-gray-100"
            @click="openNotice(notice)"
          >
            <div class="flex items-start justify-between mb-2">
              <h3 class="font-bold text-gray-900 text-base flex-1 line-clamp-2">
                {{ notice.title }}
              </h3>
              <div class="flex items-center space-x-1 ml-2 flex-shrink-0">
                <span
                  v-if="notice.isPinned"
                  class="px-2 py-0.5 bg-red-100 text-red-700 text-xs font-bold rounded"
                  >필독</span
                >
                <span
                  class="px-2 py-0.5 bg-blue-100 text-blue-700 text-xs font-medium rounded"
                  >공지</span
                >
              </div>
            </div>
            <p class="text-sm text-gray-600 line-clamp-2 mb-3">
              {{ notice.content }}
            </p>
            <div
              class="flex items-center justify-between text-xs text-gray-500"
            >
              <span>{{ formatDate(notice.createdAt) }}</span>
              <div class="flex items-center space-x-3">
                <span class="flex items-center space-x-1">
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
                      d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                    />
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                    />
                  </svg>
                  <span>{{ notice.viewCount || 0 }}</span>
                </span>
              </div>
            </div>
          </div>
        </div>

        <div v-else class="text-center py-8 text-gray-500">
          <svg
            class="w-12 h-12 mx-auto mb-2 opacity-50"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M7 8h10M7 12h4m1 8l-4-4H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-3l-4 4z"
            />
          </svg>
          <p class="text-sm">등록된 공지사항이 없습니다</p>
        </div>
      </div>

      <!-- 나의 일정 -->
      <div class="bg-white rounded-2xl shadow-lg p-5">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-lg font-bold text-gray-900">나의 일정</h2>
          <button
            class="text-sm text-primary-600 font-medium flex items-center"
            @click="navigateTo('/schedule')"
          >
            전체보기
            <svg
              class="w-4 h-4 ml-0.5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9 5l7 7-7 7"
              />
            </svg>
          </button>
        </div>

        <div v-if="upcomingSchedules.length > 0" class="space-y-3">
          <div
            v-for="schedule in upcomingSchedules"
            :key="schedule.id"
            class="flex items-start space-x-3 p-3 bg-gray-50 rounded-xl"
          >
            <div
              class="w-12 h-12 bg-primary-100 rounded-full flex items-center justify-center flex-shrink-0"
            >
              <svg
                class="w-6 h-6 text-primary-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center space-x-2 mb-1">
                <span
                  class="px-2 py-0.5 bg-primary-600 text-white text-xs font-bold rounded"
                  >{{ schedule.date }}</span
                >
                <span
                  class="px-2 py-0.5 bg-blue-100 text-blue-700 text-xs font-medium rounded"
                  >{{ schedule.time }}</span
                >
              </div>
              <h3 class="font-semibold text-gray-900 text-sm">
                {{ schedule.title }}
              </h3>
              <p v-if="schedule.location" class="text-xs text-gray-500 mt-1">
                {{ schedule.location }}
              </p>
            </div>
          </div>
        </div>

        <div v-else class="text-center py-8 text-gray-500">
          <svg
            class="w-12 h-12 mx-auto mb-2 opacity-50"
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
          <p class="text-sm">예정된 일정이 없습니다</p>
        </div>
      </div>
    </div>
  </div>
  <div v-else class="min-h-screen min-h-dvh flex items-center justify-center">
    <div class="text-center">
      <p class="text-gray-600">행사 정보를 불러오지 못했습니다.</p>
      <button
        class="mt-4 px-4 py-2 bg-primary-600 text-white rounded-lg"
        @click="handleLogout"
      >
        로그아웃
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConventionStore } from '@/stores/convention'
import { useNoticeNavigation } from '@/composables/useNoticeNavigation'
import apiClient from '@/services/api'
import DeadlineCountdown from '@/components/common/DeadlineCountdown.vue'
import ChecklistProgress from '@/components/common/ChecklistProgress.vue'
import DynamicActionRenderer from '@/dynamic-features/DynamicActionRenderer.vue'
import MainHeader from '@/components/common/MainHeader.vue'
import BaseModal from '@/components/common/BaseModal.vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const conventionStore = useConventionStore()
const { setPendingNotice } = useNoticeNavigation()

const loading = ref(true)
const convention = computed(() => conventionStore.currentConvention)
const allActions = ref([]) // 전체 동적 액션 저장
const checklistStatus = ref(null)
const myInfo = ref(null)
const infoTab = ref('me')
const showPassportModal = ref(false)

const pendingSurveys = computed(() => {
  if (!myInfo.value?.surveys) return []
  return myInfo.value.surveys.filter((s) => !s.completed)
})

// 브랜드 컬러 가져오기
const brandColor = computed(() => {
  return conventionStore.currentConvention?.brandColor || '#10b981' // 기본값: primary-600 green
})

// 헤더 그라데이션 스타일 계산
const headerGradientStyle = computed(() => {
  // 배경 이미지가 있으면 그라데이션 사용 안 함
  if (convention.value?.conventionImg) {
    return {}
  }

  const color = brandColor.value
  // 16진수 색상을 RGB로 변환
  const r = parseInt(color.slice(1, 3), 16)
  const g = parseInt(color.slice(3, 5), 16)
  const b = parseInt(color.slice(5, 7), 16)

  // 약간 더 어두운 색 계산 (0.8배)
  const darkerR = Math.floor(r * 0.8)
  const darkerG = Math.floor(g * 0.8)
  const darkerB = Math.floor(b * 0.8)

  return {
    background: `linear-gradient(to bottom right, rgb(${r}, ${g}, ${b}), rgb(${darkerR}, ${darkerG}, ${darkerB}))`,
  }
})

// 타겟 위치별 액션 필터링
const subHeaderActions = computed(() =>
  allActions.value.filter(
    (action) => action.targetLocation === 'HOME_SUB_HEADER',
  ),
)
const contentTopActions = computed(() =>
  allActions.value.filter(
    (action) => action.targetLocation === 'HOME_CONTENT_TOP',
  ),
)
const globalPopupActions = computed(() =>
  allActions.value.filter(
    (action) => action.targetLocation === 'GLOBAL_ROOT_POPUP',
  ),
)

const upcomingSchedules = ref([])
const recentNotices = ref([])
const totalNoticesCount = ref(0)

const dDay = computed(() => {
  if (!convention.value || !convention.value.startDate) return 0
  const today = new Date()
  const start = new Date(convention.value.startDate)
  const diff = Math.ceil((start - today) / (1000 * 60 * 60 * 24))
  return diff > 0 ? diff : 0
})

const remainingNoticesCount = computed(() => {
  const remaining = totalNoticesCount.value - recentNotices.value.length
  return remaining > 0 ? remaining : 0
})

function navigateTo(path) {
  const conventionId = route.params.conventionId
  if (!conventionId) {
    console.warn('Convention ID not available')
    return
  }
  router.push(`/conventions/${conventionId}${path}`)
}

const handleLogout = async () => {
  if (confirm('로그아웃하시겠습니까?')) {
    await authStore.logout()
    router.push('/login')
  }
}

function openNotice(notice) {
  if (!notice || !notice.id || notice.id === 'undefined') {
    console.warn('Invalid notice:', notice)
    return
  }
  const conventionId = route.params.conventionId
  setPendingNotice(notice.id)
  router.push(`/conventions/${conventionId}/notices`)
}

function formatDate(dateStr) {
  const date = new Date(dateStr)
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

// 날짜를 '2025-11-10(월)' 형식으로 변환
function formatDateWithDay(dateStr) {
  if (!dateStr) return ''

  const date = new Date(dateStr)
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  const days = ['일', '월', '화', '수', '목', '금', '토']
  const dayOfWeek = days[date.getDay()]

  return `${year}-${month}-${day}(${dayOfWeek})`
}

// 행사 기간 포맷
const formattedConventionPeriod = computed(() => {
  if (!convention.value) return ''

  const start = formatDateWithDay(convention.value.startDate)
  const end = formatDateWithDay(convention.value.endDate)

  return `${start} ~ ${end}`
})

async function loadTodaySchedules() {
  try {
    const userId = authStore.user?.id
    const conventionId = conventionStore.currentConvention?.id

    if (!userId || !conventionId) {
      console.warn(
        '[loadTodaySchedules] userId or conventionId not available:',
        { userId, conventionId },
      )
      return
    }

    const response = await apiClient.get(
      `/user-schedules/${userId}/${conventionId}`,
    )
    const allSchedules = response.data
    const now = new Date()
    const today = now.toISOString().split('T')[0]

    // 현재 날짜 이후의 일정을 필터링합니다.
    let upcoming = allSchedules.filter((item) => {
      const scheduleDate = item.scheduleDate.split('T')[0]
      return scheduleDate >= today
    })

    // 다가오는 일정이 없으면(행사가 종료된 경우) 전체 일정을 표시합니다.
    if (upcoming.length === 0 && allSchedules.length > 0) {
      upcoming = allSchedules
    }

    upcomingSchedules.value = upcoming
      .sort((a, b) => {
        // 날짜와 시간으로 정렬
        const dateCompare = a.scheduleDate.localeCompare(b.scheduleDate)
        if (dateCompare !== 0) return dateCompare
        return a.startTime.localeCompare(b.startTime)
      })
      .slice(0, 3) // 최대 3개만 표시
      .map((item) => {
        const date = new Date(item.scheduleDate)
        const month = date.getMonth() + 1
        const day = date.getDate()
        return {
          id: item.id,
          date: `${month}/${day}`,
          time: item.startTime,
          title: item.title,
          location: item.location || '장소 미정',
        }
      })
  } catch (error) {
    console.error('Failed to load today schedules:', error)
  }
}

async function loadRecentNotices() {
  try {
    const conventionId = conventionStore.currentConvention?.id
    if (!conventionId) return

    const response = await apiClient.get('/notices', {
      params: {
        conventionId: conventionId,
        page: 1,
        pageSize: 2, // 최대 2개만
      },
    })

    recentNotices.value = response.data.items || []
    totalNoticesCount.value =
      response.data.total || response.data.totalCount || 0
  } catch (error) {
    console.error('Failed to load notices:', error)
  }
}

async function loadChecklist() {
  try {
    const conventionId = conventionStore.currentConvention?.id
    if (!conventionId) return

    const response = await apiClient.get(
      `/conventions/${conventionId}/actions/checklist-status`,
    )
    checklistStatus.value = response.data
  } catch (error) {
    console.error('Failed to load checklist:', error)
    checklistStatus.value = null
  }
}

async function loadMyInfo() {
  try {
    const conventionId = conventionStore.currentConvention?.id
    if (!conventionId) return

    const response = await apiClient.get(
      `/users/my-convention-info/${conventionId}`,
    )
    myInfo.value = response.data
  } catch (error) {
    console.error('Failed to load my info:', error)
    myInfo.value = null
  }
}

async function loadDynamicActions() {
  try {
    const conventionId = conventionStore.currentConvention?.id
    if (!conventionId) return

    // 액션 목록과 상태 정보를 병렬로 가져오기
    const [actionsResponse, statusesResponse] = await Promise.all([
      apiClient.get(`/conventions/${conventionId}/actions/all`, {
        params: {
          targetLocation: 'HOME_SUB_HEADER,HOME_CONTENT_TOP,GLOBAL_ROOT_POPUP',
          isActive: true,
        },
      }),
      apiClient.get(`/conventions/${conventionId}/actions/statuses`),
    ])

    const actions = actionsResponse.data || []
    console.log('Home Actions:', actions) // 홈 화면 액션 데이터 확인용 로그
    const statuses = statusesResponse.data || []

    // 상태 정보를 맵으로 변환
    const statusMap = new Map(statuses.map((s) => [s.conventionActionId, s]))

    // 액션에 isComplete 정보 추가
    allActions.value = actions.map((action) => ({
      ...action,
      isComplete: statusMap.get(action.id)?.isComplete || false,
    }))
  } catch (error) {
    console.error('Failed to load dynamic actions:', error)
    allActions.value = []
  }
}

// 여행 가이드 링크 표시 여부
const hasTravelGuide = ref(false)

async function checkTravelGuide() {
  try {
    const res = await apiClient.get(`/conventions/${conventionId}/travel-guide`)
    const d = res.data
    hasTravelGuide.value = !!(
      d.emergencyContacts ||
      d.meetingPointInfo ||
      d.location
    )
  } catch {
    hasTravelGuide.value = false
  }
}

onMounted(async () => {
  loading.value = true

  if (!authStore.user) {
    await authStore.fetchCurrentUser()
  }

  await Promise.all([
    loadTodaySchedules(),
    loadRecentNotices(),
    loadChecklist(),
    loadDynamicActions(),
    loadMyInfo(),
    checkTravelGuide(),
  ])

  loading.value = false
})
</script>

<style scoped>
.fade-down-enter-active,
.fade-down-leave-active {
  transition: all 0.2s ease-out;
}
.fade-down-enter-from,
.fade-down-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
