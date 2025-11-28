import { ref } from 'vue'

const pendingNoticeId = ref(null)

export function useNoticeNavigation() {
  const setPendingNotice = (noticeId) => {
    pendingNoticeId.value = noticeId
  }

  const getPendingNotice = () => {
    const id = pendingNoticeId.value
    pendingNoticeId.value = null // 한 번만 사용
    return id
  }

  return {
    setPendingNotice,
    getPendingNotice,
  }
}
