# 태스크 결과: 참석자 엑셀 업로드 가변 속성 컬럼 지원

## 상태
완료

## 완료 기준 체크
- [x] dotnet build --no-restore 성공 (경고 0, 오류 0)
- [x] npm run build 성공
- [x] 가변 속성 컬럼(8번째~)이 GuestAttribute로 저장
- [x] 그룹-일정매핑 시트가 GuestScheduleTemplate으로 연결 (기존 로직 유지)
- [x] 기존 참석자 업로드 호환 (가변 컬럼 없으면 기존대로 동작)
- [x] 안내 행(※로 시작) 스킵

## 생성/수정 파일

### 수정
- `Services/Upload/UserUploadService.cs`
  - 시트1 헤더에서 8번째 컬럼부터 attributeHeaders 파싱
  - 각 데이터 행에서 ※로 시작하면 스킵
  - rowAttributesMap에 행별 속성 임시 수집
  - SaveChanges 후 ProcessInlineAttributesAsync 호출
  - ProcessInlineAttributesAsync 메서드 추가 (기존 GuestAttribute 있으면 덮어쓰기)
  - ProcessScheduleMappingSheet 주석에 코스설명(C열) 지원 명시

- `Controllers/UploadController.cs`
  - DownloadGuests: User.GuestAttributes Include 추가
  - 다운로드 시 참석자 속성 키를 8번째 컬럼부터 출력
  - 그룹-일정매핑 시트에 코스설명(C열) 추가

- `ClientApp/src/components/admin/BulkUpload.vue`
  - 시트1 형식 안내: 고정 7컬럼 + 가변 속성 컬럼 설명 추가
  - 시트2 형식 안내: 코스설명(C열) 추가
  - 샘플 엑셀: 룸번호/룸메이트/버스좌석 예시 컬럼 추가, ※ 안내 행 예시 추가
  - 업로드 결과 메시지: 속성 생성/수정 건수 표시

## 구현 판단 기록
- 가변 속성은 SaveChanges 이후 userId 확정 시점에 처리 (신규 User의 Id는 EF가 SaveChanges 후 부여)
- 속성 수집은 rowAttributesMap에 미리 저장했다가 일괄 처리
- 기존 ProcessAttributeSheet는 코드 유지 (별도 속성 전용 업로드에서 사용 가능)
- 다운로드 시 해당 행사 참석자들의 속성 키를 Union해서 컬럼 생성 (없는 사람은 빈 값)
