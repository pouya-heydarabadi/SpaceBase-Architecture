using Identity.Api.Application.Interfaces;
using StackExchange.Redis;
namespace Identity.Api.Infrastructure.Redis;

public sealed class RedisService(string? connectionString) : IRedisService
{
    private readonly Lazy<ConnectionMultiplexer> _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
    {
        if (connectionString != null)
            return ConnectionMultiplexer.Connect(connectionString);
        throw new RedisException("No connection string was provided.");
    });

    public IDatabase GetDatabase()
    {
        return _lazyConnection.Value.GetDatabase();
    }
}