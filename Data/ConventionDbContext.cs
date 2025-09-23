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
            entity.HasIndex(e => e.StartDate).HasDatabaseName("IX_Convention_StartDate");
            entity.HasIndex(e => e.ConventionType).HasDatabaseName("IX_Convention_ConventionType");
        });

        // Guest (참석자)
        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(e => e.Id);
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
            entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_Schedule_ConventionId");
        });

        // GuestAttribute (참석자 부가 속성)
        modelBuilder.Entity<GuestAttribute>(entity =>
        {
            entity.HasKey(e => e.Id);
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
                  .OnDelete(DeleteBehavior.NoAction); // 일정이 삭제되도 연결 정보는 일단 남도록 설정 (정책에 따라 Cascade로 변경 가능)
        });

        // Feature (기능)
        modelBuilder.Entity<Feature>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Unique Constraint: 한 행사에 동일한 기능 이름이 중복될 수 없음
            entity.HasIndex(e => new { e.ConventionId, e.FeatureName })
                  .IsUnique()
                  .HasDatabaseName("UQ_Features_ConventionId_FeatureName");
        });

        // --- 나머지 엔티티 설정 (필요시 추가) ---
        // Owner, Menu, Section 등은 기본 규칙으로도 충분히 동작하지만,
        // 명시적으로 관계를 설정하고 싶다면 위와 같은 패턴으로 추가할 수 있습니다.
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