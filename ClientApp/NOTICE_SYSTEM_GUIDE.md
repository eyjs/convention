# 공지사항 시스템 구축 완료 📢

## 📁 구현된 파일 목록

### 프론트엔드 (Vue 3 + JavaScript)

#### 1. 유틸리티 & 서비스
- `src/utils/fileUpload.js` - 공통 파일 업로드 핸들러
- `src/services/noticeService.js` - 공지사항 API 서비스

#### 2. 사용자 페이지 (읽기 전용)
- `src/views/NoticeList.vue` - 공지사항 목록 페이지
- `src/views/NoticeDetail.vue` - 공지사항 상세보기 페이지

#### 3. 관리자 페이지 (CRUD)
- `src/components/admin/NoticeManagement.vue` - 관리자 공지사항 관리 페이지
- `src/components/admin/NoticeFormModal.vue` - 공지사항 작성/수정 모달 (Quill 에디터 포함)

#### 4. 설정
- `src/router/index.js` - 라우터 설정 업데이트
- `src/services/api.js` - Upload API 추가

### 백엔드 (C# .NET 8)
- `Models/DTOs/NoticeModels.cs` - 공지사항 관련 DTO 모델

---

## 🎯 주요 기능

### 사용자 기능
✅ **공지사항 목록 조회**
- 페이지네이션 (20개씩)
- 검색 기능 (제목/내용/제목+내용)
- 고정 공지사항 상단 표시
- NEW 배지 (3일 이내 작성)
- 첨부파일 아이콘 표시

✅ **공지사항 상세보기**
- Quill 에디터로 작성된 내용 표시 (읽기 전용)
- 첨부파일 다운로드
- 이전글/다음글 네비게이션
- 조회수 자동 증가

### 관리자 기능
✅ **공지사항 생성**
- Quill 에디터로 리치 텍스트 작성
- 에디터 내 이미지 직접 업로드
- 첨부파일 업로드 (최대 10MB)
- 공지사항 고정 설정

✅ **공지사항 수정**
- 기존 내용 불러오기
- 첨부파일 추가/삭제

✅ **공지사항 삭제**
- 확인 메시지와 함께 삭제

✅ **공지사항 고정 토글**
- 한 번의 클릭으로 고정/해제

✅ **검색 및 필터링**
- 제목/내용 검색
- 페이지네이션

---

## 🛠️ 기술 스택

### 프론트엔드
- **Vue 3** (Composition API)
- **Vue Router** - 라우팅
- **Axios** - HTTP 클라이언트
- **Quill Editor** (@vueup/vue-quill) - 리치 텍스트 에디터
- **Tailwind CSS** - 스타일링
- **Day.js** - 날짜 포맷팅

### 백엔드 (구현 필요)
- **ASP.NET Core 8** Web API
- **Entity Framework Core** - ORM
- **SQL Server** - 데이터베이스

---

## 📝 백엔드 구현 가이드

### 1. Entity 모델 생성 필요
```csharp
// Data/Entities/Notice.cs
public class Notice
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public bool IsPinned { get; set; }
    public int ViewCount { get; set; }
    public int AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navigation Properties
    public virtual User Author { get; set; }
    public virtual ICollection<FileAttachment> Attachments { get; set; }
}

// Data/Entities/FileAttachment.cs
public class FileAttachment
{
    public int Id { get; set; }
    public string OriginalName { get; set; }
    public string SavedName { get; set; }
    public string FilePath { get; set; }
    public long Size { get; set; }
    public string ContentType { get; set; }
    public string Category { get; set; } // notice, board, etc.
    public int? NoticeId { get; set; }
    public DateTime UploadedAt { get; set; }
    
    // Navigation Property
    public virtual Notice? Notice { get; set; }
}
```

### 2. DbContext 업데이트 필요
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Notice> Notices { get; set; }
    public DbSet<FileAttachment> FileAttachments { get; set; }
    
    // ... existing DbSets
}
```

### 3. 컨트롤러 생성 필요
```csharp
// Controllers/NoticeController.cs
[ApiController]
[Route("api/[controller]")]
public class NoticeController : ControllerBase
{
    // GET /api/notices - 목록 조회
    // GET /api/notices/{id} - 상세 조회
    // POST /api/notices - 생성 (관리자)
    // PUT /api/notices/{id} - 수정 (관리자)
    // DELETE /api/notices/{id} - 삭제 (관리자)
    // POST /api/notices/{id}/view - 조회수 증가
    // POST /api/notices/{id}/toggle-pin - 고정 토글 (관리자)
}

