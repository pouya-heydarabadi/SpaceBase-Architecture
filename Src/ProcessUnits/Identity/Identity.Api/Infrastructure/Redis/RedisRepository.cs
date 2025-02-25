using System.Text.Json;
using Identity.Api.Application.Interfaces;

namespace Identity.Api.Infrastructure.Redis;

public class RedisRepository<T>(IRedisService _redisService) : IRedisRepository<T>
{

    public async Task SetAsync(string key, T value)
    {
        var jsonData = JsonSerializer.Serialize(value);
        await _redisService.GetDatabase().StringSetAsync(key, jsonData);
    }

    public async Task<T?> GetAsync(string key)
    {
        var jsonData = await _redisService.GetDatabase().StringGetAsync(key);
        if (jsonData.IsNullOrEmpty)
            return default; 

        return JsonSerializer.Deserialize<T>(jsonData!);
    }

    public async Task DeleteAsync(string key)
    {
        await _redisService.GetDatabase().KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _redisService.GetDatabase().KeyExistsAsync(key);
    }
}