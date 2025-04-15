using System.Text.Json;
using Confluent.Kafka;
using Identity.Api.Application.Events.Events;
using Identity.Api.Application.Interfaces;
using Identity.Api.Models.Entities;
using Quartz;
using StackExchange.Redis;

namespace Identity.Api.Application.Jobs;

public sealed class SendDataToMessageQueueJob(IRedisService _redisService, IRedisRepository<User> _redisRepository) : IJob
{
    private const string UserCreatedTopic = "UserCreated-Topic";
    private const string UserUpdatedTopic = "UserUpdated-Topic";

    public Task Execute(IJobExecutionContext context)
    {
        var dateThreshold = DateTime.Now.AddMinutes(-2);
        double thresholdTicks = dateThreshold.Ticks;

        RedisValue[] findUsers = _redisService.GetDatabase().SortedSetRangeByScoreAsync("users_sorted_set",
            thresholdTicks, double.PositiveInfinity).GetAwaiter().GetResult();

        if (findUsers.Length == 0)
            return Task.CompletedTask;

        var config = new ProducerConfig()
        {
            BootstrapServers = "localhost:19092", AllowAutoCreateTopics = true, EnableIdempotence = true
        };

        var producer = new ProducerBuilder<Null, string>(config).Build();

        foreach (var user in findUsers)
        {
            User? findUserFromSet = _redisRepository.GetAsync(user.ToString()).GetAwaiter().GetResult();
            if (findUserFromSet is null)
                continue;

            if (findUserFromSet.CreatedAt == findUserFromSet.UpdatedAt)
            {
                UserCreated userCreated = new(
                    findUserFromSet.Id,
                    findUserFromSet.Name,
                    findUserFromSet.Email,
                    findUserFromSet.Age,
                    findUserFromSet.Address,
                    findUserFromSet.PhoneNumber,
                    findUserFromSet.CreatedAt,
                    findUserFromSet.UpdatedAt);

               var result =  producer.ProduceAsync(topic: UserCreatedTopic, new Message<Null, string>()
               {
                    Value = JsonSerializer.Serialize(userCreated)
               }).GetAwaiter().GetResult();

            }

            else
            {
                UserUpdated userUpdated = new(
                    findUserFromSet.Id,
                    findUserFromSet.Name,
                    findUserFromSet.Email,
                    findUserFromSet.Age,
                    findUserFromSet.Address,
                    findUserFromSet.PhoneNumber,
                    findUserFromSet.CreatedAt,
                    findUserFromSet.UpdatedAt);

                producer.ProduceAsync(topic: UserUpdatedTopic, new Message<Null, string>()
                {
                    Value = JsonSerializer.Serialize(userUpdated)
                }).GetAwaiter().GetResult();
            }
        }
        return Task.CompletedTask;
    }
}
        
