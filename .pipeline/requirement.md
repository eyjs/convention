# 요구사항: 통합 알림 시스템 (Convention Notification)

## 배경
현재 시스템에 인앱 알림 기능이 없음. 관리자가 참석자들에게 실시간으로 다양한 유형의 알림을 보내고, 읽음/미읽음 추적이 필요함.

## 알림 유형 (범용)

| 유형 | 예시 | 딥링크 |
|---|---|---|
| `TEXT` | "로비에서 10분 뒤 출발합니다" | 없음 |
| `NOTICE` | "공지사항을 확인하세요" | `/conventions/:id/board/:noticeId` |
| `SURVEY` | "설문조사를 4/15까지 완료해주세요" | `/conventions/:id/surveys/:surveyId` |
| `SCHEDULE` | "2일차 저녁 일정이 변경되었습니다" | `/conventions/:id/schedule` |
| `SEAT` | "만찬 좌석이 배정되었습니다" | `/conventions/:id/my-seat?layout=:id` |
| `LINK` | 관리자 지정 URL | 커스텀 URL |

## 데이터 모델

### Notification (알림 발송 건)
```csharp
public class Notification
{
    public int Id { get; set; }
    public int ConventionId { get; set; }
    public string Type { get; set; }        // TEXT, NOTICE, SURVEY, SCHEDULE, SEAT, LINK
    public string Title { get; set; }       // "일정 변경 안내"
    public string Body { get; set; }        // 상세 내용
    public string? LinkUrl { get; set; }    // 딥링크 (nullable)
    public int? ReferenceId { get; set; }   // 관련 엔티티 ID (Notice.Id, Survey.Id 등)
    public string TargetScope { get; set; } // ALL, GROUP, INDIVIDUAL
    public string? TargetGroupName { get; set; } // 특정 그룹만
    public int SentByUserId { get; set; }   // 발송 관리자
    public DateTime CreatedAt { get; set; }

    public Convention Convention { get; set; }
    public User SentByUser { get; set; }
    public ICollection<UserNotification> UserNotifications { get; set; }
}

// 수신자별 읽음 상태
public class UserNotification
{
    public int Id { get; set; }
    public int NotificationId { get; set; }
    public int UserId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    public Notification Notification { get; set; }
    public User User { get; set; }
}
```

## API 설계

### 관리자
| Method | Endpoint | 설명 |
|---|---|---|
| GET | `/api/admin/conventions/{id}/notifications` | 발송 이력 목록 |
| POST | `/api/admin/conventions/{id}/notifications` | 알림 발송 |
| GET | `/api/admin/notifications/{id}/stats` | 읽음 통계 (읽음/미읽음/전체) |

### 사용자
| Method | Endpoint | 설명 |
|---|---|---|
| GET | `/api/notifications/my?conventionId={id}` | 내 알림 목록 |
| GET | `/api/notifications/my/unread-count?conventionId={id}` | 미읽음 개수 |
| PUT | `/api/notifications/{id}/read` | 읽음 처리 |
| PUT | `/api/notifications/read-all?conventionId={id}` | 전체 읽음 |

## 관리자 UI

### 알림 발송 페이지 (`/admin/conventions/:id/notifications`)
```
┌─────────────────────────────────────┐
│ 알림 발송                            │
├─────────────────────────────────────┤
│ 유형: [텍스트 ▼]                     │
│       텍스트 | 공지 | 설문 | 일정 | 좌석│
│                                     │
│ 제목: [_______________________]      │
│ 내용: [_______________________]      │
│       [_______________________]      │
│                                     │
│ 대상: ○ 전체  ○ 그룹별  ○ 개별       │
│       [A조 ▼]                        │
│                                     │
│ [공지 선택] ← 유형=NOTICE 시 표시     │
│ [설문 선택] ← 유형=SURVEY 시 표시     │
│                                     │
│ 미리보기: ┌───────────────┐          │
│          │ 📢 일정 변경 안내 │        │
│          │ 2일차 저녁...   │          │
│          │ [자세히 보기]   │          │
│          └───────────────┘          │
│                                     │
│ [발송] (300명에게 전달됩니다)          │
├─────────────────────────────────────┤
│ 발송 이력                            │
│ ┌─────────────────────────────────┐ │
│ │ 📢 일정 변경      읽음 245/300  │ │
│ │ 📋 설문 독촉      읽음 180/300  │ │
│ │ 💬 텍스트 안내    읽음 290/300  │ │
│ └─────────────────────────────────┘ │
└─────────────────────────────────────┘
```

## 사용자 UI

### 알림 벨 아이콘 (상단 헤더)
- 행사 상세 페이지 헤더에 🔔 아이콘
- 미읽음 개수 뱃지 (빨간 원)
- 탭 → 알림 목록 SlideUpModal

### 알림 목록
```
┌───────────────────────┐
│ 🔔 알림 (3건 미읽음)    │
├───────────────────────┤
│ ● 📢 일정 변경 안내     │  ← 미읽음 = 굵게
│   2일차 저녁이 변경...   │
│   방금 전               │
│                        │
│ ● 📋 설문 독촉          │
│   4/15까지 완료해주세요  │
│   1시간 전              │
│                        │
│ ○ 💬 로비 10분 뒤 출발   │  ← 읽음 = 연하게
│   2시간 전              │
└───────────────────────┘
```

- 알림 탭 → `IsRead = true` 자동 처리
- 딥링크 있으면 탭 시 해당 페이지 이동

## 영향 범위

### 백엔드 (신규)
- `Entities/Notification.cs`, `Entities/UserNotification.cs`
- DB 마이그레이션
- `Services/Convention/NotificationService.cs`
- `Controllers/Admin/AdminNotificationController.cs`
- `Controllers/Convention/UserNotificationController.cs`

### 프론트엔드 (신규)
- `components/admin/NotificationSender.vue` (관리자 발송 페이지)
- `components/common/NotificationBell.vue` (헤더 벨 아이콘 + 뱃지)
- `components/common/NotificationList.vue` (알림 목록 모달)
- 라우터: `/admin/conventions/:id/notifications`
- `useAdminNav.js`에 메뉴 추가

### 수정
- `ConventionHome.vue` 또는 헤더에 벨 아이콘 삽입
- 폴링 또는 진입 시 미읽음 카운트 조회

## 완료 기준
- [ ] 관리자가 6가지 유형 알림 발송 가능
- [ ] 대상 선택 (전체/그룹/개별)
- [ ] 사용자 알림 목록 + 미읽음 뱃지
- [ ] 읽음/미읽음 추적 + 통계
- [ ] 딥링크 → 해당 페이지 이동
- [ ] `dotnet build` + `npm run build` 통과
- [ ] DB 마이그레이션

## 스코프 아웃
- Push 알림 (PWA/FCM) — 이번엔 인앱만
- 실시간 WebSocket — 폴링으로 대체
- 알림 예약 발송 — 향후
