-- 액션 템플릿 테이블 생성
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ActionTemplates')
BEGIN
    CREATE TABLE ActionTemplates (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TemplateType NVARCHAR(100) NOT NULL UNIQUE,
        TemplateName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Category NVARCHAR(100) NOT NULL,
        IconClass NVARCHAR(100) NULL,
        DefaultRoute NVARCHAR(200) NOT NULL,
        DefaultConfigJson NVARCHAR(MAX) NULL,
        RequiredFields NVARCHAR(MAX) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        OrderNum INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    );
    
    CREATE INDEX IX_ActionTemplate_Category ON ActionTemplates(Category);
    CREATE INDEX IX_ActionTemplate_IsActive ON ActionTemplates(IsActive);
END
GO

-- ConventionActions 테이블에 새 컬럼 추가
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ConventionActions' AND COLUMN_NAME = 'TemplateId')
BEGIN
    ALTER TABLE ConventionActions ADD TemplateId INT NULL;
    ALTER TABLE ConventionActions ADD IsRequired BIT NOT NULL DEFAULT 0;
    ALTER TABLE ConventionActions ADD IconClass NVARCHAR(100) NULL;
    ALTER TABLE ConventionActions ADD Category NVARCHAR(100) NULL;
    
    -- 외래키 추가
    ALTER TABLE ConventionActions 
    ADD CONSTRAINT FK_ConventionActions_ActionTemplates_TemplateId 
    FOREIGN KEY (TemplateId) REFERENCES ActionTemplates(Id);
    
    CREATE INDEX IX_ConventionAction_TemplateId ON ConventionActions(TemplateId);
    CREATE INDEX IX_ConventionAction_Category ON ConventionActions(Category);
END
GO

-- 기본 액션 템플릿 삽입
IF NOT EXISTS (SELECT * FROM ActionTemplates)
BEGIN
    -- 참가자 정보 카테고리
    INSERT INTO ActionTemplates (TemplateType, TemplateName, Description, Category, IconClass, DefaultRoute, DefaultConfigJson, IsActive, OrderNum)
    VALUES 
    ('PROFILE_BASIC', '기본 정보 입력', '이름, 연락처 등 기본 정보를 입력합니다', '참가자 정보', 'fa-user', '/actions/profile-basic', NULL, 1, 1),
    ('PROFILE_OVERSEAS', '해외여행 정보', '여권, 비자 등 해외여행 관련 정보를 입력합니다', '참가자 정보', 'fa-passport', '/actions/profile-overseas', NULL, 1, 2),
    ('PROFILE_MEDICAL', '건강 정보', '알레르기, 복용약 등 건강 관련 정보를 입력합니다', '참가자 정보', 'fa-heartbeat', '/actions/profile-medical', NULL, 1, 3),
    ('PROFILE_DIETARY', '식단 제한사항', '채식, 할랄 등 식단 제한사항을 입력합니다', '참가자 정보', 'fa-utensils', '/actions/profile-dietary', NULL, 1, 4);

    -- 일정 관리 카테고리
    INSERT INTO ActionTemplates (TemplateType, TemplateName, Description, Category, IconClass, DefaultRoute, DefaultConfigJson, IsActive, OrderNum)
    VALUES 
    ('SCHEDULE_CHOICE', '일정 선택', '참여할 일정을 선택합니다', '일정 관리', 'fa-calendar-check', '/actions/schedule-choice', NULL, 1, 1),
    ('SCHEDULE_PREFERENCE', '선호 일정 조사', '선호하는 일정 유형을 조사합니다', '일정 관리', 'fa-star', '/actions/schedule-preference', NULL, 1, 2),
    ('TOUR_OPTION', '옵션 투어 신청', '추가 투어 옵션을 선택합니다', '일정 관리', 'fa-map-marked', '/actions/tour-option', NULL, 1, 3);

    -- 문서 제출 카테고리
    INSERT INTO ActionTemplates (TemplateType, TemplateName, Description, Category, IconClass, DefaultRoute, DefaultConfigJson, IsActive, OrderNum)
    VALUES 
    ('DOCUMENT_UPLOAD', '문서 업로드', '필요한 문서를 업로드합니다', '문서 제출', 'fa-file-upload', '/actions/document-upload', NULL, 1, 1),
    ('CONSENT_FORM', '동의서 제출', '참가 동의서를 작성하고 제출합니다', '문서 제출', 'fa-file-signature', '/actions/consent-form', NULL, 1, 2),
    ('CONTRACT_SIGN', '계약서 서명', '전자 계약서에 서명합니다', '문서 제출', 'fa-signature', '/actions/contract-sign', NULL, 1, 3);

    -- 피드백 카테고리
    INSERT INTO ActionTemplates (TemplateType, TemplateName, Description, Category, IconClass, DefaultRoute, DefaultConfigJson, IsActive, OrderNum)
    VALUES 
    ('SURVEY_PRE', '사전 설문조사', '행사 전 설문조사를 진행합니다', '피드백', 'fa-poll', '/actions/survey-pre', NULL, 1, 1),
    ('SURVEY_POST', '사후 설문조사', '행사 후 만족도 조사를 진행합니다', '피드백', 'fa-poll-h', '/actions/survey-post', NULL, 1, 2),
    ('FEEDBACK_DAILY', '일일 피드백', '매일 피드백을 제출합니다', '피드백', 'fa-comment-dots', '/actions/feedback-daily', NULL, 1, 3);

    -- 결제 카테고리
    INSERT INTO ActionTemplates (TemplateType, TemplateName, Description, Category, IconClass, DefaultRoute, DefaultConfigJson, IsActive, OrderNum)
    VALUES 
    ('PAYMENT_DEPOSIT', '계약금 납부', '참가 계약금을 납부합니다', '결제', 'fa-credit-card', '/actions/payment-deposit', NULL, 1, 1),
    ('PAYMENT_BALANCE', '잔금 납부', '잔금을 납부합니다', '결제', 'fa-money-check', '/actions/payment-balance', NULL, 1, 2),
    ('PAYMENT_OPTION', '옵션 비용 납부', '추가 옵션 비용을 납부합니다', '결제', 'fa-coins', '/actions/payment-option', NULL, 1, 3);

    -- 커뮤니케이션 카테고리
    INSERT INTO ActionTemplates (TemplateType, TemplateName, Description, Category, IconClass, DefaultRoute, DefaultConfigJson, IsActive, OrderNum)
    VALUES 
    ('EMERGENCY_CONTACT', '비상연락처 등록', '비상연락처를 등록합니다', '커뮤니케이션', 'fa-phone-alt', '/actions/emergency-contact', NULL, 1, 1),
    ('ROOM_PREFERENCE', '룸메이트 선호도', '룸메이트 선호사항을 입력합니다', '커뮤니케이션', 'fa-bed', '/actions/room-preference', NULL, 1, 2),
    ('QUESTION_SUBMIT', '질문 제출', '행사 관련 질문을 제출합니다', '커뮤니케이션', 'fa-question-circle', '/actions/question-submit', NULL, 1, 3);
END
GO

PRINT 'ActionTemplate 마이그레이션 완료';