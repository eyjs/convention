# 참석자 생성 시 User 자동생성 방식

## 현재 구현 방식 (AdminController.CreateGuest)

```csharp
// 1. Guest 레코드 생성
var guest = new Guest
{
    ConventionId = conventionId,
    GuestName = "홍길동",
    Telephone = "01012345678",
    AccessToken = GenerateAccessToken(), // 64자리 GUID
    IsRegisteredUser = false
};
_context.Guests.Add(guest);
await _context.SaveChangesAsync(); // guest.Id 생성됨

// 2. 비밀번호 결정 (우선순위)
string passwordToHash;
if (!string.IsNullOrWhiteSpace(dto.Password))
    passwordToHash = dto.Password; // 수기 입력
else if (!string.IsNullOrWhiteSpace(dto.ResidentNumber) && dto.ResidentNumber.Length >= 6)
    passwordToHash = dto.ResidentNumber.Substring(0, 6); // 주민번호 앞6자리
else
    passwordToHash = "123456"; // 기본값

// 3. User 계정 자동 생성
var user = new User
{
    LoginId = guest.AccessToken, // AccessToken을 LoginId로 사용!
    PasswordHash = _authService.HashPassword(passwordToHash),
    Name = guest.GuestName,
    Phone = guest.Telephone,
    Role = "Guest",
    IsActive = true
};
_context.Users.Add(user);
await _context.SaveChangesAsync(); // user.Id 생성됨

// 4. Guest와 User 연결
guest.UserId = user.Id;
guest.IsRegisteredUser = true;
await _context.SaveChangesAsync();
```

## 핵심 포인트

1. **LoginId = AccessToken**: 64자리 고유값이므로 중복 없음
2. **자동 회원 가입**: Guest 생성과 동시에 User 계정 생성
3. **두 가지 로그인 방식**:
   - 회원: `/login` → LoginId(AccessToken) + Password
   - 비회원: `/guest/{conventionId}/{accessToken}` → AccessToken만

## 문제점

현재 방식에서는 **사용자가 LoginId(AccessToken)를 모르면 로그인 불가**

### 해결 방안
- 아이디 찾기: 이름 + 전화번호 → LoginId 알려주기
- 비밀번호 찾기: 이름 + 전화번호 + 인증코드 → 비밀번호 재설정
