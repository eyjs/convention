using LocalRAG.DTOs.UploadModels;
using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Upload;

/// <summary>
/// 옵션투어 업로드 서비스
/// 옵션투어 정보와 참석자 매핑을 처리
/// </summary>
public class OptionTourUploadService : IOptionTourUploadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OptionTourUploadService> _logger;

    public OptionTourUploadService(
        IUnitOfWork unitOfWork,
        ILogger<OptionTourUploadService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<OptionTourUploadResult> UploadOptionToursAsync(
        int conventionId,
        OptionTourUploadRequest request)
    {
        var result = new OptionTourUploadResult();

        try
        {
            var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
            if (convention == null)
            {
                result.Errors.Add($"Convention {conventionId}를 찾을 수 없습니다.");
                return result;
            }

            _logger.LogInformation(
                "Processing {OptionCount} options and {MappingCount} mappings for convention {ConventionId}",
                request.Options.Count,
                request.ParticipantMappings.Count,
                conventionId);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // 1. 기존 옵션투어 조회 (UPSERT 위해)
                var existingOptions = await _unitOfWork.OptionTours.Query
                    .Where(ot => ot.ConventionId == conventionId)
                    .ToListAsync();
                var existingByCustomId = existingOptions.ToDictionary(ot => ot.CustomOptionId);

                var optionTourMap = new Dictionary<int, int>();

                foreach (var optionDto in request.Options)
                {
                    if (!DateTime.TryParse(optionDto.Date, out var date))
                    {
                        result.Warnings.Add($"옵션 '{optionDto.Name}': 잘못된 날짜 형식 ({optionDto.Date})");
                        continue;
                    }

                    if (existingByCustomId.TryGetValue(optionDto.OptionId, out var existing))
                    {
                        // UPDATE
                        existing.Date = date;
                        existing.StartTime = optionDto.StartTime;
                        existing.EndTime = optionDto.EndTime;
                        existing.Name = optionDto.Name;
                        existing.Location = string.IsNullOrEmpty(optionDto.Location) ? null : optionDto.Location;
                        existing.MapUrl = string.IsNullOrEmpty(optionDto.MapUrl) ? null : optionDto.MapUrl;
                        existing.Content = optionDto.Content;
                        _unitOfWork.OptionTours.Update(existing);
                        optionTourMap[optionDto.OptionId] = existing.Id;
                        result.OptionsCreated++;
                    }
                    else
                    {
                        // INSERT
                        var optionTour = new OptionTour
                        {
                            ConventionId = conventionId,
                            Date = date,
                            StartTime = optionDto.StartTime,
                            EndTime = optionDto.EndTime,
                            Name = optionDto.Name,
                            Location = string.IsNullOrEmpty(optionDto.Location) ? null : optionDto.Location,
                            MapUrl = string.IsNullOrEmpty(optionDto.MapUrl) ? null : optionDto.MapUrl,
                            CustomOptionId = optionDto.OptionId,
                            Content = optionDto.Content,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.OptionTours.AddAsync(optionTour);
                        await _unitOfWork.SaveChangesAsync();
                        optionTourMap[optionDto.OptionId] = optionTour.Id;
                        result.OptionsCreated++;
                    }

                    _logger.LogInformation(
                        "Upserted option tour: {Name} (CustomId: {CustomId})",
                        optionDto.Name, optionDto.OptionId);
                }

                // 2. 참석자 매핑 — 메모리에서 매칭
                var conventionUserIds = await _unitOfWork.UserConventions.Query
                    .Where(uc => uc.ConventionId == conventionId)
                    .Select(uc => uc.UserId)
                    .ToListAsync();

                var users = await _unitOfWork.Users.Query
                    .Where(u => conventionUserIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.Name, u.Phone, u.ResidentNumber })
                    .ToListAsync();

                foreach (var mappingDto in request.ParticipantMappings)
                {
                    var normalizedPhone = NormalizeValue(mappingDto.Phone);
                    var normalizedIdNumber = NormalizeValue(mappingDto.IdNumber);

                    if (string.IsNullOrEmpty(mappingDto.Name) ||
                        (string.IsNullOrEmpty(normalizedPhone) && string.IsNullOrEmpty(normalizedIdNumber)))
                    {
                        result.Warnings.Add(
                            $"매핑 스킵: 이름 또는 식별정보(전화/주민번호) 누락 — {mappingDto.Name}");
                        continue;
                    }

                    var matchedUser = users.FirstOrDefault(u =>
                        u.Name == mappingDto.Name &&
                        !string.IsNullOrEmpty(normalizedPhone) &&
                        NormalizeValue(u.Phone) == normalizedPhone);

                    matchedUser ??= users.FirstOrDefault(u =>
                        u.Name == mappingDto.Name &&
                        !string.IsNullOrEmpty(normalizedIdNumber) &&
                        NormalizeValue(u.ResidentNumber) == normalizedIdNumber);

                    if (matchedUser == null)
                    {
                        result.Warnings.Add(
                            $"참석자를 찾을 수 없습니다: {mappingDto.Name} (전화: {mappingDto.Phone}, 주민번호: {mappingDto.IdNumber})");
                        continue;
                    }

                    var existingMappings = await _unitOfWork.UserOptionTours.Query
                        .Where(uot => uot.UserId == matchedUser.Id && uot.ConventionId == conventionId)
                        .Select(uot => uot.OptionTourId)
                        .ToListAsync();

                    var existingSet = new HashSet<int>(existingMappings);

                    foreach (var customOptionId in mappingDto.OptionIds)
                    {
                        if (!optionTourMap.TryGetValue(customOptionId, out var optionTourId))
                        {
                            result.Warnings.Add(
                                $"참석자 {mappingDto.Name}: 옵션번호 {customOptionId}를 찾을 수 없습니다.");
                            continue;
                        }

                        if (existingSet.Contains(optionTourId))
                            continue;

                        await _unitOfWork.UserOptionTours.AddAsync(new UserOptionTour
                        {
                            UserId = matchedUser.Id,
                            OptionTourId = optionTourId,
                            ConventionId = conventionId,
                            CreatedAt = DateTime.UtcNow
                        });

                        existingSet.Add(optionTourId);
                        result.MappingsCreated++;
                    }
                }
            });

            result.Success = true;

            _logger.LogInformation(
                "Upload completed: {OptionsCreated} options, {MappingsCreated} mappings, {Warnings} warnings",
                result.OptionsCreated, result.MappingsCreated, result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Option tour upload failed");
            result.Errors.Add($"업로드 중 오류 발생: {ex.Message}");
            return result;
        }
    }

    private static string NormalizeValue(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
    }
}
