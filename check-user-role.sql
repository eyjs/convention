-- 현재 사용자 역할 확인
SELECT 
    Id,
    LoginId,
    Name,
    Email,
    Role,
    IsActive
FROM Users
ORDER BY CreatedAt DESC;

-- 관리자 역할 업데이트 (필요시)
-- UPDATE Users 
-- SET Role = 'SystemAdmin'
-- WHERE LoginId = 'admin' OR LoginId = '여기에_당신의_로그인ID';
