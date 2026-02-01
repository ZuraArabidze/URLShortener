using StackExchange.Redis;
using System.Text.Json;
using URLShortener.Core.Interfaces;

namespace URLShortener.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;
        private readonly IConnectionMultiplexer _redis;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, serializedValue, expiration);
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }
    }
}
