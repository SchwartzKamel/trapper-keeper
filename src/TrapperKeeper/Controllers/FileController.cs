using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;

namespace TrapperKeeper.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly JsonConversationStore _store;
    private readonly BlobContainerClient _containerClient;

    public FileController(JsonConversationStore store, BlobContainerClient containerClient)
    {
        _store = store;
        _containerClient = containerClient;
    }

    [HttpPost("upload/{sessionId}")]
    public async Task<IActionResult> UploadAttachment(Guid sessionId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Invalid file upload");

        if (file.Length > 10_000_000)
            return BadRequest("File size exceeds 10MB limit");

        var blobName = $"{sessionId}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        try
        {
            using var stream = file.OpenReadStream();
            await _containerClient.UploadBlobAsync(blobName, stream);

            var attachment = new FileAttachment
            {
                Id = Guid.NewGuid().ToString("N"),
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length,
                StorageUrl = $"{_containerClient.Uri}/{blobName}",
                UploadedAt = DateTime.UtcNow
            };

            var conversation = await _store.LoadAsync(sessionId);
            conversation.Attachments.Add(attachment);
            await _store.SaveAsync(conversation);

            return Created($"/api/attachments/{attachment.Id}", attachment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"File upload failed: {ex.Message}");
        }
    }
}