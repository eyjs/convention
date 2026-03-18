<template>
  <BaseModal :is-open="true" max-width="5xl" @close="closeModal">
    <template #header>
      <h2 class="text-2xl font-bold text-gray-900">
        {{ isEdit ? '공지사항 수정' : '새 공지사항 작성' }}
      </h2>
    </template>
    <template #body>
      <form class="space-y-6" @submit.prevent="handleSubmit">
        <!-- 카테고리 -->
        <div>
          <label
            for="category"
            class="block text-sm font-semibold text-gray-700 mb-2"
            >카테고리</label
          >
          <select
            id="category"
            v-model="form.noticeCategoryId"
            class="w-full px-4 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option :value="null">카테고리 선택</option>
            <option
              v-for="category in categories"
              :key="category.id"
              :value="category.id"
            >
              {{ category.name }}
            </option>
          </select>
        </div>

        <!-- 제목 -->
        <div>
          <label class="block text-sm font-semibold text-gray-700 mb-2">
            제목 <span class="text-red-500">*</span>
          </label>
          <input
            v-model="form.title"
            type="text"
            placeholder="공지사항 제목을 입력하세요"
            class="w-full px-4 py-3 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <!-- 고정 여부 (관리자만) -->
        <div v-if="authStore.isAdmin" class="flex items-center gap-2">
          <input
            id="isPinned"
            v-model="form.isPinned"
            type="checkbox"
            class="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
          />
          <label
            for="isPinned"
            class="text-sm font-medium text-gray-700 cursor-pointer"
          >
            이 공지사항을 상단에 고정
          </label>
        </div>

        <!-- 내용 (Quill 에디터) -->
        <div>
          <label class="block text-sm font-semibold text-gray-700 mb-2">
            내용 <span class="text-red-500">*</span>
          </label>
          <div class="border rounded-lg overflow-hidden text-lg">
            <QuillEditor
              ref="quillEditor"
              v-model:content="form.content"
              content-type="html"
              :toolbar="editorToolbar"
              theme="snow"
              placeholder="공지사항 내용을 입력하세요"
              style="min-height: 400px"
            />
          </div>
        </div>

        <!-- 첨부파일 -->
        <div>
          <label class="block text-sm font-semibold text-gray-700 mb-2">
            첨부파일
          </label>

          <!-- 파일 업로드 버튼 -->
          <div class="mb-4">
            <input
              ref="fileInput"
              type="file"
              multiple
              class="hidden"
              @change="handleFileSelect"
            />
            <button
              type="button"
              class="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors flex items-center gap-2"
              @click="$refs.fileInput.click()"
            >
              <span>📎</span>
              <span>파일 선택</span>
            </button>
            <p class="mt-2 text-xs text-gray-500">
              * 최대 10MB, 이미지/문서 파일만 업로드 가능합니다
            </p>
          </div>

          <!-- 업로드된 파일 목록 -->
          <div v-if="form.attachments.length > 0" class="space-y-2">
            <div
              v-for="(file, index) in form.attachments"
              :key="index"
              class="flex items-center gap-3 p-3 bg-gray-50 rounded-lg"
            >
              <span class="text-xl">📎</span>
              <div class="flex-1">
                <p class="font-medium text-gray-900 text-sm">
                  {{ file.originalName || file.name }}
                </p>
                <p class="text-xs text-gray-500">
                  {{ formatFileSize(file.size) }}
                </p>
              </div>
              <button
                type="button"
                class="text-red-600 hover:text-red-800 transition-colors"
                @click="removeFile(index)"
              >
                <span class="text-xl">×</span>
              </button>
            </div>
          </div>

          <!-- 파일 업로드 진행 상태 -->
          <div v-if="uploading" class="mt-4">
            <div class="flex items-center gap-2 text-sm text-gray-600">
              <div
                class="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600"
              ></div>
              <span>파일 업로드 중... {{ uploadProgress }}%</span>
            </div>
          </div>
        </div>
      </form>
    </template>
    <template #footer>
      <button
        type="button"
        class="px-6 py-2 border rounded-lg hover:bg-gray-50 transition-colors"
        @click="closeModal"
      >
        취소
      </button>
      <button
        :disabled="saving || !isFormValid"
        class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
        @click="handleSubmit"
      >
        <span
          v-if="saving"
          class="animate-spin rounded-full h-4 w-4 border-b-2 border-white"
        ></span>
        <span>{{ saving ? '저장 중...' : isEdit ? '수정' : '등록' }}</span>
      </button>
    </template>
  </BaseModal>
</template>

<script>
import { ref, computed, onMounted, watch } from 'vue'
import { QuillEditor } from '@vueup/vue-quill'
import '@vueup/vue-quill/dist/vue-quill.snow.css'
import { noticeAPI } from '@/services/noticeService'
import { useAuthStore } from '@/stores/auth'
import {
  uploadFile,
  validateFileExtension,
  validateFileSize,
  formatFileSize,
  handleQuillImageUpload,
} from '@/utils/fileUpload'
import BaseModal from '@/components/common/BaseModal.vue'

