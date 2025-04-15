using Confluent.Kafka;
using Identity.Api.Application.Events.Events;
using System.Text.Json;
using Identity.Api.Application.Commands;
using MediatR;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Identity.Api.Application.Events.EventHandlers;

public class UserCreatedEventHandler
{
    private readonly ConsumerConfig _config;
    private readonly IConsumer<Null, string> _consumer;
    private const string Topic = "UserCreated-Topic";
    private readonly ISender _sender;
    private readonly ILogger _logger;
    
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
        _logger = Log.ForContext<UserCreatedEventHandler>();
    }

    public async Task StartConsuming()
    {
        try
        {
            _logger.Information("Starting to consume UserCreated events from Kafka");
            
            while (true)
            {
                var consumeResult = _consumer.Consume();
                _logger.Information("Received message from Kafka: {Message}", consumeResult.Message.Value);
                
                var userCreated = JsonSerializer.Deserialize<UserCreated>(consumeResult.Message.Value);
                if(userCreated is null)
                {
                    _logger.Error("Failed to deserialize UserCreated event");
                    throw new OperationCanceledException();
                }

                _logger.Information("Processing UserCreated event for user {UserId}", userCreated.Id);
                await _sender.Send(new UserCreateInDatabaseCommand(new List<UserCreated>() { userCreated }));
                _consumer.Commit(consumeResult);
                _logger.Information("Successfully processed UserCreated event for user {UserId}", userCreated.Id);
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.Warning(ex, "Operation was canceled");
            // _consumer.Close();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred while consuming UserCreated events");
            throw;
        }
    }
}