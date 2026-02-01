using System.ComponentModel.DataAnnotations;

public class CreateRole
{
    [Required(ErrorMessage = "Error: Role name is required.")]
    public string RoleName { get; set; }
}