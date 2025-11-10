namespace LocalRAG.DTOs.FormBuilder
{
    public class FormSubmissionRequestDto
    {
        public string FormDataJson { get; set; } = string.Empty;
        public string? FileFieldKey { get; set; }
        public IFormFile? File { get; set; } // 파일 속성 추가
    }
}
