using LocalRAG.Constants;
using LocalRAG.DTOs.AdminModels;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using LocalRAG.Services.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminAlimtalkController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKakaoAlimtalkService _alimtalkService;
    private readonly ITemplateVariableService _templateService;
    private readonly SmsTemplateContextFactory _contextFactory;
    private readonly IConfiguration _configuration;

    public AdminAlimtalkController(
        IUnitOfWork unitOfWork,
        IKakaoAlimtalkService alimtalkService,
        ITemplateVariableService templateService,
        SmsTemplateContextFactory contextFactory,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _alimtalkService = alimtalkService;
        _templateService = templateService;
        _contextFactory = contextFactory;
        _configuration = configuration;
    }

    /// <summary>
    /// 팝빌 알림톡 템플릿 목록 조회
    /// </summary>
    [HttpGet("alimtalk/templates")]
    public IActionResult GetAlimtalkTemplates()
    {
        try
        {
            var templates = _alimtalkService.ListTemplates();
            return Ok(templates.Select(t => new
            {
                templateCode = t.templateCode,
                templateName = t.templateName,
                template = t.template,
                plusFriendID = t.plusFriendID,
                ads = t.ads,
                appendix = t.appendix,
                secureYN = t.secureYN,
                state = t.state,
                stateDT = t.stateDT
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"템플릿 조회 실패: {ex.Message}" });
        }
    }

    /// <summary>
    /// 팝빌 잔여 포인트 조회
    /// </summary>
    [HttpGet("alimtalk/balance")]
    public IActionResult GetBalance()
    {
        try
        {
            var balance = _alimtalkService.GetBalance();
            return Ok(new { balance });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"잔여 포인트 조회 실패: {ex.Message}" });
        }
    }

    /// <summary>
    /// 알림톡 대량 발송
    /// </summary>
    [HttpPost("conventions/{conventionId}/alimtalk/send")]
    public async Task<IActionResult> SendAlimtalkBulk(int conventionId, [FromBody] SendAlimtalkRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TemplateCode))
            return BadRequest(new { message = "템플릿 코드가 필요합니다." });

        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest(new { message = "발송 내용이 없습니다." });

        if (dto.TargetUserIds == null || !dto.TargetUserIds.Any())
            return BadRequest(new { message = "수신자가 선택되지 않았습니다." });

        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null) return NotFound(new { message = "행사 정보를 찾을 수 없습니다." });

        var users = await _unitOfWork.Users.Query
            .Where(u => dto.TargetUserIds.Contains(u.Id))
            .Include(u => u.GuestAttributes)
            .ToListAsync();

        var senderNum = _configuration["Popbill:SenderNumber"] ?? "";

        // 해당 행사의 좌석 배치도 조회 (딥링크용)
        var seatLayout = await _unitOfWork.SeatingLayouts.Query
            .Where(sl => sl.ConventionId == conventionId && !sl.IsDeleted)
            .OrderByDescending(sl => sl.UpdatedAt ?? sl.CreatedAt)
            .FirstOrDefaultAsync();

        // 수신자별 메시지 치환
        var receivers = new List<AlimtalkReceiver>();
        foreach (var user in users)
        {
            if (string.IsNullOrWhiteSpace(user.Phone)) continue;

            // 해당 사용자의 AccessToken 조회 (자동 로그인용)
            var userConvention = await _unitOfWork.UserConventions
                .GetByUserAndConventionAsync(user.Id, conventionId);
            var context = _contextFactory.Create(user, convention, seatLayout?.Id, userConvention?.AccessToken);
            string message = _templateService.ReplaceVariables(dto.Content, context);
            string? altMessage = dto.AltContent != null
                ? _templateService.ReplaceVariables(dto.AltContent, context)
                : null;

            var receiver = new AlimtalkReceiver
            {
                ReceiverNum = user.Phone,
                ReceiverName = user.Name,
                Content = message,
                AltContent = altMessage,
            };

            // 좌석 딥링크가 있으면 버튼 자동 추가 (Mobile + PC 동일 URL)
            if (!string.IsNullOrEmpty(context.SeatLink))
            {
                receiver.Buttons = new List<AlimtalkButton>
                {
                    new AlimtalkButton { Name = "좌석안내", Type = "WL", Url = context.SeatLink }
                };
            }

            receivers.Add(receiver);
        }

        if (receivers.Count == 0)
            return BadRequest(new { message = "전화번호가 있는 수신자가 없습니다." });

        var (successCount, failCount, receiptNum) = await _alimtalkService.SendBulkAsync(
            dto.TemplateCode,
            senderNum,
            receivers);

        return Ok(new
        {
            message = $"총 {receivers.Count}건 중 성공: {successCount}, 실패: {failCount}",
            successCount,
            failCount,
            receiptNum
        });
    }

    /// <summary>
    /// 알림톡 미리보기
    /// </summary>
    [HttpPost("conventions/{conventionId}/alimtalk/preview")]
    public async Task<IActionResult> PreviewAlimtalk(int conventionId, [FromBody] SmsPreviewRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest(new { message = "내용이 없습니다." });

        var convention = await _unitOfWork.Conventions.GetByIdAsync(conventionId);
        if (convention == null) return NotFound(new { message = "행사 정보를 찾을 수 없습니다." });

        var user = await _unitOfWork.Users.Query
            .Include(u => u.GuestAttributes)
            .FirstOrDefaultAsync(u => u.Id == dto.TargetUserId);

        if (user == null) return NotFound(new { message = "사용자를 찾을 수 없습니다." });

        var seatLayout2 = await _unitOfWork.SeatingLayouts.Query
            .Where(sl => sl.ConventionId == conventionId && !sl.IsDeleted)
            .OrderByDescending(sl => sl.UpdatedAt ?? sl.CreatedAt)
            .FirstOrDefaultAsync();

        var context = _contextFactory.Create(user, convention, seatLayout2?.Id);
        string previewMessage = _templateService.ReplaceVariables(dto.Content, context);

        return Ok(new { previewMessage, seatLink = context.SeatLink });
    }
}
