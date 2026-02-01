using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Starter_App.src.backend.Models;

public partial class AspNetRole : IdentityRole
{
    [Column(TypeName = "nvarchar(max)")]
    public string? Permissions { get; set; }
}
