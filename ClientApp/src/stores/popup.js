// ClientApp/src/stores/popup.js
import { defineStore } from 'pinia'
import { markRaw } from 'vue'

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
     * @param {Object} [payload.component] - 팝업 내부에 렌더링할 Vue 컴포넌트
     * @param {number} [payload.targetId] - 팝업 컴포넌트에 전달할 ID
     * @param {string} [payload.modalType='SlideUpModal'] - 사용할 모달 타입
     * @param {Object} [payload.options] - 팝업 자체에 대한 추가 옵션
     */
    openPopup({
      title,
      content,
      component,
      targetId = null,
      modalType = 'SlideUpModal',
      options = {},
    }) {
      this.title = title || ''
      this.content = content || ''
      this.props = targetId !== null ? { id: targetId } : {}
      this.modalType = modalType
      this.options = options
      this.component = component ? markRaw(component) : null
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
