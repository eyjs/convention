using LocalRAG.DTOs.ChatModels;

namespace LocalRAG.Interfaces;

public interface IUserContextFactory
{
    ChatUserContext? CreateUserContext();
}
