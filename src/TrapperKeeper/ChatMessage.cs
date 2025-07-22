namespace TrapperKeeper
{
    public abstract record ChatMessage(
        string Content,
        DateTimeOffset Timestamp
    );
    
    public record UserChatMessage(string Content)
        : ChatMessage(Content, DateTimeOffset.UtcNow);
    
    public record AssistantChatMessage(string Content)
        : ChatMessage(Content, DateTimeOffset.UtcNow);
}