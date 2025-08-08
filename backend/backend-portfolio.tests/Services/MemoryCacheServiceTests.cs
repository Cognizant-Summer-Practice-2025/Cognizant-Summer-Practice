using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using AutoFixture;
using backend_portfolio.Services;
using System.Linq;

namespace backend_portfolio.tests.Services
{
    public class MemoryCacheServiceTests : IDisposable
    {
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<ILogger<MemoryCacheService>> _mockLogger;
        private readonly MemoryCacheService _cacheService;
        private readonly Fixture _fixture;

        public MemoryCacheServiceTests()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _mockLogger = new Mock<ILogger<MemoryCacheService>>();
            _cacheService = new MemoryCacheService(_memoryCache, _mockLogger.Object);
            _fixture = new Fixture();
        }

        public void Dispose()
        {
            _memoryCache.Dispose();
        }

        [Fact]
        public async Task GetAsync_WithValidKey_WhenCacheHit_ShouldReturnCachedValue()
        {
            // Arrange
            var key = "test-key";
            var expectedValue = _fixture.Create<string>();
            await _cacheService.SetAsync(key, expectedValue);

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            result.Should().Be(expectedValue);
        }

        [Fact]
        public async Task GetAsync_WithValidKey_WhenCacheMiss_ShouldReturnNull()
        {
            // Arrange
            var key = "non-existent-key";

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SetAsync_WithValidKeyAndValue_ShouldStoreValue()
        {
            // Arrange
            var key = "test-key";
            var value = _fixture.Create<string>();

            // Act
            await _cacheService.SetAsync(key, value);

            // Assert
            var result = await _cacheService.GetAsync<string>(key);
            result.Should().Be(value);
        }

        [Fact]
        public async Task SetAsync_WithExpiration_ShouldExpireAfterTimeout()
        {
            // Arrange
            var key = "expiring-key";
            var value = _fixture.Create<string>();
            var expiration = TimeSpan.FromMilliseconds(100);

            // Act
            await _cacheService.SetAsync(key, value, expiration);

            var initialResult = await _cacheService.GetAsync<string>(key);
            initialResult.Should().Be(value);

            // Wait for expiration
            await Task.Delay(150);

            // Assert
            var expiredResult = await _cacheService.GetAsync<string>(key);
            expiredResult.Should().BeNull();
        }

        [Fact]
        public async Task RemoveAsync_WithExistingKey_ShouldRemoveValue()
        {
            // Arrange
            var key = "test-key";
            var value = _fixture.Create<string>();
            await _cacheService.SetAsync(key, value);

            var initialResult = await _cacheService.GetAsync<string>(key);
            initialResult.Should().NotBeNull();

            // Act
            await _cacheService.RemoveAsync(key);

            // Assert
            var result = await _cacheService.GetAsync<string>(key);
            result.Should().BeNull();
        }

        [Fact]
        public async Task RemoveAsync_WithNonExistentKey_ShouldNotThrow()
        {
            // Arrange
            var key = "non-existent-key";

            // Act & Assert
            await _cacheService.RemoveAsync(key); 
        }

        [Fact]
        public async Task RemoveByPatternAsync_WithMatchingKeys_ShouldRemoveAllMatches()
        {
            // Arrange
            var keys = new[] { "user:123", "user:456", "portfolio:789", "user:999" };
            var values = keys.Select(k => _fixture.Create<string>()).ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                await _cacheService.SetAsync(keys[i], values[i]);
            }

            // Act 
            await _cacheService.RemoveByPatternAsync("^user:.*");

            // Assert
            var userResult1 = await _cacheService.GetAsync<string>("user:123");
            var userResult2 = await _cacheService.GetAsync<string>("user:456");
            var userResult3 = await _cacheService.GetAsync<string>("user:999");
            var portfolioResult = await _cacheService.GetAsync<string>("portfolio:789");

            userResult1.Should().BeNull();
            userResult2.Should().BeNull();
            userResult3.Should().BeNull();
            portfolioResult.Should().NotBeNull(); 
        }

        [Fact]
        public async Task ExistsAsync_WithExistingKey_ShouldReturnTrue()
        {
            // Arrange
            var key = "test-key";
            var value = _fixture.Create<string>();
            await _cacheService.SetAsync(key, value);

            // Act
            var result = await _cacheService.ExistsAsync(key);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistentKey_ShouldReturnFalse()
        {
            // Arrange
            var key = "non-existent-key";

            // Act
            var result = await _cacheService.ExistsAsync(key);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GenerateKey_WithPrefixAndParameters_ShouldCreateCorrectKey()
        {
            // Arrange
            var prefix = "portfolio";
            var param1 = Guid.NewGuid();
            var param2 = "user";
            var param3 = 123;

            // Act
            var result = _cacheService.GenerateKey(prefix, param1, param2, param3);

            // Assert
            result.Should().Be($"{prefix}:{param1}:{param2}:{param3}");
        }

        [Fact]
        public void GenerateKey_WithNullParameters_ShouldHandleCorrectly()
        {
            // Arrange
            var prefix = "portfolio";

            // Act
            var result = _cacheService.GenerateKey(prefix, null, "test");

            // Assert
            result.Should().Be($"{prefix}:null:test");
        }

        [Theory]
        [InlineData(typeof(string), "test-string")]
        [InlineData(typeof(object), "test-object")]
        public async Task GetAsync_WithDifferentReferenceTypes_ShouldHandleCorrectly(Type type, object value)
        {
            // Arrange
            var key = $"test-{type.Name}";

            var setMethod = typeof(MemoryCacheService).GetMethod("SetAsync")!
                .MakeGenericMethod(type);
            await (Task)setMethod.Invoke(_cacheService, new[] { key, value })!;

            var getMethod = typeof(MemoryCacheService).GetMethod("GetAsync")!
                .MakeGenericMethod(type);
            var task = (Task)getMethod.Invoke(_cacheService, new[] { key })!;
            await task;

            // Act & Assert
            var resultProperty = task.GetType().GetProperty("Result")!;
            var result = resultProperty.GetValue(task);
            result.Should().Be(value);
        }

        [Fact]
        public async Task SetAsync_WithZeroExpiration_ShouldExpireImmediately()
        {
            // Arrange
            var key = "immediate-expiry";
            var value = _fixture.Create<string>();

            // Act
            await _cacheService.SetAsync(key, value, TimeSpan.Zero);

            // Assert
            var result = await _cacheService.GetAsync<string>(key);
            result.Should().BeNull();
        }

        [Fact]
        public async Task SetAsync_WithNegativeExpiration_ShouldExpireImmediately()
        {
            // Arrange
            var key = "negative-expiry";
            var value = _fixture.Create<string>();

            // Act
            await _cacheService.SetAsync(key, value, TimeSpan.FromSeconds(-1));

            // Assert
            var result = await _cacheService.GetAsync<string>(key);
            result.Should().BeNull();
        }

        [Fact]
        public async Task ConcurrentOperations_ShouldHandleCorrectly()
        {
            // Arrange
            var key = "concurrent-key";
            var tasks = new Task[100];

            // Act - Perform 100 concurrent operations
            for (int i = 0; i < 100; i++)
            {
                var index = i;
                tasks[i] = Task.Run(async () =>
                {
                    await _cacheService.SetAsync($"{key}-{index}", $"value-{index}");
                    var result = await _cacheService.GetAsync<string>($"{key}-{index}");
                    result.Should().Be($"value-{index}");
                });
            }

            // Assert
            await Task.WhenAll(tasks);
        }
    }
} 