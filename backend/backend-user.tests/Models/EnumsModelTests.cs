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
        public void OAuthProviderType_ToString_ShouldReturnCorrectString()
        {
            // Assert
            OAuthProviderType.Google.ToString().Should().Be("Google");
            OAuthProviderType.Facebook.ToString().Should().Be("Facebook");
            OAuthProviderType.GitHub.ToString().Should().Be("GitHub");
            OAuthProviderType.LinkedIn.ToString().Should().Be("LinkedIn");
        }

        [Fact]
        public void OAuthProviderType_ShouldBeDefined()
        {
            // Assert
            Enum.IsDefined(typeof(OAuthProviderType), OAuthProviderType.Google).Should().BeTrue();
        }

        [Fact]
        public void OAuthProviderType_Parsing_ShouldWorkCorrectly()
        {
            // Assert
            Enum.Parse<OAuthProviderType>("Google").Should().Be(OAuthProviderType.Google);
        }
    }
}
