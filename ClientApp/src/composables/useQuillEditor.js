import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import Quill from 'quill'
import 'quill/dist/quill.snow.css'
import apiClient from '@/services/api'

/**
 * Quill 에디터 Composable
 *
 * 주요 기능:
 * 1. Quill 에디터 초기화 및 생명주기 관리
 * 2. 이미지 업로드 핸들링 (커스텀 이미지 핸들러)
 * 3. 에디터 내용 양방향 바인딩
 *
 * @param {Object} options - 설정 옵션
 * @param {String} options.placeholder - 플레이스홀더 텍스트
 * @param {String} options.theme - 에디터 테마 (기본값: 'snow')
 * @param {Boolean} options.readOnly - 읽기 전용 모드
 * @returns {Object} { editorRef, content, setContent, getHTML, getText }
 */
export function useQuillEditor(options = {}) {
  // ========== 상태 관리 ==========
  const editorRef = ref(null) // 에디터가 마운트될 DOM 요소
  const quillInstance = ref(null) // Quill 인스턴스
  const content = ref('') // 에디터 내용 (HTML)

  // ========== 기본 설정 ==========
  const defaultOptions = {
    placeholder: '내용을 입력하세요...',
    theme: 'snow', // 'snow' 또는 'bubble'
    readOnly: false,
    modules: {
      toolbar: [
        // 텍스트 포맷팅
        [{ header: [1, 2, 3, false] }],
        ['bold', 'italic', 'underline', 'strike'],

        // 색상 및 배경
        [{ color: [] }, { background: [] }],

        // 리스트 및 정렬
        [{ list: 'ordered' }, { list: 'bullet' }],
        [{ align: [] }],

        // 링크 및 이미지
        ['link', 'image'],

        // 기타
        ['clean'], // 포맷 제거 버튼
      ],
    },
  }

  const finalOptions = { ...defaultOptions, ...options }

  /**
   * 이미지 업로드 핸들러
   *
   * 동작 원리:
   * 1. 사용자가 툴바에서 이미지 아이콘 클릭
   * 2. 파일 선택 대화상자 표시
   * 3. 선택된 이미지를 서버로 업로드
   * 4. 서버에서 반환된 URL을 에디터에 삽입
   */
  function imageHandler() {
    // 파일 입력 요소 동적 생성
    const input = document.createElement('input')
    input.setAttribute('type', 'file')
    input.setAttribute('accept', 'image/*') // 이미지 파일만 허용
    input.click()

    input.onchange = async () => {
      const file = input.files[0]

      // 파일이 선택되지 않았으면 종료
      if (!file) return

      // 파일 크기 검증 (5MB 제한)
      const MAX_FILE_SIZE = 5 * 1024 * 1024 // 5MB
      if (file.size > MAX_FILE_SIZE) {
        alert('파일 크기는 5MB를 초과할 수 없습니다.')
        return
      }

      try {
        // FormData 생성 - 파일을 서버로 전송하기 위한 형식
        const formData = new FormData()
        formData.append('file', file)

        // 서버로 파일 업로드
        const response = await apiClient.post(
          '/api/file/upload/image',
          formData,
          {
            headers: {
              'Content-Type': 'multipart/form-data',
            },
          },
        )

        // 서버에서 반환된 이미지 URL
        const imageUrl = response.data.url

        // 현재 커서 위치에 이미지 삽입
        const range = quillInstance.value.getSelection(true)
        quillInstance.value.insertEmbed(range.index, 'image', imageUrl)

        // 커서를 이미지 다음으로 이동
        quillInstance.value.setSelection(range.index + 1)
      } catch (error) {
        console.error('이미지 업로드 실패:', error)
        alert('이미지 업로드에 실패했습니다.')
      }
    }
  }

  watch(editorRef, (newEl) => {
    if (newEl && !quillInstance.value) {
      // check for element and if not already initialized
      // Quill 인스턴스 생성
      quillInstance.value = new Quill(newEl, finalOptions)

      // 커스텀 이미지 핸들러 등록 (toolbar가 있는 경우에만)
      const toolbar = quillInstance.value.getModule('toolbar')
      if (toolbar) {
        toolbar.addHandler('image', imageHandler)
      }

      // 에디터 내용 변경 감지
      quillInstance.value.on('text-change', () => {
        // HTML 형식으로 내용 저장
        content.value = quillInstance.value.root.innerHTML
      })

      // 초기 내용 설정
      if (content.value) {
        quillInstance.value.root.innerHTML = content.value
      }
    }
  })

  /**
   * 컴포넌트 언마운트 시 정리
   * 메모리 누수 방지를 위해 Quill 인스턴스 제거
   */
  onBeforeUnmount(() => {
    if (quillInstance.value) {
      quillInstance.value = null
    }
  })

  /**
   * 외부에서 내용 설정 (양방향 바인딩)
   *
   * @param {String} newContent - 설정할 HTML 내용
   */
  function setContent(newContent) {
    content.value = newContent
    if (quillInstance.value) {
      quillInstance.value.root.innerHTML = newContent
    }
  }

  /**
   * HTML 형식으로 내용 가져오기
   *
   * @returns {String} HTML 형식의 에디터 내용
   */
  function getHTML() {
    return quillInstance.value?.root.innerHTML || ''
  }

  /**
   * 텍스트만 가져오기 (HTML 태그 제거)
   *
   * @returns {String} 순수 텍스트
   */
  function getText() {
    return quillInstance.value?.getText() || ''
  }

  /**
   * 에디터 비활성화/활성화
   *
   * @param {Boolean} disabled - true면 비활성화
   */
  function setDisabled(disabled) {
    if (quillInstance.value) {
      quillInstance.value.enable(!disabled)
    }
  }

  // Watch content changes from outside
  watch(
    () => content.value,
    (newValue) => {
      if (
        quillInstance.value &&
        quillInstance.value.root.innerHTML !== newValue
      ) {
        quillInstance.value.root.innerHTML = newValue
      }
    },
  )

  // 외부에서 사용할 수 있도록 반환
  return {
    editorRef, // 에디터 DOM ref
    content, // 에디터 내용 (반응형)
    setContent, // 내용 설정 함수
    getHTML, // HTML 가져오기
    getText, // 텍스트만 가져오기
    setDisabled, // 비활성화 설정
  }
}
