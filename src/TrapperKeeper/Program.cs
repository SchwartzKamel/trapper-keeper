// Warnings
#pragma warning disable CS8600 // User Input

// Namespace
using TrapperKeeper;
using TrapperKeeper.Services;

// Imports
using Microsoft.Extensions.Configuration.Yaml;
using Azure.Storage.Blobs;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

public static class Program
{

    public static async Task Main(string[] args)
    {
        var root = Directory.GetCurrentDirectory();
        var dotenv = Path.Combine(root, ".env");
        DotEnv.Load(dotenv);

        // Other code
        var config =
            new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddYamlFile("appsettings.yaml")
                .Build();

        var endpoint = new Uri(config["azure:endpoint"] ?? throw new InvalidOperationException("Missing azure:endpoint in configuration"));
        var deploymentName = config["azure:deployment"] ?? throw new InvalidOperationException("Missing azure:deployment in configuration");
        var apiKey = config["azure:apiKey"] ?? throw new InvalidOperationException("Missing azure:apiKey in configuration");
        var storageConnectionString = config["azure:storage:connectionString"] ?? throw new InvalidOperationException("Missing azure:storage:connectionString in configuration");

        AzureOpenAIClient azureClient = new(
            endpoint,
            new AzureKeyCredential(apiKey));

        var blobServiceClient = new BlobServiceClient(storageConnectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient("attachments");
        await containerClient.CreateIfNotExistsAsync();

        ChatClient chatClient = azureClient.GetChatClient(deploymentName);

        if (!args.Contains("--console"))
        {
            // Web Application Setup
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Trapper Keeper API", Version = "v1" });
            });
            builder.Services.AddSingleton(containerClient);
            builder.Services.AddSingleton<IConversationStore, JsonConversationStore>();
            // Configure Azure OpenAI client
            var openAIClient = new OpenAIClient(
                new Uri(config["azure:endpoint"]),
                new AzureKeyCredential(config["azure:apiKey"]));
            
            // Register services
            builder.Services.AddSingleton(openAIClient);
            builder.Services.AddSingleton<IAIModelAdapter>(provider =>
                new AzureOpenAIAdapter(
                    provider.GetRequiredService<OpenAIClient>(),
                    config["azure:deployment"]));
            builder.Services.AddScoped<IChatCompletionService, AIChatService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.MapControllers();
            app.Run();
        }
        else
        {
            // Console Mode
            var requestOptions = new ChatCompletionOptions()
            {
                MaxOutputTokenCount = 800,
                Temperature = 1.0f,
                TopP = 1.0f,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f,
            };

            List<ChatMessage> messages =
            [
                new SystemChatMessage("You are a helpful assistant that talks like a pirate."),
                new UserChatMessage("Hi, can you help me?"),
                new AssistantChatMessage("Arrr! Of course, me hearty! What can I do for ye?"),
            ];

            var conversation = new Conversation();
            conversation.Messages.AddRange(messages);

            while (true)
            {
                Console.WriteLine("What be yer query matey? (Enter 'quit' to exit)");
                string user_input = Console.ReadLine();

                if (user_input?.ToLower() == "quit")
                    break;

                conversation.Messages.Add(new UserChatMessage(user_input));
                ChatCompletion completion = chatClient.CompleteChat(messages, requestOptions);
                var response = completion.Content[0].Text;
                messages.Add(new AssistantChatMessage(response));
                conversation.MessageTimestamps[messages.Count - 1] = DateTime.UtcNow;

                conversation.Messages.AddRange(messages);
                await new JsonConversationStore().SaveAsync(conversation);

                Console.WriteLine("\nConversation History:");
                foreach (var msg in messages.Where(m => m is UserChatMessage || m is AssistantChatMessage))
                {
                    int index = messages.IndexOf(msg);
                    var timestamp = conversation.MessageTimestamps.TryGetValue(index, out var ts)
                        ? ts.ToString("HH:mm")
                        : "PRE:FED";
                    Console.WriteLine($"[{timestamp}] {msg.GetType().Name.Replace("ChatMessage", "")}: {msg.Content[0].Text}");
                }
                Console.WriteLine();
            }
        }
    }
}