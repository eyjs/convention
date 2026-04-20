import apiClient from '@/services/api'

/**
 * 파일 업로드 공통 핸들러
 * 이미지, 첨부파일 등 모든 파일 업로드에 사용
 */

/**
 * 이미지 압축 (Canvas API 기반 리사이징 + JPEG 변환)
 * - 1MB 이하이거나 이미지가 아닌 파일은 원본 반환
 * - maxWidth/maxHeight 초과 시에만 리사이징
 * @param {File} file - 원본 파일
 * @param {Object} options
 * @param {number} options.maxWidth - 최대 너비 (기본 1920)
 * @param {number} options.maxHeight - 최대 높이 (기본 1920)
 * @param {number} options.quality - JPEG 품질 0~1 (기본 0.8)
 * @returns {Promise<File>} 압축된 파일 또는 원본
 */
export async function compressImage(file, { maxWidth = 1920, maxHeight = 1920, quality = 0.8 } = {}) {
  if (file.size <= 1024 * 1024) return file // 1MB 이하 스킵
  if (!file.type.startsWith('image/')) return file

  return new Promise((resolve) => {
    const img = new Image()
    img.onload = () => {
      let { width, height } = img
      if (width <= maxWidth && height <= maxHeight) {
        resolve(file)
        return
      }
      const ratio = Math.min(maxWidth / width, maxHeight / height)
      width = Math.round(width * ratio)
      height = Math.round(height * ratio)

      const canvas = document.createElement('canvas')
      canvas.width = width
      canvas.height = height
      const ctx = canvas.getContext('2d')
      ctx.drawImage(img, 0, 0, width, height)
      canvas.toBlob(
        (blob) => {
          if (!blob) {
            resolve(file)
            return
          }
          resolve(new File([blob], file.name, { type: 'image/jpeg' }))
        },
        'image/jpeg',
        quality,
      )
    }
    img.onerror = () => resolve(file) // 로드 실패 시 원본 반환
    img.src = URL.createObjectURL(file)
  })
}

// 허용된 이미지 확장자
const ALLOWED_IMAGE_EXTENSIONS = [
  '.jpg',
  '.jpeg',
  '.png',
  '.gif',
  '.webp',
  '.bmp',
]

// 허용된 문서 확장자
const ALLOWED_DOCUMENT_EXTENSIONS = [
  '.pdf',
  '.doc',
  '.docx',
  '.xls',
  '.xlsx',
  '.ppt',
  '.pptx',
  '.txt',
  '.hwp',
]

// 최대 파일 크기 (10MB)
const MAX_FILE_SIZE = 10 * 1024 * 1024

/**
 * 파일 확장자 검증
 * @param {string} fileName - 파일명
 * @param {string} type - 'image' | 'document' | 'all'
 * @returns {boolean}
 */
export const validateFileExtension = (fileName, type = 'all') => {
  const extension = fileName.substring(fileName.lastIndexOf('.')).toLowerCase()

  if (type === 'image') {
    return ALLOWED_IMAGE_EXTENSIONS.includes(extension)
  } else if (type === 'document') {
    return ALLOWED_DOCUMENT_EXTENSIONS.includes(extension)
  } else {
    return [
      ...ALLOWED_IMAGE_EXTENSIONS,
      ...ALLOWED_DOCUMENT_EXTENSIONS,
    ].includes(extension)
  }
}

/**
 * 파일 크기 검증
 * @param {File} file - 파일 객체
 * @param {number} maxSize - 최대 크기 (bytes)
 * @returns {boolean}
 */
export const validateFileSize = (file, maxSize = MAX_FILE_SIZE) => {
  return file.size <= maxSize
}

/**
 * 파일 크기를 읽기 쉬운 형식으로 변환
 * @param {number} bytes - 바이트 크기
 * @returns {string}
 */
export const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes'

  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))

  return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i]
}

/**
 * 단일 파일 업로드
 * @param {File} file - 업로드할 파일
 * @param {string} category - 파일 카테고리 (notice, board, etc.)
 * @param {Function} onProgress - 진행률 콜백
 * @returns {Promise<Object>} 업로드된 파일 정보
 */
