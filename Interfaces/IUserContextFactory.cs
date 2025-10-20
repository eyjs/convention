using LocalRAG.Models;

namespace LocalRAG.Interfaces;

public interface IUserContextFactory
{
    ChatUserContext? CreateUserContext();
}
