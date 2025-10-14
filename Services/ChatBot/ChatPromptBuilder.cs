using System.Text;

namespace LocalRAG.Services.ChatBot
{
    public class ChatPromptBuilder
    {
        public string BuildPrompt(string question, string? context = null, string? userRole = null)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(context))
                sb.AppendLine($"[행사 문맥]\n{context}\n");

            if (!string.IsNullOrEmpty(userRole))
                sb.AppendLine($"[사용자 역할: {userRole}]\n");

            sb.AppendLine($"[사용자 질문]\n{question}");

            return sb.ToString();
        }
    }
}
