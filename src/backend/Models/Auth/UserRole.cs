using System.ComponentModel.DataAnnotations;

public class UserRole
{
    [Required(ErrorMessage = "Error: Username is required.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Error: Role name is required.")]
    public string? RoleName{ get; set; }
}