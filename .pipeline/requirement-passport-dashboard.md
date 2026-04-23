# 여권 검증 대시보드 + 승인/거절 시스템

## 생성일시
2026-04-23

## 목적
- 관리자가 참석자의 여권 등록/검증 현황을 한눈에 파악
- 검증(승인/거절) 처리를 대시보드에서 즉시 수행
- 미등록자 독려 및 거절 사유 관리

## 현재 상태
- User 테이블에 `PassportVerified`, `PassportVerifiedAt` 필드 존재
- 관리자가 참석자 목록에서 개별적으로 "검증/미검증" 토글 가능
- 거절 사유 필드 없음
- 대시보드 통계에서 여권 현황 미표시

## 핵심 기능

### P0 (필수)

#### 1. 여권 검증 대시보드 (관리자)
현재 대시보드 또는 별도 탭에 여권 현황 카드 3개:

| 카드 | 조건 | 클릭 시 |
|------|------|---------|
| 승인완료 | PassportVerified = true | 승인 참석자 리스트 |
| 승인대기 | PassportImageUrl 있음 + PassportVerified = false | 대기 참석자 리스트 (즉시 승인/거절) |
| 미등록 | PassportImageUrl 없음 | 미등록 참석자 리스트 |

#### 2. 리스트 화면
- 각 카드 클릭 시 참석자 목록 표시
- 참석자 클릭 → 참석자 상세 정보 (여권정보 포함) 표시
- **승인대기** 리스트에서:
  - 여권 이미지 미리보기
  - 여권 텍스트 정보 (영문명, 번호, 만료일) 표시
  - **승인** 버튼 → PassportVerified = true
  - **거절 + 사유** 버튼 → 거절 사유 입력 모달 → 저장

#### 3. 거절 사유 관리
DB 변경:
```sql
ALTER TABLE Users ADD PassportRejectionReason NVARCHAR(500) NULL;
ALTER TABLE Users ADD PassportRejectedAt DATETIME2 NULL;
```

거절 시:
- PassportVerified = false
- PassportRejectionReason = "사유 텍스트"
- PassportRejectedAt = 현재 시각

승인 시:
- PassportVerified = true
- PassportVerifiedAt = 현재 시각
- PassportRejectionReason = null (초기화)

### P1 (중요)

#### 4. 사용자 화면 거절 사유 표시
- 내 프로필에서 여권 상태 표시:
  - 미등록 → "여권 정보를 등록해주세요"
  - 대기중 → "검증 대기 중입니다"
  - 승인 → ✅ 검증 완료
  - 거절 → ❌ 거절됨 + 사유 표시 + 재등록 유도

#### 5. 알림 연동
- 승인 시 → 참석자에게 "여권 검증 완료" 알림
- 거절 시 → 참석자에게 "여권 검증 거절: {사유}" 알림 + 재등록 유도

## API 설계

```
-- 여권 현황 통계
GET /api/admin/conventions/{id}/passport-stats
Response: { approved: 250, pending: 30, unregistered: 20 }

-- 상태별 참석자 목록
GET /api/admin/conventions/{id}/passport-status?status=approved|pending|unregistered
Response: [{ id, name, phone, passportNumber, passportImageUrl, passportVerified, ... }]

-- 여권 승인
POST /api/admin/guests/{id}/passport/approve
Response: { message: "승인 완료" }

-- 여권 거절
POST /api/admin/guests/{id}/passport/reject
Body: { reason: "여권 만료일이 지났습니다" }
Response: { message: "거절 처리됨" }
```

## 성공 기준
- 대시보드에서 승인/대기/미등록 현황 즉시 확인
- 승인대기 리스트에서 여권 이미지 보고 바로 승인/거절
- 거절 시 사유 입력 필수, 참석자에게 알림
