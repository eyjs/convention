-- 관리자 계정 생성
-- 비밀번호: admin123

INSERT INTO Users (LoginId, PasswordHash, Name, Email, Phone, Role, IsActive, EmailVerified, PhoneVerified, CreatedAt, UpdatedAt)
VALUES (
    'admin',
    -- BCrypt 해시: admin123
    '$2a$11$6EBIrJZOJBxzQPj9.lvSFOxJGhPp3Z6P6bZ6xNJWJQxKxZvJXq5Zu',
    '관리자',
    'admin@convention.com',
    '010-0000-0000',
    'Admin',
    1,
    1,
    1,
    GETDATE(),
    GETDATE()
);

SELECT * FROM Users WHERE LoginId = 'admin';
