// ClientApp/src/popups/popupComponents.js
// 이 파일에 동적으로 로드될 모든 팝업 컨텐츠 컴포넌트를 등록합니다.
// key: 백엔드 configJson에서 사용할 컴포넌트 이름 (문자열)
// value: 해당 컴포넌트를 비동기적으로 임포트하는 함수
export const popupComponents = {
  TravelInfoPopup: () => import('@/components/custom/TravelInfoPopup.vue'),
}
