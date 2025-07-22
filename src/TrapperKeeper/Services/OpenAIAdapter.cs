namespace TrapperKeeper.Services
{
    public static class OpenAIAdapter
    {
        public class OpenAIMessage
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }

        public static IEnumerable<OpenAIMessage> ConvertMessages(IEnumerable<ChatMessage> messages)
        {
            return messages.Select(message => new OpenAIMessage
            {
                Role = message switch
                {
                    UserChatMessage _ => "user",
                    AssistantChatMessage _ => "assistant",
                    _ => "system"
                },
                Content = message.Content
            });
        }
    }
}