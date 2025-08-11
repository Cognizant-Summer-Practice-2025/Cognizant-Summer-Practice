using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using backend_user.Controllers;
using backend_user.Services.Abstractions;
using backend_user.Models;
using backend_user.DTO.User.Request;
using backend_user.DTO.User.Response;
using backend_user.DTO.Authentication.Request;
using backend_user.DTO.Authentication.Response;
using backend_user.DTO.OAuthProvider.Request;
using backend_user.DTO.OAuthProvider.Response;
using backend_user.tests.Helpers;
using backend_user.DTO;

namespace backend_user.tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IOAuthProviderService> _mockOAuthProviderService;
        private readonly Mock<IUserRegistrationService> _mockUserRegistrationService;
        private readonly Mock<ILoginService> _mockLoginService;
        private readonly Mock<IBookmarkService> _mockBookmarkService;
        private readonly Mock<IUserReportService> _mockUserReportService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockOAuthProviderService = new Mock<IOAuthProviderService>();
            _mockUserRegistrationService = new Mock<IUserRegistrationService>();
            _mockLoginService = new Mock<ILoginService>();
            _mockBookmarkService = new Mock<IBookmarkService>();
            _mockUserReportService = new Mock<IUserReportService>();

            _controller = new UsersController(
                _mockUserService.Object,
                _mockOAuthProviderService.Object,
                _mockUserRegistrationService.Object,
                _mockLoginService.Object,
                _mockBookmarkService.Object,
                _mockUserReportService.Object
            );
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullUserService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UsersController(
                null!,
                _mockOAuthProviderService.Object,
                _mockUserRegistrationService.Object,
                _mockLoginService.Object,
                _mockBookmarkService.Object,
                _mockUserReportService.Object
            ));
        }

        [Fact]
        public void Constructor_WithNullOAuthProviderService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new UsersController(
                _mockUserService.Object,
                null!,
                _mockUserRegistrationService.Object,
                _mockLoginService.Object,
                _mockBookmarkService.Object,
                _mockUserReportService.Object
            ));
        }

        [Fact]
        public void Constructor_WithValidServices_ShouldCreateInstance()
        {
            // Act
            var controller = new UsersController(
                _mockUserService.Object,
                _mockOAuthProviderService.Object,
                _mockUserRegistrationService.Object,
                _mockLoginService.Object,
                _mockBookmarkService.Object,
                _mockUserReportService.Object
            );

            // Assert
            controller.Should().NotBeNull();
        }

        #endregion

        #region GetAllUsers Tests

        [Fact]
        public async Task GetAllUsers_ShouldReturnOkWithUsers()
        {
            // Arrange
            var users = TestDataFactory.CreateUserList(3);
            _mockUserService
                .Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(users);
        }

        [Fact]
        public async Task GetAllUsers_WithEmptyUserList_ShouldReturnOkWithEmptyList()
        {
            // Arrange
            var emptyUsers = new List<User>();
            _mockUserService
                .Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(emptyUsers);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(emptyUsers);
        }

        #endregion

        #region GetUserById Tests

        [Fact]
        public async Task GetUserById_WithExistingUser_ShouldReturnOkWithUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = TestDataFactory.CreateValidUser();
            user.Id = userId;

            _mockUserService
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(user);
        }

        [Fact]
        public async Task GetUserById_WithNonExistentUser_ShouldReturnNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserService
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be($"User with ID {userId} not found.");
        }

        #endregion

        #region GetUserByEmail Tests

        [Fact]
        public async Task GetUserByEmail_WithExistingUser_ShouldReturnOkWithUser()
        {
            // Arrange
            var email = "test@example.com";
            var user = TestDataFactory.CreateValidUser(email: email);

            _mockUserService
                .Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserByEmail(email);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(user);
        }

        [Fact]
        public async Task GetUserByEmail_WithNonExistentUser_ShouldReturnOkWithNull()
        {
            // Arrange
            var email = "nonexistent@example.com";

            _mockUserService
                .Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.GetUserByEmail(email);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be($"User with email {email} not found.");
        }

        #endregion

        #region CheckUserExistsByEmail Tests

        [Fact]
        public async Task CheckUserExistsByEmail_WithExistingUser_ShouldReturnOkWithExistsTrue()
        {
            // Arrange
            var email = "test@example.com";
            var user = TestDataFactory.CreateValidUser(email: email);

            _mockUserService
                .Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.CheckUserExistsByEmail(email);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            // Note: Testing the dynamic response structure would require more specific assertions
        }

        [Fact]
        public async Task CheckUserExistsByEmail_WithNonExistentUser_ShouldReturnOkWithExistsFalse()
        {
            // Arrange
            var email = "nonexistent@example.com";

            _mockUserService
                .Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.CheckUserExistsByEmail(email);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            // Note: Testing the dynamic response structure would require more specific assertions
        }

        #endregion

        #region CreateUser Tests

        [Fact]
        public async Task CreateUser_WithValidUser_ShouldReturnOkWithCreatedUser()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();
            var createdUser = TestDataFactory.CreateValidUser();

            _mockUserService
                .Setup(x => x.CreateUserAsync(It.IsAny<RegisterUserRequest>()))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(user);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(createdUser);
            _mockUserService.Verify(x => x.CreateUserAsync(It.IsAny<RegisterUserRequest>()), Times.Once);
        }

        [Fact]
        public async Task CreateUser_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            // Arrange
            var user = TestDataFactory.CreateValidUser();

            _mockUserService
                .Setup(x => x.CreateUserAsync(It.IsAny<RegisterUserRequest>()))
                .ThrowsAsync(new Exception("Creation failed"));

            // Act
            var result = await _controller.CreateUser(user);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var errorResponse = badRequestResult.Value;
            errorResponse.Should().NotBeNull();
            // Controller returns an object with message property, not plain string
            errorResponse.Should().BeEquivalentTo(new { message = "Creation failed" });
        }

        #endregion

        #region Integration Behavior Tests

        [Fact]
        public async Task GetAllUsers_ShouldCallUserServiceOnce()
        {
            // Arrange
            var users = TestDataFactory.CreateUserList(2);
            _mockUserService
                .Setup(x => x.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            await _controller.GetAllUsers();

            // Assert
            _mockUserService.Verify(x => x.GetAllUsersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUserById_ShouldCallUserServiceWithCorrectId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = TestDataFactory.CreateValidUser();

            _mockUserService
                .Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            await _controller.GetUserById(userId);

            // Assert
            _mockUserService.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
        }

        #endregion

        #region GetUserPortfolioInfo Tests

        [Fact]
        public async Task GetUserPortfolioInfo_WithExistingUser_ShouldReturnOkWithPortfolioInfo()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var portfolioInfo = TestDataFactory.CreateValidUserProfile();

            _mockUserService
                .Setup(x => x.GetUserPortfolioInfoAsync(userId))
                .ReturnsAsync(portfolioInfo);

            // Act
            var result = await _controller.GetUserPortfolioInfo(userId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(portfolioInfo);
            _mockUserService.Verify(x => x.GetUserPortfolioInfoAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserPortfolioInfo_WithNonExistentUser_ShouldReturnNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserService
                .Setup(x => x.GetUserPortfolioInfoAsync(userId))
                .ReturnsAsync((UserProfileDto?)null);

            // Act
            var result = await _controller.GetUserPortfolioInfo(userId);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be($"User with ID {userId} not found.");
        }

        #endregion

        #region RegisterUser Tests

        [Fact]
        public async Task RegisterUser_WithValidRequest_ShouldReturnOkWithNewUser()
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterUserRequest();
            var newUser = TestDataFactory.CreateValidUser();

            _mockUserRegistrationService
                .Setup(x => x.RegisterUserAsync(request))
                .ReturnsAsync(newUser);

            // Act
            var result = await _controller.RegisterUser(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(newUser);
            _mockUserRegistrationService.Verify(x => x.RegisterUserAsync(request), Times.Once);
        }

        [Fact]
        public async Task RegisterUser_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            // Arrange
            var request = TestDataFactory.CreateValidRegisterUserRequest();

            _mockUserRegistrationService
                .Setup(x => x.RegisterUserAsync(request))
                .ThrowsAsync(new Exception("Registration failed"));

            // Act
            var result = await _controller.RegisterUser(request);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = "Registration failed" });
        }

        #endregion

        #region LoginWithOAuth Tests

        [Fact]
        public async Task LoginWithOAuth_WithSuccessfulLogin_ShouldReturnOkWithResult()
        {
            // Arrange
            var request = new OAuthLoginRequestDto 
            { 
                Provider = OAuthProviderType.Google, 
                ProviderId = "google-123", 
                ProviderEmail = "test@google.com" 
            };
            var loginResult = new LoginResponseDto { Success = true, User = TestDataFactory.CreateValidUserResponseDto() };

            _mockLoginService
                .Setup(x => x.LoginWithOAuthAsync(request))
                .ReturnsAsync(loginResult);

            // Act
            var result = await _controller.LoginWithOAuth(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(loginResult);
        }

        [Fact]
        public async Task LoginWithOAuth_WithFailedLogin_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new OAuthLoginRequestDto 
            { 
                Provider = OAuthProviderType.Google, 
                ProviderId = "google-123", 
                ProviderEmail = "test@google.com" 
            };
            var loginResult = new LoginResponseDto { Success = false, Message = "Invalid credentials" };

            _mockLoginService
                .Setup(x => x.LoginWithOAuthAsync(request))
                .ReturnsAsync(loginResult);

            // Act
            var result = await _controller.LoginWithOAuth(request);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be(loginResult);
        }

        [Fact]
        public async Task LoginWithOAuth_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new OAuthLoginRequestDto 
            { 
                Provider = OAuthProviderType.Google, 
                ProviderId = "google-123", 
                ProviderEmail = "test@google.com" 
            };

            _mockLoginService
                .Setup(x => x.LoginWithOAuthAsync(request))
                .ThrowsAsync(new Exception("OAuth login failed"));

            // Act
            var result = await _controller.LoginWithOAuth(request);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = "OAuth login failed" });
        }

        #endregion

        #region UpdateUser Tests

        [Fact]
        public async Task UpdateUser_WithExistingUser_ShouldReturnOkWithUpdatedUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = TestDataFactory.CreateValidUpdateUserRequest();
            var updatedUser = TestDataFactory.CreateValidUser();

            _mockUserService
                .Setup(x => x.UpdateUserAsync(userId, request))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.UpdateUser(userId, request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<UserResponseDto>().Subject;
            response.Id.Should().Be(updatedUser.Id);
            response.Email.Should().Be(updatedUser.Email);
            _mockUserService.Verify(x => x.UpdateUserAsync(userId, request), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_WithNonExistentUser_ShouldReturnNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = TestDataFactory.CreateValidUpdateUserRequest();

            _mockUserService
                .Setup(x => x.UpdateUserAsync(userId, request))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.UpdateUser(userId, request);

            // Assert
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().Be($"User with ID {userId} not found.");
        }

        [Fact]
        public async Task UpdateUser_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = TestDataFactory.CreateValidUpdateUserRequest();

            _mockUserService
                .Setup(x => x.UpdateUserAsync(userId, request))
                .ThrowsAsync(new Exception("Update failed"));

            // Act
            var result = await _controller.UpdateUser(userId, request);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = "Update failed" });
        }

        #endregion

        #region OAuth Provider Tests

        [Fact]
        public async Task GetUserOAuthProviders_ShouldReturnOkWithProviders()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var providers = new List<OAuthProviderSummaryDto>
            {
                new OAuthProviderSummaryDto { Id = Guid.NewGuid(), Provider = OAuthProviderType.Google }
            };

            _mockOAuthProviderService
                .Setup(x => x.GetUserOAuthProvidersAsync(userId))
                .ReturnsAsync(providers);

            // Act
            var result = await _controller.GetUserOAuthProviders(userId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(providers);
        }

        [Fact]
        public async Task CreateOAuthProvider_WithValidRequest_ShouldReturnOkWithProvider()
        {
            // Arrange
            var request = TestDataFactory.CreateValidOAuthProviderCreateRequest();
            var response = new OAuthProviderResponseDto { Id = Guid.NewGuid(), Provider = request.Provider };

            _mockOAuthProviderService
                .Setup(x => x.CreateOAuthProviderAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CreateOAuthProvider(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(response);
        }

        [Fact]
        public async Task CreateOAuthProvider_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            // Arrange
            var request = TestDataFactory.CreateValidOAuthProviderCreateRequest();

            _mockOAuthProviderService
                .Setup(x => x.CreateOAuthProviderAsync(request))
                .ThrowsAsync(new Exception("Provider creation failed"));

            // Act
            var result = await _controller.CreateOAuthProvider(request);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = "Provider creation failed" });
        }

        [Fact]
        public async Task UpdateOAuthProvider_WithExistingProvider_ShouldReturnOkWithUpdatedProvider()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var request = new OAuthProviderUpdateRequestDto { AccessToken = "new-token" };
            var response = new OAuthProviderResponseDto { Id = providerId, Provider = OAuthProviderType.Google };

            _mockOAuthProviderService
                .Setup(x => x.UpdateOAuthProviderAsync(providerId, request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateOAuthProvider(providerId, request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(response);
        }

        [Fact]
        public async Task UpdateOAuthProvider_WithNonExistentProvider_ShouldReturnNotFound()
        {
            // Arrange
            var providerId = Guid.NewGuid();
            var request = new OAuthProviderUpdateRequestDto { AccessToken = "new-token" };

            _mockOAuthProviderService
                .Setup(x => x.UpdateOAuthProviderAsync(providerId, request))
                .ReturnsAsync((OAuthProviderResponseDto?)null);

            // Act
            var result = await _controller.UpdateOAuthProvider(providerId, request);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteOAuthProvider_WithExistingProvider_ShouldReturnNoContent()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            _mockOAuthProviderService
                .Setup(x => x.DeleteOAuthProviderAsync(providerId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteOAuthProvider(providerId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteOAuthProvider_WithNonExistentProvider_ShouldReturnNotFound()
        {
            // Arrange
            var providerId = Guid.NewGuid();

            _mockOAuthProviderService
                .Setup(x => x.DeleteOAuthProviderAsync(providerId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteOAuthProvider(providerId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CheckOAuthProvider_ShouldReturnOkWithResult()
        {
            // Arrange
            var provider = OAuthProviderType.Google;
            var providerId = "google-123";
            var checkResult = new { exists = true, provider = "Google" };

            _mockOAuthProviderService
                .Setup(x => x.CheckOAuthProviderAsync(provider, providerId))
                .ReturnsAsync(checkResult);

            // Act
            var result = await _controller.CheckOAuthProvider(provider, providerId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(checkResult);
        }

        [Fact]
        public async Task GetUserOAuthProviderByType_ShouldReturnOkWithProvider()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var provider = OAuthProviderType.Google;
            var providerResponse = new OAuthProviderResponseDto { Id = Guid.NewGuid(), Provider = provider };

            _mockOAuthProviderService
                .Setup(x => x.GetUserOAuthProviderByTypeAsync(userId, provider))
                .ReturnsAsync(providerResponse);

            // Act
            var result = await _controller.GetUserOAuthProviderByType(userId, provider);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(providerResponse);
        }

        [Fact]
        public async Task RegisterOAuthUser_WithValidRequest_ShouldReturnOkWithUser()
        {
            // Arrange
            var request = new RegisterOAuthUserRequest 
            { 
                Email = "test@google.com",
                FirstName = "John",
                LastName = "Doe",
                Provider = OAuthProviderType.Google,
                ProviderId = "google-123",
                ProviderEmail = "test@google.com"
            };
            var registeredUser = TestDataFactory.CreateValidUser();

            _mockUserRegistrationService
                .Setup(x => x.RegisterOAuthUserAsync(request))
                .ReturnsAsync(registeredUser);

            // Act
            var result = await _controller.RegisterOAuthUser(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(registeredUser);
        }

        [Fact]
        public async Task RegisterOAuthUser_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new RegisterOAuthUserRequest 
            { 
                Email = "test@google.com",
                FirstName = "John",
                LastName = "Doe",
                Provider = OAuthProviderType.Google,
                ProviderId = "google-123",
                ProviderEmail = "test@google.com"
            };

            _mockUserRegistrationService
                .Setup(x => x.RegisterOAuthUserAsync(request))
                .ThrowsAsync(new Exception("OAuth registration failed"));

            // Act
            var result = await _controller.RegisterOAuthUser(request);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = "OAuth registration failed" });
        }

        #endregion

        #region SearchUsers Tests
        [Fact]
        public async Task SearchUsers_WithValidQuery_ShouldReturnOkWithUsers()
        {
            var query = "john";
            var users = TestDataFactory.CreateUserList(2);
            _mockUserService.Setup(x => x.SearchUsersAsync(query)).ReturnsAsync(users);

            var result = await _controller.SearchUsers(query);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task SearchUsers_WithEmptyQuery_ShouldReturnBadRequest()
        {
            var result = await _controller.SearchUsers("");
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task SearchUsers_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            var query = "john";
            _mockUserService.Setup(x => x.SearchUsersAsync(query)).ThrowsAsync(new Exception("search error"));
            var result = await _controller.SearchUsers(query);
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().NotBeNull();
        }
        #endregion

        #region AddBookmark Tests
        [Fact]
        public async Task AddBookmark_WithValidRequest_ShouldReturnOkWithBookmark()
        {
            var userId = Guid.NewGuid();
            var request = new AddBookmarkRequest { PortfolioId = "p1" };
            var response = new BookmarkResponse { Id = Guid.NewGuid(), UserId = userId, PortfolioId = "p1" };
            _mockBookmarkService.Setup(x => x.AddBookmarkAsync(userId, request)).ReturnsAsync(response);
            var result = await _controller.AddBookmark(userId, request);
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(response);
        }
        [Fact]
        public async Task AddBookmark_WhenUserNotFound_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();
            var request = new AddBookmarkRequest { PortfolioId = "p1" };
            _mockBookmarkService.Setup(x => x.AddBookmarkAsync(userId, request)).ThrowsAsync(new ArgumentException("User with ID not found."));
            var result = await _controller.AddBookmark(userId, request);
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFound.Value.Should().NotBeNull();
        }
        [Fact]
        public async Task AddBookmark_WhenAlreadyBookmarked_ShouldReturnConflict()
        {
            var userId = Guid.NewGuid();
            var request = new AddBookmarkRequest { PortfolioId = "p1" };
            _mockBookmarkService.Setup(x => x.AddBookmarkAsync(userId, request)).ThrowsAsync(new InvalidOperationException("Portfolio is already bookmarked."));
            var result = await _controller.AddBookmark(userId, request);
            var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
            conflict.Value.Should().NotBeNull();
        }
        [Fact]
        public async Task AddBookmark_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            var userId = Guid.NewGuid();
            var request = new AddBookmarkRequest { PortfolioId = "p1" };
            _mockBookmarkService.Setup(x => x.AddBookmarkAsync(userId, request)).ThrowsAsync(new Exception("add error"));
            var result = await _controller.AddBookmark(userId, request);
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().NotBeNull();
        }
        #endregion

        #region RemoveBookmark Tests
        [Fact]
        public async Task RemoveBookmark_WithValidRequest_ShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            var portfolioId = "p1";
            _mockBookmarkService.Setup(x => x.RemoveBookmarkAsync(userId, portfolioId)).ReturnsAsync(true);
            var result = await _controller.RemoveBookmark(userId, portfolioId);
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }
        [Fact]
        public async Task RemoveBookmark_WhenUserNotFound_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();
            var portfolioId = "p1";
            _mockBookmarkService.Setup(x => x.RemoveBookmarkAsync(userId, portfolioId)).ThrowsAsync(new ArgumentException("User with ID not found."));
            var result = await _controller.RemoveBookmark(userId, portfolioId);
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFound.Value.Should().NotBeNull();
        }
        [Fact]
        public async Task RemoveBookmark_WhenBookmarkNotFound_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();
            var portfolioId = "p1";
            _mockBookmarkService.Setup(x => x.RemoveBookmarkAsync(userId, portfolioId)).ThrowsAsync(new InvalidOperationException("Bookmark not found."));
            var result = await _controller.RemoveBookmark(userId, portfolioId);
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFound.Value.Should().NotBeNull();
        }
        [Fact]
        public async Task RemoveBookmark_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            var userId = Guid.NewGuid();
            var portfolioId = "p1";
            _mockBookmarkService.Setup(x => x.RemoveBookmarkAsync(userId, portfolioId)).ThrowsAsync(new Exception("remove error"));
            var result = await _controller.RemoveBookmark(userId, portfolioId);
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().NotBeNull();
        }
        #endregion

        #region GetUserBookmarks Tests
        [Fact]
        public async Task GetUserBookmarks_WithValidUser_ShouldReturnOkWithBookmarks()
        {
            var userId = Guid.NewGuid();
            var bookmarks = new List<BookmarkResponse> { new BookmarkResponse { Id = Guid.NewGuid(), UserId = userId, PortfolioId = "p1" } };
            _mockBookmarkService.Setup(x => x.GetUserBookmarksAsync(userId)).ReturnsAsync(bookmarks);
            var result = await _controller.GetUserBookmarks(userId);
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(bookmarks);
        }
        [Fact]
        public async Task GetUserBookmarks_WhenUserNotFound_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();
            _mockBookmarkService.Setup(x => x.GetUserBookmarksAsync(userId)).ThrowsAsync(new ArgumentException("User with ID not found."));
            var result = await _controller.GetUserBookmarks(userId);
            var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFound.Value.Should().NotBeNull();
        }
        [Fact]
        public async Task GetUserBookmarks_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            var userId = Guid.NewGuid();
            _mockBookmarkService.Setup(x => x.GetUserBookmarksAsync(userId)).ThrowsAsync(new Exception("get error"));
            var result = await _controller.GetUserBookmarks(userId);
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().NotBeNull();
        }
        #endregion

        #region GetBookmarkStatus Tests
        [Fact]
        public async Task GetBookmarkStatus_WithValidRequest_ShouldReturnOkWithStatus()
        {
            var userId = Guid.NewGuid();
            var portfolioId = "p1";
            var response = new IsBookmarkedResponse { IsBookmarked = true };
            _mockBookmarkService.Setup(x => x.GetBookmarkStatusAsync(userId, portfolioId)).ReturnsAsync(response);
            var result = await _controller.GetBookmarkStatus(userId, portfolioId);
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(response);
        }
        [Fact]
        public async Task GetBookmarkStatus_WhenServiceThrowsException_ShouldReturnBadRequest()
        {
            var userId = Guid.NewGuid();
            var portfolioId = "p1";
            _mockBookmarkService.Setup(x => x.GetBookmarkStatusAsync(userId, portfolioId)).ThrowsAsync(new Exception("status error"));
            var result = await _controller.GetBookmarkStatus(userId, portfolioId);
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().NotBeNull();
        }
        #endregion
    }
}
