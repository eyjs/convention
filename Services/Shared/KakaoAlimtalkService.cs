using LocalRAG.Interfaces;
using Popbill;
using Popbill.Kakao;

namespace LocalRAG.Services.Shared;

/// <summary>
/// 카카오 알림톡 서비스 — 팝빌(Popbill) NuGet 1.61.0
/// 정적 초기화로 앱 전체에서 하나의 KakaoService 인스턴스 공유
/// </summary>
public class KakaoAlimtalkService : IKakaoAlimtalkService
{
    private static readonly object _lock = new();
    private static bool _initialized;
    private static KakaoService _kakaoService = null!;
    private static string _corpNum = string.Empty;
    private static string _userId = string.Empty;
    private static string _senderNumber = string.Empty;

    private readonly ILogger<KakaoAlimtalkService> _logger;

    public KakaoAlimtalkService(ILogger<KakaoAlimtalkService> logger, IConfiguration configuration)
    {
        _logger = logger;

        if (!_initialized)
        {
            lock (_lock)
            {
                if (!_initialized)
                {
                    InitializeStatic(configuration);
                    _initialized = true;
                }
            }
        }
    }

    private static void InitializeStatic(IConfiguration configuration)
    {
        var section = configuration.GetSection("Popbill");
        var linkId = section["LinkId"] ?? "IFACLOUD";
        var secretKey = section["SecretKey"] ?? "";
        _corpNum = section["CorpNum"] ?? "";
        _userId = section["UserId"] ?? "IFACLOUD";
        _senderNumber = section["SenderNumber"] ?? "";

        _kakaoService = new KakaoService(linkId, secretKey);
        _kakaoService.IsTest = false;
        _kakaoService.IPRestrictOnOff = true;
        _kakaoService.UseStaticIP = false;
        _kakaoService.UseLocalTimeYN = true;
    }

    public List<ATSTemplate> ListTemplates()
    {
        try
        {
            return _kakaoService.ListATSTemplate(_corpNum, _userId);
        }
        catch (PopbillException ex)
        {
            _logger.LogError(ex, "알림톡 템플릿 목록 조회 실패 — Code: {Code}", ex.code);
            throw;
        }
    }

    public double GetBalance()
    {
        try
        {
            return _kakaoService.GetPartnerBalance(_corpNum);
        }
        catch (PopbillException ex)
        {
            _logger.LogError(ex, "팝빌 잔여 포인트 조회 실패 — Code: {Code}", ex.code);
            throw;
        }
    }

    /// <summary>
    /// 알림톡 단건 발송
    /// SendATS(CorpNum, templateCode, snd, receiveNum, receiveName, msg, altmsg, altSendType, sndDT, requestNum, UserID, buttons, altSubject, emphasizeTitle)
    /// </summary>
    public Task<(bool Success, string? ReceiptNum, string? ErrorMessage)> SendAsync(
        string templateCode,
        string senderNum,
        string receiverNum,
        string receiverName,
        string content,
        string? altContent = null)
    {
        try
        {
            var sender = string.IsNullOrEmpty(senderNum) ? _senderNumber : senderNum;

            _logger.LogInformation("알림톡 단건 발송 — Template: {Template}, To: {Receiver}", templateCode, receiverNum);

            var receiptNum = _kakaoService.SendATS(
                _corpNum,
                templateCode,
                sender,
                receiverNum,        // receiveNum
                receiverName,       // receiveName
                content,            // msg
                altContent ?? "",   // altmsg
                altContent != null ? "A" : null,  // altSendType
                (DateTime?)null,    // sndDT (즉시발송)
                "",                 // requestNum
                _userId,            // UserID
                null,               // buttons
                null,               // altSubject
                null                // emphasizeTitle
            );

            _logger.LogInformation("알림톡 발송 성공 — ReceiptNum: {Receipt}", receiptNum);
            return Task.FromResult<(bool, string?, string?)>((true, receiptNum, null));
        }
        catch (PopbillException ex)
        {
            _logger.LogError(ex, "알림톡 단건 발송 실패 — Code: {Code}, To: {Receiver}", ex.code, receiverNum);
            return Task.FromResult<(bool, string?, string?)>((false, null, $"[{ex.code}] {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "알림톡 단건 발송 실패 — To: {Receiver}", receiverNum);
            return Task.FromResult<(bool, string?, string?)>((false, null, ex.Message));
        }
    }

    /// <summary>
    /// 알림톡 대량 발송
    /// SendATS(CorpNum, templateCode, snd, receivers, altSendType, sndDT, requestNum, UserID, buttons)
    /// </summary>
    public Task<(int SuccessCount, int FailCount, string? ReceiptNum)> SendBulkAsync(
        string templateCode,
        string senderNum,
        List<AlimtalkReceiver> receivers)
    {
        try
        {
            var sender = string.IsNullOrEmpty(senderNum) ? _senderNumber : senderNum;

            _logger.LogInformation("알림톡 대량 발송 — Template: {Template}, Count: {Count}", templateCode, receivers.Count);

            var kakaoReceivers = receivers.Select(r =>
            {
                var kr = new KakaoReceiver
                {
                    rcv = r.ReceiverNum,
                    rcvnm = r.ReceiverName,
                    msg = r.Content,
                    altmsg = r.AltContent,
                };
                // 수신자별 버튼 (딥링크 등)
                if (r.Buttons != null && r.Buttons.Count > 0)
                {
                    kr.btns = r.Buttons.Select(b => new KakaoButton
                    {
                        n = b.Name,
                        t = b.Type, // "WL" = 웹링크
                        u1 = b.Url,
                        u2 = b.Url,
                    }).ToList();
                }
                return kr;
            }).ToList();

            var receiptNum = _kakaoService.SendATS(
                _corpNum,
                templateCode,
                sender,
                kakaoReceivers,     // receivers
                null,               // altSendType
                (DateTime?)null,    // sndDT
                "",                 // requestNum
                _userId,            // UserID
                null                // buttons
            );

            _logger.LogInformation("알림톡 대량 발송 성공 — ReceiptNum: {Receipt}", receiptNum);
            return Task.FromResult((receivers.Count, 0, (string?)receiptNum));
        }
        catch (PopbillException ex)
        {
            _logger.LogError(ex, "알림톡 대량 발송 실패 — Code: {Code}", ex.code);
            return Task.FromResult((0, receivers.Count, (string?)null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "알림톡 대량 발송 실패 — Count: {Count}", receivers.Count);
            return Task.FromResult((0, receivers.Count, (string?)null));
        }
    }
}
