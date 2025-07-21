using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Moq;
using TrapperKeeper;
using TrapperKeeper.Controllers;
using Xunit;
using Moq.Protected;

namespace TrapperKeeper.Tests.Controllers
{
    public class ChatControllerTests : IAsyncDisposable
    {
        private readonly Mock<JsonConversationStore> _mockStore;
        private readonly Mock<BlobServiceClient> _mockBlobService;
        private readonly Mock<IHostEnvironment> _mockEnv;
        private readonly ChatController _controller;

        public ChatControllerTests()
        {
            _mockEnv = new Mock<IHostEnvironment>();
            _mockEnv.Setup(e => e.ContentRootPath).Returns("test/path");
            
            _mockStore = new Mock<JsonConversationStore>(_mockEnv.Object);
            _mockBlobService = new Mock<BlobServiceClient>();
            _controller = new ChatController(_mockStore.Object, _mockBlobService.Object);
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
            var newConversation = new Conversation();
            _mockStore.Setup(x => x.CreateConversation(newConversation))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.StartNewConversation();

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(newConversation, createdResult.Value);
            _mockStore.Verify(x => x.CreateConversation(newConversation), Times.Once);
        }
    }
}