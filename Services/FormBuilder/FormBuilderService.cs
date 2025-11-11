using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.DTOs.FormBuilder;
using LocalRAG.Entities.FormBuilder;
using LocalRAG.Interfaces;
using System.Text.Json;
using System.IO;

namespace LocalRAG.Services.FormBuilder;

public class FormBuilderService : IFormBuilderService
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<FormBuilderService> _logger;

    public FormBuilderService(
        ConventionDbContext context,
        ILogger<FormBuilderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FormDefinitionDto?> GetFormDefinitionAsync(int formDefinitionId)
    {
        var formDto = await _context.FormDefinitions
            .Where(f => f.Id == formDefinitionId)
            .Select(f => new FormDefinitionDto
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                ConventionId = f.ConventionId,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt,
                Fields = f.Fields.OrderBy(field => field.OrderIndex).Select(field => new FormFieldDto
                {
                    Id = field.Id,
                    Key = field.Key,
                    Label = field.Label,
                    FieldType = field.FieldType,
                    OrderIndex = field.OrderIndex,
                    IsRequired = field.IsRequired,
                    Placeholder = field.Placeholder,
                    OptionsJson = field.OptionsJson
                }).ToList()
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return formDto;
    }

    public async Task<string?> GetUserSubmissionAsync(int formDefinitionId, int userId)
    {
        var submission = await _context.FormSubmissions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.FormDefinitionId == formDefinitionId && s.UserId == userId);

        return submission?.SubmissionDataJson;
    }

    public async Task<bool> SubmitFormDataAsync(
        int formDefinitionId,
        int userId,
        string formDataJson,
        IFormFile? file,
        string? fileFieldKey)
    {
        _logger.LogInformation("SubmitFormData called for FormDefinitionId: {FormDefinitionId}", formDefinitionId);

        // FormDefinition 존재 확인
        if (!await _context.FormDefinitions.AnyAsync(f => f.Id == formDefinitionId))
        {
            _logger.LogWarning("FormDefinition not found: {FormDefinitionId}", formDefinitionId);
            return false;
        }

        // JSON 데이터 파싱
        _logger.LogInformation("Received FormDataJson: {FormDataJson}", formDataJson);

        if (string.IsNullOrWhiteSpace(formDataJson))
        {
            _logger.LogError("FormDataJson is null or whitespace.");
            return false;
        }

        var tempDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(formDataJson);
        if (tempDict == null)
        {
            _logger.LogError("Failed to deserialize FormDataJson: {FormDataJson}", formDataJson);
            return false;
        }

        // 파일 처리
        if (file != null && file.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/{uniqueFileName}";

            // fileFieldKey가 제공되면 해당 키의 값을 파일 URL로 업데이트
            if (!string.IsNullOrEmpty(fileFieldKey) && tempDict.ContainsKey(fileFieldKey))
            {
                tempDict[fileFieldKey] = JsonDocument.Parse($"\"{fileUrl}\"").RootElement;
            }
            else
            {
                // fileFieldKey가 없거나 해당 키가 formDataJson에 없으면 새로운 속성으로 추가
                tempDict.Add("uploadedFileUrl", JsonDocument.Parse($"\"{fileUrl}\"").RootElement);
            }
            formDataJson = JsonSerializer.Serialize(tempDict);
        }

        var submission = await _context.FormSubmissions
            .FirstOrDefaultAsync(s => s.FormDefinitionId == formDefinitionId && s.UserId == userId);

        if (submission != null)
        {
            // Update
            submission.SubmissionDataJson = formDataJson;
            submission.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Create
            submission = new FormSubmission
            {
                FormDefinitionId = formDefinitionId,
                UserId = userId,
                SubmissionDataJson = formDataJson,
            };
            _context.FormSubmissions.Add(submission);
        }

        // 연결된 ConventionAction의 상태 업데이트
        var action = await _context.ConventionActions
            .FirstOrDefaultAsync(a => a.TargetId == formDefinitionId &&
                                     a.BehaviorType == Entities.Action.BehaviorType.FormBuilder);

        if (action != null)
        {
            var status = await _context.UserActionStatuses
                .FirstOrDefaultAsync(s => s.UserId == userId && s.ConventionActionId == action.Id);

            if (status == null)
            {
                _context.UserActionStatuses.Add(new Entities.UserActionStatus
                {
                    UserId = userId,
                    ConventionActionId = action.Id,
                    IsComplete = true,
                    CompletedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                status.IsComplete = true;
                status.CompletedAt = DateTime.UtcNow;
                status.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<FormSubmissionDto>> GetAllSubmissionsAsync(int formDefinitionId)
    {
        if (!await _context.FormDefinitions.AnyAsync(f => f.Id == formDefinitionId))
        {
            return new List<FormSubmissionDto>();
        }

        var submissionsRaw = await _context.FormSubmissions
            .Include(s => s.User)
            .Where(s => s.FormDefinitionId == formDefinitionId)
            .ToListAsync();

        var submissions = submissionsRaw.Select(s => new FormSubmissionDto
        {
            Id = s.Id,
            UserId = s.UserId,
            UserName = s.User?.Name,
            UserEmail = s.User?.Email,
            SubmissionData = JsonDocument.Parse(s.SubmissionDataJson).RootElement,
            SubmittedAt = s.SubmittedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList();

        return submissions;
    }

    public bool ValidateFilePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        // 경로 검증 (보안)
        if (path.Contains("..") || !path.StartsWith("/uploads/"))
        {
            return false;
        }

        return true;
    }

    public string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }
}
