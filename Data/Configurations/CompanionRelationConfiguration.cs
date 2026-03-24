using LocalRAG.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalRAG.Data.Configurations;

public class CompanionRelationConfiguration : IEntityTypeConfiguration<CompanionRelation>
{
    public void Configure(EntityTypeBuilder<CompanionRelation> builder)
    {
        builder.HasOne(cr => cr.Convention)
            .WithMany()
            .HasForeignKey(cr => cr.ConventionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cr => cr.User)
            .WithMany(u => u.Companions)
            .HasForeignKey(cr => cr.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(cr => cr.CompanionUser)
            .WithMany(u => u.CompanionOf)
            .HasForeignKey(cr => cr.CompanionUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(cr => new { cr.ConventionId, cr.UserId, cr.CompanionUserId })
            .IsUnique();
    }
}
