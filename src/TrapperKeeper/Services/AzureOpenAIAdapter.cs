using Azure;

namespace TrapperKeeper.Services
{
    public class AzureOpenAIAdapter : IAIModelAdapter
    {
        private readonly OpenAIClient _client;
        private readonly string _deployment;

        public AzureOpenAIAdapter(OpenAIClient client, string deployment)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _deployment = deployment ?? throw new ArgumentNullException(nameof(deployment));
        }

        public async Task<string> GenerateResponseAsync(IEnumerable<ChatMessage> messages)
        {
            var options = new ChatCompletionsOptions(
                messages.Select(m => 
                    new ChatRequestUserMessage(m.Content)).ToList()
            )
            {
                MaxTokens = 800,
                Temperature = 0.7f
            };

            try
            {
                Response<ChatCompletions> response = await _client.GetChatCompletionsAsync(
                    _deployment,
                    options);

                return response.Value.Choices[0].Message.Content;
            }
            catch (RequestFailedException ex)
            {
                // Handle Azure-specific errors
                throw new AIServiceException("Azure OpenAI request failed", ex);
            }
        }
    }

    public class AIServiceException : Exception
    {
        public AIServiceException(string message, Exception inner) 
            : base(message, inner) { }
    }
}