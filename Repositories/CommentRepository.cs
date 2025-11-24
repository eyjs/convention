using LocalRAG.Data;
using LocalRAG.Entities;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Repositories;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(ConventionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Comment>> GetCommentsByNoticeIdAsync(int noticeId, CancellationToken cancellationToken = default)
    {
        return await _context.Comments
            .Where(c => c.NoticeId == noticeId)
            .Include(c => c.Author)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
