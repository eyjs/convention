namespace LocalRAG.Entities
{
    /// <summary>
    /// 행사별 속성 정의 (버스 호차, 티셔츠 사이즈 등)
    /// </summary>
    public class AttributeDefinition
    {
        public int Id { get; set; }
        public int ConventionId { get; set; }
        
        /// <summary>
        /// 속성 키 (예: "버스", "티셔츠", "방번호")
        /// </summary>
        public string AttributeKey { get; set; } = string.Empty;
        
        /// <summary>
        /// 속성 값 옵션들 (JSON 배열: ["1호차", "2호차", "3호차"])
        /// </summary>
        public string? Options { get; set; }
        
        /// <summary>
        /// 정렬 순서
        /// </summary>
        public int OrderNum { get; set; }
        
        /// <summary>
        /// 필수 여부
        /// </summary>
        public bool IsRequired { get; set; }
        
        public Convention? Convention { get; set; }
    }
}
