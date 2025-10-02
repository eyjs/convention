using LocalRAG.Models.Convention;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LocalRAG.Models;

namespace LocalRAG.Data;

public class ConventionDbContext : DbContext
{
    public ConventionDbContext(DbContextOptions<ConventionDbContext> options) : base(options)
    {
    }

    // --- DbSet Properties ---
    public DbSet<Convention> Conventions { get; set; }
    public DbSet<Guest> Guests { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<GuestAttribute> GuestAttributes { get; set; }
    public DbSet<Companion> Companions { get; set; }
    public DbSet<GuestSchedule> GuestSchedules { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Owner> Owners { get; set; }
    public DbSet<VectorStore> VectorStores { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Entity Configurations ---

        // Convention (행사)
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

        // Guest (참석자)
        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Guest_ConventionId");
            entity.HasIndex(e => e.GuestName).HasDatabaseName("IX_Guest_GuestName");

            // 1:N Relationship (Convention -> Guests)
            entity.HasOne(g => g.Convention)
                  .WithMany(c => c.Guests)
                  .HasForeignKey(g => g.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Schedule (일정)
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.OrderNum).HasDefaultValue(0);
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Schedule_ConventionId");
        });

        // GuestAttribute (참석자 부가 속성)
        modelBuilder.Entity<GuestAttribute>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            // Unique Constraint: 한 참석자에게 동일한 속성 키가 중복될 수 없음
            entity.HasIndex(e => new { e.GuestId, e.AttributeKey })
                  .IsUnique()
                  .HasDatabaseName("UQ_GuestAttributes_GuestId_AttributeKey");

            // 1:N Relationship (Guest -> Attributes)
            entity.HasOne(ga => ga.Guest)
                  .WithMany(g => g.Attributes)
                  .HasForeignKey(ga => ga.GuestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Companion (동반자)
        modelBuilder.Entity<Companion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.GuestId).HasDatabaseName("IX_Companion_GuestId");

            // 1:N Relationship (Guest -> Companions)
            entity.HasOne(c => c.Guest)
                  .WithMany(g => g.Companions)
                  .HasForeignKey(c => c.GuestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // GuestSchedule (참석자-일정 연결 테이블, Many-to-Many)
        modelBuilder.Entity<GuestSchedule>(entity =>
        {
            // Composite Primary Key
            entity.HasKey(gs => new { gs.GuestId, gs.ScheduleId });

            // Relationship to Guest
            entity.HasOne(gs => gs.Guest)
                  .WithMany(g => g.GuestSchedules)
                  .HasForeignKey(gs => gs.GuestId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Relationship to Schedule
            entity.HasOne(gs => gs.Schedule)
                  .WithMany(s => s.GuestSchedules)
                  .HasForeignKey(gs => gs.ScheduleId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // Feature (기능)
        modelBuilder.Entity<Feature>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.IsEnabled).HasDefaultValue("Y");
            
            // Unique Constraint: 한 행사에 동일한 기능 이름이 중복될 수 없음
            entity.HasIndex(e => new { e.ConventionId, e.FeatureName })
                  .IsUnique()
                  .HasDatabaseName("UQ_Features_ConventionId_FeatureName");
        });

        // Menu (메뉴)
        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.RegDtm).HasDefaultValueSql("getdate()");
            entity.Property(e => e.DeleteYn).HasDefaultValue("N");
            
            entity.HasOne(m => m.Convention)
                  .WithMany(c => c.Menus)
                  .HasForeignKey(m => m.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Section (섹션)
        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.RegDtm).HasDefaultValueSql("getdate()");
            entity.Property(e => e.DeleteYn).HasDefaultValue("N");
            
            entity.HasOne(s => s.Menu)
                  .WithMany(m => m.Sections)
                  .HasForeignKey(s => s.MenuId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Owner (담당자)
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.HasOne(o => o.Convention)
                  .WithMany(c => c.Owners)
                  .HasForeignKey(o => o.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // VectorStore (벡터 저장소)
        modelBuilder.Entity<VectorStore>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("newid()");
            entity.Property(e => e.RegDtm).HasDefaultValueSql("getdate()");
            
            entity.HasOne(v => v.Convention)
                  .WithMany(c => c.VectorStores)
                  .HasForeignKey(v => v.ConventionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // 개발 환경에서 쿼리 로깅
            optionsBuilder.LogTo(Console.WriteLine,
                new[] { DbLoggerCategory.Database.Command.Name },
                LogLevel.Information);
        }
    }
}
