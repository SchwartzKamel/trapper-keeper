using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TrapperKeeper.Services;

namespace TrapperKeeper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        public class MessageRequest
        {
            [Required]
            [StringLength(1000, MinimumLength = 1)]
            public required string Content { get; set; }
        }

        private readonly IConversationStore _store;
        private readonly IChatCompletionService _aiService;

        public ChatController(IConversationStore conversationStore, IChatCompletionService aiService)
        {
            _store = conversationStore;
            _aiService = aiService;
        }

        [HttpPost]
        public async Task<IActionResult> StartNewConversation()
        {
            var conversation = new Conversation();
            await _store.CreateConversation(conversation);
            return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, conversation);
        }

        [HttpPost("{id}/messages")]
        public async Task<IActionResult> AddMessage(
            [FromRoute] Guid id,
            [FromBody] MessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var conversation = await _store.GetConversation(id);
            if (conversation is null)
            {
                return NotFound($"Conversation {id} not found");
            }

            var userMessage = new UserChatMessage(request.Content);
            conversation.Messages.Add(userMessage);

            // Generate AI response
            var aiResponse = await _aiService.GetResponseAsync(conversation.Messages);
            conversation.Messages.Add(new AssistantChatMessage(aiResponse));

            await _store.UpdateConversation(conversation);

            return Ok(new
            {
                UserMessage = userMessage,
                AssistantResponse = aiResponse,
                ConversationId = conversation.Id
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConversation([FromRoute] Guid id)
        {
            var conversation = await _store.GetConversation(id);
            if (conversation is null)
            {
                return NotFound($"Conversation {id} not found");
            }

            return Ok(new ConversationResponse(
                conversation.Id,
                conversation.Messages,
                $"SYSTEM> Active conversation: {id}"
            ));
        }
    }

    public record ConversationResponse(
        Guid Id,
        List<ChatMessage> Messages,
        string SystemMessage
    );
}