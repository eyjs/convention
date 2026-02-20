namespace LocalRAG.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Guest = "Guest";

    public static readonly string[] All = [Admin, User, Guest];

    public static bool IsValid(string role) => All.Contains(role);
}

public static class DeleteStatus
{
    /// <summary>
    /// Convention 엔티티용: 활성 상태
    /// </summary>
    public const string Active = "N";

    /// <summary>
    /// Convention 엔티티용: 삭제 상태
    /// </summary>
    public const string Deleted = "Y";

    /// <summary>
    /// SmsTemplate, Notice 등: 활성 상태 (숫자형)
    /// </summary>
    public const string ActiveNumeric = "2";

    /// <summary>
    /// SmsTemplate, Notice 등: 삭제 상태 (숫자형)
    /// </summary>
    public const string DeletedNumeric = "1";
}
