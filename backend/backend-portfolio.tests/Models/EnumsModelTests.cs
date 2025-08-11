using Xunit;
using FluentAssertions;
using backend_portfolio.Models;

namespace backend_portfolio.tests.Models
{
    public class EnumsModelTests
    {
        [Fact]
        public void Visibility_ShouldHaveCorrectValues()
        {
            // Assert
            ((int)Visibility.Public).Should().Be(0);
            ((int)Visibility.Private).Should().Be(1);
            ((int)Visibility.Unlisted).Should().Be(2);
        }

        [Fact]
        public void Visibility_ShouldHaveCorrectEnumNames()
        {
            // Act & Assert
            Enum.GetName(typeof(Visibility), Visibility.Public).Should().Be("Public");
            Enum.GetName(typeof(Visibility), Visibility.Private).Should().Be("Private");
            Enum.GetName(typeof(Visibility), Visibility.Unlisted).Should().Be("Unlisted");
        }

        [Fact]
        public void Visibility_ShouldHaveThreeValues()
        {
            // Act
            var values = Enum.GetValues<Visibility>();

            // Assert
            values.Should().HaveCount(3);
            values.Should().Contain(Visibility.Public);
            values.Should().Contain(Visibility.Private);
            values.Should().Contain(Visibility.Unlisted);
        }

        [Theory]
        [InlineData(Visibility.Public, 0)]
        [InlineData(Visibility.Private, 1)]
        [InlineData(Visibility.Unlisted, 2)]
        public void Visibility_ShouldCastToCorrectIntegerValue(Visibility visibility, int expectedValue)
        {
            // Act
            var intValue = (int)visibility;

            // Assert
            intValue.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(0, Visibility.Public)]
        [InlineData(1, Visibility.Private)]
        [InlineData(2, Visibility.Unlisted)]
        public void Visibility_ShouldCastFromIntegerValue(int intValue, Visibility expectedVisibility)
        {
            // Act
            var visibility = (Visibility)intValue;

            // Assert
            visibility.Should().Be(expectedVisibility);
        }

        [Fact]
        public void Visibility_ShouldSupportToStringConversion()
        {
            // Act & Assert
            Visibility.Public.ToString().Should().Be("Public");
            Visibility.Private.ToString().Should().Be("Private");
            Visibility.Unlisted.ToString().Should().Be("Unlisted");
        }

        [Theory]
        [InlineData("Public", true, Visibility.Public)]
        [InlineData("Private", true, Visibility.Private)]
        [InlineData("Unlisted", true, Visibility.Unlisted)]
        [InlineData("public", true, Visibility.Public)] // Case insensitive
        [InlineData("PRIVATE", true, Visibility.Private)] // Case insensitive
        [InlineData("InvalidValue", false, default(Visibility))]
        public void Visibility_ShouldSupportParseFromString(string input, bool expectedSuccess, Visibility expectedValue)
        {
            // Act
            var success = Enum.TryParse<Visibility>(input, true, out var result);

            // Assert
            success.Should().Be(expectedSuccess);
            if (expectedSuccess)
            {
                result.Should().Be(expectedValue);
            }
        }

        [Fact]
        public void Visibility_ShouldBeDefinedEnum()
        {
            // Act & Assert
            Enum.IsDefined(typeof(Visibility), Visibility.Public).Should().BeTrue();
            Enum.IsDefined(typeof(Visibility), Visibility.Private).Should().BeTrue();
            Enum.IsDefined(typeof(Visibility), Visibility.Unlisted).Should().BeTrue();
            Enum.IsDefined(typeof(Visibility), 99).Should().BeFalse(); // Invalid value
        }
    }
} 