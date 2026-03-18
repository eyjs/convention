using LocalRAG.Entities.PersonalTrip;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalRAG.Data.Configurations;

public class PersonalTripConfiguration : IEntityTypeConfiguration<PersonalTrip>
{
    public void Configure(EntityTypeBuilder<PersonalTrip> entity)
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

        entity.HasMany(e => e.ChecklistCategories)
            .WithOne(c => c.PersonalTrip)
            .HasForeignKey(c => c.PersonalTripId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");

        entity.HasIndex(e => e.PersonalTripId).HasDatabaseName("IX_Flight_PersonalTripId");
        entity.HasIndex(e => e.DepartureTime).HasDatabaseName("IX_Flight_DepartureTime");
    }
}

public class AccommodationConfiguration : IEntityTypeConfiguration<Accommodation>
{
    public void Configure(EntityTypeBuilder<Accommodation> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");

        entity.HasIndex(e => e.PersonalTripId).HasDatabaseName("IX_Accommodation_PersonalTripId");
        entity.HasIndex(e => e.CheckInTime).HasDatabaseName("IX_Accommodation_CheckInTime");
    }
}

public class ItineraryItemConfiguration : IEntityTypeConfiguration<ItineraryItem>
{
    public void Configure(EntityTypeBuilder<ItineraryItem> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();

        entity.HasIndex(e => e.PersonalTripId).HasDatabaseName("IX_ItineraryItem_PersonalTripId");
        entity.HasIndex(e => e.DayNumber).HasDatabaseName("IX_ItineraryItem_DayNumber");
    }
}

public class ChecklistCategoryConfiguration : IEntityTypeConfiguration<ChecklistCategory>
{
    public void Configure(EntityTypeBuilder<ChecklistCategory> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasIndex(e => e.PersonalTripId).HasDatabaseName("IX_ChecklistCategory_PersonalTripId");

        entity.HasMany(e => e.Items)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.ChecklistCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChecklistItemConfiguration : IEntityTypeConfiguration<ChecklistItem>
{
    public void Configure(EntityTypeBuilder<ChecklistItem> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasIndex(e => e.ChecklistCategoryId).HasDatabaseName("IX_ChecklistItem_ChecklistCategoryId");
    }
}
