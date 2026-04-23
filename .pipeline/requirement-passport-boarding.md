# 여권정보 일괄 업로드 시스템

## 생성일시
2026-04-23 (수정)

## 목적
- 해외행사 시 300~500명의 여권정보를 효율적으로 관리
- 관리자가 개발자 도움 없이 직접 일괄 업로드 가능
- 참석자 업로드와 분리 (국내행사에서는 불필요)

## 스코프

### 포함
- [ ] Step 1: 여권 텍스트 엑셀 일괄 업로드 (영문명, 여권번호, 만료일)
- [ ] Step 2: 여권 이미지 ZIP 일괄 업로드 (파일명 매칭)
- [ ] Step 3: 매칭 결과 리포트 (성공/실패 목록) + 웹에서 실패건 수정
- [ ] 관리자 전용 별도 탭 (참석자 업로드와 분리)

### 제외
- 탑승권 PDF 처리 (별도 작업)
- OCR 자동 인식
- 참석자 업로드에 통합 (국내행사 불필요하므로 분리)

## 기술스택
- 백엔드: .NET 8 / System.IO.Compression (ZIP 처리)
- 프론트: Vue 3 / PDF.js 또는 iframe 뷰어
- DB: UserConvention에 BoardingPassUrl 추가

## 핵심 기능

### P0 (필수)
1. 여권 텍스트 엑셀 일괄 업로드
2. 여권 이미지 ZIP 일괄 업로드
3. 탑승권 PDF ZIP 일괄 업로드
4. 사용자 탑승권 뷰어

### P1 (중요)
5. 매칭 실패 리포트 + 수동 매칭 UI
6. 기존 데이터 덮어쓰기/건너뛰기 옵션

## 상세 설계

### 1. 매칭 규칙 (엑셀 E열 ↔ ZIP 파일명)

**엑셀 E열에 파일명을 기입하고, ZIP 내 동일 파일명의 이미지와 매칭**

- 시스템은 파일명 형식을 강제하지 않음 (이름이든 숫자든 자유)
- 스태프가 수동으로 엑셀 E열과 실제 파일명을 맞춰서 준비
- 시스템은 **E열 값 = ZIP 내 파일명(확장자 제외)** 일치 여부만 체크
- 매칭 실패(파일명 불일치, 중복) 건은 웹에서 리포트 + 수동 수정 가능

**엑셀 행 ↔ 참석자 매칭**: 이름(A열) 기준으로 행사 참석자 DB와 매칭
- 동명이인 시 경고 + 수동 매칭
- 연락처 없는 동반자도 이름으로 매칭 (User 엔티티에 이미 존재)

### 2. DB 변경

```sql
-- UserConvention에 탑승권 경로 추가
ALTER TABLE UserConventions ADD BoardingPassUrl NVARCHAR(MAX) NULL;
```

여권 관련 필드는 User 테이블에 이미 존재:
- PassportNumber, PassportExpiryDate, PassportImageUrl
- FirstName, LastName

### 3. API 설계

```
-- 여권 텍스트 엑셀 일괄
POST /api/admin/conventions/{id}/passport/bulk-text
Body: multipart/form-data (엑셀 파일)
Response: { matched: 280, failed: 20, failedList: [...] }

-- 여권 이미지 ZIP 일괄
POST /api/admin/conventions/{id}/passport/bulk-images
Body: multipart/form-data (ZIP 파일)
Response: { matched: 250, failed: 50, failedList: [...] }

-- 탑승권 PDF ZIP 일괄
POST /api/admin/conventions/{id}/boarding-pass/bulk
Body: multipart/form-data (ZIP 파일)
Response: { matched: 290, failed: 10, failedList: [...] }

-- 사용자 탑승권 조회
GET /api/conventions/{id}/my-boarding-pass
Response: { url: "/uploads/boarding/..." } 또는 404
```

### 4. 엑셀 양식 (여권 텍스트 + 파일명)

| A열 | B열 | C열 | D열 | E열 |
|-----|-----|-----|-----|-----|
| 이름 | 영문성(Last) | 영문이름(First) | 여권만료일 | 파일명 |
| 김도현 | KIM | DOHYUN | 2030-12-31 | 김도현_여권 |
| 강경요 | KANG | KYOUNGYO | 2029-06-15 | 001 |

- E열 파일명은 ZIP 내 파일명과 일치해야 함 (확장자 제외)
- E열이 비어있으면 이미지 매칭 건너뜀 (텍스트 정보만 업데이트)
- 이름(A열)으로 행사 참석자 DB와 매칭

### 5. 프론트엔드 화면

#### 관리자 (BulkUpload 탭 추가)
- 여권정보 탭: 엑셀 업로드 + ZIP 이미지 업로드
- 탑승권 탭: ZIP PDF 업로드
- 업로드 후 매칭 결과 표시 (성공/실패 테이블)
- 실패 건 수동 매칭 드롭다운

#### 사용자 (더보기 또는 일정 상세)
- "내 탑승권" 버튼 → PDF 뷰어 (iframe 또는 새 탭)

### 6. 처리 흐름 (All-or-Nothing)

```
Step 1: 엑셀 + ZIP 업로드 (프리뷰)
  → 엑셀 파싱 (이름, 영문성, 영문이름, 만료일, 파일명)
  → ZIP 압축 해제 (임시)
  → 검증:
    ✓ 엑셀 내 파일명 중복 체크
    ✓ 이름으로 참석자 DB 매칭 체크
    ✓ E열 파일명 ↔ ZIP 파일명 매칭 체크
  → 에러 있으면: DB 삽입 안 함, 에러 목록 반환
  → 관리자가 엑셀 수정 후 재업로드

Step 2: 검증 통과 시 확정
  → 참석자별 여권 텍스트 정보 DB 업데이트
  → 이미지 파일 uploads/ 폴더로 이동
  → PassportImageUrl DB 업데이트
  → 결과 반환 (N명 처리 완료)
```

**핵심: 에러 있으면 아예 저장 안 함 → 엑셀 수정 → 재업로드**

## 제약사항
- ZIP 파일 최대 500MB
- 개별 이미지 최대 10MB
- 개별 PDF 최대 20MB
- 동명이인 매칭 시 자동 스킵 + 수동 매칭 요구

## 성공 기준
- 300명 ZIP 업로드 후 90%+ 자동 매칭
- 매칭 실패 건 리포트에서 즉시 확인 가능
- 사용자가 앱에서 자신의 탑승권 확인 가능
