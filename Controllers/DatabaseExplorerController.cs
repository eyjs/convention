using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LocalRAG.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseExplorerController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseExplorerController> _logger;

    public DatabaseExplorerController(IConfiguration configuration, ILogger<DatabaseExplorerController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // 데이터베이스 연결 테스트
    [HttpGet("test-connection")]
    public async Task<ActionResult<object>> TestConnection()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("SQLConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest(new { Error = "SQLConnectionString connection string not configured in appsettings.json" });
            }

            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new DbContext(optionsBuilder.Options);
            var canConnect = await context.Database.CanConnectAsync();
            
            var dbName = "Unknown";
            var serverVersion = "Unknown";
            
            if (canConnect)
            {
                try
                {
                    var connection = context.Database.GetDbConnection();
                    await connection.OpenAsync();
                    dbName = connection.Database;
                    serverVersion = connection.ServerVersion;
                    await connection.CloseAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not get database details, but connection test passed");
                }
            }
            
            return Ok(new
            {
                CanConnect = canConnect,
                DatabaseName = dbName,
                ServerVersion = serverVersion,
                ConnectionString = connectionString.Substring(0, Math.Min(50, connectionString.Length)) + "...",
                TestTime = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed");
            return BadRequest(new { Error = ex.Message });
        }
    }

    // 모든 테이블 목록 조회
    [HttpGet("tables")]
    public async Task<ActionResult<object>> GetAllTables()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("SQLConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest(new { Error = "SQLConnectionString connection string not configured" });
            }

            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new DbContext(optionsBuilder.Options);
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    TABLE_SCHEMA as SchemaName,
                    TABLE_NAME as TableName,
                    TABLE_TYPE as TableType
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_TYPE = 'BASE TABLE'
                ORDER BY TABLE_SCHEMA, TABLE_NAME";

            using var reader = await command.ExecuteReaderAsync();
            var tables = new List<object>();

            while (await reader.ReadAsync())
            {
                tables.Add(new
                {
                    SchemaName = reader.GetString("SchemaName"),
                    TableName = reader.GetString("TableName"),
                    TableType = reader.GetString("TableType")
                });
            }

            return Ok(new
            {
                DatabaseName = context.Database.GetDbConnection().Database,
                TableCount = tables.Count,
                Tables = tables
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get table list");
            return BadRequest(new { Error = ex.Message });
        }
    }

    // 특정 테이블의 컬럼 정보 조회
    [HttpGet("tables/{tableName}/columns")]
    public async Task<ActionResult<object>> GetTableColumns(string tableName)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("SQLConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                return BadRequest(new { Error = "SQLConnectionString connection string not configured" });
            }

            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new DbContext(optionsBuilder.Options);
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    c.COLUMN_NAME as ColumnName,
                    c.DATA_TYPE as DataType,
                    c.IS_NULLABLE as IsNullable,
                    c.CHARACTER_MAXIMUM_LENGTH as MaxLength,
                    c.NUMERIC_PRECISION as Precision,
                    c.NUMERIC_SCALE as Scale,
                    c.COLUMN_DEFAULT as DefaultValue,
                    c.ORDINAL_POSITION as Position,
                    CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END as IsPrimaryKey,
                    CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END as IsForeignKey,
                    fk.REFERENCED_TABLE_NAME as ReferencedTable,
                    fk.REFERENCED_COLUMN_NAME as ReferencedColumn
                FROM INFORMATION_SCHEMA.COLUMNS c
                LEFT JOIN (
                    SELECT ku.COLUMN_NAME
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
                    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
                        ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY' 
                        AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                        AND ku.TABLE_NAME = @tableName
                ) pk ON c.COLUMN_NAME = pk.COLUMN_NAME
                LEFT JOIN (
                    SELECT 
                        ku.COLUMN_NAME,
                        ccu.TABLE_NAME AS REFERENCED_TABLE_NAME,
                        ccu.COLUMN_NAME AS REFERENCED_COLUMN_NAME
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc 
                    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
                        ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                        AND tc.TABLE_NAME = ku.TABLE_NAME
                    INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS ccu
                        ON ccu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                    WHERE tc.CONSTRAINT_TYPE = 'FOREIGN KEY' 
                        AND ku.TABLE_NAME = @tableName
                ) fk ON c.COLUMN_NAME = fk.COLUMN_NAME
                WHERE c.TABLE_NAME = @tableName
                ORDER BY c.ORDINAL_POSITION";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            using var reader = await command.ExecuteReaderAsync();
            var columns = new List<object>();

            while (await reader.ReadAsync())
            {
                // 안전한 값 추출을 위한 헬퍼 메서드 사용
                columns.Add(new
                {
                    ColumnName = reader.GetString("ColumnName"),
                    DataType = reader.GetString("DataType"),
                    IsNullable = reader.GetString("IsNullable"),
                    MaxLength = reader.IsDBNull("MaxLength") ? (int?)null : Convert.ToInt32(reader["MaxLength"]),
                    Precision = reader.IsDBNull("Precision") ? (int?)null : Convert.ToInt32(reader["Precision"]),
                    Scale = reader.IsDBNull("Scale") ? (int?)null : Convert.ToInt32(reader["Scale"]),
                    DefaultValue = reader.IsDBNull("DefaultValue") ? null : reader["DefaultValue"]?.ToString(),
                    Position = Convert.ToInt32(reader["Position"]),
                    IsPrimaryKey = Convert.ToInt32(reader["IsPrimaryKey"]) == 1,
                    IsForeignKey = Convert.ToInt32(reader["IsForeignKey"]) == 1,
                    ReferencedTable = reader.IsDBNull("ReferencedTable") ? null : reader["ReferencedTable"]?.ToString(),
                    ReferencedColumn = reader.IsDBNull("ReferencedColumn") ? null : reader["ReferencedColumn"]?.ToString()
                });
            }

            if (!columns.Any())
            {
                return NotFound(new { Error = $"Table '{tableName}' not found or has no columns" });
            }

            return Ok(new
            {
                TableName = tableName,
                ColumnCount = columns.Count,
                Columns = columns
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get columns for table {TableName}", tableName);
            return BadRequest(new { Error = ex.Message });
        }
    }

    // 특정 테이블의 데이터 샘플 조회
    [HttpGet("tables/{tableName}/sample")]
    public async Task<ActionResult<object>> GetTableSample(string tableName, [FromQuery] int rows = 5)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("SQLConnectionString");
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new DbContext(optionsBuilder.Options);
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();

            // 테이블 존재 여부 확인
            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = @tableName AND TABLE_TYPE = 'BASE TABLE'";
            
            var parameter = checkCommand.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            checkCommand.Parameters.Add(parameter);

            var tableExists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
            if (!tableExists)
            {
                return NotFound(new { Error = $"Table '{tableName}' not found" });
            }

            // 총 레코드 수 확인
            using var countCommand = connection.CreateCommand();
            countCommand.CommandText = $"SELECT COUNT(*) FROM [{tableName}]";
            var totalRecords = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

            // 샘플 데이터 조회
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT TOP ({rows}) * FROM [{tableName}]";

            using var reader = await command.ExecuteReaderAsync();
            var results = new List<Dictionary<string, object>>();
            var columnNames = new List<string>();

            // 컬럼 이름 가져오기
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames.Add(reader.GetName(i));
            }

            // 데이터 읽기
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                results.Add(row);
            }

            return Ok(new
            {
                TableName = tableName,
                TotalRecords = totalRecords,
                SampleSize = results.Count,
                Columns = columnNames,
                SampleData = results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get sample data for table {TableName}", tableName);
            return BadRequest(new { Error = ex.Message });
        }
    }
}
