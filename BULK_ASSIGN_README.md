# 참석자 속성 일괄 매핑 기능 - 빠른 시작 가이드

## 🎯 개요

300~500명의 참석자에게 속성(버스 호차, 티셔츠 사이즈 등)을 **한번에 설정**할 수 있는 기능입니다.

## ⚡ 빠른 시작

### 1. 백엔드 실행
```bash
cd C:\Users\USER\dev\startour\convention
dotnet run
```

### 2. 프론트엔드 실행
```bash
cd ClientApp
npm run dev
```

### 3. 접속
```
http://localhost:5173/admin/bulk-assign
```

## 📝 간단 사용법

1. **행사 선택** → 드롭다운에서 행사 선택
2. **참석자 선택** → 체크박스 또는 "전체 선택" 클릭
3. **속성 설정** → "선택한 참석자 속성 설정" 버튼 클릭
4. **저장** → 모달에서 속성 입력 후 "일괄 저장"

## 🔑 주요 기능

✅ 다중 선택으로 여러 참석자 한번에 처리  
✅ 검색 기능으로 특정 참석자만 필터링  
✅ 미리보기로 저장 전 확인  
✅ 트랜잭션 기반으로 안전한 처리  

## 📂 변경된 파일

### 백엔드
- `Models/DTOs/GuestAttributeDtos.cs` (신규)
- `Controllers/GuestController.cs` (수정)

### 프론트엔드
- `ClientApp/src/views/BulkAssignAttributes.vue` (신규)
- `ClientApp/src/router/index.js` (수정)

## 🔧 필수 설정

### 속성 정의 추가 (최초 1회)

```sql
INSERT INTO AttributeDefinitions 
  (ConventionId, AttributeKey, Options, OrderNum, IsRequired)
VALUES 
  (1, '버스', '["1호차","2호차","3호차"]', 1, 1),
  (1, '티셔츠', '["S","M","L","XL"]', 2, 0);
```

## 📚 상세 문서

전체 구현 내용과 상세 가이드는 [BULK_ASSIGN_GUIDE.md](./BULK_ASSIGN_GUIDE.md) 참고

## 🐛 문제 해결

### "401 Unauthorized"
→ Admin 계정으로 다시 로그인

### 참석자 목록이 안 보임
→ Guests 테이블에 데이터 있는지 확인

### 속성 정의가 안 보임
→ 위의 SQL로 AttributeDefinitions 추가

## ✅ 체크리스트

- [ ] 백엔드 실행 확인
- [ ] 프론트엔드 실행 확인
- [ ] Admin 권한 확인
- [ ] 속성 정의 추가 완료
- [ ] 참석자 데이터 존재 확인

---

**작성일**: 2025-10-14  
**버전**: 1.0.0
