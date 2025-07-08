using Xunit;

namespace Backend.Tests
{
    public class DummyTest
    {
        [Fact]
        public void Simple_Test_Should_Pass()
        {
            // Arrange
            var expected = 2;
            
            // Act
            var result = 1 + 1;
            
            // Assert
            Assert.Equal(expected, result);
        }
    }
}