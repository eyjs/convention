// ClientApp/src/popups/popupComponents.js
// 이 파일에 동적으로 로드될 모든 팝업 컨텐츠 컴포넌트를 등록합니다.
// key: 백엔드 configJson에서 사용할 컴포넌트 이름 (문자열)
// value: 해당 컴포넌트를 비동기적으로 임포트하는 함수
export const popupComponents = {
  // 예시: 'MyInfoComponent'라는 이름으로 MyInfoComponent.vue를 로드
  MyInfoComponent: () => import('@/components/custom/MyInfoComponent.vue'),
  // 여기에 다른 팝업 컨텐츠 컴포넌트들을 추가합니다.
  // 'AnotherPopupContent': () => import('@/components/custom/AnotherPopupContent.vue'),
};