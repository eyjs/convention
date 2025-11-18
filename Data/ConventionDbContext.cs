using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LocalRAG.Entities;
using LocalRAG.Entities.Action;
using LocalRAG.Entities.FormBuilder;
using LocalRAG.Entities.PersonalTrip;
using LocalRAG.DTOs.ScheduleModels;

namespace LocalRAG.Data;

public class ConventionDbContext : DbContext
{
    public ConventionDbContext(DbContextOptions<ConventionDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Convention> Conventions { get; set; }
    public DbSet<UserConvention> UserConventions { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<GuestAttribute> GuestAttributes { get; set; }
    public DbSet<AttributeDefinition> AttributeDefinitions { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Owner> Owners { get; set; }

    public DbSet<ScheduleTemplate> ScheduleTemplates { get; set; }
    public DbSet<ScheduleItem> ScheduleItems { get; set; }
    public DbSet<GuestScheduleTemplate> GuestScheduleTemplates { get; set; }
    public DbSet<AttributeTemplate> AttributeTemplates { get; set; }
    public DbSet<Notice> Notices { get; set; }
    public DbSet<NoticeCategory> NoticeCategories { get; set; }
    public DbSet<Comment> Comments { get; set; }
    
    // Action Management
    public DbSet<ConventionAction> ConventionActions { get; set; }
    public DbSet<UserActionStatus> UserActionStatuses { get; set; }
    public DbSet<ActionTemplate> ActionTemplates { get; set; }
    public DbSet<ActionSubmission> ActionSubmissions { get; set; }
    public DbSet<FileAttachment> FileAttachments { get; set; }
    public DbSet<Gallery> Galleries { get; set; }
    public DbSet<GalleryImage> GalleryImages { get; set; }
    public DbSet<ConventionChatMessage> ConventionChatMessages { get; set; }

    // Form Builder System
    public DbSet<FormDefinition> FormDefinitions { get; set; }
    public DbSet<FormField> FormFields { get; set; }
    public DbSet<FormSubmission> FormSubmissions { get; set; }

    public DbSet<VectorDataEntry> VectorDataEntries { get; set; } //  DbSet 추가
    
    public DbSet<LlmSetting> LlmSettings { get; set; }
    public DbSet<Survey> Surveys { get; set; }
    public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<SurveyResponse> SurveyResponses { get; set; }
    public DbSet<SurveyResponseDetail> SurveyResponseDetails { get; set; }

    // Personal Trip
    public DbSet<PersonalTrip> PersonalTrips { get; set; }
    public DbSet<Flight> Flights { get; set; }
    public DbSet<Accommodation> Accommodations { get; set; }
    public DbSet<ItineraryItem> ItineraryItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // VectorDataEntry 테이블 설정
        modelBuilder.Entity<VectorDataEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmbeddingData).HasColumnType("varbinary(max)").IsRequired(); // 벡터 타입
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => e.ConventionId); // ConventionId 인덱스
            entity.Property(e => e.MetadataJson).HasMaxLength(2048); // 필요시 길이 제한
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("getdate()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Role).HasDefaultValue("Guest");

            entity.HasIndex(e => e.LoginId).IsUnique().HasDatabaseName("UQ_User_LoginId");
            entity.HasIndex(e => e.Email).HasDatabaseName("IX_User_Email");
            entity.HasIndex(e => e.Phone).HasDatabaseName("IX_User_Phone");
            entity.HasIndex(e => e.Role).HasDatabaseName("IX_User_Role");
            entity.HasIndex(e => e.Name).HasDatabaseName("IX_User_Name");

            entity.HasMany(u => u.UserConventions)
                  .WithOne(uc => uc.User)
                  .HasForeignKey(uc => uc.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.GuestAttributes)
                  .WithOne(ga => ga.User)
                  .HasForeignKey(ga => ga.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.UserActionStatuses)
                  .WithOne(gas => gas.User)
                  .HasForeignKey(gas => gas.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Convention>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.RegDtm).HasDefaultValueSql("getdate()");
            entity.Property(e => e.DeleteYn).HasDefaultValue("N");
            entity.Property(e => e.ConventionType).HasDefaultValue("DOMESTIC");
            entity.Property(e => e.RenderType).HasDefaultValue("STANDARD");

            entity.HasIndex(e => e.StartDate).HasDatabaseName("IX_Convention_StartDate");
            entity.HasIndex(e => e.ConventionType).HasDatabaseName("IX_Convention_ConventionType");

            entity.HasMany(c => c.UserConventions)
                  .WithOne(uc => uc.Convention)
                  .HasForeignKey(uc => uc.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // UserConvention (User-Convention 다대다 매핑 테이블)
        modelBuilder.Entity<UserConvention>(entity =>
        {
            entity.HasKey(uc => new { uc.UserId, uc.ConventionId });

            entity.Property(uc => uc.CreatedAt).HasDefaultValueSql("getdate()");

            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_UserConvention_UserId");
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_UserConvention_ConventionId");
            entity.HasIndex(e => e.AccessToken).IsUnique().HasDatabaseName("UQ_UserConvention_AccessToken");
            entity.HasIndex(e => new { e.UserId, e.ConventionId })
                  .IsUnique()
                  .HasDatabaseName("UQ_UserConvention_UserId_ConventionId");

            entity.HasOne(uc => uc.User)
                  .WithMany(u => u.UserConventions)
                  .HasForeignKey(uc => uc.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(uc => uc.Convention)
                  .WithMany(c => c.UserConventions)
                  .HasForeignKey(uc => uc.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.OrderNum).HasDefaultValue(0);
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Schedule_ConventionId");
        });

        modelBuilder.Entity<GuestAttribute>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasIndex(e => new { e.UserId, e.AttributeKey })
                  .IsUnique()
                  .HasDatabaseName("UQ_GuestAttributes_UserId_AttributeKey");

            entity.HasOne(ga => ga.User)
                  .WithMany(u => u.GuestAttributes)
                  .HasForeignKey(ga => ga.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<AttributeDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_AttributeDefinition_ConventionId");
            entity.HasIndex(e => new { e.ConventionId, e.AttributeKey })
                  .IsUnique()
                  .HasDatabaseName("UQ_AttributeDefinition_ConventionId_AttributeKey");

            entity.HasOne(ad => ad.Convention)
                  .WithMany()
                  .HasForeignKey(ad => ad.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<ScheduleTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_ScheduleTemplate_ConventionId");

            entity.HasOne(st => st.Convention)
                  .WithMany(c => c.ScheduleTemplates)
                  .HasForeignKey(st => st.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScheduleItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.ScheduleTemplateId).HasDatabaseName("IX_ScheduleItem_ScheduleTemplateId");

            entity.HasOne(si => si.ScheduleTemplate)
                  .WithMany(st => st.ScheduleItems)
                  .HasForeignKey(si => si.ScheduleTemplateId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GuestScheduleTemplate>(entity =>
        {
            entity.HasKey(gst => new { gst.UserId, gst.ScheduleTemplateId });

            entity.HasOne(gst => gst.User)
                  .WithMany(u => u.GuestScheduleTemplates)
                  .HasForeignKey(gst => gst.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gst => gst.ScheduleTemplate)
                  .WithMany(st => st.GuestScheduleTemplates)
                  .HasForeignKey(gst => gst.ScheduleTemplateId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<AttributeTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_AttributeTemplate_ConventionId");
            entity.HasIndex(e => new { e.ConventionId, e.AttributeKey })
                  .IsUnique()
                  .HasDatabaseName("UQ_AttributeTemplate_ConventionId_AttributeKey");

            entity.HasOne(at => at.Convention)
                  .WithMany()
                  .HasForeignKey(at => at.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Feature_ConventionId");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Menu_ConventionId");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.MenuId).HasDatabaseName("IX_Section_MenuId");
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Owner_ConventionId");
        });

        // NoticeCategory 설정
        modelBuilder.Entity<NoticeCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_NoticeCategory_ConventionId");
            entity.HasIndex(e => new { e.ConventionId, e.Name })
                  .HasDatabaseName("IX_NoticeCategory_ConventionId_Name");
            
            entity.HasOne(e => e.Convention)
                  .WithMany()
                  .HasForeignKey(e => e.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Notice 설정
        modelBuilder.Entity<Notice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");
            entity.Property(e => e.ViewCount).HasDefaultValue(0);
            entity.Property(e => e.IsPinned).HasDefaultValue(false);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Notice_ConventionId");
            entity.HasIndex(e => e.AuthorId).HasDatabaseName("IX_Notice_AuthorId");
            entity.HasIndex(e => e.IsPinned).HasDatabaseName("IX_Notice_IsPinned");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_Notice_CreatedAt");
            entity.HasIndex(e => e.NoticeCategoryId).HasDatabaseName("IX_Notice_NoticeCategoryId");
            
            entity.HasOne(e => e.Author)
                  .WithMany()
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Convention)
                  .WithMany()
                  .HasForeignKey(e => e.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.NoticeCategory)
                  .WithMany(c => c.Notices)
                  .HasForeignKey(e => e.NoticeCategoryId)
                  .OnDelete(DeleteBehavior.NoAction);
            
            entity.HasMany(e => e.Attachments)
                  .WithOne(a => a.Notice)
                  .HasForeignKey(a => a.NoticeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // FileAttachment 설정
        modelBuilder.Entity<FileAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("getdate()");
            entity.HasIndex(e => e.NoticeId).HasDatabaseName("IX_FileAttachment_NoticeId");
            entity.HasIndex(e => e.Category).HasDatabaseName("IX_FileAttachment_Category");
        });

        // Gallery 설정
        modelBuilder.Entity<Gallery>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Gallery_ConventionId");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_Gallery_CreatedAt");
            
            entity.HasOne(e => e.Convention)
                  .WithMany()
                  .HasForeignKey(e => e.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Author)
                  .WithMany()
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(e => e.Images)
                  .WithOne(i => i.Gallery)
                  .HasForeignKey(i => i.GalleryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // GalleryImage 설정
        modelBuilder.Entity<GalleryImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImageUrl).IsRequired();
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("getdate()");
            entity.HasIndex(e => e.GalleryId).HasDatabaseName("IX_GalleryImage_GalleryId");
        });

        // ChatMessage 설정
        modelBuilder.Entity<ConventionChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");

            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_ChatMessage_ConventionId");
            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_ChatMessage_UserId");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_ChatMessage_CreatedAt");

            entity.HasOne(e => e.Convention)
                  .WithMany() // Convention 모델에 ChatMessages 컬렉션이 필요하면 추가
                  .HasForeignKey(e => e.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                  .WithMany() // User 모델에 ChatMessages 컬렉션이 필요하면 추가
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.NoAction); // 순환 종속성 오류 방지
        });

