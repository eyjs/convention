using LocalRAG.Data; // DbContext 네임스페이스
using LocalRAG.Interfaces;
using LocalRAG.Models; // VectorDataEntry 네임스페이스
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics; // 벡터 계산용
using System.Text.Json;
using System.Threading.Tasks;

namespace LocalRAG.Storage;

public class MssqlVectorStore : IVectorStore
{
    private readonly ConventionDbContext _dbContext;
    private readonly ILogger<MssqlVectorStore> _logger;

    // 생성자 및 로거는 이전과 동일
    public MssqlVectorStore(ConventionDbContext dbContext, ILogger<MssqlVectorStore> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _logger.LogInformation("MssqlVectorStore 초기화됨.");
    }

    // 메타데이터에서 ConventionId 추출 헬퍼 (이전과 동일)
    private int GetConventionIdFromMetadata(Dictionary<string, object>? metadata)
    {
        const string key = "conventionId";
        if (metadata != null && metadata.TryGetValue(key, out var valueObj))
        {
            if (valueObj is int intValue) return intValue;
            if (valueObj is long longValue && longValue >= int.MinValue && longValue <= int.MaxValue) return (int)longValue;
            if (valueObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out int jsonInt)) return jsonInt;
            if (valueObj is string strValue && int.TryParse(strValue, out int parsedInt)) return parsedInt;

            _logger.LogWarning("메타데이터 키 '{Key}'의 값이 유효한 정수가 아닙니다. Value: {Value}", key, valueObj);
        }
        _logger.LogWarning("메타데이터에 유효한 '{Key}' 키가 없습니다.", key);
        throw new ArgumentException($"Document metadata must contain a valid integer '{key}'.");
    }

    // AddDocumentAsync 메서드 (이전과 동일)
    public async Task<string> AddDocumentAsync(string content, float[] embedding, Dictionary<string, object>? metadata = null)
    {
        var entry = new VectorDataEntry
        {
            ConventionId = GetConventionIdFromMetadata(metadata),
            Content = content,
            Embedding = embedding,
            MetadataJson = metadata != null ? JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = false }) : null
        };

        _dbContext.VectorDataEntries.Add(entry);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("문서 추가 완료. ID: {DocumentId}", entry.Id);
        return entry.Id;
    }

    // AddDocumentsAsync 메서드 (이전과 동일)
    public async Task AddDocumentsAsync(IEnumerable<VectorDocument> documents)
    {
        var entities = new List<VectorDataEntry>();
        foreach (var doc in documents)
        {
            try
            {
                entities.Add(new VectorDataEntry
                {
                    ConventionId = GetConventionIdFromMetadata(doc.Metadata),
                    Content = doc.Content,
                    Embedding = doc.Embedding,
                    MetadataJson = doc.Metadata != null ? JsonSerializer.Serialize(doc.Metadata, new JsonSerializerOptions { WriteIndented = false }) : null
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "문서 추가 중 오류: ConventionId 누락 또는 형식 오류. Content: {Content}", doc.Content.Substring(0, Math.Min(100, doc.Content.Length)));
            }
        }

        if (!entities.Any())
        {
            _logger.LogInformation("추가할 유효한 문서가 없습니다.");
            return;
        }

        _dbContext.VectorDataEntries.AddRange(entities);

        try
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("{Count}개 문서를 MSSQL에 추가했습니다.", entities.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MSSQL에 문서 일괄 추가 중 오류 발생");
            throw;
        }
    }

    public async Task<List<VectorSearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5, Dictionary<string, object>? filter = null)
    {
        IQueryable<VectorDataEntry> query = _dbContext.VectorDataEntries.AsNoTracking();

        // --- 필터링 (이전과 동일) ---
        if (filter != null)
        {
            foreach (var kvp in filter)
            {
                if (kvp.Key.Equals("conventionId", StringComparison.OrdinalIgnoreCase))
                {
                    int conventionId;
                    if (kvp.Value is int intValue) conventionId = intValue;
                    else if (kvp.Value is long longValue && longValue >= int.MinValue && longValue <= int.MaxValue) conventionId = (int)longValue;
                    else if (kvp.Value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number && jsonElement.TryGetInt32(out int jsonInt)) conventionId = jsonInt;
                    else if (kvp.Value is string strValue && int.TryParse(strValue, out int parsedInt)) conventionId = parsedInt;
                    else
                    {
                        _logger.LogWarning("검색 필터에 유효하지 않은 conventionId 값이 있습니다: {Value}. 검색 결과가 없을 수 있습니다.", kvp.Value);
                        return new List<VectorSearchResult>();
                    }
                    query = query.Where(v => v.ConventionId == conventionId);
                    _logger.LogInformation("ConventionId '{ConventionId}' 필터 적용됨", conventionId);
                }
                // 다른 필터링 로직 추가 가능
            }
        }

        // ⚠️ 성능 경고: 메모리로 로드 (이전과 동일)
        List<VectorDataEntry> candidates;
        try
        {
            candidates = await query.ToListAsync();
            _logger.LogInformation("{Count}개의 후보 벡터를 DB에서 메모리로 로드했습니다.", candidates.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DB에서 후보 벡터 로드 중 오류 발생");
            return new List<VectorSearchResult>();
        }

        if (!candidates.Any() || queryEmbedding == null || queryEmbedding.Length == 0)
        {
            _logger.LogInformation("검색 대상 벡터가 없거나 쿼리 벡터가 유효하지 않습니다.");
            return new List<VectorSearchResult>();
        }

        Vector<float> queryVec;
        float queryMagnitude; // 쿼리 벡터 크기 미리 계산
        try
        {
            queryVec = new Vector<float>(queryEmbedding);
            // ⭐️ 수정: Vector.Dot()을 사용하여 크기 제곱 계산 후 제곱근
            queryMagnitude = MathF.Sqrt(Vector.Dot(queryVec, queryVec));
            if (queryMagnitude == 0f) throw new ArgumentException("쿼리 벡터의 크기는 0일 수 없습니다.");
        }
        catch (ArgumentException ex) // Handle potential dimension mismatch if vector length isn't multiple of Vector<float>.Count
        {
            _logger.LogError(ex, "쿼리 벡터 생성 중 오류 발생. 벡터 차원 확인 필요.");
            return new List<VectorSearchResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "쿼리 벡터 생성 중 예상치 못한 오류 발생.");
            return new List<VectorSearchResult>();
        }


        // --- 메모리 내 유사도 계산 ---
        // ⭐️ 수정: 중간 결과 타입을 명시적인 튜플로 변경
        var resultsWithScores = new List<(VectorDataEntry Candidate, float Score)>();
        foreach (var candidate in candidates)
        {
            try
            {
                if (candidate.EmbeddingData == null || candidate.EmbeddingData.Length == 0) continue;

                Vector<float> candidateVec;
                float candidateMagnitude;
                try
                {
                    candidateVec = new Vector<float>(candidate.Embedding); // Getter 사용
                    // ⭐️ 수정: Vector.Dot()을 사용하여 크기 제곱 계산 후 제곱근
                    candidateMagnitude = MathF.Sqrt(Vector.Dot(candidateVec, candidateVec));
                }
                catch (ArgumentException innerEx) // Catch dimension mismatch
                {
                    _logger.LogWarning(innerEx, "벡터 ID '{Id}'의 차원이 쿼리 벡터와 맞지 않거나 Vector<float> 크기의 배수가 아닙니다. 건너<0xEB><0x9A><0x8E>니다.", candidate.Id);
                    continue;
                }


                if (candidateMagnitude == 0f) continue; // 크기 0 벡터 건너뛰기

                var dotProduct = Vector.Dot(queryVec, candidateVec);
                // ⭐️ 수정: 미리 계산된 크기 사용
                var score = dotProduct / (queryMagnitude * candidateMagnitude);

                // NaN 처리
                if (float.IsNaN(score)) score = (dotProduct > 0) ? 1.0f : -1.0f;

                resultsWithScores.Add((candidate, score));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "벡터 ID '{Id}' 유사도 계산 중 오류 발생. 건너<0xEB><0x9A><0x8E>니다.", candidate.Id);
            }
        }

        // --- 결과 정렬 및 최종 변환 ---
        // ⭐️ 수정: resultsWithScores 리스트를 직접 사용
        var finalResults = resultsWithScores
            .Where(r => r.Score >= 0) // 유효 점수만
            .OrderByDescending(r => r.Score)
            .Take(topK)
            .Select(r => new VectorSearchResult( // r은 (VectorDataEntry Candidate, float Score) 튜플
                r.Candidate.Id,
                r.Candidate.Content,
                r.Score,
                r.Candidate.MetadataJson != null
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(r.Candidate.MetadataJson)
                    : null
            ))
            .ToList();

        _logger.LogInformation("유사도 검색 완료. {Count}개 결과 반환.", finalResults.Count);
        return finalResults;
    }

    // DeleteDocumentAsync 메서드 (이전과 동일)
    public async Task<bool> DeleteDocumentAsync(string documentId)
    {
        try
        {
            int affectedRows = await _dbContext.VectorDataEntries
                .Where(v => v.Id == documentId)
                .ExecuteDeleteAsync(); // EF Core 7+ 필요

            bool deleted = affectedRows > 0;
            if (deleted) _logger.LogInformation("문서 ID '{DocumentId}' 삭제 성공.", documentId);
            else _logger.LogWarning("문서 ID '{DocumentId}'를 찾을 수 없어 삭제 실패.", documentId);
            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "문서 ID '{DocumentId}' 삭제 중 오류 발생.", documentId);
            return false;
        }
    }

    // DeleteDocumentsByMetadataAsync 메서드 (이전과 동일)
    public async Task DeleteDocumentsByMetadataAsync(string key, object value)
    {
        _logger.LogWarning("메타데이터 기반 삭제는 MSSQL에서 비효율적일 수 있습니다. (JSON 파싱 필요)");
        string? valueStr = value?.ToString();
        if (string.IsNullOrEmpty(valueStr))
        {
            _logger.LogError("메타데이터 삭제 값({Key})이 null이거나 비어있습니다.", key);
            return;
        }
        try
        {
            var candidates = await _dbContext.VectorDataEntries.AsNoTracking()
                                             .Where(v => v.MetadataJson != null)
                                             .Select(v => new { v.Id, v.MetadataJson })
                                             .ToListAsync();
            var idsToDelete = new List<string>();
            foreach (var item in candidates)
            {
                try
                {
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(item.MetadataJson!);
                    if (metadata != null &&
                        metadata.TryGetValue(key, out var metaValue) &&
                        metaValue?.ToString()?.Equals(valueStr, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        idsToDelete.Add(item.Id);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "메타데이터 JSON 파싱 오류 (ID: {Id}). 건너<0xEB><0x9A><0x8E>니다.", item.Id);
                }
            }
            if (!idsToDelete.Any()) { _logger.LogInformation("메타데이터 '{Key}' = '{Value}' 조건에 맞는 문서 없음.", key, valueStr); return; }

            int deletedCount = await _dbContext.VectorDataEntries
                .Where(v => idsToDelete.Contains(v.Id))
                .ExecuteDeleteAsync(); // EF Core 7+ 필요
            _logger.LogInformation("메타데이터 '{Key}' = '{Value}' 조건 문서 {Count}개 삭제 완료.", key, valueStr, deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "메타데이터 '{Key}' = '{Value}' 기반 삭제 중 오류 발생.", key, valueStr);
            throw;
        }
    }

    // GetDocumentCountAsync 메서드 (이전과 동일)
    public Task<int> GetDocumentCountAsync()
    {
        return _dbContext.VectorDataEntries.CountAsync();
    }

    // GetDocumentCountAsync(int conventionId) 메서드 (이전과 동일)
    public Task<int> GetDocumentCountAsync(int conventionId)
    {
        return _dbContext.VectorDataEntries
               .CountAsync(v => v.ConventionId == conventionId);
    }
}