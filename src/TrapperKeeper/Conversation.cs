namespace TrapperKeeper;

public class Conversation
{
    public Guid Id { get; } = Guid.NewGuid();
    public List<ChatMessage> Messages { get; } = new();
    public List<FileAttachment> Attachments { get; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<int, DateTime> MessageTimestamps { get; } = new();

    public Conversation() { }
}