using LocalRAG.Entities;
using LocalRAG.Entities.Action;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalRAG.Data.Configurations;

public class ConventionActionConfiguration : IEntityTypeConfiguration<ConventionAction>
{
    public void Configure(EntityTypeBuilder<ConventionAction> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
        entity.Property(e => e.Description).HasMaxLength(4000);
        entity.Property(e => e.MapsTo).IsRequired().HasMaxLength(200);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");
        entity.Property(e => e.UpdatedAt).HasDefaultValueSql("getdate()");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
        entity.Property(e => e.OrderNum).HasDefaultValue(0);
        entity.Property(e => e.ActionCategory).HasMaxLength(50);
        entity.Property(e => e.TargetLocation).HasMaxLength(100);

        entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_ConventionAction_ConventionId");

        entity.HasOne(e => e.Convention)
              .WithMany()
              .HasForeignKey(e => e.ConventionId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserActionStatusConfiguration : IEntityTypeConfiguration<UserActionStatus>
{
    public void Configure(EntityTypeBuilder<UserActionStatus> entity)
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
              .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.ConventionAction)
              .WithMany(ca => ca.UserActionStatuses)
              .HasForeignKey(e => e.ConventionActionId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ActionSubmissionConfiguration : IEntityTypeConfiguration<ActionSubmission>
{
    public void Configure(EntityTypeBuilder<ActionSubmission> entity)
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
              .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(e => e.ConventionAction)
              .WithMany()
              .HasForeignKey(e => e.ConventionActionId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}

