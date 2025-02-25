using System.Text.Json;
using Identity.Api.Application.Interfaces;
using Identity.Api.Models.Entities;
using StackExchange.Redis;

namespace Identity.Api.Infrastructure.Redis;

public class RedisRepository<T>(IRedisService _redisService) : IRedisRepository<T>
{

    public async Task SetAsync(string key, T value)
    {
        string originalKey = "User-" + key;

        var jsonData = JsonSerializer.Serialize(value);
        await _redisService.GetDatabase().StringSetAsync(originalKey, jsonData);
    }
    
    public async Task SortedSetAsync(string key, T value)
    {
        if(value is not User user)
            throw new Exception("Your Value Is Not User");
        
        string originalKey = "User-" + key;
        double ticks = user.UpdatedAt.Ticks;
        
        await _redisService.GetDatabase().SortedSetAddAsync("users_sorted_set", originalKey, ticks);

        var jsonData = JsonSerializer.Serialize(value);

        await _redisService.GetDatabase().StringSetAsync(originalKey, jsonData);
    }

    public async Task<T?> GetAsync(string key)
    {
        string originalKey = key;
        
        if(!key.Contains("User-"))
            originalKey = "User-" + key;

        RedisValue jsonData = await _redisService.GetDatabase().StringGetAsync(originalKey);
        if (jsonData.IsNullOrEmpty)
            return default; 

        return JsonSerializer.Deserialize<T>(jsonData!);
    }

    public async Task DeleteAsync(string key)
    {
        string originalKey = key;
        
        if(!key.Contains("User-"))
            originalKey = "User-" + key;
        
        await _redisService.GetDatabase().KeyDeleteAsync(originalKey);
        await _redisService.GetDatabase().SortedSetRemoveAsync("users_sorted_set", originalKey);

        
    }

    public async Task<bool> ExistsAsync(string key)
    {
        string originalKey = key;
        
        if(!key.Contains("User-"))
            originalKey = "User-" + key;
        
        return await _redisService.GetDatabase().KeyExistsAsync(originalKey);
    }
}