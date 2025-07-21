using backend_user.DTO.User.Request;
using backend_user.DTO.User.Response;
using backend_user.Models;

namespace backend_user.Services.Mappers
{
    /// <summary>
    /// Mapper for User entity and related DTOs.
    /// </summary>
    public static class UserMapper
    {
        /// <summary>
        /// Maps User entity to UserResponseDto.
        /// </summary>
        public static UserResponseDto ToResponseDto(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfessionalTitle = user.ProfessionalTitle,
                Bio = user.Bio,
                Location = user.Location,
                AvatarUrl = user.AvatarUrl,
                IsActive = user.IsActive,
                IsAdmin = user.IsAdmin,
                LastLoginAt = user.LastLoginAt
            };
        }

        /// <summary>
        /// Maps RegisterUserRequest to User entity.
        /// </summary>
        public static User ToEntity(RegisterUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var username = request.Email.Split('@')[0];

            return new User
            {
                Email = request.Email,
                Username = username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                ProfessionalTitle = request.ProfessionalTitle,
                Bio = request.Bio,
                Location = request.Location,
                AvatarUrl = request.ProfileImage
            };
        }

        /// <summary>
        /// Maps RegisterOAuthUserRequest to User entity.
        /// </summary>
        public static User ToEntity(RegisterOAuthUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var username = request.Email.Split('@')[0];

            return new User
            {
                Email = request.Email,
                Username = username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                ProfessionalTitle = request.ProfessionalTitle,
                Bio = request.Bio,
                Location = request.Location,
                AvatarUrl = request.ProfileImage
            };
        }

        /// <summary>
        /// Creates portfolio info object from User entity.
        /// </summary>
        public static object ToPortfolioInfo(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return new
            {
                userId = user.Id,
                username = user.Username,
                name = $"{user.FirstName} {user.LastName}".Trim(),
                professionalTitle = user.ProfessionalTitle ?? "Portfolio Creator",
                location = user.Location ?? "Location not specified",
                avatarUrl = user.AvatarUrl
            };
        }

        /// <summary>
        /// Maps User entity to RegisterUserRequest (for service compatibility).
        /// </summary>
        public static RegisterUserRequest ToRegisterRequest(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return new RegisterUserRequest
            {
                Email = user.Email,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                ProfessionalTitle = user.ProfessionalTitle,
                Bio = user.Bio,
                Location = user.Location,
                ProfileImage = user.AvatarUrl
            };
        }
    }
}
