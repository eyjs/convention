using LocalRAG.Data;
using LocalRAG.Constants;
using LocalRAG.Interfaces;
using LocalRAG.Entities;
using LocalRAG.DTOs.AdminModels;
using LocalRAG.Services.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminSmsController : ControllerBase
{
    private readonly ConventionDbContext _context;
    private readonly ISmsService _smsService;
    private readonly ITemplateVariableService _templateService;
    private readonly SmsTemplateContextFactory _contextFactory;

    public AdminSmsController(
        ConventionDbContext context,
        ISmsService smsService,
        ITemplateVariableService templateService,
        SmsTemplateContextFactory contextFactory)
    {
        _context = context;
        _smsService = smsService;
        _templateService = templateService;
        _contextFactory = contextFactory;
    }

    [HttpPost("conventions/{conventionId}/sms/send")]
    public async Task<IActionResult> SendSmsBulk(int conventionId, [FromBody] SendSmsRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest(new { message = "발송할 내용이 없습니다." });

        if (dto.TargetUserIds == null || !dto.TargetUserIds.Any())
            return BadRequest(new { message = "수신자가 선택되지 않았습니다." });

        var convention = await _context.Conventions.FindAsync(conventionId);
        if (convention == null) return NotFound("행사 정보를 찾을 수 없습니다.");

        var users = await _context.Users
            .Where(u => dto.TargetUserIds.Contains(u.Id))
            .Include(u => u.GuestAttributes)
            .ToListAsync();

        int successCount = 0;
        int failCount = 0;

        foreach (var user in users)
        {
            var context = _contextFactory.Create(user, convention);
            string message = _templateService.ReplaceVariables(dto.Content, context);
            bool result = await _smsService.SendSmsAsync(conventionId, user.Name, user.Phone, message);

            if (result) successCount++;
            else failCount++;
        }

        return Ok(new
        {
            message = $"총 {dto.TargetUserIds.Count}건 중 성공: {successCount}, 실패: {failCount}",
            successCount,
            failCount
        });
    }

    [HttpPost("conventions/{conventionId}/sms/preview")]
    public async Task<IActionResult> PreviewSms(int conventionId, [FromBody] SmsPreviewRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest(new { message = "내용이 없습니다." });

        var convention = await _context.Conventions.FindAsync(conventionId);
        if (convention == null) return NotFound("행사 정보를 찾을 수 없습니다.");

        var user = await _context.Users
            .Include(u => u.GuestAttributes)
            .FirstOrDefaultAsync(u => u.Id == dto.TargetUserId);

        if (user == null) return NotFound("사용자를 찾을 수 없습니다.");

        var context = _contextFactory.Create(user, convention);
        string previewMessage = _templateService.ReplaceVariables(dto.Content, context);

        return Ok(new { previewMessage, context });
    }

    [HttpGet("sms-templates")]
    public async Task<IActionResult> GetSmsTemplates()
    {
        var templates = await _context.SmsTemplates
            .OrderByDescending(t => t.Id)
            .Select(t => new SmsTemplateDto
            {
                Id = t.Id,
                Title = t.TemplateName,
                Content = t.TemplateContent,
                CreatedAt = t.RegDtm
            })
            .ToListAsync();
        return Ok(templates);
    }

    [HttpPost("sms-templates")]
    public async Task<IActionResult> CreateSmsTemplate([FromBody] SmsTemplateDto dto)
    {
        var template = new SmsTemplate
        {
            TemplateName = dto.Title,
            TemplateContent = dto.Content,
            RegDtm = DateTime.UtcNow,
            DeleteYn = DeleteStatus.ActiveNumeric
        };
        _context.SmsTemplates.Add(template);
        await _context.SaveChangesAsync();

        dto.Id = template.Id;
        dto.CreatedAt = template.RegDtm;

        return Ok(dto);
    }

    [HttpPut("sms-templates/{id}")]
    public async Task<IActionResult> UpdateSmsTemplate(int id, [FromBody] SmsTemplateDto dto)
    {
        var template = await _context.SmsTemplates.FindAsync(id);
        if (template == null) return NotFound();

        template.TemplateName = dto.Title;
        template.TemplateContent = dto.Content;

        await _context.SaveChangesAsync();
        return Ok(dto);
    }
}
