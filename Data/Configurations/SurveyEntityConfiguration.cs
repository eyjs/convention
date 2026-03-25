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

        // 꼬리질문: 선택지 → 후속 질문 (SQL Server 다중 cascade path 방지)
        entity.HasOne(q => q.ParentOption)
            .WithMany()
            .HasForeignKey(q => q.ParentOptionId)
            .OnDelete(DeleteBehavior.NoAction);

        // 설문별 질문 조회 인덱스
        entity.HasIndex(q => q.SurveyId);
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

        // 중복 제출 방지 + 조회 최적화
        entity.HasIndex(r => new { r.SurveyId, r.UserId }).IsUnique();
    }
}

public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> entity)
    {
        entity.HasOne(o => o.OptionTour)
            .WithMany()
            .HasForeignKey(o => o.OptionTourId)
            .OnDelete(DeleteBehavior.SetNull);
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

        // 질문별 조회 인덱스
        entity.HasIndex(d => d.QuestionId);
    }
}
