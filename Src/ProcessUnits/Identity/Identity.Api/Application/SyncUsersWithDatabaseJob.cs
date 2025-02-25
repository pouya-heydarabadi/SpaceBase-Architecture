using Identity.Api.Application.Interfaces;
using Identity.Api.Infrastructure.SqlServer.Configurations;
using Identity.Api.Models.Entities;
using Quartz;
using StackExchange.Redis;

namespace Identity.Api.Application;

public sealed class SyncUsersWithDatabaseJob(IRedisService _redisService, IRedisRepository<User> _redisRepository,
    AppDbContext _dbContext):IJob
{

    public Task Execute(IJobExecutionContext context)
    {
        var dateThreshold = DateTime.Now.AddMinutes(-2);
        double thresholdTicks = dateThreshold.Ticks;

        RedisValue[] findUsers = _redisService.GetDatabase().SortedSetRangeByScoreAsync("users_sorted_set",
            thresholdTicks, double.PositiveInfinity).GetAwaiter().GetResult();
        
        if(findUsers.Length == 0)
            return Task.CompletedTask;
        
        foreach (var user in findUsers)
        {
            User? findUserFromSet = _redisRepository.GetAsync(user.ToString()).GetAwaiter().GetResult();
            if (findUserFromSet.CreatedAt == findUserFromSet.UpdatedAt)
            {
                _dbContext.Add(findUserFromSet);
            }
            else
            {
                _dbContext.Update(findUserFromSet);
            }
        }
        _dbContext.SaveChanges();
        
        return Task.CompletedTask;
    }
}