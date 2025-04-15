using Identity.Api.Application.Commands;
using Identity.Api.Application.Events.Events;
using Identity.Api.Application.Events.EventHandlers;
using MediatR;
using Quartz;

namespace Identity.Api.Application.Jobs;

public class UserUpdatedDatabaseSyncJob : IJob
{
    private readonly UserUpdatedEventHandler _eventHandler;
    private readonly IMediator _mediator;

    public UserUpdatedDatabaseSyncJob(UserUpdatedEventHandler eventHandler, IMediator mediator)
    {
        _eventHandler = eventHandler;
        _mediator = mediator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _eventHandler.StartConsuming();
    }
} 