using LocalRAG.DTOs.ScheduleModels;
using LocalRAG.Entities;
using LocalRAG.Entities.Flight;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalRAG.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
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
    }
}

public class ConventionConfiguration : IEntityTypeConfiguration<Convention>
{
    public void Configure(EntityTypeBuilder<Convention> entity)
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
    }
}

public class UserConventionConfiguration : IEntityTypeConfiguration<UserConvention>
{
    public void Configure(EntityTypeBuilder<UserConvention> entity)
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
    }
}

public class GuestAttributeConfiguration : IEntityTypeConfiguration<GuestAttribute>
{
    public void Configure(EntityTypeBuilder<GuestAttribute> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();

        entity.HasIndex(e => new { e.ConventionId, e.UserId, e.AttributeKey })
              .IsUnique()
              .HasDatabaseName("UQ_GuestAttributes_ConventionId_UserId_AttributeKey");

        entity.HasOne(ga => ga.User)
              .WithMany(u => u.GuestAttributes)
              .HasForeignKey(ga => ga.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(ga => ga.Convention)
              .WithMany()
              .HasForeignKey(ga => ga.ConventionId)
              .OnDelete(DeleteBehavior.NoAction);
    }
}

public class ScheduleTemplateConfiguration : IEntityTypeConfiguration<ScheduleTemplate>
{
    public void Configure(EntityTypeBuilder<ScheduleTemplate> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_ScheduleTemplate_ConventionId");

        entity.HasOne(st => st.Convention)
              .WithMany(c => c.ScheduleTemplates)
              .HasForeignKey(st => st.ConventionId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ScheduleItemConfiguration : IEntityTypeConfiguration<ScheduleItem>
{
    public void Configure(EntityTypeBuilder<ScheduleItem> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasIndex(e => e.ScheduleTemplateId).HasDatabaseName("IX_ScheduleItem_ScheduleTemplateId");

        entity.HasOne(si => si.ScheduleTemplate)
              .WithMany(st => st.ScheduleItems)
              .HasForeignKey(si => si.ScheduleTemplateId)
              .OnDelete(DeleteBehavior.Cascade);

    }
}

public class GuestScheduleTemplateConfiguration : IEntityTypeConfiguration<GuestScheduleTemplate>
{
    public void Configure(EntityTypeBuilder<GuestScheduleTemplate> entity)
    {
        entity.HasKey(gst => new { gst.UserId, gst.ScheduleTemplateId, gst.ConventionId });

        entity.HasOne(gst => gst.User)
              .WithMany(u => u.GuestScheduleTemplates)
              .HasForeignKey(gst => gst.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(gst => gst.ScheduleTemplate)
              .WithMany(st => st.GuestScheduleTemplates)
              .HasForeignKey(gst => gst.ScheduleTemplateId)
              .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(gst => gst.Convention)
              .WithMany()
              .HasForeignKey(gst => gst.ConventionId)
              .OnDelete(DeleteBehavior.NoAction);

        entity.HasIndex(gst => gst.ConventionId).HasDatabaseName("IX_GuestScheduleTemplate_ConventionId");
    }
}

public class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Feature_ConventionId");
    }
}

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Menu_ConventionId");
    }
}

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasIndex(e => e.MenuId).HasDatabaseName("IX_Section_MenuId");
    }
}

public class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Owner_ConventionId");
    }
}

public class NoticeCategoryConfiguration : IEntityTypeConfiguration<NoticeCategory>
{
    public void Configure(EntityTypeBuilder<NoticeCategory> entity)
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
    }
}

public class NoticeConfiguration : IEntityTypeConfiguration<Notice>
{
    public void Configure(EntityTypeBuilder<Notice> entity)
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
    }
}

