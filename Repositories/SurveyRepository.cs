using LocalRAG.Data;
using LocalRAG.Entities;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Repositories;

public class SurveyRepository : Repository<Survey>, ISurveyRepository
{
    public SurveyRepository(ConventionDbContext context) : base(context)
    {
    }

    public async Task<Survey?> GetSurveyWithQuestionsAndOptionsAsync(int surveyId, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Include(s => s.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(s => s.Id == surveyId, cancellationToken);
    }

    public async Task<Survey?> GetSurveyWithAllDataAsync(int surveyId, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Include(s => s.Questions.OrderBy(q => q.OrderIndex))
            .ThenInclude(q => q.Options.OrderBy(o => o.OrderIndex))
            .Include(s => s.Responses)
            .ThenInclude(r => r.Details)
            .ThenInclude(d => d.SelectedOption)
            .FirstOrDefaultAsync(s => s.Id == surveyId, cancellationToken);
    }

    public async Task<IEnumerable<Survey>> GetSurveysByConventionIdAsync(int conventionId, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Where(s => s.ConventionId == conventionId)
            .OrderBy(s => s.Title)
            .ToListAsync(cancellationToken);
    }
}