export default {
  name: 'NoticeFormModal',
  components: {
    QuillEditor,
    BaseModal,
  },
  props: {
    notice: {
      type: Object,
      default: null,
    },
    categories: {
      type: Array,
      required: true,
    },
    defaultCategoryId: {
      type: Number,
      default: null,
    },
    conventionId: {
      type: Number,
      required: true,
    },
  },
  emits: ['close', 'saved'],
  setup(props, { emit }) {
    const authStore = useAuthStore()
    const quillEditor = ref(null)
    const fileInput = ref(null)
    const saving = ref(false)
    const uploading = ref(false)
    const uploadProgress = ref(0)

    // 폼 데이터
    const form = ref({
      title: '',
      content: '',
      isPinned: false,
      attachments: [],
      noticeCategoryId: null,
    })

    // Quill 에디터 툴바 설정
    const editorToolbar = [
      [{ header: [1, 2, 3, false] }],
      ['bold', 'italic', 'underline', 'strike'],
      [{ color: [] }, { background: [] }],
      [{ align: [] }],
      [{ list: 'ordered' }, { list: 'bullet' }],
      ['blockquote', 'code-block'],
      ['link', 'image'],
      ['clean'],
    ]

    // 계산된 속성
    const isEdit = computed(() => !!props.notice)
    const isFormValid = computed(() => {
      return form.value.title.trim() && form.value.content.trim()
    })

    // 메서드
    const closeModal = () => {
      if (saving.value) return
      emit('close')
    }

    const handleFileSelect = async (event) => {
      const files = Array.from(event.target.files)
      if (files.length === 0) return

      uploading.value = true
      uploadProgress.value = 0

      try {
        // 파일 검증
        for (const file of files) {
          if (!validateFileExtension(file.name)) {
            alert(`${file.name}: 지원하지 않는 파일 형식입니다.`)
            continue
          }
          if (!validateFileSize(file)) {
            alert(`${file.name}: 파일 크기가 10MB를 초과합니다.`)
            continue
          }

          // 파일 업로드
          const result = await uploadFile(file, 'notice', (progress) => {
            uploadProgress.value = progress
          })

          // 업로드된 파일 정보 추가
          form.value.attachments.push({
            id: result.id,
            url: result.url,
            originalName: result.originalName,
            size: result.size,
          })
        }

        // 파일 인풋 초기화
        event.target.value = ''
      } catch (error) {
        console.error('File upload error:', error)
        alert('파일 업로드에 실패했습니다.')
      } finally {
        uploading.value = false
        uploadProgress.value = 0
      }
    }

    const removeFile = (index) => {
      form.value.attachments.splice(index, 1)
    }

    const handleSubmit = async () => {
      if (!isFormValid.value || saving.value) return

      saving.value = true
      try {
        const data = {
          title: form.value.title.trim(),
          content: form.value.content.trim(),
          isPinned: form.value.isPinned,
          noticeCategoryId: form.value.noticeCategoryId,
          attachmentIds: form.value.attachments
            .map((a) => a.id)
            .filter(Boolean),
        }

        if (isEdit.value) {
          await noticeAPI.updateNotice(props.notice.id, data)
          alert('수정되었습니다.')
        } else {
          await noticeAPI.createNotice(props.conventionId, data)
          alert('등록되었습니다.')
        }

        emit('saved')
      } catch (error) {
        console.error('Failed to save notice:', error)
        alert(isEdit.value ? '수정에 실패했습니다.' : '등록에 실패했습니다.')
      } finally {
        saving.value = false
      }
    }

    const setupQuillImageHandler = () => {
      const quill = quillEditor.value?.getQuill()
      if (!quill) return

      const toolbar = quill.getModule('toolbar')
      toolbar.addHandler('image', () => {
        handleQuillImageUpload(quill)
      })
    }

    // 초기화
    const initializeForm = () => {
      if (props.notice) {
        form.value = {
          title: props.notice.title || '',
          content: props.notice.content || '',
          isPinned: props.notice.isPinned || false,
          attachments: props.notice.attachments || [],
          noticeCategoryId: props.notice.noticeCategoryId || null,
        }
      } else {
        form.value = {
          title: '',
          content: '',
          isPinned: false,
          attachments: [],
          noticeCategoryId: props.defaultCategoryId || null,
        }
      }
    }

    // 생명주기
    onMounted(() => {
      initializeForm()

      // Quill 이미지 핸들러 설정 (약간의 지연 필요)
      setTimeout(() => {
        setupQuillImageHandler()
      }, 100)
    })

    watch(
      () => props.notice,
      () => {
        initializeForm()
      },
    )

    return {
      authStore,
      quillEditor,
      fileInput,
      saving,
      uploading,
      uploadProgress,
      form,
      editorToolbar,
      isEdit,
      isFormValid,
      closeModal,
      handleFileSelect,
      removeFile,
      handleSubmit,
      formatFileSize,
    }
  },
}
</script>

<style scoped>
/* Quill 에디터 커스텀 스타일 */

:deep(.ql-editor) {
  min-height: 400px;
  padding: 20px;
}

:deep(.ql-editor.ql-blank::before) {
  color: #999;
  font-style: normal;
}

:deep(.ql-toolbar) {
  border-top-left-radius: 8px;
  border-top-right-radius: 8px;
  background-color: #f9fafb;
}

:deep(.ql-container) {
  border-bottom-left-radius: 8px;
  border-bottom-right-radius: 8px;
}
</style>