public class FileAttachmentConfiguration : IEntityTypeConfiguration<FileAttachment>
{
    public void Configure(EntityTypeBuilder<FileAttachment> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.UploadedAt).HasDefaultValueSql("getdate()");
        entity.HasIndex(e => e.NoticeId).HasDatabaseName("IX_FileAttachment_NoticeId");
        entity.HasIndex(e => e.Category).HasDatabaseName("IX_FileAttachment_Category");
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
        entity.Property(e => e.Body).HasMaxLength(4000);
        entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
        entity.Property(e => e.TargetScope).IsRequired().HasMaxLength(20);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");
        entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Notification_ConventionId");
        entity.HasOne(e => e.Convention).WithMany().HasForeignKey(e => e.ConventionId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.SentByUser).WithMany().HasForeignKey(e => e.SentByUserId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.IsRead).HasDefaultValue(false);
        entity.HasIndex(e => new { e.UserId, e.NotificationId }).IsUnique().HasDatabaseName("IX_UserNotification_User_Notification");
        entity.HasIndex(e => e.UserId).HasDatabaseName("IX_UserNotification_UserId");
        entity.HasOne(e => e.Notification).WithMany(n => n.UserNotifications).HasForeignKey(e => e.NotificationId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class GalleryConfiguration : IEntityTypeConfiguration<Gallery>
{
    public void Configure(EntityTypeBuilder<Gallery> entity)
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
    }
}

public class GalleryImageConfiguration : IEntityTypeConfiguration<GalleryImage>
{
    public void Configure(EntityTypeBuilder<GalleryImage> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.ImageUrl).IsRequired();
        entity.Property(e => e.UploadedAt).HasDefaultValueSql("getdate()");
        entity.HasIndex(e => e.GalleryId).HasDatabaseName("IX_GalleryImage_GalleryId");
    }
}

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> entity)
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
    }
}

public class IncheonFlightDataConfiguration : IEntityTypeConfiguration<IncheonFlightData>
{
    public void Configure(EntityTypeBuilder<IncheonFlightData> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");

        entity.HasIndex(e => new { e.FlightId, e.ScheduleDate, e.FlightType })
              .HasDatabaseName("IX_IncheonFlightData_FlightId_ScheduleDate_FlightType");

        entity.HasIndex(e => e.ScheduleDate)
              .HasDatabaseName("IX_IncheonFlightData_ScheduleDate");

        entity.HasIndex(e => e.FlightId)
              .HasDatabaseName("IX_IncheonFlightData_FlightId");
    }
}

public class OptionTourConfiguration : IEntityTypeConfiguration<OptionTour>
{
    public void Configure(EntityTypeBuilder<OptionTour> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        entity.Property(e => e.StartTime).IsRequired().HasMaxLength(10);
        entity.Property(e => e.EndTime).HasMaxLength(10);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");

        entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_OptionTour_ConventionId");
        entity.HasIndex(e => e.Date).HasDatabaseName("IX_OptionTour_Date");
        entity.HasIndex(e => new { e.ConventionId, e.CustomOptionId })
              .HasDatabaseName("IX_OptionTour_ConventionId_CustomOptionId");

        entity.HasOne(e => e.Convention)
              .WithMany()
              .HasForeignKey(e => e.ConventionId)
              .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.UserOptionTours)
              .WithOne(u => u.OptionTour)
              .HasForeignKey(u => u.OptionTourId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserOptionTourConfiguration : IEntityTypeConfiguration<UserOptionTour>
{
    public void Configure(EntityTypeBuilder<UserOptionTour> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");

        entity.HasIndex(e => e.UserId).HasDatabaseName("IX_UserOptionTour_UserId");
        entity.HasIndex(e => e.OptionTourId).HasDatabaseName("IX_UserOptionTour_OptionTourId");
        entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_UserOptionTour_ConventionId");
        entity.HasIndex(e => new { e.UserId, e.ConventionId, e.OptionTourId })
              .IsUnique()
              .HasDatabaseName("UQ_UserOptionTour_UserId_ConventionId_OptionTourId");

        entity.HasOne(e => e.User)
              .WithMany(u => u.UserOptionTours)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.OptionTour)
              .WithMany(o => o.UserOptionTours)
              .HasForeignKey(e => e.OptionTourId)
              .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.Convention)
              .WithMany()
              .HasForeignKey(e => e.ConventionId)
              .OnDelete(DeleteBehavior.NoAction);
    }
}
