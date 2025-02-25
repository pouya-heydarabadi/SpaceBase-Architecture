using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Models.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Range(0, 120)]
    public int Age { get; set; }
    public string Address { get; set; } = string.Empty;
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}