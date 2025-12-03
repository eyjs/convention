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
            // Convention 존재 확인
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

            // 1. 옵션투어 저장
            var optionTourMap = new Dictionary<int, int>(); // CustomOptionId -> OptionTourId
            foreach (var optionDto in request.Options)
            {
                try
                {
                    if (!DateTime.TryParse(optionDto.Date, out var date))
                    {
                        result.Warnings.Add($"옵션 '{optionDto.Name}': 잘못된 날짜 형식 ({optionDto.Date})");
                        continue;
                    }

                    var optionTour = new OptionTour
                    {
                        ConventionId = conventionId,
                        Date = date,
                        StartTime = optionDto.StartTime,
                        EndTime = optionDto.EndTime,
                        Name = optionDto.Name,
                        CustomOptionId = optionDto.OptionId,
                        Content = optionDto.Content,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.OptionTours.AddAsync(optionTour);
                    await _unitOfWork.SaveChangesAsync();

                    optionTourMap[optionDto.OptionId] = optionTour.Id;
                    result.OptionsCreated++;

                    _logger.LogInformation(
                        "Created option tour: {Name} (CustomId: {CustomId}, Id: {Id})",
                        optionTour.Name,
                        optionTour.CustomOptionId,
                        optionTour.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create option tour: {Name}", optionDto.Name);
                    result.Warnings.Add($"옵션 '{optionDto.Name}' 생성 실패: {ex.Message}");
                }
            }

            // 2. 참석자 매핑 처리
            foreach (var mappingDto in request.ParticipantMappings)
            {
                try
                {
                    // 참석자 찾기: 이름 + (전화번호 OR 주민번호)
                    User? user = null;

                    if (!string.IsNullOrEmpty(mappingDto.Phone))
                    {
                        // 전화번호로 매칭 시도
                        var normalizedPhone = NormalizePhone(mappingDto.Phone);
                        user = await _unitOfWork.Users.GetAsync(u =>
                            u.Name == mappingDto.Name &&
                            u.Phone != null &&
                            u.Phone.Replace("-", "").Replace(" ", "") == normalizedPhone);
                    }

                    if (user == null && !string.IsNullOrEmpty(mappingDto.IdNumber))
                    {
                        // 주민번호로 매칭 시도
                        var normalizedIdNumber = mappingDto.IdNumber.Replace("-", "").Replace(" ", "");
                        user = await _unitOfWork.Users.GetAsync(u =>
                            u.Name == mappingDto.Name &&
                            u.ResidentNumber != null &&
                            u.ResidentNumber.Replace("-", "").Replace(" ", "") == normalizedIdNumber);
                    }

                    if (user == null)
                    {
                        result.Warnings.Add(
                            $"참석자를 찾을 수 없습니다: {mappingDto.Name} (전화: {mappingDto.Phone}, 주민번호: {mappingDto.IdNumber})");
                        continue;
                    }

                    // 해당 참석자가 행사에 등록되어 있는지 확인
                    var userConvention = await _unitOfWork.UserConventions.GetAsync(uc =>
                        uc.UserId == user.Id && uc.ConventionId == conventionId);

                    if (userConvention == null)
                    {
                        result.Warnings.Add(
                            $"참석자 {mappingDto.Name}는 이 행사에 등록되지 않았습니다.");
                        continue;
                    }

                    // 옵션ID 리스트로 매핑 생성
                    foreach (var customOptionId in mappingDto.OptionIds)
                    {
                        if (!optionTourMap.TryGetValue(customOptionId, out var optionTourId))
                        {
                            result.Warnings.Add(
                                $"참석자 {mappingDto.Name}: 옵션ID {customOptionId}를 찾을 수 없습니다.");
                            continue;
                        }

                        // 중복 체크
                        var existing = await _unitOfWork.UserOptionTours.GetAsync(uot =>
                            uot.UserId == user.Id &&
                            uot.ConventionId == conventionId &&
                            uot.OptionTourId == optionTourId);

                        if (existing != null)
                        {
                            continue; // 이미 매핑되어 있음
                        }

                        var userOptionTour = new UserOptionTour
                        {
                            UserId = user.Id,
                            OptionTourId = optionTourId,
                            ConventionId = conventionId,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.UserOptionTours.AddAsync(userOptionTour);
                        result.MappingsCreated++;
                    }

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation(
                        "Created mappings for user {UserName} (UserId: {UserId}): {OptionCount} options",
                        user.Name,
                        user.Id,
                        mappingDto.OptionIds.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create mapping for participant: {Name}", mappingDto.Name);
                    result.Warnings.Add($"참석자 '{mappingDto.Name}' 매핑 실패: {ex.Message}");
                }
            }

            result.Success = result.Errors.Count == 0;

            _logger.LogInformation(
                "Upload completed: {OptionsCreated} options, {MappingsCreated} mappings, {Errors} errors, {Warnings} warnings",
                result.OptionsCreated,
                result.MappingsCreated,
                result.Errors.Count,
                result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during option tour upload");
            result.Errors.Add($"업로드 중 오류 발생: {ex.Message}");
            return result;
        }
    }

    private string NormalizePhone(string phone)
    {
        return phone.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
    }
}
