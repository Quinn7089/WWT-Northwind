namespace Starter_App.src.backend.Models.DTOs;

// DTO for updating user profile
public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; }
}

