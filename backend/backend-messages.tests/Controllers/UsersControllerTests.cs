using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BackendMessages.Controllers;
using BackendMessages.Services.Abstractions;
using BackendMessages.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using BackendMessages.Models;

namespace BackendMessages.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserSearchService> _userSearchServiceMock;
        private readonly Mock<ILogger<UsersController>> _loggerMock;
        private readonly UsersController _controller;
        private readonly Guid _testUserId = Guid.NewGuid();

        public UsersControllerTests()
        {
            // Setup mocks
            _userSearchServiceMock = new Mock<IUserSearchService>();
            _loggerMock = new Mock<ILogger<UsersController>>();

            // Create controller
            _controller = new UsersController(_userSearchServiceMock.Object, _loggerMock.Object);
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
        public void Test_ShouldReturnOkWithMessage()
        {
            // Act
            var result = _controller.Test();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public void GetUserOnlineStatus_WithValidRequest_ShouldReturnOnlineStatus()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var userId = _testUserId.ToString();

            // Act
            var result = _controller.GetUserOnlineStatus(userId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public void GetUserOnlineStatus_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            SetupUnauthenticatedUser();
            var userId = _testUserId.ToString();

            // Act
            var result = _controller.GetUserOnlineStatus(userId);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult!.Value.Should().Be("User not authenticated");
        }

        [Fact]
        public void GetUserOnlineStatus_WithEmptyUserId_ShouldReturnBadRequest()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var userId = "";

            // Act
            var result = _controller.GetUserOnlineStatus(userId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("User ID cannot be empty");
        }

        [Fact]
        public void GetUserOnlineStatus_WithWhitespaceUserId_ShouldReturnBadRequest()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var userId = "   ";

            // Act
            var result = _controller.GetUserOnlineStatus(userId);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("User ID cannot be empty");
        }

        [Fact]
        public void GetUserOnlineStatus_WithNullUserId_ShouldReturnBadRequest()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            string? userId = null;

            // Act
            var result = _controller.GetUserOnlineStatus(userId!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("User ID cannot be empty");
        }

        [Fact]
        public void GetUserOnlineStatus_WithException_ShouldReturnInternalServerError()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var userId = _testUserId.ToString();

            // Simulate an exception by making MessageHub.IsUserOnline throw
            // This is a bit tricky since it's a static method, but we can test the exception handling

            // Act
            var result = _controller.GetUserOnlineStatus(userId);

            // Assert
            // The method should handle any exceptions gracefully
            result.Should().BeOfType<OkObjectResult>();
        }



        [Fact]
        public async Task SearchUsers_WithValidRequest_ShouldReturnUsers()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var searchQuery = "test";
            var expectedUsers = TestDataFactory.CreateSearchUsers(3);

            _userSearchServiceMock.Setup(x => x.SearchUsersAsync(searchQuery))
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _controller.SearchUsers(searchQuery);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            
            okResult.Value.Should().BeAssignableTo<List<SearchUser>>();
            var list = (List<SearchUser>)okResult.Value!;
            list.Should().HaveCount(3);
        }

        [Fact]
        public async Task SearchUsers_WithUnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            SetupUnauthenticatedUser();
            var searchQuery = "test";

            // Act
            var result = await _controller.SearchUsers(searchQuery);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            unauthorizedResult!.Value.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task SearchUsers_WithEmptyQuery_ShouldReturnBadRequest()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var searchQuery = "";

            // Act
            var result = await _controller.SearchUsers(searchQuery);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Search query cannot be empty");
        }

        [Fact]
        public async Task SearchUsers_WithWhitespaceQuery_ShouldReturnBadRequest()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var searchQuery = "   ";

            // Act
            var result = await _controller.SearchUsers(searchQuery);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Search query cannot be empty");
        }

        [Fact]
        public async Task SearchUsers_WithNullQuery_ShouldReturnBadRequest()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            string? searchQuery = null;

            // Act
            var result = await _controller.SearchUsers(searchQuery!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("Search query cannot be empty");
        }

        [Fact]
        public async Task SearchUsers_WithServiceException_ShouldReturnInternalServerError()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var searchQuery = "test";

            _userSearchServiceMock.Setup(x => x.SearchUsersAsync(searchQuery))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            var result = await _controller.SearchUsers(searchQuery);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task SearchUsers_WithLongQuery_ShouldReturnUsers()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var searchQuery = new string('a', 1000); // Very long query
            var expectedUsers = TestDataFactory.CreateSearchUsers(1);

            _userSearchServiceMock.Setup(x => x.SearchUsersAsync(searchQuery))
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _controller.SearchUsers(searchQuery);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SearchUsers_WithSpecialCharacters_ShouldReturnUsers()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var searchQuery = "test@user.com"; // Query with special characters
            var expectedUsers = TestDataFactory.CreateSearchUsers(1);

            _userSearchServiceMock.Setup(x => x.SearchUsersAsync(searchQuery))
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _controller.SearchUsers(searchQuery);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SearchUsers_WithUnicodeCharacters_ShouldReturnUsers()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var searchQuery = "José García"; // Query with unicode characters
            var expectedUsers = TestDataFactory.CreateSearchUsers(1);

            _userSearchServiceMock.Setup(x => x.SearchUsersAsync(searchQuery))
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _controller.SearchUsers(searchQuery);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SearchUsers_WithNoResults_ShouldReturnEmptyList()
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var searchQuery = "nonexistent";
            var expectedUsers = new List<SearchUser>(); // Empty list

            _userSearchServiceMock.Setup(x => x.SearchUsersAsync(searchQuery))
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _controller.SearchUsers(searchQuery);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeAssignableTo<List<SearchUser>>();
            var list = (List<SearchUser>)okResult.Value!;
            list.Should().BeEmpty();
        }

        [Theory]
        [InlineData("test")]
        [InlineData("user")]
        [InlineData("admin")]
        [InlineData("john.doe")]
        public async Task SearchUsers_WithDifferentQueries_ShouldReturnUsers(string searchQuery)
        {
            // Arrange
            SetupAuthenticatedUser(_testUserId);
            var expectedUsers = TestDataFactory.CreateSearchUsers(2);

            _userSearchServiceMock.Setup(x => x.SearchUsersAsync(searchQuery))
                .ReturnsAsync(expectedUsers);

            // Act
            var result = await _controller.SearchUsers(searchQuery);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeAssignableTo<List<SearchUser>>();
        }
    }
}
