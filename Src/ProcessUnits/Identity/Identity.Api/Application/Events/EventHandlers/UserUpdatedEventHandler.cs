using Confluent.Kafka;
using Identity.Api.Application.Events.Events;
using System.Text.Json;
using Identity.Api.Application.Commands;
using MediatR;

namespace Identity.Api.Application.Events.EventHandlers;

public class UserUpdatedEventHandler
{
    private readonly ConsumerConfig _config;
    private readonly IConsumer<Null, string> _consumer;
    private const string Topic = "UserUpdated-Topic";
    private readonly ISender _sender;
    
    public UserUpdatedEventHandler(ISender sender)
    {
        _config = new ConsumerConfig
        {
            BootstrapServers = "localhost:19092",
            GroupId = "user-updated-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            IsolationLevel = IsolationLevel.ReadCommitted,
            EnableAutoCommit = false,
        };
        _consumer = new ConsumerBuilder<Null, string>(_config).Build();
        _consumer.Subscribe(Topic);
        _sender = sender;
    }

    public async Task StartConsuming()
    {
        try
        {
            while (true)
            {
                var consumeResult = _consumer.Consume();
                var userUpdated = JsonSerializer.Deserialize<UserUpdated>(consumeResult.Message.Value);
                if(userUpdated is null)
                    throw new OperationCanceledException();

                await _sender.Send(new UserUpdateInDatabaseCommand(new List<UserUpdated>() { userUpdated }));
                _consumer.Commit(consumeResult);
            }
        }
        catch (OperationCanceledException)
        {
            _consumer.Close();
        }
    }
} 