namespace TrapperKeeper.Services
{
    public interface IAIModelAdapter
    {
        Task<string> GenerateResponseAsync(IEnumerable<ChatMessage> messages);
    }
}