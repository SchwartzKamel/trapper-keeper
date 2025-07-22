using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Moq;
using TrapperKeeper.Controllers;

namespace TrapperKeeper.Tests.Controllers
{
    public class ChatControllerTests : IAsyncDisposable
    {
        private readonly Mock<IConversationStore> _mockStore;
        private readonly Mock<BlobServiceClient> _mockBlobService;
        private readonly Mock<IHostEnvironment> _mockEnv;
        private readonly ChatController _controller;

        public ChatControllerTests()
        {
            _mockEnv = new Mock<IHostEnvironment>();
            _mockEnv.Setup(e => e.ContentRootPath).Returns("test/path");

            _mockStore = new Mock<IConversationStore>();
            _mockBlobService = new Mock<BlobServiceClient>();
            _controller = new ChatController(_mockStore.Object, _mockBlobService.Object);
        }

        public async ValueTask DisposeAsync()
        {
            if (_controller != null)
            {
                // Dispose controller resources if needed
            }
            await Task.CompletedTask;
        }

        [Fact]
        public async Task GetConversation_ReturnsNotFound_WhenInvalidId()
        {
            // Arrange
            var invalidId = "invalid-guid-format";

            // Act
            var result = await _controller.GetConversation(invalidId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task StartNewConversation_ReturnsCreatedResult_WithNewConversation()
        {
            // Arrange
            Conversation createdConversation = null;
            _mockStore.Setup(x => x.CreateConversation(It.IsAny<Conversation>()))
                .Callback<Conversation>(c => createdConversation = c)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.StartNewConversation();

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedConv = (Conversation)createdResult.Value;
            Assert.NotEqual(Guid.Empty, returnedConv.Id);
            Assert.InRange(returnedConv.Timestamp, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            _mockStore.Verify(x => x.CreateConversation(newConversation), Times.Once);
        }
    }
}