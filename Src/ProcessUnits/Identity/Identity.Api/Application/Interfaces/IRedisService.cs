
using StackExchange.Redis;

namespace Identity.Api.Application.Interfaces;


public interface IRedisService
{
    IDatabase GetDatabase();
}