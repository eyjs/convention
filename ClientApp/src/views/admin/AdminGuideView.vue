<template>
  <div class="max-w-4xl mx-auto p-6">
    <h1 class="text-2xl font-bold text-gray-900 mb-1">사용 가이드</h1>
    <p class="text-sm text-gray-400 mb-8">비 개발자를 위한 업무 매뉴얼</p>

    <!-- 전체 플로우 차트 -->
    <div class="mb-10">
      <h2 class="text-lg font-bold text-gray-800 mb-4">전체 업무 흐름</h2>
      <div class="flex flex-col items-center gap-1">
        <!-- Step 1 -->
        <div
          class="border rounded-xl w-full max-w-lg flex items-center gap-3 px-4 py-3 bg-blue-50 border-blue-200 text-blue-800"
        >
          <span
            class="bg-blue-600 text-white rounded-full w-8 h-8 flex items-center justify-center text-sm font-bold flex-shrink-0"
            >1</span
          >
          <div>
            <span class="font-bold">행사 생성</span>
            <p class="text-xs opacity-70">행사 관리에서 새 행사 추가</p>
          </div>
        </div>
        <div class="text-gray-300 text-lg">↓</div>

        <!-- Step 2 -->
        <div
          class="border rounded-xl w-full max-w-lg flex items-center gap-3 px-4 py-3 bg-purple-50 border-purple-200 text-purple-800"
        >
          <span
            class="bg-purple-600 text-white rounded-full w-8 h-8 flex items-center justify-center text-sm font-bold flex-shrink-0"
            >2</span
          >
          <div>
            <span class="font-bold">일정 업로드</span>
            <p class="text-xs opacity-70">엑셀로 일정 코스 등록</p>
          </div>
        </div>
        <div class="text-gray-300 text-lg">↓</div>

        <!-- Step 3 (핵심) -->
        <div
          class="border-2 rounded-xl w-full max-w-lg flex items-center gap-3 px-4 py-3 bg-primary-50 border-primary-400 text-primary-800"
        >
          <span
            class="bg-primary-600 text-white rounded-full w-8 h-8 flex items-center justify-center text-sm font-bold flex-shrink-0"
            >3</span
          >
          <div class="flex-1">
            <span class="font-bold">참석자 업로드</span>
            <span
              class="ml-2 px-1.5 py-0.5 bg-red-500 text-white text-[10px] rounded-full font-bold"
              >핵심</span
            >
            <p class="text-xs opacity-70">
              명단 + 일정매핑 + 속성 한번에
            </p>
          </div>
        </div>
        <div class="text-gray-300 text-lg">↓</div>

        <!-- Step 4-5 병렬 -->
        <div class="flex gap-3 w-full max-w-lg">
          <div
            class="border rounded-xl flex-1 flex items-center gap-2 px-3 py-2.5 bg-orange-50 border-orange-200 text-orange-800"
          >
            <span
              class="bg-orange-600 text-white rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold flex-shrink-0"
              >4</span
            >
            <div>
              <span class="font-semibold text-sm">옵션투어</span>
              <p class="text-[11px] opacity-70">선택 투어 + 매핑</p>
            </div>
          </div>
          <div
            class="border rounded-xl flex-1 flex items-center gap-2 px-3 py-2.5 bg-green-50 border-green-200 text-green-800"
          >
            <span
              class="bg-green-600 text-white rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold flex-shrink-0"
              >5</span
            >
            <div>
              <span class="font-semibold text-sm">여행 배정</span>
              <p class="text-[11px] opacity-70">호차·호텔·방번호</p>
            </div>
          </div>
        </div>
        <div class="text-gray-300 text-lg">↓</div>

        <!-- Step 6-7 병렬 -->
        <div class="flex gap-3 w-full max-w-lg">
          <div
            class="border rounded-xl flex-1 flex items-center gap-2 px-3 py-2.5 bg-pink-50 border-pink-200 text-pink-800"
          >
            <span
              class="bg-pink-600 text-white rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold flex-shrink-0"
              >6</span
            >
            <div>
              <span class="font-semibold text-sm">액션 관리</span>
              <p class="text-[11px] opacity-70">버튼·배너·팝업</p>
            </div>
          </div>
          <div
            class="border rounded-xl flex-1 flex items-center gap-2 px-3 py-2.5 bg-amber-50 border-amber-200 text-amber-800"
          >
            <span
              class="bg-amber-600 text-white rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold flex-shrink-0"
              >7</span
            >
            <div>
              <span class="font-semibold text-sm">설문 / 공지</span>
              <p class="text-[11px] opacity-70">설문 발행·공지 등록</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 아코디언 상세 -->
    <div class="space-y-3">
      <div
        v-for="(section, i) in sections"
        :key="section.id"
        class="border border-gray-200 rounded-xl overflow-hidden"
      >
        <button
          class="w-full px-4 py-3 flex items-center gap-3 hover:bg-gray-50 transition-colors text-left"
          @click="toggle(section.id)"
        >
          <span
            class="w-7 h-7 rounded-full text-white flex items-center justify-center text-xs font-bold flex-shrink-0"
            :class="section.numColor"
            >{{ i + 1 }}</span
          >
          <span class="font-semibold text-gray-800 flex-1">{{
            section.title
          }}</span>
          <svg
            class="w-5 h-5 text-gray-400 transition-transform"
            :class="{ 'rotate-180': openId === section.id }"
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
        <div
          v-if="openId === section.id"
          class="px-5 pb-5 pt-3 border-t border-gray-100 text-sm text-gray-700 leading-relaxed"
          v-html="section.content"
        ></div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const openId = ref(null)

