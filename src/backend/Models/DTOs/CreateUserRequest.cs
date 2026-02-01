namespace Starter_App.src.backend.Models.DTOs;

// DTO for creating a user
public class CreateUserRequest
{
    public string StudentId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new List<string> { "User" };
    public bool IsActive { get; set; } = true;
}

