using Identity.Api.Application.Events.Events;
using Identity.Api.Infrastructure.SqlServer.Configurations;
using Identity.Api.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Application.Commands;

public sealed record UserUpdateInDatabaseCommand(List<UserUpdated> UserUpdatedEvents) : IRequest<bool>;

public sealed class UserUpdateInDatabaseHandler(AppDbContext _dbContext) : IRequestHandler<UserUpdateInDatabaseCommand, bool>
{
    public async Task<bool> Handle(UserUpdateInDatabaseCommand request, CancellationToken cancellationToken)
    {
        foreach (var requestUserUpdatedEvent in request.UserUpdatedEvents)
        {
            UserUpdated userForUpdate = requestUserUpdatedEvent;
            
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userForUpdate.Id);
            if (existingUser == null)
                continue;
               
            existingUser.Address = userForUpdate.Address;
            existingUser.Email = userForUpdate.Email;
            existingUser.Name = userForUpdate.Name;
            existingUser.UpdatedAt = userForUpdate.UpdatedAt;
            existingUser.PhoneNumber = userForUpdate.PhoneNumber;
            existingUser.Age = userForUpdate.Age;

            _dbContext.Users.Update(existingUser);
        }
        await _dbContext.SaveChangesAsync();
        return true;
    }
} 