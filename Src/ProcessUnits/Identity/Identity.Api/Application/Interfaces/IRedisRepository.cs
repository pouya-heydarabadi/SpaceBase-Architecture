namespace Identity.Api.Application.Interfaces;

public interface IRedisRepository<T>
{
    Task SetAsync(string key, T value);
    Task SortedSetAsync(string key, T value);
    Task<T?> GetAsync(string key);
    Task DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
}