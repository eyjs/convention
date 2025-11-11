namespace LocalRAG.DTOs.UserModels;

public class UpdateUserProfileDto
{
    /// <summary>
    /// 연락처
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 영문 이름 (First Name)
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// 영문 성 (Last Name)
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// 여권 번호
    /// </summary>
    public string? PassportNumber { get; set; }

    /// <summary>
    /// 여권 만료일 (yyyy-MM-dd)
    /// </summary>
    public string? PassportExpiryDate { get; set; }

    /// <summary>
    /// 소속 정보
    /// </summary>
    public string? Affiliation { get; set; }
}
