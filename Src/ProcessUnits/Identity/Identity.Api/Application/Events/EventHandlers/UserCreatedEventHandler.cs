using Confluent.Kafka;
using Identity.Api.Application.Events.Events;
using System.Text.Json;
using Identity.Api.Application.Commands;
using MediatR;

namespace Identity.Api.Application.Events.EventHandlers;

public class UserCreatedEventHandler
{
    private readonly ConsumerConfig _config;
    private readonly IConsumer<Null, string> _consumer;
    private const string Topic = "UserCreated-Topic";
    private readonly ISender _sender;
    
    public UserCreatedEventHandler(ISender sender)
    {
        _config = new ConsumerConfig
        {
            BootstrapServers = "localhost:19092",
            GroupId = "user-created-group",
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
            List<UserCreated?> userCreatedEvents = new List<UserCreated?>();
            
            while (true)
            {
                var consumeResult = _consumer.Consume();
                var userCreated = JsonSerializer.Deserialize<UserCreated>(consumeResult.Message.Value);
                if(userCreated is null)
                    throw new OperationCanceledException();

                await _sender.Send(new UserCreateInDatabaseCommand(new List<UserCreated>() { userCreated }));
                _consumer.Commit(consumeResult);
            }
        }
        catch (OperationCanceledException)
        {
            _consumer.Close();
        }
    }
}