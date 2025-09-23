using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalRAG.Data;
using LocalRAG.Models.Convention;
using LocalRAG.Configuration;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseTestController : ControllerBase
{
    private readonly ConventionDbContext _conventionContext;
    private readonly IConnectionStringProvider _connectionProvider;
    private readonly ILogger<DatabaseTestController> _logger;

    public DatabaseTestController(
        ConventionDbContext conventionContext,
        IConnectionStringProvider connectionProvider,
        ILogger<DatabaseTestController> logger)
    {
        _conventionContext = conventionContext;
        _connectionProvider = connectionProvider;
        _logger = logger;
    }

    /// <summary>
    /// 데이터베이스 연결 상태 확인
    /// </summary>
    [HttpGet("connection-status")]
    public async Task<IActionResult> GetConnectionStatus()
    {
        var connectionInfo = _connectionProvider.GetConnectionInfo();
        var result = new
        {
            timestamp = DateTime.Now,
            connectionInfo = new
            {
                environment = connectionInfo.Environment,
                connectionType = connectionInfo.ConnectionType,
                serverName = connectionInfo.ServerName,
                databaseName = connectionInfo.DatabaseName,
                isLocalConnection = connectionInfo.IsLocalConnection,
                resolvedAt = connectionInfo.ResolvedAt
            },
            databaseStatus = new Dictionary<string, object>()
        };

        // Convention Database 연결 테스트
        try
        {
            var startTime = DateTime.Now;
            var canConnect = await _conventionContext.Database.CanConnectAsync();
            var connectionTime = DateTime.Now - startTime;
            
            if (canConnect)
            {
                var conventionCount = await _conventionContext.Conventions.CountAsync();
                var guestCount = await _conventionContext.Guests.CountAsync();
                var scheduleCount = await _conventionContext.Schedules.CountAsync();
                
                result.databaseStatus.Add("Convention DB", new
                {
                    status = "✅ 연결됨",
                    canConnect = true,
                    connectionTimeMs = connectionTime.TotalMilliseconds,
                    tableCounts = new
                    {
                        conventions = conventionCount,
                        guests = guestCount,
                        schedules = scheduleCount
                    },
                    lastChecked = DateTime.Now
                });
                
                _logger.LogInformation("데이터베이스 연결 성공: {ConnectionType}, 응답시간: {ResponseTime}ms", 
                    connectionInfo.ConnectionType, connectionTime.TotalMilliseconds);
            }
            else
            {
                result.databaseStatus.Add("Convention DB", new
                {
                    status = "❌ 연결 실패",
                    canConnect = false,
                    connectionTimeMs = connectionTime.TotalMilliseconds,
                    error = "Database connection failed",
                    lastChecked = DateTime.Now
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "데이터베이스 연결 테스트 실패");
            result.databaseStatus.Add("Convention DB", new
            {
                status = "❌ 연결 오류",
                canConnect = false,
                error = ex.Message,
                errorType = ex.GetType().Name,
                lastChecked = DateTime.Now
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// 데이터베이스 초기화 및 테스트 데이터 삽입
    /// </summary>
    [HttpPost("initialize-database")]
    public async Task<IActionResult> InitializeDatabase()
    {
        try
        {
            var connectionInfo = _connectionProvider.GetConnectionInfo();
            _logger.LogInformation("데이터베이스 초기화 시작: {ConnectionType}", connectionInfo.ConnectionType);

            // 데이터베이스 생성 (이미 존재하면 무시)
            await _conventionContext.Database.EnsureCreatedAsync();
            _logger.LogInformation("데이터베이스 생성 완료: {DatabaseName}", connectionInfo.DatabaseName);

            // 테스트 데이터가 이미 있는지 확인
            var existingConventions = await _conventionContext.Conventions.CountAsync();
            if (existingConventions > 0)
            {
                return Ok(new
                {
                    message = "데이터베이스가 이미 초기화되어 있습니다.",
                    connectionInfo = new
                    {
                        connectionInfo.Environment,
                        connectionInfo.ConnectionType,
                        connectionInfo.ServerName,
                        connectionInfo.DatabaseName
                    },
                    existingData = new
                    {
                        conventions = existingConventions,
                        guests = await _conventionContext.Guests.CountAsync(),
                        schedules = await _conventionContext.Schedules.CountAsync()
                    },
                    timestamp = DateTime.Now
                });
            }

            // 환경별 테스트 데이터 생성
            var testConvention = CreateTestConvention(connectionInfo.Environment);
            _conventionContext.Conventions.Add(testConvention);
            await _conventionContext.SaveChangesAsync();

            // 테스트 게스트 추가
            var testGuests = CreateTestGuests(testConvention.Id, connectionInfo.Environment);
            _conventionContext.Guests.AddRange(testGuests);
            await _conventionContext.SaveChangesAsync();

            // 테스트 일정 추가
            var testSchedules = CreateTestSchedules(testConvention.Id, connectionInfo.Environment);
            _conventionContext.Schedules.AddRange(testSchedules);
            await _conventionContext.SaveChangesAsync();

            // 게스트 속성 추가
            var guestAttributes = CreateTestGuestAttributes(testGuests);
            _conventionContext.GuestAttributes.AddRange(guestAttributes);
            await _conventionContext.SaveChangesAsync();

            _logger.LogInformation("테스트 데이터 삽입 완료: {Environment}", connectionInfo.Environment);

            return Ok(new
            {
                message = "데이터베이스 초기화 및 테스트 데이터 삽입 완료",
                connectionInfo = new
                {
                    connectionInfo.Environment,
                    connectionInfo.ConnectionType,
                    connectionInfo.ServerName,
                    connectionInfo.DatabaseName
                },
                createdData = new
                {
                    convention = new
                    {
                        testConvention.Id,
                        testConvention.Title,
                        testConvention.ConventionType
                    },
                    counts = new
                    {
                        guests = testGuests.Count,
                        schedules = testSchedules.Count,
                        attributes = guestAttributes.Count
                    }
                },
                timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "데이터베이스 초기화 중 오류 발생");
            return StatusCode(500, new
            {
                message = "초기화 중 오류가 발생했습니다.",
                error = ex.Message,
                connectionType = _connectionProvider.GetConnectionInfo().ConnectionType,
                timestamp = DateTime.Now
            });
        }
    }

    /// <summary>
    /// 연결 문자열 정보 조회 (개발 환경에서만)
    /// </summary>
    [HttpGet("connection-strings")]
    public IActionResult GetConnectionStrings()
    {
        var connectionInfo = _connectionProvider.GetConnectionInfo();
        
        // 운영 환경에서는 연결 문자열 정보 숨김
        if (connectionInfo.Environment == "Production")
        {
            return Ok(new
            {
                message = "연결 문자열 정보는 운영 환경에서 숨겨집니다.",
                environment = "Production",
                connectionType = connectionInfo.ConnectionType,
                basicInfo = new
                {
                    serverName = connectionInfo.ServerName,
                    databaseName = connectionInfo.DatabaseName
                }
            });
        }

        return Ok(new
        {
            currentConnection = new
            {
                connectionInfo.Environment,
                connectionInfo.ConnectionType,
                connectionInfo.ServerName,
                connectionInfo.DatabaseName,
                connectionInfo.IsLocalConnection
            },
            availableConnections = new
            {
                developmentConnectionString = new
                {
                    name = "DevelopmentConnectionString",
                    description = "로컬 개발 DB (localhost/startour)",
                    server = "localhost",
                    database = "startour",
                    userId = "wnstn1342"
                },
                operationConnectionString = new
                {
                    name = "OperationConnectionString", 
                    description = "운영 DB (172.25.1.21/ifa)",
                    server = "172.25.1.21",
                    database = "ifa",
                    userId = "ifadb"
                }
            },
            switchInstructions = new
            {
                useDevelopmentDb = "개발 환경 실행: ./dev-run.ps1",
                useOperationDbInDev = "개발 환경에서 운영 DB 테스트: ./dev-run.ps1 -UseRemote",
                useProductionEnv = "운영 환경 실행: ./prod-run.ps1"
            }
        });
    }

    /// <summary>
    /// 테스트 데이터 조회
    /// </summary>
    [HttpGet("test-data")]
    public async Task<IActionResult> GetTestData()
    {
        try
        {
            var connectionInfo = _connectionProvider.GetConnectionInfo();
            
            var conventions = await _conventionContext.Conventions
                .Include(c => c.Guests)
                .Include(c => c.Schedules)
                .Take(10)
                .Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.ConventionType,
                    c.StartDate,
                    c.EndDate,
                    GuestCount = c.Guests.Count,
                    ScheduleCount = c.Schedules.Count
                })
                .ToListAsync();

            return Ok(new
            {
                message = "테스트 데이터 조회 성공",
                connectionInfo = new
                {
                    connectionInfo.Environment,
                    connectionInfo.ConnectionType,
                    connectionInfo.ServerName,
                    connectionInfo.DatabaseName
                },
                data = new
                {
                    conventions = conventions,
                    totalCount = conventions.Count
                },
                timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "테스트 데이터 조회 중 오류 발생");
            return StatusCode(500, new
            {
                message = "데이터 조회 중 오류가 발생했습니다.",
                error = ex.Message,
                timestamp = DateTime.Now
            });
        }
    }

    /// <summary>
    /// 데이터베이스 스키마 정보 조회
    /// </summary>
    [HttpGet("schema-info")]
    public IActionResult GetSchemaInfo()
    {
        try
        {
            var connectionInfo = _connectionProvider.GetConnectionInfo();
            
            var result = new
            {
                connectionInfo = new
                {
                    connectionInfo.Environment,
                    connectionInfo.ConnectionType,
                    connectionInfo.ServerName,
                    connectionInfo.DatabaseName
                },
                conventionDatabase = new
                {
                    description = $"컨벤션 관리 시스템 - {connectionInfo.ConnectionType}",
                    tables = new[]
                    {
                        new { name = "t_corp_convention", description = "행사 기본 정보", model = "Convention" },
                        new { name = "t_corp_convention_guest", description = "참석자 정보", model = "Guest" },
                        new { name = "t_corp_convention_schedule", description = "일정 정보", model = "Schedule" },
                        new { name = "t_corp_convention_guest_attributes", description = "참석자 속성", model = "GuestAttribute" },
                        new { name = "t_corp_convention_companions", description = "동반자 정보", model = "Companion" },
                        new { name = "t_corp_convention_guest_schedule", description = "참석자-일정 연결", model = "GuestSchedule" },
                        new { name = "t_corp_convention_features", description = "기능 설정", model = "Feature" },
                        new { name = "t_corp_convention_menu", description = "메뉴 정보", model = "Menu" },
                        new { name = "t_corp_convention_section", description = "섹션 정보", model = "Section" },
                        new { name = "t_corp_convention_owner", description = "담당자 정보", model = "Owner" },
                        new { name = "t_vector_store", description = "벡터 저장소", model = "VectorStore" }
                    }
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "스키마 정보 조회 중 오류가 발생했습니다.",
                error = ex.Message,
                timestamp = DateTime.Now
            });
        }
    }

    // 환경별 테스트 데이터 생성 메서드들
    private Convention CreateTestConvention(string environment)
    {
        var isProduction = environment == "Production";
        return new Convention
        {
            MemberId = "admin",
            RegDtm = DateTime.Now,
            DeleteYn = "N",
            Title = isProduction ? "2024 연말 정기총회" : $"[{environment}] 2024 테스트 워크샵",
            ConventionType = isProduction ? "DOMESTIC" : "DOMESTIC",
            RenderType = "STANDARD",
            ConventionImg = $"/images/convention_{environment.ToLower()}.jpg",
            StartDate = DateTime.Now.AddDays(isProduction ? 60 : 30),
            EndDate = DateTime.Now.AddDays(isProduction ? 62 : 32)
        };
    }

    private List<Guest> CreateTestGuests(int conventionId, string environment)
    {
        var isProduction = environment == "Production";
        var prefix = isProduction ? "" : $"[{environment}] ";
        
        return new List<Guest>
        {
            new Guest
            {
                ConventionId = conventionId,
                CorpHrId = "EMP001",
                CorpPart = "개발팀",
                GuestName = $"{prefix}김철수",
                PasswordHash = "hashed_password_1",
                Telephone = "010-1234-5678"
            },
            new Guest
            {
                ConventionId = conventionId,
                CorpHrId = "EMP002", 
                CorpPart = "디자인팀",
                GuestName = $"{prefix}이영희",
                PasswordHash = "hashed_password_2",
                Telephone = "010-9876-5432"
            },
            new Guest
            {
                ConventionId = conventionId,
                CorpHrId = "EMP003",
                CorpPart = "마케팅팀",
                GuestName = $"{prefix}박민수",
                PasswordHash = "hashed_password_3",
                Telephone = "010-5555-1111"
            }
        };
    }

    private List<Schedule> CreateTestSchedules(int conventionId, string environment)
    {
        var isProduction = environment == "Production";
        var baseDate = DateTime.Now.AddDays(isProduction ? 60 : 30);
        
        return new List<Schedule>
        {
            new Schedule
            {
                ConventionId = conventionId,
                Name = isProduction ? "정기총회 개회식" : $"[{environment}] 개회식",
                Group = "전체",
                ScheduleDate = baseDate,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(10, 0, 0),
                Description = isProduction ? "2024 정기총회 개회식" : $"{environment} 환경 테스트 개회식",
                OrderNum = 1
            },
            new Schedule
            {
                ConventionId = conventionId,
                Name = isProduction ? "사업보고 발표" : $"[{environment}] 팀 빌딩",
                Group = "전체",
                ScheduleDate = baseDate,
                StartTime = new TimeSpan(10, 30, 0),
                EndTime = new TimeSpan(12, 0, 0),
                Description = isProduction ? "연간 사업 실적 보고" : "팀워크 향상 활동",
                OrderNum = 2
            }
        };
    }

    private List<GuestAttribute> CreateTestGuestAttributes(List<Guest> guests)
    {
        return new List<GuestAttribute>
        {
            new GuestAttribute
            {
                GuestId = guests[0].Id,
                AttributeKey = "식이제한",
                AttributeValue = "없음"
            },
            new GuestAttribute
            {
                GuestId = guests[1].Id,
                AttributeKey = "식이제한",
                AttributeValue = "채식주의"
            },
            new GuestAttribute
            {
                GuestId = guests[2].Id,
                AttributeKey = "교통편",
                AttributeValue = "개인차량"
            }
        };
    }
}
