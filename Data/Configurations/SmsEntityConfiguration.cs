using LocalRAG.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalRAG.Data.Configurations;

public class SmsTemplateConfiguration : IEntityTypeConfiguration<SmsTemplate>
{
    public void Configure(EntityTypeBuilder<SmsTemplate> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.RegDtm).HasDefaultValueSql("getdate()");
        entity.Property(e => e.DeleteYn).HasDefaultValue("2");

        entity.HasQueryFilter(e => e.DeleteYn == "2");
    }
}

public class SmsLogConfiguration : IEntityTypeConfiguration<SmsLog>
{
    public void Configure(EntityTypeBuilder<SmsLog> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.SentAt).HasDefaultValueSql("getdate()");

        entity.HasIndex(e => e.ConventionId).HasDatabaseName("IX_SmsLog_ConventionId");
        entity.HasIndex(e => e.ExternalId).HasDatabaseName("IX_SmsLog_ExternalId");
        entity.HasIndex(e => e.SentAt).HasDatabaseName("IX_SmsLog_SentAt");

        entity.HasOne(e => e.Convention)
              .WithMany()
              .HasForeignKey(e => e.ConventionId)
              .OnDelete(DeleteBehavior.SetNull);
    }
}
