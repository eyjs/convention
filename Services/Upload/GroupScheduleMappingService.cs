using LocalRAG.DTOs.UploadModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Upload;

/// <summary>
/// 그룹-일정 매핑 서비스
/// 특정 그룹의 모든 참석자에게 여러 일정(ConventionAction)을 일괄 배정
/// </summary>
public class GroupScheduleMappingService : IGroupScheduleMappingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GroupScheduleMappingService> _logger;

    public GroupScheduleMappingService(
        IUnitOfWork unitOfWork,
        ILogger<GroupScheduleMappingService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GroupScheduleMappingResult> MapGroupToSchedulesAsync(GroupScheduleMappingRequest request)
    {
        var result = new GroupScheduleMappingResult();

        try
        {
            // Convention 존재 확인
            var convention = await _unitOfWork.Conventions.GetByIdAsync(request.ConventionId);
            if (convention == null)
            {
                result.Errors.Add($"Convention {request.ConventionId}를 찾을 수 없습니다.");
                return result;
            }

            // ConventionAction 존재 확인
            var actions = await _unitOfWork.ConventionActions
                .FindAsync(a => a.ConventionId == request.ConventionId
                             && request.ActionIds.Contains(a.Id));

            var actionList = actions.ToList();

            if (actionList.Count != request.ActionIds.Count)
            {
                var foundIds = actionList.Select(a => a.Id).ToList();
                var missingIds = request.ActionIds.Except(foundIds).ToList();
                result.Errors.Add($"다음 ConventionAction ID를 찾을 수 없습니다: {string.Join(", ", missingIds)}");
                return result;
            }

            // 그룹에 속한 Guest 찾기
            var guests = await _unitOfWork.Guests
                .FindAsync(g => g.ConventionId == request.ConventionId
                             && g.GroupName == request.GuestGroup);

            var guestList = guests.ToList();

            if (guestList.Count == 0)
            {
                result.Errors.Add($"그룹 '{request.GuestGroup}'에 속한 참석자가 없습니다.");
                return result;
            }

            _logger.LogInformation("Mapping {ActionCount} actions to {GuestCount} guests in group '{Group}'",
                actionList.Count, guestList.Count, request.GuestGroup);

            // 트랜잭션으로 처리
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var guest in guestList)
                {
                    foreach (var action in actionList)
                    {
                        // 중복 확인
                        var existingMappings = await _unitOfWork.GuestActionStatuses
                            .FindAsync(gas => gas.GuestId == guest.Id
                                           && gas.ConventionActionId == action.Id);

                        if (existingMappings.Any())
                        {
                            result.DuplicatesSkipped++;
                            _logger.LogDebug("Skipping duplicate mapping: Guest {GuestId} - Action {ActionId}",
                                guest.Id, action.Id);
                            continue;
                        }

                        // 새 매핑 생성
                        var guestActionStatus = new Entities.GuestActionStatus
                        {
                            GuestId = guest.Id,
                            ConventionActionId = action.Id,
                            IsComplete = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.GuestActionStatuses.AddAsync(guestActionStatus);
                        result.MappingsCreated++;
                    }

                    result.GuestsAffected++;
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                result.Success = true;

                _logger.LogInformation("Group mapping completed: {Mappings} mappings created for {Guests} guests, {Duplicates} duplicates skipped",
                    result.MappingsCreated, result.GuestsAffected, result.DuplicatesSkipped);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Transaction failed during group mapping");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Group mapping failed");
            result.Errors.Add($"매핑 실패: {ex.Message}");
            if (ex.InnerException != null)
            {
                result.Errors.Add($"상세: {ex.InnerException.Message}");
            }
        }

        return result;
    }

    public async Task<List<string>> GetGroupsInConventionAsync(int conventionId)
    {
        try
        {
            var guests = await _unitOfWork.Guests
                .FindAsync(g => g.ConventionId == conventionId && g.GroupName != null);

            var groups = guests
                .Select(g => g.GroupName!)
                .Distinct()
                .OrderBy(g => g)
                .ToList();

            _logger.LogInformation("Found {Count} groups in convention {ConventionId}", groups.Count, conventionId);

            return groups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get groups for convention {ConventionId}", conventionId);
            return new List<string>();
        }
    }
}
