using Microsoft.EntityFrameworkCore;
using LocalRAG.Data.Entities;

namespace LocalRAG.Data;

public class TravelDbContext : DbContext
{
    public TravelDbContext(DbContextOptions<TravelDbContext> options) : base(options)
    {
    }

    public DbSet<TravelTrip> TravelTrips { get; set; }
    public DbSet<ScheduleItem> ScheduleItems { get; set; }
    public DbSet<TripGuest> TripGuests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ScheduleItem 설정
        modelBuilder.Entity<ScheduleItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.DeleteYn).HasDefaultValue((byte)0);
        });

        // TravelTrip 설정
        modelBuilder.Entity<TravelTrip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Budget).HasColumnType("decimal(18,2)");
        });

        // TripGuest 설정
        modelBuilder.Entity<TripGuest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Foreign Key 관계 설정
            entity.HasOne(e => e.TravelTrip)
                  .WithMany(t => t.TripGuests)
                  .HasForeignKey(e => e.TripId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 인덱스 설정
        modelBuilder.Entity<ScheduleItem>()
            .HasIndex(e => new { e.ConvId, e.GuestId })
            .HasDatabaseName("IX_ScheduleItems_ConvId_GuestId");

        modelBuilder.Entity<TripGuest>()
            .HasIndex(e => e.TripId)
            .HasDatabaseName("IX_TripGuests_TripId");
    }
}