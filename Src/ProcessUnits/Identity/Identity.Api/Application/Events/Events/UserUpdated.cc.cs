namespace Identity.Api.Application.Events.Events;

public sealed record UserUpdated(Guid Id, string Name, string Email, int Age, string Address, string PhoneNumber, DateTime CreatedAt, DateTime UpdatedAt) 
    :UserCreated(Id, Name, Email, Age, Address, PhoneNumber, CreatedAt, UpdatedAt)
{
    
}