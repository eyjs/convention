using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Admin;

public class CompanionService : ICompanionService
{
    private readonly IUnitOfWork _unitOfWork;

    public CompanionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<object> GetCompanionsAsync(int conventionId, int userId)
    {
        var companions = await _unitOfWork.CompanionRelations.Query
            .Where(cr => cr.ConventionId == conventionId && cr.UserId == userId)
            .Include(cr => cr.CompanionUser)
            .Select(cr => new
            {
                cr.Id,
                cr.CompanionUserId,
                Name = cr.CompanionUser.Name,
                Phone = cr.CompanionUser.Phone,
                cr.RelationType,
                Passport = new
                {
                    HasNumber = !string.IsNullOrEmpty(cr.CompanionUser.PassportNumber),
                    HasExpiry = cr.CompanionUser.PassportExpiryDate != null,
                    HasImage = !string.IsNullOrEmpty(cr.CompanionUser.PassportImageUrl),
                    cr.CompanionUser.PassportVerified
                }
            })
            .ToListAsync();

        return companions;
    }

    public async Task<(bool Success, object Result, int StatusCode)> AddCompanionAsync(
        int conventionId, int userId, int companionUserId, string relationType)
    {
        if (userId == companionUserId)
            return (false, new { message = "자기 자신을 동반자로 등록할 수 없습니다." }, 400);

        // 동반자가 같은 행사에 참석하는지 확인
        var companionInConvention = await _unitOfWork.UserConventions
            .GetByUserAndConventionAsync(companionUserId, conventionId);
        if (companionInConvention == null)
            return (false, new { message = "해당 사용자는 이 행사의 참석자가 아닙니다." }, 400);

        // 중복 관계 확인
        var existing = await _unitOfWork.CompanionRelations.Query
            .AnyAsync(cr => cr.ConventionId == conventionId
                && cr.UserId == userId
                && cr.CompanionUserId == companionUserId);
        if (existing)
            return (false, new { message = "이미 등록된 동반자입니다." }, 409);

        var relation = new CompanionRelation
        {
            ConventionId = conventionId,
            UserId = userId,
            CompanionUserId = companionUserId,
            RelationType = relationType
        };

        await _unitOfWork.CompanionRelations.AddAsync(relation);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "동반자가 등록되었습니다.", id = relation.Id }, 201);
    }

    public async Task<(bool Success, object Result, int StatusCode)> RemoveCompanionAsync(int relationId)
    {
        var relation = await _unitOfWork.CompanionRelations.GetByIdAsync(relationId);
        if (relation == null)
            return (false, new { message = "동반자 관계를 찾을 수 없습니다." }, 404);

        _unitOfWork.CompanionRelations.Remove(relation);
        await _unitOfWork.SaveChangesAsync();

        return (true, new { message = "동반자 관계가 삭제되었습니다." }, 200);
    }

    public async Task<object> GetAllCompanionRelationsAsync(int conventionId)
    {
        var relations = await _unitOfWork.CompanionRelations.Query
            .Where(cr => cr.ConventionId == conventionId)
            .Include(cr => cr.User)
            .Include(cr => cr.CompanionUser)
            .Select(cr => new
            {
                cr.Id,
                cr.UserId,
                UserName = cr.User.Name,
                cr.CompanionUserId,
                CompanionName = cr.CompanionUser.Name,
                cr.RelationType
            })
            .ToListAsync();

        return relations;
    }
}
