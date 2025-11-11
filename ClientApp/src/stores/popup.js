// ClientApp/src/stores/popup.js
import { defineStore } from 'pinia'
import { markRaw, defineAsyncComponent } from 'vue'
import { popupComponents } from '@/popups/popupComponents' // 팝업 컴포넌트 레지스트리 임포트

export const usePopupStore = defineStore('popup', {
  state: () => ({
    isOpen: false,
    title: '',
    content: '', // 컴포넌트가 없을 경우 일반 텍스트 내용
    component: null, // 동적으로 로드할 컴포넌트 객체
    props: {}, // 컴포넌트에 전달할 props
    modalType: 'SlideUpModal', // 'SlideUpModal' 또는 'BaseModal'
    options: {}, // 팝업 자체에 대한 추가 옵션 (예: z-index-class)
  }),
  actions: {
    /**
     * 팝업을 엽니다.
     * @param {Object} payload
     * @param {string} payload.title - 팝업 제목
     * @param {string} [payload.content] - 팝업 내용 (컴포넌트가 없을 경우)
     * @param {string} [payload.componentName] - 팝업 내부에 렌더링할 Vue 컴포넌트의 이름 (MapsTo 필드에서 가져옴)
     * @param {number} [payload.targetId] - 팝업 컴포넌트에 전달할 ID (TargetId 필드에서 가져옴)
     * @param {string} [payload.modalType='SlideUpModal'] - 사용할 모달 타입 ('SlideUpModal' 또는 'BaseModal')
     * @param {Object} [payload.options] - 팝업 자체에 대한 추가 옵션 (예: z-index-class)
     */
    openPopup({ title, content, componentName, targetId = null, modalType = 'SlideUpModal', options = {} }) {
      this.title = title || ''
      this.content = content || ''
      // targetId를 props의 'id'로 전달하거나, 필요에 따라 다른 이름으로 매핑
      this.props = targetId !== null ? { id: targetId } : {}
      this.modalType = modalType
      this.options = options

      if (componentName && popupComponents[componentName]) {
        // 비동기 컴포넌트로 로드하여 번들 크기 최적화
        this.component = markRaw(defineAsyncComponent(popupComponents[componentName]))
      } else if (componentName) {
        console.warn(`Popup component '${componentName}' not found in popupComponents registry.`)
        this.component = null
      } else {
        this.component = null
      }

      this.isOpen = true
    },
    closePopup() {
      this.isOpen = false
      this.title = ''
      this.content = ''
      this.component = null
      this.props = {}
      this.modalType = 'SlideUpModal'
      this.options = {}
    },
  },
})