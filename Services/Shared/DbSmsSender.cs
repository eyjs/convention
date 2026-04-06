using LocalRAG.Data;
using LocalRAG.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Shared;

/// <summary>
/// DB 프로시저를 통해 문자를 발송하는 인프라 스트럭처 서비스
/// </summary>
public class DbSmsSender : ISmsSender
{
    private readonly ConventionDbContext _context;
    private readonly ILogger<DbSmsSender> _logger;

    public DbSmsSender(ConventionDbContext context, ILogger<DbSmsSender> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string?> SendSmsAsync(string mobile, string message, string? receiverName = null)
    {
        try
        {
            string procedureName = "usp_member_send_sms";
            string memberName = "System"; // 발송자명 (고정 또는 설정값)
            string rcvName = receiverName ?? "Guest";

            var connection = _context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = procedureName;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@p_member_id", memberName));
                command.Parameters.Add(new SqlParameter("@p_rcv_mobile", mobile));
                command.Parameters.Add(new SqlParameter("@p_rcv_name", rcvName));
                command.Parameters.Add(new SqlParameter("@p_msg", message));

                // 프로시저가 2개 결과셋 반환:
                // 1번째: 디버깅용 (SenderID, SendDtm, ...) → 스킵
                // 2번째: SELECT 'result' = @return_value → 실제 발송 결과
                using (var reader = await command.ExecuteReaderAsync())
                {
                    // 1번째 결과셋 스킵
                    await reader.NextResultAsync();

                    // 2번째 결과셋에서 return_value 읽기
                    if (await reader.ReadAsync())
                    {
                        var returnValue = reader.GetValue(0)?.ToString();
                        _logger.LogInformation("SMS Proc return_value: {Result}, To: {Mobile}", returnValue, mobile);

                        // return_value가 있으면 발송 성공으로 간주
                        if (!string.IsNullOrEmpty(returnValue))
                        {
                            return returnValue;
                        }
                    }
                }
            }
            
            _logger.LogWarning("SMS Core executed but no ID returned. To: {Mobile}", mobile);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS 발송 실패. To: {Mobile}", mobile);
            return null;
        }
    }
}