// Controllers/UploadController.cs
[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    // POST /api/upload - 파일 업로드
    // DELETE /api/upload/{id} - 파일 삭제
}
```

### 4. 서비스 레이어 생성 필요
```csharp
// Services/NoticeService.cs
public interface INoticeService
{
    Task<PagedNoticeResponse> GetNoticesAsync(int page, int pageSize, string? searchType, string? searchKeyword);
    Task<NoticeResponse> GetNoticeAsync(int id);
    Task<NoticeResponse> CreateNoticeAsync(CreateNoticeRequest request, int authorId);
    Task<NoticeResponse> UpdateNoticeAsync(int id, UpdateNoticeRequest request, int userId);
    Task DeleteNoticeAsync(int id, int userId);
    Task IncrementViewCountAsync(int id);
    Task<NoticeResponse> TogglePinAsync(int id, int userId);
}

// Services/FileUploadService.cs
public interface IFileUploadService
{
    Task<FileUploadResponse> UploadFileAsync(IFormFile file, string category);
    Task DeleteFileAsync(int fileId);
}
```

---

## 🚀 사용 방법

### 사용자 페이지 접근
```
/notices - 공지사항 목록
/notices/:id - 공지사항 상세보기
```

### 관리자 페이지 접근
관리자 대시보드에서 "공지사항 관리" 탭 또는 컴포넌트를 추가하여 사용:
```vue
<NoticeManagement />
```

---

## 📌 주요 특징

### 1. 파일 업로드 시스템
- **공통 핸들러**: 모든 게시판에서 재사용 가능
- **검증**: 파일 크기(10MB), 확장자 검증
- **진행률 표시**: 실시간 업로드 진행률
- **다중 파일**: 여러 파일 동시 업로드 지원

### 2. Quill 에디터
- **리치 텍스트**: 다양한 서식 지원
- **이미지 삽입**: 에디터 내에서 직접 이미지 업로드
- **읽기 전용 뷰**: 사용자는 읽기 전용 스타일로 표시
- **커스터마이징**: 툴바 옵션 설정 가능

### 3. UX 개선사항
- **반응형 디자인**: 모바일/태블릿 대응
- **로딩 상태**: 스피너로 로딩 표시
- **에러 핸들링**: 사용자 친화적 에러 메시지
- **확인 다이얼로그**: 중요한 액션 전 확인
- **NEW 배지**: 최신 공지사항 강조
- **고정 공지**: 중요 공지사항 상단 고정

---

## ⚙️ 환경 설정

### 프론트엔드 패키지 확인
이미 설치된 패키지:
```json
{
  "quill": "^2.0.2",
  "@vueup/vue-quill": "^1.2.0",
  "axios": "^1.5.0",
  "dayjs": "^1.11.18"
}
```

### 백엔드 패키지 필요
```bash
# Entity Framework Core (이미 설치됨)
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```

---

## 🔐 권한 관리

### 사용자
- 공지사항 목록 조회 ✅
- 공지사항 상세 조회 ✅
- 첨부파일 다운로드 ✅

### 관리자
- 모든 사용자 권한 ✅
- 공지사항 작성 ✅
- 공지사항 수정 ✅
- 공지사항 삭제 ✅
- 공지사항 고정/해제 ✅

---

## 📊 데이터베이스 마이그레이션

### 마이그레이션 생성
```bash
cd C:\Users\USER\dev\convention
dotnet ef migrations add AddNoticeAndFileAttachment
```

### 마이그레이션 적용
```bash
dotnet ef database update
```

---

## 🧪 테스트 체크리스트

### 사용자 페이지
- [ ] 공지사항 목록 로딩
- [ ] 페이지네이션 동작
- [ ] 검색 기능 동작
- [ ] 공지사항 상세보기
- [ ] 첨부파일 다운로드
- [ ] 이전글/다음글 이동
- [ ] 조회수 증가

### 관리자 페이지
- [ ] 공지사항 작성
- [ ] Quill 에디터 동작
- [ ] 이미지 업로드 (에디터 내)
- [ ] 첨부파일 업로드
- [ ] 공지사항 수정
- [ ] 공지사항 삭제
- [ ] 고정/해제 토글
- [ ] 검색 및 필터링

---

## 🔄 다음 단계

### 1. 백엔드 구현 (우선순위 순)
1. Entity 모델 생성
2. DbContext 업데이트
3. 마이그레이션 실행
4. NoticeController 구현
5. UploadController 구현
6. NoticeService 구현
7. FileUploadService 구현

### 2. 추가 기능 (선택사항)
- [ ] 댓글 시스템
- [ ] 좋아요 기능
- [ ] 공지사항 카테고리
- [ ] 임시저장 기능
- [ ] 공지사항 예약 발행
- [ ] 이메일 알림
- [ ] 푸시 알림

### 3. 다른 게시판 확장
- [ ] 일반 게시판
- [ ] FAQ
- [ ] QnA
- [ ] 갤러리

---

## 📖 코드 구조 설명

### fileUpload.js의 역할
```javascript
// 1. 파일 검증 함수들
validateFileExtension() // 확장자 검증
validateFileSize()      // 크기 검증
formatFileSize()        // 크기 포맷팅

