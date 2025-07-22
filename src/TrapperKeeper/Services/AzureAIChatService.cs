using Azure;
using OpenAI;

namespace TrapperKeeper.Services
{
    /// <summary>
    /// Provides Azure OpenAI chat completion services
    /// </summary>
    public class AzureAIChatService : IChatCompletionService
    {
        private readonly OpenAIClient _azureClient;
        private readonly string _deploymentName;
        private readonly ILogger<AzureAIChatService> _logger;

        /// <summary>
        /// Initializes a new instance of the AzureAIChatService
        /// </summary>
        public AzureAIChatService(
            OpenAIClient azureClient,
            string deploymentName,
            ILogger<AzureAIChatService> logger)
        {
            _azureClient = azureClient ?? throw new ArgumentNullException(nameof(azureClient));
            _deploymentName = deploymentName ?? throw new ArgumentNullException(nameof(deploymentName));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public class AzureAIServiceException : Exception
        {
            public AzureAIServiceException(string message, Exception innerException)
                : base(message, innerException) { }
        }

        public async Task<string> GetResponseAsync(IEnumerable<ChatMessage> messages)
        {
            var options = new ChatCompletionsOptions()
            {
                DeploymentName = _deploymentName
            };

            foreach (var message in messages)
            {
                options.Messages.Add(new Azure.AI.OpenAI.ChatRequestMessage(
                    message.Role switch
                    {
                        "System" => ChatRole.System,
                        "User" => ChatRole.User,
                        "Assistant" => ChatRole.Assistant,
                        _ => ChatRole.User
                    },
                    message.Content
                ));
            }

            Response<ChatCompletions> response = await _client.GetChatCompletionsAsync(options);
            return response.Value.Choices[0].Message.Content;
        }
    }
}