function toggle(id) {
  openId.value = openId.value === id ? null : id
}

const sections = [
  {
    id: 'create',
    title: '행사 생성',
    numColor: 'bg-blue-600',
    content: `
      <p class="mb-2"><strong>행사 관리</strong> 메뉴에서 새 행사를 추가합니다.</p>
      <ul class="list-disc pl-5 space-y-1 text-gray-600">
        <li>행사명, 시작/종료일, 장소 등 기본 정보 입력</li>
        <li>생성 후 클릭하면 상세 관리 화면으로 이동</li>
      </ul>
    `,
  },
  {
    id: 'schedules',
    title: '일정 업로드',
    numColor: 'bg-purple-600',
    content: `
      <p class="mb-2">일정이 확정되면 엑셀로 업로드합니다. <strong>엑셀 업로드 → 일정</strong> 탭</p>
      <div class="bg-purple-50 rounded-lg p-3 my-3 text-xs">
        <p class="font-semibold text-purple-800 mb-1">엑셀 형식</p>
        <table class="w-full text-purple-700">
          <tr><td class="pr-3 font-medium w-12">A열</td><td>날짜 (2026-04-01)</td></tr>
          <tr><td class="pr-3 font-medium">B열</td><td>시작시간 (09:00)</td></tr>
          <tr><td class="pr-3 font-medium">C열</td><td>종료시간</td></tr>
          <tr><td class="pr-3 font-medium">D열</td><td>장소</td></tr>
          <tr><td class="pr-3 font-medium">E열</td><td>일정명</td></tr>
          <tr><td class="pr-3 font-medium">F열</td><td>메모</td></tr>
        </table>
      </div>
      <div class="bg-blue-50 rounded-lg p-3 text-xs text-blue-700">💡 <strong>현재 일정 다운로드</strong> 버튼으로 기존 데이터를 받아 수정 후 재업로드 가능</div>
    `,
  },
  {
    id: 'guests',
    title: '참석자 업로드 (일정매핑 + 속성)',
    numColor: 'bg-primary-600',
    content: `
      <p class="mb-3"><strong>엑셀 업로드 → 참석자</strong> 탭. 하나의 엑셀에 3개 시트로 한번에 처리합니다.</p>
      <div class="space-y-2 my-3">
        <div class="bg-primary-50 rounded-lg p-3 text-xs">
          <p class="font-semibold text-primary-800 mb-1">시트1: 참석자 (필수)</p>
          <p class="text-primary-700">소속 | 부서 | 이름 | 주민번호 | 전화번호 | 그룹</p>
        </div>
        <div class="bg-green-50 rounded-lg p-3 text-xs">
          <p class="font-semibold text-green-800 mb-1">시트2: 그룹-일정 매핑 (선택)</p>
          <p class="text-green-700">그룹명 | 일정코스명 → 해당 그룹 전원에게 일정 자동 배정</p>
        </div>
        <div class="bg-amber-50 rounded-lg p-3 text-xs">
          <p class="font-semibold text-amber-800 mb-1">시트3: 속성 (선택)</p>
          <p class="text-amber-700">1행=속성명 헤더, 2행~=속성값. 시트1과 같은 행 = 같은 사람</p>
        </div>
      </div>
      <div class="bg-red-50 rounded-lg p-3 text-xs text-red-700">⚠️ 이름 + (전화번호 OR 주민번호) 동일 시 업데이트. 중복 등록 안 됨</div>
    `,
  },
  {
    id: 'option-tours',
    title: '옵션투어 업로드',
    numColor: 'bg-orange-600',
    content: `
      <p class="mb-3"><strong>엑셀 업로드 → 옵션투어</strong> 탭. 2개 시트 엑셀.</p>
      <div class="space-y-2 my-3">
        <div class="bg-orange-50 rounded-lg p-3 text-xs">
          <p class="font-semibold text-orange-800 mb-1">시트1: 옵션 목록</p>
          <p class="text-orange-700">날짜 | 시작시간 | 종료시간 | 옵션명 | 옵션내용</p>
          <p class="text-orange-500 mt-1">1행=헤더, 2행부터 데이터. 행 번호 = 옵션 번호</p>
        </div>
        <div class="bg-orange-50 rounded-lg p-3 text-xs">
          <p class="font-semibold text-orange-800 mb-1">시트2: 참석자 매핑</p>
          <p class="text-orange-700">이름 | 주민번호 | 연락처 | 옵션번호 (콤마 구분: 2,3,4)</p>
        </div>
      </div>
    `,
  },
  {
    id: 'travel',
    title: '여행 배정 (호차/호텔/방번호)',
    numColor: 'bg-green-600',
    content: `
      <p class="mb-3"><strong>여행 배정</strong> 메뉴에서 일자별 호차/호텔/방번호를 관리합니다.</p>
      <div class="grid grid-cols-2 gap-3 my-3">
        <div class="bg-green-50 rounded-lg p-3 text-xs">
          <p class="font-semibold text-green-800 mb-1">📤 사전 세팅</p>
          <p class="text-green-700">엑셀 업로드<br/>(시트명=날짜, 행=이름|전화|호차|호텔|방번호)</p>
        </div>
        <div class="bg-blue-50 rounded-lg p-3 text-xs">
          <p class="font-semibold text-blue-800 mb-1">✏️ 현장 수정</p>
          <p class="text-blue-700">날짜 탭 → 셀 클릭<br/>→ 직접 수정 (자동 저장)</p>
        </div>
      </div>
      <div class="space-y-1.5 text-xs">
        <div class="bg-blue-50 rounded-lg p-2.5 text-blue-700">💡 빈 셀 클릭 시 위 행 값 자동 입력</div>
        <div class="bg-blue-50 rounded-lg p-2.5 text-blue-700">💡 이전 날짜 복사 버튼으로 전일 호차/호텔 복사</div>
        <div class="bg-blue-50 rounded-lg p-2.5 text-blue-700">💡 같은 방번호 → 룸메이트 자동 표시</div>
        <div class="bg-blue-50 rounded-lg p-2.5 text-blue-700">💡 방번호 미배정 → 사용자에게 "배정 전" 표시</div>
      </div>
    `,
  },
  {
    id: 'actions',
    title: '액션 관리 (동적 기능)',
    numColor: 'bg-pink-600',
    content: `
      <p class="mb-3">코드 수정 없이 참석자 앱에 기능을 추가합니다. <strong>액션 관리</strong> 메뉴.</p>
      <div class="grid grid-cols-3 gap-2 my-3 text-center">
        <div class="bg-gray-50 rounded-lg p-2.5"><p class="text-xl mb-0.5">✅</p><p class="font-semibold text-xs">체크리스트</p></div>
        <div class="bg-gray-50 rounded-lg p-2.5"><p class="text-xl mb-0.5">🔗</p><p class="font-semibold text-xs">링크 메뉴</p></div>
        <div class="bg-gray-50 rounded-lg p-2.5"><p class="text-xl mb-0.5">🖼️</p><p class="font-semibold text-xs">배너</p></div>
        <div class="bg-gray-50 rounded-lg p-2.5"><p class="text-xl mb-0.5">📋</p><p class="font-semibold text-xs">안내 카드</p></div>
        <div class="bg-gray-50 rounded-lg p-2.5"><p class="text-xl mb-0.5">🔔</p><p class="font-semibold text-xs">팝업 공지</p></div>
        <div class="bg-gray-50 rounded-lg p-2.5"><p class="text-xl mb-0.5">🔘</p><p class="font-semibold text-xs">동적 버튼</p></div>
      </div>
      <div class="bg-pink-50 rounded-lg p-3 text-xs text-pink-700">💡 동적 버튼 → 컴포넌트 팝업 → <strong>TravelInfoPopup</strong>으로 연결하면 호차/호텔/룸메이트 확인 버튼 제공</div>
    `,
  },
  {
    id: 'survey',
    title: '설문조사 / 공지사항',
    numColor: 'bg-amber-600',
    content: `
      <div class="grid grid-cols-2 gap-3">
        <div class="bg-amber-50 rounded-lg p-3 text-xs">
          <p class="font-semibold text-amber-800 mb-1">설문 관리</p>
          <ul class="text-amber-700 space-y-0.5"><li>• 단답형 / 장문형 / 단일선택 / 복수선택</li><li>• 옵션투어 연동 설문</li><li>• 시작/종료일 설정</li></ul>
        </div>
        <div class="bg-blue-50 rounded-lg p-3 text-xs">
          <p class="font-semibold text-blue-800 mb-1">공지사항</p>
          <ul class="text-blue-700 space-y-0.5"><li>• HTML 에디터 (이미지 첨부)</li><li>• 상단 고정</li><li>• 카테고리 분류</li></ul>
        </div>
      </div>
    `,
  },
  {
    id: 'field',
    title: '현장 운영 대응',
    numColor: 'bg-blue-600',
    content: `
      <div class="space-y-2.5">
        <div class="flex items-start gap-3 bg-gray-50 rounded-lg p-3">
          <span class="text-xl">🚌</span>
          <div class="text-xs"><p class="font-semibold text-gray-800">호차 급변경</p><p class="text-gray-500">여행 배정 → 날짜 탭 → 그룹 필터 → 일괄 호차 변경</p></div>
        </div>
        <div class="flex items-start gap-3 bg-gray-50 rounded-lg p-3">
          <span class="text-xl">🔑</span>
          <div class="text-xs"><p class="font-semibold text-gray-800">방번호 실시간 배정</p><p class="text-gray-500">여행 배정 → 방번호 컬럼 직접 입력 → 자동 저장</p></div>
        </div>
        <div class="flex items-start gap-3 bg-gray-50 rounded-lg p-3">
          <span class="text-xl">🔔</span>
          <div class="text-xs"><p class="font-semibold text-gray-800">긴급 공지</p><p class="text-gray-500">액션 관리 → 팝업 공지 추가 → HTML 작성 → 즉시 노출</p></div>
        </div>
        <div class="flex items-start gap-3 bg-gray-50 rounded-lg p-3">
          <span class="text-xl">📊</span>
          <div class="text-xs"><p class="font-semibold text-gray-800">긴급 설문</p><p class="text-gray-500">설문 관리 → 설문 생성 → 결과 확인 → 여행 배정 수정</p></div>
        </div>
      </div>
    `,
  },
]
</script>
