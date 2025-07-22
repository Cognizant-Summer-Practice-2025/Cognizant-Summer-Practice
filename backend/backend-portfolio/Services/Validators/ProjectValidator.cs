using backend_portfolio.DTO.Portfolio.Request;
using backend_portfolio.DTO.Project.Request;
using backend_portfolio.DTO.Experience.Request;
using backend_portfolio.DTO.Skill.Request;
using backend_portfolio.DTO.BlogPost.Request;
using backend_portfolio.DTO.Bookmark.Request;
using backend_portfolio.DTO.PortfolioTemplate.Request;
using backend_portfolio.DTO.ImageUpload.Request;
using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.Services.Validators
{
    public class ProjectValidator : IValidationService<ProjectCreateRequest>
    {
        public ValidationResult Validate(ProjectCreateRequest entity)
        {
            var errors = new List<string>();

            if (entity == null)
            {
                errors.Add("Project request cannot be null.");
                return ValidationResult.Failure(errors.ToArray());
            }

            if (string.IsNullOrWhiteSpace(entity.Title))
                errors.Add("Project title is required.");

            if (entity.PortfolioId == Guid.Empty)
                errors.Add("Portfolio ID is required.");

            if (entity.Title?.Length > 255)
                errors.Add("Project title cannot exceed 255 characters.");

            if (!string.IsNullOrWhiteSpace(entity.DemoUrl) && !Uri.IsWellFormedUriString(entity.DemoUrl, UriKind.Absolute))
                errors.Add("Demo URL must be a valid URL.");

            if (!string.IsNullOrWhiteSpace(entity.GithubUrl) && !Uri.IsWellFormedUriString(entity.GithubUrl, UriKind.Absolute))
                errors.Add("GitHub URL must be a valid URL.");

            if (!string.IsNullOrWhiteSpace(entity.ImageUrl))
            {
                bool isValidImageUrl = Uri.IsWellFormedUriString(entity.ImageUrl, UriKind.Absolute) || 
                                     Uri.IsWellFormedUriString(entity.ImageUrl, UriKind.Relative) ||
                                     entity.ImageUrl.StartsWith("server/portfolio/");
                
                if (!isValidImageUrl)
                    errors.Add("Image URL must be a valid URL or server path.");
            }

            return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
        }

        public Task<ValidationResult> ValidateAsync(ProjectCreateRequest entity)
        {
            return Task.FromResult(Validate(entity));
        }
    }

    public class ProjectUpdateValidator : IValidationService<ProjectUpdateRequest>
    {
        public ValidationResult Validate(ProjectUpdateRequest entity)
        {
            var errors = new List<string>();

            if (entity == null)
            {
                errors.Add("Project update request cannot be null.");
                return ValidationResult.Failure(errors.ToArray());
            }

            if (entity.Title?.Length > 255)
                errors.Add("Project title cannot exceed 255 characters.");

            if (!string.IsNullOrWhiteSpace(entity.DemoUrl) && !Uri.IsWellFormedUriString(entity.DemoUrl, UriKind.Absolute))
                errors.Add("Demo URL must be a valid URL.");

            if (!string.IsNullOrWhiteSpace(entity.GithubUrl) && !Uri.IsWellFormedUriString(entity.GithubUrl, UriKind.Absolute))
                errors.Add("GitHub URL must be a valid URL.");

            if (!string.IsNullOrWhiteSpace(entity.ImageUrl))
            {
                bool isValidImageUrl = Uri.IsWellFormedUriString(entity.ImageUrl, UriKind.Absolute) || 
                                     Uri.IsWellFormedUriString(entity.ImageUrl, UriKind.Relative) ||
                                     entity.ImageUrl.StartsWith("server/portfolio/");
                
                if (!isValidImageUrl)
                    errors.Add("Image URL must be a valid URL or server path.");
            }

            return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
        }

        public Task<ValidationResult> ValidateAsync(ProjectUpdateRequest entity)
        {
            return Task.FromResult(Validate(entity));
        }
    }
} 