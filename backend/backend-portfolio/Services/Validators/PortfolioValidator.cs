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
    public class PortfolioValidator : IValidationService<PortfolioCreateRequest>
    {
        public ValidationResult Validate(PortfolioCreateRequest entity)
        {
            var errors = new List<string>();

            if (entity == null)
            {
                errors.Add("Portfolio request cannot be null.");
                return ValidationResult.Failure(errors.ToArray());
            }

            if (entity.UserId == Guid.Empty)
                errors.Add("User ID is required.");

            if (string.IsNullOrWhiteSpace(entity.Title))
                errors.Add("Portfolio title is required.");

            if (string.IsNullOrWhiteSpace(entity.TemplateName))
                errors.Add("Template name is required.");

            if (entity.Title?.Length > 255)
                errors.Add("Portfolio title cannot exceed 255 characters.");

            return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
        }

        public Task<ValidationResult> ValidateAsync(PortfolioCreateRequest entity)
        {
            return Task.FromResult(Validate(entity));
        }
    }

    public class PortfolioUpdateValidator : IValidationService<PortfolioUpdateRequest>
    {
        public ValidationResult Validate(PortfolioUpdateRequest entity)
        {
            var errors = new List<string>();

            if (entity == null)
            {
                errors.Add("Portfolio update request cannot be null.");
                return ValidationResult.Failure(errors.ToArray());
            }

            if (entity.Title?.Length > 255)
                errors.Add("Portfolio title cannot exceed 255 characters.");

            return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
        }

        public Task<ValidationResult> ValidateAsync(PortfolioUpdateRequest entity)
        {
            return Task.FromResult(Validate(entity));
        }
    }
} 