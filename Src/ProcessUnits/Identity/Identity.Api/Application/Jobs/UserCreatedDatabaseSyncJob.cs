using Identity.Api.Application.Commands;
using Identity.Api.Application.Events.Events;
using Identity.Api.Application.Events.EventHandlers;
using MediatR;
using Quartz;

namespace Identity.Api.Application.Jobs;

public class UserCreatedDatabaseSyncJob : IJob
{
    private readonly UserCreatedEventHandler _eventHandler;

    public UserCreatedDatabaseSyncJob(UserCreatedEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public  Task Execute(IJobExecutionContext context)
    { 
        _eventHandler.StartConsuming().GetAwaiter().GetResult();
        return Task.CompletedTask;
    }
} 