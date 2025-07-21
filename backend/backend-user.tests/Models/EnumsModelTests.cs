using Xunit;
using FluentAssertions;
using backend_user.Models;

namespace backend_user.tests.Models
{
    public class EnumsModelTests
    {
        [Fact]
        public void OAuthProviderType_ShouldHaveCorrectValues()
        {
            // Assert
            Enum.GetValues<OAuthProviderType>().Should().Contain(new[]
            {
                OAuthProviderType.Google,
                OAuthProviderType.Facebook,
                OAuthProviderType.GitHub,
                OAuthProviderType.LinkedIn
            });
        }

        [Theory]
        [InlineData(OAuthProviderType.Google, 0)]
        [InlineData(OAuthProviderType.GitHub, 1)]
        [InlineData(OAuthProviderType.LinkedIn, 2)]
        [InlineData(OAuthProviderType.Facebook, 3)]
        public void OAuthProviderType_ShouldHaveExpectedNumericValues(OAuthProviderType provider, int expectedValue)
        {
            // Assert
            ((int)provider).Should().Be(expectedValue);
        }

        [Fact]
        public void ReportedType_ShouldHaveCorrectValues()
        {
            // Assert
            Enum.GetValues<ReportedType>().Should().Contain(new[]
            {
                ReportedType.User,
                ReportedType.Portfolio,
                ReportedType.Message,
                ReportedType.BlogPost,
                ReportedType.Comment
            });
        }

        [Theory]
        [InlineData(ReportedType.User, 0)]
        [InlineData(ReportedType.Portfolio, 1)]
        [InlineData(ReportedType.Message, 2)]
        [InlineData(ReportedType.BlogPost, 3)]
        [InlineData(ReportedType.Comment, 4)]
        public void ReportedType_ShouldHaveExpectedNumericValues(ReportedType reportedType, int expectedValue)
        {
            // Assert
            ((int)reportedType).Should().Be(expectedValue);
        }

        [Fact]
        public void ReportType_ShouldHaveCorrectValues()
        {
            // Assert
            Enum.GetValues<ReportType>().Should().Contain(new[]
            {
                ReportType.Spam,
                ReportType.Harassment,
                ReportType.InappropriateContent,
                ReportType.FakeProfile,
                ReportType.Copyright,
                ReportType.Other
            });
        }

        [Theory]
        [InlineData(ReportType.Spam, 0)]
        [InlineData(ReportType.Harassment, 1)]
        [InlineData(ReportType.InappropriateContent, 2)]
        [InlineData(ReportType.FakeProfile, 3)]
        [InlineData(ReportType.Copyright, 4)]
        [InlineData(ReportType.Other, 5)]
        public void ReportType_ShouldHaveExpectedNumericValues(ReportType reportType, int expectedValue)
        {
            // Assert
            ((int)reportType).Should().Be(expectedValue);
        }

        [Fact]
        public void ReportStatus_ShouldHaveCorrectValues()
        {
            // Assert
            Enum.GetValues<ReportStatus>().Should().Contain(new[]
            {
                ReportStatus.Pending,
                ReportStatus.UnderReview,
                ReportStatus.Resolved,
                ReportStatus.Dismissed
            });
        }

        [Theory]
        [InlineData(ReportStatus.Pending, 0)]
        [InlineData(ReportStatus.UnderReview, 1)]
        [InlineData(ReportStatus.Resolved, 2)]
        [InlineData(ReportStatus.Dismissed, 3)]
        public void ReportStatus_ShouldHaveExpectedNumericValues(ReportStatus status, int expectedValue)
        {
            // Assert
            ((int)status).Should().Be(expectedValue);
        }

        [Fact]
        public void OAuthProviderType_ToString_ShouldReturnCorrectString()
        {
            // Assert
            OAuthProviderType.Google.ToString().Should().Be("Google");
            OAuthProviderType.Facebook.ToString().Should().Be("Facebook");
            OAuthProviderType.GitHub.ToString().Should().Be("GitHub");
            OAuthProviderType.LinkedIn.ToString().Should().Be("LinkedIn");
        }

        [Fact]
        public void ReportedType_ToString_ShouldReturnCorrectString()
        {
            // Assert
            ReportedType.User.ToString().Should().Be("User");
            ReportedType.Portfolio.ToString().Should().Be("Portfolio");
            ReportedType.Message.ToString().Should().Be("Message");
            ReportedType.BlogPost.ToString().Should().Be("BlogPost");
            ReportedType.Comment.ToString().Should().Be("Comment");
        }

        [Fact]
        public void ReportType_ToString_ShouldReturnCorrectString()
        {
            // Assert
            ReportType.Spam.ToString().Should().Be("Spam");
            ReportType.Harassment.ToString().Should().Be("Harassment");
            ReportType.InappropriateContent.ToString().Should().Be("InappropriateContent");
            ReportType.FakeProfile.ToString().Should().Be("FakeProfile");
            ReportType.Copyright.ToString().Should().Be("Copyright");
            ReportType.Other.ToString().Should().Be("Other");
        }

        [Fact]
        public void ReportStatus_ToString_ShouldReturnCorrectString()
        {
            // Assert
            ReportStatus.Pending.ToString().Should().Be("Pending");
            ReportStatus.UnderReview.ToString().Should().Be("UnderReview");
            ReportStatus.Resolved.ToString().Should().Be("Resolved");
            ReportStatus.Dismissed.ToString().Should().Be("Dismissed");
        }

        [Fact]
        public void AllEnums_ShouldBeDefined()
        {
            // Assert
            Enum.IsDefined(typeof(OAuthProviderType), OAuthProviderType.Google).Should().BeTrue();
            Enum.IsDefined(typeof(ReportedType), ReportedType.User).Should().BeTrue();
            Enum.IsDefined(typeof(ReportType), ReportType.Spam).Should().BeTrue();
            Enum.IsDefined(typeof(ReportStatus), ReportStatus.Pending).Should().BeTrue();
        }

        [Fact]
        public void EnumParsing_ShouldWorkCorrectly()
        {
            // Assert
            Enum.Parse<OAuthProviderType>("Google").Should().Be(OAuthProviderType.Google);
            Enum.Parse<ReportedType>("User").Should().Be(ReportedType.User);
            Enum.Parse<ReportType>("Spam").Should().Be(ReportType.Spam);
            Enum.Parse<ReportStatus>("Pending").Should().Be(ReportStatus.Pending);
        }
    }
}
