using System;
using System.ComponentModel.DataAnnotations;
using Xunit;
using FluentAssertions;
using backend_portfolio.Models.Validation;

namespace backend_portfolio.tests.Models
{
    public class ValidationTests
    {
        public class NotEmptyGuidAttributeTests
        {
            private readonly NotEmptyGuidAttribute _attribute;

            public NotEmptyGuidAttributeTests()
            {
                _attribute = new NotEmptyGuidAttribute();
            }

            [Fact]
            public void IsValid_WithValidGuid_ReturnsTrue()
            {
                // Arrange
                var validGuid = Guid.NewGuid();

                // Act
                var result = _attribute.IsValid(validGuid);

                // Assert
                result.Should().BeTrue();
            }

            [Fact]
            public void IsValid_WithEmptyGuid_ReturnsFalse()
            {
                // Arrange
                var emptyGuid = Guid.Empty;

                // Act
                var result = _attribute.IsValid(emptyGuid);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public void IsValid_WithNullValue_ReturnsFalse()
            {
                // Arrange
                object? nullValue = null;

                // Act
                var result = _attribute.IsValid(nullValue);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public void IsValid_WithNonGuidValue_ReturnsFalse()
            {
                // Arrange
                var stringValue = "not-a-guid";

                // Act
                var result = _attribute.IsValid(stringValue);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public void IsValid_WithIntegerValue_ReturnsFalse()
            {
                // Arrange
                var intValue = 123;

                // Act
                var result = _attribute.IsValid(intValue);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public void FormatErrorMessage_WithFieldName_ReturnsFormattedMessage()
            {
                // Arrange
                var fieldName = "UserId";

                // Act
                var result = _attribute.FormatErrorMessage(fieldName);

                // Assert
                result.Should().Be("UserId cannot be empty.");
            }

            [Fact]
            public void FormatErrorMessage_WithEmptyFieldName_ReturnsFormattedMessage()
            {
                // Arrange
                var fieldName = "";

                // Act
                var result = _attribute.FormatErrorMessage(fieldName);

                // Assert
                result.Should().Be(" cannot be empty.");
            }

            [Fact]
            public void FormatErrorMessage_WithNullFieldName_ReturnsFormattedMessage()
            {
                // Arrange
                string? fieldName = null;

                // Act
                var result = _attribute.FormatErrorMessage(fieldName!);

                // Assert
                result.Should().Be(" cannot be empty.");
            }

            [Theory]
            [InlineData("TestField")]
            [InlineData("Portfolio Id")]
            [InlineData("User_Id")]
            [InlineData("template-id")]
            public void FormatErrorMessage_WithVariousFieldNames_ReturnsCorrectMessage(string fieldName)
            {
                // Act
                var result = _attribute.FormatErrorMessage(fieldName);

                // Assert
                result.Should().Be($"{fieldName} cannot be empty.");
            }

            [Fact]
            public void IsValid_WithNullableGuidHavingValue_ReturnsTrue()
            {
                // Arrange
                Guid? nullableGuid = Guid.NewGuid();

                // Act
                var result = _attribute.IsValid(nullableGuid);

                // Assert
                result.Should().BeTrue();
            }

            [Fact]
            public void IsValid_WithNullableGuidHavingEmptyValue_ReturnsFalse()
            {
                // Arrange
                Guid? nullableGuid = Guid.Empty;

                // Act
                var result = _attribute.IsValid(nullableGuid);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public void IsValid_WithNullableGuidBeingNull_ReturnsFalse()
            {
                // Arrange
                Guid? nullableGuid = null;

                // Act
                var result = _attribute.IsValid(nullableGuid);

                // Assert
                result.Should().BeFalse();
            }
        }
    }
} 