export const uploadFile = async (
  file,
  category = 'notice',
  onProgress = null,
) => {
  // 파일 검증
  if (!validateFileSize(file)) {
    throw new Error(
      `파일 크기는 ${formatFileSize(MAX_FILE_SIZE)} 이하여야 합니다.`,
    )
  }

  if (!validateFileExtension(file.name)) {
    throw new Error('지원하지 않는 파일 형식입니다.')
  }

  // 이미지인 경우 업로드 전 압축
  const uploadTarget = file.type.startsWith('image/') ? await compressImage(file) : file

  const formData = new FormData()
  formData.append('file', uploadTarget)
  formData.append('category', category)

  try {
    const response = await apiClient.post('/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
      onUploadProgress: (progressEvent) => {
        if (onProgress) {
          const percentCompleted = Math.round(
            (progressEvent.loaded * 100) / progressEvent.total,
          )
          onProgress(percentCompleted)
        }
      },
    })

    return response.data
  } catch (error) {
    console.error('File upload error:', error)
    throw error
  }
}

/**
 * 여러 파일 업로드
 * @param {FileList|Array} files - 업로드할 파일들
 * @param {string} category - 파일 카테고리
 * @param {Function} onProgress - 진행률 콜백
 * @returns {Promise<Array>} 업로드된 파일 정보 배열
 */
export const uploadMultipleFiles = async (
  files,
  category = 'notice',
  onProgress = null,
) => {
  const uploadPromises = Array.from(files).map((file, index) => {
    return uploadFile(file, category, (progress) => {
      if (onProgress) {
        onProgress(index, progress)
      }
    })
  })

  try {
    const results = await Promise.all(uploadPromises)
    return results
  } catch (error) {
    console.error('Multiple files upload error:', error)
    throw error
  }
}

/**
 * 이미지 미리보기 URL 생성
 * @param {File} file - 이미지 파일
 * @returns {Promise<string>} 미리보기 URL
 */
export const getImagePreviewUrl = (file) => {
  return new Promise((resolve, reject) => {
    if (!file.type.startsWith('image/')) {
      reject(new Error('이미지 파일이 아닙니다.'))
      return
    }

    const reader = new FileReader()
    reader.onload = (e) => resolve(e.target.result)
    reader.onerror = (e) => reject(e)
    reader.readAsDataURL(file)
  })
}

/**
 * 파일 삭제
 * @param {string} fileId - 삭제할 파일 ID
 * @returns {Promise<void>}
 */
export const deleteFile = async (fileId) => {
  try {
    await apiClient.delete(`/upload/${fileId}`)
  } catch (error) {
    console.error('File delete error:', error)
    throw error
  }
}

/**
 * Quill 에디터용 이미지 핸들러
 * @param {Object} quillInstance - Quill 인스턴스
 */
export const handleQuillImageUpload = (quillInstance) => {
  const input = document.createElement('input')
  input.setAttribute('type', 'file')
  input.setAttribute('accept', 'image/*')

  input.onchange = async () => {
    const file = input.files[0]
    if (!file) return

    if (!validateFileExtension(file.name, 'image')) {
      alert('이미지 파일만 업로드 가능합니다.')
      return
    }

    if (!validateFileSize(file)) {
      alert(`파일 크기는 ${formatFileSize(MAX_FILE_SIZE)} 이하여야 합니다.`)
      return
    }

    try {
      // 업로드 중 표시
      const range = quillInstance.getSelection(true)
      quillInstance.insertText(range.index, '이미지 업로드 중...')
      quillInstance.setSelection(range.index + 13)

      // 파일 업로드
      const result = await uploadFile(file, 'notice')

      // 업로드 중 텍스트 제거
      quillInstance.deleteText(range.index, 13)

      // 이미지 삽입
      quillInstance.insertEmbed(range.index, 'image', result.url)
      quillInstance.setSelection(range.index + 1)
    } catch (error) {
      alert('이미지 업로드에 실패했습니다.')
      console.error(error)
    }
  }

  input.click()
}
