using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LocalRAG.Models;

namespace LocalRAG.Data;

public class ConventionDbContext : DbContext
{
    public ConventionDbContext(DbContextOptions<ConventionDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Convention> Conventions { get; set; }
    public DbSet<Guest> Guests { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<GuestAttribute> GuestAttributes { get; set; }
    public DbSet<AttributeDefinition> AttributeDefinitions { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Owner> Owners { get; set; }
    public DbSet<VectorStore> VectorStores { get; set; }
    
    public DbSet<ScheduleTemplate> ScheduleTemplates { get; set; }
    public DbSet<ScheduleItem> ScheduleItems { get; set; }
    public DbSet<GuestScheduleTemplate> GuestScheduleTemplates { get; set; }
    public DbSet<AttributeTemplate> AttributeTemplates { get; set; }
    public DbSet<Notice> Notices { get; set; }
    public DbSet<FileAttachment> FileAttachments { get; set; }
    public DbSet<Gallery> Galleries { get; set; }
    public DbSet<GalleryImage> GalleryImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
            
            entity.HasMany(u => u.Guests)
                  .WithOne(g => g.User)
                  .HasForeignKey(g => g.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
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
        });

        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Guest_ConventionId");
            entity.HasIndex(e => e.GuestName).HasDatabaseName("IX_Guest_GuestName");
            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_Guest_UserId");
            entity.HasIndex(e => e.AccessToken).IsUnique().HasDatabaseName("UQ_Guest_AccessToken");

            entity.HasOne(g => g.Convention)
                  .WithMany(c => c.Guests)
                  .HasForeignKey(g => g.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(g => g.User)
                  .WithMany(u => u.Guests)
                  .HasForeignKey(g => g.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
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
            
            entity.HasIndex(e => new { e.GuestId, e.AttributeKey })
                  .IsUnique()
                  .HasDatabaseName("UQ_GuestAttributes_GuestId_AttributeKey");

            entity.HasOne(ga => ga.Guest)
                  .WithMany(g => g.GuestAttributes)
                  .HasForeignKey(ga => ga.GuestId)
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
            entity.HasKey(gst => new { gst.GuestId, gst.ScheduleTemplateId });

            entity.HasOne(gst => gst.Guest)
                  .WithMany(g => g.GuestScheduleTemplates)
                  .HasForeignKey(gst => gst.GuestId)
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

        modelBuilder.Entity<VectorStore>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_VectorStore_ConventionId");
            entity.HasIndex(e => e.SourceType).HasDatabaseName("IX_VectorStore_SourceType");
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
            
            entity.HasOne(e => e.Author)
                  .WithMany()
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Convention)
                  .WithMany()
                  .HasForeignKey(e => e.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
            
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
    }
}
