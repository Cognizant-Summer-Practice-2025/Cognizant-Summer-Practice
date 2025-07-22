using backend_portfolio.Services.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace backend_portfolio.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private static readonly ConcurrentHashMap<string, bool> _cacheKeys = new();

        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var value = _memoryCache.Get<string>(key);
                if (value == null)
                {
                    return null;
                }

                return await Task.FromResult(JsonSerializer.Deserialize<T>(value));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            try
            {
                var jsonValue = JsonSerializer.Serialize(value);
                var options = new MemoryCacheEntryOptions();
                
                if (expiry.HasValue)
                {
                    options.SetAbsoluteExpiration(expiry.Value);
                }
                else
                {
                    // Default cache expiry for portfolio data: 5 minutes
                    options.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                }

                // Remove from cache when memory pressure is high
                options.SetPriority(CacheItemPriority.Normal);
                
                // Add callback to remove key from tracking when expired
                options.RegisterPostEvictionCallback((k, v, r, s) => 
                {
                    _cacheKeys.TryRemove(k.ToString()!, out _);
                });

                _memoryCache.Set(key, jsonValue, options);
                _cacheKeys.TryAdd(key, true);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                var keysToRemove = _cacheKeys.Keys
                    .Where(key => regex.IsMatch(key))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.TryRemove(key, out _);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await Task.FromResult(_memoryCache.TryGetValue(key, out _));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }

        public string GenerateKey(string prefix, params object[] parameters)
        {
            var keyParts = new List<string> { prefix };
            
            foreach (var param in parameters)
            {
                keyParts.Add(param?.ToString() ?? "null");
            }
            
            return string.Join(":", keyParts);
        }
    }

    // Simple concurrent hash map implementation
    public class ConcurrentHashMap<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new();
        private readonly ReaderWriterLockSlim _lock = new();

        public IEnumerable<TKey> Keys
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _dictionary.Keys.ToList();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_dictionary.ContainsKey(key))
                    return false;

                _dictionary[key] = value;
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryRemove(TKey key, out TValue? value)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_dictionary.TryGetValue(key, out value))
                {
                    _dictionary.Remove(key);
                    return true;
                }
                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
