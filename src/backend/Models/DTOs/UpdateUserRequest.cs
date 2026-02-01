namespace Starter_App.src.backend.Models.DTOs;

// DTO for updating a user
public class UpdateUserRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; }
    public bool? EmailConfirmed { get; set; }
    public bool? TwoFactorEnabled { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> Roles { get; set; } = new List<string> { "User" };
}

