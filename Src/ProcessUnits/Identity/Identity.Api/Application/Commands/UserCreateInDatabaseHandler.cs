using Identity.Api.Application.Events.Events;
using Identity.Api.Infrastructure.SqlServer.Configurations;
using Identity.Api.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Application.Commands;

public sealed record UserCreateInDatabaseCommand(List<UserCreated> UserCreatedEvents) : IRequest<bool>;

public sealed class UserCreateInDatabaseHandler(AppDbContext _dbContext) : IRequestHandler<UserCreateInDatabaseCommand, bool>
{

    public async Task<bool> Handle(UserCreateInDatabaseCommand request, CancellationToken cancellationToken)
    {
        foreach (var requestUserCreatedEvent in request.UserCreatedEvents)
        {

            UserCreated userForCreate = requestUserCreatedEvent;
            
            if(_dbContext.Users.Any(x=> x.Id == requestUserCreatedEvent.Id))
                continue;
               
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
        }
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
