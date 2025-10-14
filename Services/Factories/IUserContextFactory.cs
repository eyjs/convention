// Services/Factories/IUserContextFactory.cs
using LocalRAG.Models;

namespace LocalRAG.Services.Factories;

public interface IUserContextFactory
{
    ChatUserContext? CreateUserContext();
}