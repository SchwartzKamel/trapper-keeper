using Azure;
using OpenAI;

namespace TrapperKeeper.Services
{
    public interface IOpenAIFacade
    {
        Task<string> GetChatResponseAsync(IEnumerable<ChatMessage> messages);
    }

    public class AzureOpenAIFacade : IOpenAIFacade
    {
        private readonly OpenAIClient _client;
        private readonly string _deploymentName;

        public AzureOpenAIFacade(OpenAIClient client, string deploymentName)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _deploymentName = deploymentName ?? throw new ArgumentNullException(nameof(deploymentName));
        }

        public async Task<string> GetChatResponseAsync(IEnumerable<ChatMessage> messages)
        {
            var options = new ChatCompletionsOptions();
            
            foreach (var message in messages)
            {
                options.AddMessage(new ChatRole(message.Role), message.Content);
            }

            Response<ChatCompletions> response = await _client.GetChatCompletionsAsync(
                _deploymentName,
                options);

            return response.Value.Choices[0].Message.Content;
        }
    }
}