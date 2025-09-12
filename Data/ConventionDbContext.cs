using Microsoft.EntityFrameworkCore;
using LocalRAG.Models.Convention;

namespace LocalRAG.Data;

public class ConventionDbContext : DbContext
{
    public ConventionDbContext(DbContextOptions<ConventionDbContext> options) : base(options)
    {
    }

    // 컨벤션 테이블
    public DbSet<CorpConvention> CorpConventions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CorpConvention 엔티티 설정
        modelBuilder.Entity<CorpConvention>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // 인덱스 설정 (성능 최적화)
            entity.HasIndex(e => e.MemberId)
                .HasDatabaseName("IX_CorpConvention_MemberId");
            
            entity.HasIndex(e => e.DeleteYn)
                .HasDatabaseName("IX_CorpConvention_DeleteYn");
            
            entity.HasIndex(e => e.StartDate)
                .HasDatabaseName("IX_CorpConvention_StartDate");
            
            entity.HasIndex(e => e.RegDtm)
                .HasDatabaseName("IX_CorpConvention_RegDtm");
                
            // 복합 인덱스 (자주 같이 사용되는 컬럼들)
            entity.HasIndex(e => new { e.DeleteYn, e.StartDate })
                .HasDatabaseName("IX_CorpConvention_DeleteYn_StartDate");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // 개발 환경에서 쿼리 로깅
            optionsBuilder.LogTo(Console.WriteLine, 
                new[] { DbLoggerCategory.Database.Command.Name },
                Microsoft.Extensions.Logging.LogLevel.Information);
        }
    }
}
