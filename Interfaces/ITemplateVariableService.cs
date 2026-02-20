using LocalRAG.Services.Shared.Models;

namespace LocalRAG.Interfaces;

public interface ITemplateVariableService
{
    /// <summary>
    /// 템플릿 내의 변수를 컨텍스트 데이터로 치환합니다.
    /// </summary>
    /// <param name="templateContent">원본 템플릿 내용</param>
    /// <param name="context">가공된 템플릿 컨텍스트</param>
    /// <returns>치환된 문자열</returns>
    string ReplaceVariables(string templateContent, SmsTemplateContext context);
}