        // ConventionAction 설정
        modelBuilder.Entity<ConventionAction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(4000); // 상세 설명 (HTML 지원)
            entity.Property(e => e.MapsTo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ConfigJson).HasMaxLength(4000); // JSON 설정
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("getdate()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OrderNum).HasDefaultValue(0);
            entity.Property(e => e.ActionCategory).HasMaxLength(50); // BUTTON, MENU, AUTO_POPUP, BANNER, CARD
            entity.Property(e => e.TargetLocation).HasMaxLength(100); // HOME_SUB_HEADER, SCHEDULE_CONTENT_TOP 등

            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_ConventionAction_ConventionId");

            entity.HasOne(e => e.Convention)
                  .WithMany()
                  .HasForeignKey(e => e.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // GuestActionStatus 설정
        modelBuilder.Entity<UserActionStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsComplete).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("getdate()");

            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_GuestActionStatus_UserId");
            entity.HasIndex(e => e.ConventionActionId).HasDatabaseName("IX_GuestActionStatus_ConventionActionId");
            entity.HasIndex(e => new { e.UserId, e.ConventionActionId })
                  .IsUnique()
                  .HasDatabaseName("UQ_GuestActionStatus_UserId_ConventionActionId");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserActionStatuses)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.NoAction); // 순환 참조 방지

            entity.HasOne(e => e.ConventionAction)
                  .WithMany(ca => ca.UserActionStatuses)
                  .HasForeignKey(e => e.ConventionActionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ActionSubmission 설정 (GenericForm 응답 저장)
        modelBuilder.Entity<ActionSubmission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.SubmissionDataJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.SubmittedAt).HasDefaultValueSql("getdate()");

            entity.HasIndex(e => e.ConventionActionId).HasDatabaseName("IX_ActionSubmission_ConventionActionId");
            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_ActionSubmission_UserId");
            entity.HasIndex(e => new { e.UserId, e.ConventionActionId })
                  .IsUnique()
                  .HasDatabaseName("UQ_ActionSubmission_UserId_ConventionActionId");

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.NoAction); // 순환 참조 방지

            entity.HasOne(e => e.ConventionAction)
                  .WithMany()
                  .HasForeignKey(e => e.ConventionActionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Notice)
                .WithMany(n => n.Comments)
                .HasForeignKey(e => e.NoticeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Survey
        modelBuilder.Entity<Survey>()
            .HasMany(s => s.Questions)
            .WithOne(q => q.Survey)
            .HasForeignKey(q => q.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Survey>()
            .HasMany(s => s.Responses)
            .WithOne(r => r.Survey)
            .HasForeignKey(r => r.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        // SurveyQuestion
        modelBuilder.Entity<SurveyQuestion>()
            .HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // SurveyResponse
        modelBuilder.Entity<SurveyResponse>()
            .HasMany(r => r.Details)
            .WithOne(d => d.Response)
            .HasForeignKey(d => d.ResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SurveyResponse>()
            .HasOne(sr => sr.User)
            .WithMany()
            .HasForeignKey(sr => sr.UserId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent circular dependency

        // SurveyResponseDetail
        modelBuilder.Entity<SurveyResponseDetail>()
            .HasOne(rd => rd.Question)
            .WithMany() // Assuming SurveyQuestion entity does not have a collection of ResponseDetails
            .HasForeignKey(rd => rd.QuestionId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent circular dependency

        modelBuilder.Entity<SurveyResponseDetail>()
            .HasOne(rd => rd.SelectedOption)
            .WithMany() // Assuming QuestionOption entity does not have a collection of ResponseDetails
            .HasForeignKey(rd => rd.SelectedOptionId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent circular dependency

        // PersonalTrip
        modelBuilder.Entity<PersonalTrip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");

            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_PersonalTrip_UserId");
            entity.HasIndex(e => e.StartDate).HasDatabaseName("IX_PersonalTrip_StartDate");
            entity.HasIndex(e => e.ShareToken).IsUnique().HasDatabaseName("UQ_PersonalTrip_ShareToken");

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Flights)
                  .WithOne(f => f.PersonalTrip)
                  .HasForeignKey(f => f.PersonalTripId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Accommodations)
                  .WithOne(a => a.PersonalTrip)
                  .HasForeignKey(a => a.PersonalTripId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Flight
        modelBuilder.Entity<Flight>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");

            entity.HasIndex(e => e.PersonalTripId).HasDatabaseName("IX_Flight_PersonalTripId");
            entity.HasIndex(e => e.DepartureTime).HasDatabaseName("IX_Flight_DepartureTime");
        });

        // Accommodation
        modelBuilder.Entity<Accommodation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");

            entity.HasIndex(e => e.PersonalTripId).HasDatabaseName("IX_Accommodation_PersonalTripId");
            entity.HasIndex(e => e.CheckInTime).HasDatabaseName("IX_Accommodation_CheckInTime");
        });

        // ItineraryItem
        modelBuilder.Entity<ItineraryItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasIndex(e => e.PersonalTripId).HasDatabaseName("IX_ItineraryItem_PersonalTripId");
            entity.HasIndex(e => e.DayNumber).HasDatabaseName("IX_ItineraryItem_DayNumber");
        });
    }
}
