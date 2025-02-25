using Identity.Api.Application.Events.Events;
using Identity.Api.Infrastructure.SqlServer.Configurations;
using Identity.Api.Models.Entities;
using MediatR;

namespace Identity.Api.Application.Events.EventHandlers;

public sealed record UserCreatedEventQuery(UserCreated UserCreatedEvent) : IRequest<bool>;

public sealed class UserCreatedEventQueryHandler(AppDbContext _dbContext) : IRequestHandler<UserCreatedEventQuery, bool>
{

    public async Task<bool> Handle(UserCreatedEventQuery request, CancellationToken cancellationToken)
    {
        UserCreated userForCreate = request.UserCreatedEvent;
        User user = new User()
        {
            Id = userForCreate.Id,
            Address = userForCreate.Address,
            Email = userForCreate.Email,
            Name = userForCreate.Name,
            UpdatedAt = userForCreate.UpdatedAt,
            CreatedAt = userForCreate.CreatedAt,
            PhoneNumber = userForCreate.PhoneNumber,
            Age = userForCreate.Age,
        };
        
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
