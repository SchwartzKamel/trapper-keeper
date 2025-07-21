using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using TrapperKeeper;
using OpenAI.Chat;

namespace TrapperKeeper.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        public class MessageRequest
        {
            public required string Content { get; set; }
        }

        private readonly IConversationStore _store;
        private readonly BlobServiceClient _blobClient;

        public ChatController(IConversationStore conversationStore, BlobServiceClient blobClient)
        {
            _store = conversationStore;
            _blobClient = blobClient;
        }

        [HttpPost]
        public async Task<IActionResult> StartNewConversation()
        {
            var conversation = new Conversation();
            await _store.CreateConversation(conversation);
            return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversation);
        }

        [HttpPost("{id}/messages")]
        public async Task<IActionResult> AddMessage(Guid id, [FromBody] MessageRequest request)
        {
            var conversation = await _store.GetConversation(id);
            conversation.Messages.Add(new UserChatMessage(request.Content));
            
            // Process message with AI logic here
            
            await _store.UpdateConversation(conversation);
            return Ok(conversation.Messages.Last());
        }

        [HttpGet("{id?}")]
        public async Task<IActionResult> GetConversation(string? id = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                var newConversation = new Conversation();
                await _store.CreateConversation(newConversation);
                return Ok(new
                {
                    Id = newConversation.Id,
                    Messages = newConversation.Messages,
                    SystemMessage = $"SYSTEM> New conversation created: {newConversation.Id}\nType '/exit' to end, '/save' to save"
                });
            }

            if (!Guid.TryParse(id, out var conversationId))
            {
                return BadRequest("Invalid conversation ID format");
            }
            
            var conversation = await _store.GetConversation(conversationId);
            return conversation == null
                ? NotFound()
                : Ok(new
                {
                    Id = conversation.Id,
                    Messages = conversation.Messages,
                    SystemMessage = $"SYSTEM> Resuming conversation: {conversation.Id}"
                });
        }
    }
}