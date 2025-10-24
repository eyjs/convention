// Models/VectorDataEntry.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LocalRAG.Entities;

public class VectorDataEntry
{
    [Key]
    [MaxLength(36)] // Guid 문자열 길이
    public string Id { get; set; } = Guid.NewGuid().ToString(); // ID 자동 생성

    [Required]
    public int ConventionId { get; set; } // 행사 ID (필터링용)

    [Required]
    [MaxLength(50)]
    public string SourceType { get; set; } = "Convention"; // 데이터 소스 타입 (Convention, Guest, Schedule, Notice 등)

    [Required]
    public string Content { get; set; } = string.Empty; // 원본 텍스트

    [Required]
    public byte[] EmbeddingData { get; set; } = Array.Empty<byte>(); // 벡터 임베딩 (byte 배열)

    public string? MetadataJson { get; set; } // 메타데이터 (JSON 직렬화)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 생성 시간
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // 수정 시간

    [NotMapped] // DB 컬럼 아님
    public float[] Embedding
    {
        get => ConvertByteArrayToFloatArray(EmbeddingData);
        set => EmbeddingData = ConvertFloatArrayToByteArray(value);
    }

    // --- 바이트 배열 <-> Float 배열 변환 헬퍼 ---
    public static byte[] ConvertFloatArrayToByteArray(float[] floatArray)
    {
        if (floatArray == null || floatArray.Length == 0) return Array.Empty<byte>();
        var byteArray = new byte[floatArray.Length * sizeof(float)];
        Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    public static float[] ConvertByteArrayToFloatArray(byte[] byteArray)
    {
        if (byteArray == null || byteArray.Length == 0 || byteArray.Length % sizeof(float) != 0) return Array.Empty<float>();
        var floatArray = new float[byteArray.Length / sizeof(float)];
        Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
        return floatArray;
    }
}