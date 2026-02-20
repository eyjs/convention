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

                // 결과값('result')을 읽어옵니다.
                // 프로시저가 디버깅용 SELECT 후 실제 결과를 반환하므로 NextResult()가 필요할 수 있음
                using (var reader = await command.ExecuteReaderAsync())
                {
                    do
                    {
                        if (await reader.ReadAsync())
                        {
                            try
                            {
                                // 컬럼명에 의존하지 않고 첫 번째 컬럼 값을 읽음
                                var cmpMsgId = reader.GetValue(0)?.ToString();
                                
                                // 값이 있고, 길이가 충분히 길면(ID로 추정되면) 반환
                                if (!string.IsNullOrEmpty(cmpMsgId) && cmpMsgId.Length > 1)
                                {
                                    _logger.LogInformation("SMS Core Sent. ID: {Id}, To: {Mobile}", cmpMsgId, mobile);
                                    return cmpMsgId;
                                }
                            }
                            catch (Exception)
                            {
                                // 읽기 실패 시 다음 결과셋으로
                            }
                        }
                    } while (await reader.NextResultAsync());
                }
            }
            
            _logger.LogWarning("SMS Core executed but no ID returned. To: {Mobile}", mobile);
            return null;
        }
        /* 
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS Core Error. To: {Mobile}", mobile);
            return null;
        }
        */
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS Core Error. To: {Mobile}", mobile);
            throw; // 디버깅을 위해 예외 재발생
        }
    }
}
