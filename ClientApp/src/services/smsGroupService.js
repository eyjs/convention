import apiClient from '@/services/api'

/**
 * SMS 그룹 발송 API 서비스
 */

/**
 * 그룹 목록 + 수신자 수 조회
 */
export function fetchSmsGroups(conventionId) {
  return apiClient.get(`/admin/conventions/${conventionId}/sms/groups`)
}

/**
 * 엑셀 템플릿 다운로드
 */
export function downloadExcelTemplate(conventionId, groupName) {
  return apiClient.get(
    `/admin/conventions/${conventionId}/sms/excel-template`,
    {
      params: { group: groupName },
      responseType: 'blob',
    },
  )
}

/**
 * 엑셀 파싱
 */
export function parseExcel(conventionId, file) {
  const formData = new FormData()
  formData.append('file', file)
  return apiClient.post(
    `/admin/conventions/${conventionId}/sms/parse-excel`,
    formData,
    { headers: { 'Content-Type': 'multipart/form-data' } },
  )
}

/**
 * 엑셀 변수 치환 발송
 */
export function sendWithExcel(conventionId, template, recipients) {
  return apiClient.post(`/admin/conventions/${conventionId}/sms/send-excel`, {
    template,
    recipients,
  })
}
