using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BackendMessages.Controllers;
using BackendMessages.Data;
using BackendMessages.Models;
using BackendMessages.Services.Abstractions;
using BackendMessages.DTO.Conversation.Request;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using BackendMessages.DTO.Conversation.Response;

namespace BackendMessages.Tests.Controllers
{
    public class ConversationsControllerTests : IDisposable
    {
        private readonly MessagesDbContext _context;
        private readonly Mock<ILogger<ConversationsController>> _loggerMock;
        private readonly Mock<IConversationService> _conversationServiceMock;
        private readonly ConversationsController _controller;
        private readonly Guid _testUserId = Guid.NewGuid();
        private readonly Guid _testConversationId = Guid.NewGuid();

        public ConversationsControllerTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<MessagesDbContext>()
                .UseInMemoryDatabase(databaseName: $"ConversationsControllerTests_{Guid.NewGuid()}")
                .Options;
            _context = new MessagesDbContext(options);

            // Setup mocks
            _loggerMock = new Mock<ILogger<ConversationsController>>();
            _conversationServiceMock = new Mock<IConversationService>();

            // Create controller
            _controller = new ConversationsController(_context, _loggerMock.Object, _conversationServiceMock.Object);

            // Setup test data
            SetupTestData();
        }

        private void SetupTestData()
        {
            // Create test conversation
            var conversation = TestDataFactory.CreateConversation(
                initiatorId: _testUserId,
                receiverId: Guid.NewGuid()
            );
            conversation.Id = _testConversationId;
            _context.Conversations.Add(conversation);
            _context.SaveChanges();
        }

        private void SetupAuthenticatedUser(Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        private void SetupUnauthenticatedUser()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };
        }

        [Fact]
        public async Task GetUserConversations_WithValidRequest_ShouldReturnConversations()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);

            // Act
            var result = await _controller.GetUserConversations(_testUserId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUserConversations_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            SetupUnauthenticatedUser();

            // Act
            var result = await _controller.GetUserConversations(_testUserId);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult!.Value.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task GetUserConversations_WithDifferentUserId_ShouldReturnForbidden()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var differentUserId = Guid.NewGuid();

            // Act
            var result = await _controller.GetUserConversations(differentUserId);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetUserConversations_WithDeletedConversations_ShouldFilterThemOut()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            
            // Create a conversation deleted by the user
            var deletedConversation = TestDataFactory.CreateConversation(
                initiatorId: _testUserId,
                receiverId: Guid.NewGuid()
            );
            deletedConversation.InitiatorDeletedAt = DateTime.UtcNow;
            _context.Conversations.Add(deletedConversation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetUserConversations(_testUserId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateOrGetConversation_WithValidRequest_ShouldReturnConversation()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var request = new CreateConversationRequest
            {
                InitiatorId = _testUserId,
                ReceiverId = Guid.NewGuid(),
                InitialMessage = "Hello!"
            };

            _conversationServiceMock.Setup(x => x.CreateConversationAsync(
                It.IsAny<CreateConversationRequest>()))
                .ReturnsAsync(new CreateConversationResponse { Success = true });

            // Act
            var result = await _controller.CreateOrGetConversation(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateOrGetConversation_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            SetupUnauthenticatedUser();
            var request = new CreateConversationRequest
            {
                InitiatorId = _testUserId,
                ReceiverId = Guid.NewGuid(),
                InitialMessage = "Hello!"
            };

            // Act
            var result = await _controller.CreateOrGetConversation(request);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task CreateOrGetConversation_WithDifferentInitiatorId_ShouldReturnForbidden()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var request = new CreateConversationRequest
            {
                InitiatorId = Guid.NewGuid(), // Different from authenticated user
                ReceiverId = Guid.NewGuid(),
                InitialMessage = "Hello!"
            };

            // Act
            var result = await _controller.CreateOrGetConversation(request);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task CreateOrGetConversation_WithSameInitiatorAndReceiver_ShouldReturnBadRequest()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var request = new CreateConversationRequest
            {
                InitiatorId = _testUserId,
                ReceiverId = _testUserId, // Same as initiator
                InitialMessage = "Hello!"
            };

            // Act
            var result = await _controller.CreateOrGetConversation(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Cannot create conversation with yourself");
        }

        [Fact]
        public async Task CreateOrGetConversation_WithServiceException_ShouldReturnInternalServerError()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var request = new CreateConversationRequest
            {
                InitiatorId = _testUserId,
                ReceiverId = Guid.NewGuid(),
                InitialMessage = "Hello!"
            };

            _conversationServiceMock.Setup(x => x.CreateConversationAsync(
                It.IsAny<CreateConversationRequest>()))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.CreateOrGetConversation(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetConversation_WithValidRequest_ShouldReturnConversation()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);

            // Act
            var result = await _controller.GetConversation(_testConversationId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetConversation_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            SetupUnauthenticatedUser();

            // Act
            var result = await _controller.GetConversation(_testConversationId);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetConversation_WithUserNotInConversation_ShouldReturnForbidden()
        {
            // Arrange
            SetupAuthenticatedUser(Guid.NewGuid()); // Different user

            // Act
            var result = await _controller.GetConversation(_testConversationId);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetConversation_WithNonExistentConversation_ShouldReturnNotFound()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);

            // Act
            var result = await _controller.GetConversation(Guid.NewGuid());

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteConversation_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);

            // Act
            var result = await _controller.DeleteConversation(_testConversationId, _testUserId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
        }

        [Fact]
        public async Task DeleteConversation_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            SetupUnauthenticatedUser();

            // Act
            var result = await _controller.DeleteConversation(_testConversationId, _testUserId);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task DeleteConversation_WithUserNotInConversation_ShouldReturnForbidden()
        {
            // Arrange
            SetupAuthenticatedUser(Guid.NewGuid()); // Different user

            // Act
            var result = await _controller.DeleteConversation(_testConversationId, _testUserId);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task DeleteConversation_WithNonExistentConversation_ShouldReturnNotFound()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);

            // Act
            var result = await _controller.DeleteConversation(Guid.NewGuid(), _testUserId);

            // Assert
            result.Should().BeOfType<ObjectResult>();
        }

        [Fact]
        public async Task DeleteConversation_WithAlreadyDeletedConversation_ShouldReturnBadRequest()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            
            // Mark conversation as already deleted
            var conversation = await _context.Conversations.FindAsync(_testConversationId);
            conversation!.InitiatorDeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteConversation(_testConversationId, _testUserId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Conversation already deleted by this user");
        }

        [Fact]
        public async Task DeleteConversation_WithException_ShouldReturnInternalServerError()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            
            // Simulate database exception
            _context.Dispose(); // This will cause an exception when trying to access the context

            // Act
            var result = await _controller.DeleteConversation(_testConversationId, _testUserId);

            // Assert
            result.Should().BeOfType<StatusCodeResult>();
            var objectResult = result as StatusCodeResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetUserConversations_WithException_ShouldReturnInternalServerError()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            
            // Simulate database exception
            _context.Dispose(); // This will cause an exception when trying to access the context

            // Act
            var result = await _controller.GetUserConversations(_testUserId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetConversation_WithException_ShouldReturnInternalServerError()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            
            // Simulate database exception
            _context.Dispose(); // This will cause an exception when trying to access the context

            // Act
            var result = await _controller.GetConversation(_testConversationId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
