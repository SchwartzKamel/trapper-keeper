namespace TrapperKeeper.Services
{
    public interface IChatCompletionService
    {
        Task<string> GetResponseAsync(IEnumerable<ChatMessage> messages);
    }
}