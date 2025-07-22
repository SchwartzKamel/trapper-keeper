using System.Text.Json;

namespace TrapperKeeper;

public interface IConversationStore
{
    Task CreateConversation(Conversation conversation);
    Task SaveAsync(Conversation conversation);
    Task<Conversation> LoadAsync(Guid id);
    Task<Conversation> GetConversation(Guid id);
    Task UpdateConversation(Conversation conversation);
    void CreateConversation(object newConversation);
}

public class JsonConversationStore : IConversationStore
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    public async Task CreateConversation(Conversation conversation)
    {
        Directory.CreateDirectory("conversations");
        var path = $"conversations/{conversation.Id}.json";
        await File.WriteAllTextAsync(path,
            JsonSerializer.Serialize(conversation, _jsonOptions));
    }

    public async Task SaveAsync(Conversation conversation)
    {
        Directory.CreateDirectory("conversations");
        var path = $"conversations/{conversation.Id}.json";
        await File.WriteAllTextAsync(path,
            JsonSerializer.Serialize(conversation, _jsonOptions));
    }

    public async Task<Conversation> LoadAsync(Guid id)
    {
        var path = $"conversations/{id}.json";
        if (File.Exists(path))
        {
            var content = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<Conversation>(content) ?? new Conversation();
        }
        return new Conversation();
    }

    public async Task<Conversation> GetConversation(Guid id)
    {
        return await LoadAsync(id);
    }

    public async Task UpdateConversation(Conversation conversation)
    {
        await SaveAsync(conversation);
    }

    public void CreateConversation(object newConversation)
    {
        throw new NotImplementedException();
    }
}