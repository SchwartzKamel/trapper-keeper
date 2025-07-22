using OpenAI;

namespace TrapperKeeper.Services
{
    public class AIChatService : IChatCompletionService
    {
        private readonly OpenAIClient _client;

        public AIChatService(OpenAIClient client)
        {
            _client = client;
        }

        public async Task<string> GetResponseAsync(IEnumerable<ChatMessage> messages)
        {
            var openAiMessages = OpenAIAdapter.ConvertMessages(messages)
                .Select(m => new OpenAI.Chat.ChatMessage(
                    new OpenAI.Chat.Role(m.Role),
                    m.Content
                ));

            var request = new ChatCompletionRequest(
                Messages: openAiMessages,
                Model: "gpt-4"
            );

            var response = await _client.CompleteChatAsync(request);
            return response.Choices.First().Message.Content;
        }
    }
}