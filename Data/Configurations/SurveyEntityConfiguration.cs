using LocalRAG.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalRAG.Data.Configurations;

public class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
    public void Configure(EntityTypeBuilder<Survey> entity)
    {
        entity.HasMany(s => s.Questions)
            .WithOne(q => q.Survey)
            .HasForeignKey(q => q.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(s => s.Responses)
            .WithOne(r => r.Survey)
            .HasForeignKey(r => r.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SurveyQuestionConfiguration : IEntityTypeConfiguration<SurveyQuestion>
{
    public void Configure(EntityTypeBuilder<SurveyQuestion> entity)
    {
        entity.HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SurveyResponseConfiguration : IEntityTypeConfiguration<SurveyResponse>
{
    public void Configure(EntityTypeBuilder<SurveyResponse> entity)
    {
        entity.HasMany(r => r.Details)
            .WithOne(d => d.Response)
            .HasForeignKey(d => d.ResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(sr => sr.User)
            .WithMany()
            .HasForeignKey(sr => sr.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class SurveyResponseDetailConfiguration : IEntityTypeConfiguration<SurveyResponseDetail>
{
    public void Configure(EntityTypeBuilder<SurveyResponseDetail> entity)
    {
        entity.HasOne(rd => rd.Question)
            .WithMany()
            .HasForeignKey(rd => rd.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(rd => rd.SelectedOption)
            .WithMany()
            .HasForeignKey(rd => rd.SelectedOptionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
