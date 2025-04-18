using Identity.Api.Application.Events.Events;
using Identity.Api.Infrastructure.SqlServer.Configurations;
using Identity.Api.Models.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Application.Commands;

public sealed record UserCreateInDatabaseCommand(IEnumerable<UserCreated> UserCreatedEvents) : IRequest<bool>;

public sealed class UserCreateInDatabaseHandler : IRequestHandler<UserCreateInDatabaseCommand, bool>
{
    private readonly AppDbContext _dbContext;

    public UserCreateInDatabaseHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UserCreateInDatabaseCommand request, CancellationToken cancellationToken)
    {
        var existingPhoneNumbers = await _dbContext.Users
            .Select(u => u.PhoneNumber)
            .ToHashSetAsync(cancellationToken);

        var newUsers = request.UserCreatedEvents
            .Where(e => !existingPhoneNumbers.Contains(e.PhoneNumber))
            .Select(CreateUser)
            .ToList();
        
        if (newUsers.Count == 0)
            return true; // No new users to create
        
        await _dbContext.Users.AddRangeAsync(newUsers, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex)
        {
            // Log the exception
            // Consider more specific error handling based on the exception details
            return false;
        }
    }

    private static User CreateUser(UserCreated userForCreate) =>
        new()
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
}