// 2. 업로드 함수들
uploadFile()           // 단일 파일 업로드
uploadMultipleFiles()  // 다중 파일 업로드
deleteFile()           // 파일 삭제

// 3. Quill 에디터 전용
handleQuillImageUpload() // 에디터 이미지 핸들러
getImagePreviewUrl()     // 미리보기 URL 생성
```

### NoticeFormModal의 핵심 로직
```javascript
// Quill 에디터 설정
const editorToolbar = [...] // 툴바 옵션

// 파일 업로드 처리
handleFileSelect() // 파일 선택 시
removeFile()       // 파일 제거
setupQuillImageHandler() // 에디터 이미지 핸들러 설정

// 폼 제출
handleSubmit() // 생성/수정 API 호출
```

---

## 🎨 스타일 커스터마이징

### Quill 에디터 스타일 수정
`NoticeFormModal.vue`의 `<style>` 섹션에서:
```css
:deep(.ql-editor) {
  min-height: 400px;  /* 에디터 높이 조정 */
  padding: 20px;      /* 내부 여백 */
}
```

### 목록 레이아웃 수정
`NoticeList.vue`의 grid 클래스:
```html
<div class="grid grid-cols-12 gap-4">
  <div class="col-span-1">번호</div>
  <div class="col-span-6">제목</div>
  <!-- 컬럼 비율 조정 가능 -->
</div>
```

---

## 💡 개발 팁

### 1. Quill 에디터 이미지 업로드 커스터마이징
`handleQuillImageUpload` 함수를 수정하여 이미지 처리 방식 변경 가능

### 2. 파일 업로드 제한 변경
`fileUpload.js`의 상수 수정:
```javascript
const MAX_FILE_SIZE = 10 * 1024 * 1024 // 10MB
const ALLOWED_IMAGE_EXTENSIONS = ['.jpg', '.png', ...]
```

### 3. 페이지 크기 변경
`NoticeList.vue`에서:
```javascript
const pageSize = ref(20) // 원하는 크기로 변경
```

---

## 🐛 알려진 이슈 및 해결방법

### 이슈 1: Quill 에디터 이미지 핸들러 미작동
**해결**: `setupQuillImageHandler`를 `setTimeout`으로 감싸서 약간의 지연 후 실행

### 이슈 2: 파일 업로드 후 input 초기화 안됨
**해결**: `event.target.value = ''` 추가

### 이슈 3: 모달 외부 스크롤
**해결**: 모달 컨테이너에 `overflow-hidden` 추가

---

## 📞 문의 및 지원

백엔드 구현 시 추가 도움이 필요하시면 말씀해주세요!

**구현 완료 사항**:
- ✅ 프론트엔드 전체 구조
- ✅ 공통 파일 업로드 핸들러
- ✅ 사용자 공지사항 페이지
- ✅ 관리자 CRUD 페이지
- ✅ Quill 에디터 통합
- ✅ DTO 모델

**구현 필요 사항**:
- ⏳ Entity 모델
- ⏳ Controllers
- ⏳ Services
- ⏳ Database Migration
