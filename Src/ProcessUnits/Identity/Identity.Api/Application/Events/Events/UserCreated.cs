using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Application.Events.Events;

public sealed record UserCreated( Guid Id, 
     string Name, 
     string Email, 
     int Age, 
     string Address, 
     string PhoneNumber, 
     DateTime CreatedAt, 
     DateTime UpdatedAt);