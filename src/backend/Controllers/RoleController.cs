using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Starter_App.src.backend.Models;
using Starter_App.src.backend.Attributes;
using Starter_App.src.backend.Services;

namespace Starter_App.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RoleController(RoleManager<AspNetRole> roleManager, UserManager<AspNetUser> userManager, AppStarterContext dbContext) : ControllerBase
    {
        private readonly RoleManager<AspNetRole> _roleManager = roleManager;
        private readonly UserManager<AspNetUser> _userManager = userManager;
        private readonly AppStarterContext _dbContext = dbContext;

        [RequirePermission(Permissions.CanViewManageRoles)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .OrderBy(r => r.Name)
                    .Select(r => new { id = r.Id, name = r.Name })
                    .ToListAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve roles", details = ex.Message });
            }
        }

        [RequirePermission(Permissions.CanAddRoles)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRole model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.RoleName))
                {
                    return BadRequest(new { Message = "Error: Role name is required." });
                }

                var trimmedName = model.RoleName.Trim();
                var normalizedName = trimmedName.ToUpperInvariant();

                var duplicateExists = await _roleManager.Roles
                    .AnyAsync(r => r.NormalizedName == normalizedName);

                if (duplicateExists)
                {
                    return Conflict(new { Message = $"Error: Role {trimmedName} already exists." });
                }

                var role = new AspNetRole
                {
                    Name = trimmedName,
                    NormalizedName = normalizedName
                };

                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Failed to create role.", Details = errors });
                }

                return Ok(new { Message = $"Role {trimmedName} created successfully.", Id = role.Id, Name = role.Name });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Failed to create role.", Details = ex.Message });
            }
        }

        [RequirePermission(Permissions.CanEditRoles)]
        [HttpPut("{roleId}")]
        public async Task<IActionResult> Update([FromRoute] string roleId, [FromBody] UpdateRoleRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.RoleName))
                {
                    return BadRequest(new { Message = "Error: Role name is required." });
                }

                var trimmedName = request.RoleName.Trim();
                var normalizedName = trimmedName.ToUpperInvariant();

                var role = await _roleManager.FindByIdAsync(roleId);

                if (role == null)
                {
                    return NotFound(new { Message = $"Error: Role with id {roleId} not found." });
                }

                var duplicateExists = await _roleManager.Roles
                    .AnyAsync(r => r.NormalizedName == normalizedName && r.Id != role.Id);

                if (duplicateExists)
                {
                    return Conflict(new { Message = $"Error: Role {trimmedName} already exists." });
                }

                role.Name = trimmedName;
                role.NormalizedName = normalizedName;

                var result = await _roleManager.UpdateAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Failed to update role.", Details = errors });
                }

                return Ok(new { Message = $"Role renamed to {trimmedName}.", Id = role.Id, Name = role.Name });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Failed to update role.", Details = ex.Message });
            }
        }

        [RequirePermission(Permissions.CanEditRoles)]
        [HttpDelete("{roleId}")]
        public async Task<IActionResult> Delete([FromRoute] string roleId)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);

                if (role == null)
                {
                    return NotFound(new { Message = $"Error: Role with id {roleId} not found." });
                }

                var members = await _userManager.GetUsersInRoleAsync(role.Name);
                foreach (var member in members)
                {
                    var removalResult = await _userManager.RemoveFromRoleAsync(member, role.Name);
                    if (!removalResult.Succeeded)
                    {
                        var removalErrors = string.Join(", ", removalResult.Errors.Select(e => e.Description));
                        return StatusCode(StatusCodes.Status500InternalServerError, new { Message = $"Error: Failed to remove user {member.UserName} from role before deletion.", Details = removalErrors });
                    }
                }

                var result = await _roleManager.DeleteAsync(role);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Failed to delete role.", Details = errors });
                }

                return Ok(new { Message = $"Role {role.Name} deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Failed to delete role.", Details = ex.Message });
            }
        }

        [RequirePermission(Permissions.CanViewManageRoles)]
        [HttpGet("{roleId}/users")]
        public async Task<IActionResult> GetRoleUsers([FromRoute] string roleId)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);

                if (role == null)
                {
                    return NotFound(new { Message = $"Error: Role with id {roleId} not found." });
                }

                var users = await _userManager.GetUsersInRoleAsync(role.Name);

                var result = users
                    .OrderBy(u => u.UserName)
                    .Select(u => new
                    {
                        id = u.Id,
                        userName = u.UserName,
                        email = u.Email,
                        phoneNumber = u.PhoneNumber,
                        studentId = u.StudentId
                    });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Failed to retrieve role members.", Details = ex.Message });
            }
        }

        [RequirePermission(Permissions.CanEditRoles)]
        [HttpPost("{roleId}/users/{userId}")]
        public async Task<IActionResult> AddUserToRole([FromRoute] string roleId, [FromRoute] string userId)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return NotFound(new { Message = $"Error: Role with id {roleId} not found." });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = $"Error: User with id {userId} not found." });
                }

                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    return Conflict(new { Message = $"User {user.UserName} is already assigned to role {role.Name}." });
                }

                var result = await _userManager.AddToRoleAsync(user, role.Name);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Failed to add user to role.", Details = errors });
                }

                return Ok(new { Message = $"User {user.UserName} added to role {role.Name}." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Failed to add user to role.", Details = ex.Message });
            }
        }

        [RequirePermission(Permissions.CanEditRoles)]
        [HttpDelete("{roleId}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromRole([FromRoute] string roleId, [FromRoute] string userId)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return NotFound(new { Message = $"Error: Role with id {roleId} not found." });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = $"Error: User with id {userId} not found." });
                }

                if (!await _userManager.IsInRoleAsync(user, role.Name))
                {
                    return NotFound(new { Message = $"User {user.UserName} is not assigned to role {role.Name}." });
                }

                var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Failed to remove user from role.", Details = errors });
                }

                return Ok(new { Message = $"User {user.UserName} removed from role {role.Name}." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Failed to remove user from role.", Details = ex.Message });
            }
        }

        [RequirePermission(Permissions.CanViewManageRoles)]
        [HttpGet("{roleId}/permissions")]
        public async Task<IActionResult> GetRolePermissions([FromRoute] string roleId)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return NotFound(new { Message = $"Error: Role with id {roleId} not found." });
                }

                var permissions = PermissionService.GetPermissionsForRole(role.Name, _dbContext);

                return Ok(new { roleId = role.Id, roleName = role.Name, permissions = permissions });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Failed to retrieve role permissions.", Details = ex.Message });
            }
        }

        [RequirePermission(Permissions.CanEditRoles)]
        [HttpPut("{roleId}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions([FromRoute] string roleId, [FromBody] UpdateRolePermissionsRequest request)
        {
            try
            {
                if (request == null || request.Permissions == null)
                {
                    return BadRequest(new { Message = "Error: Permissions array is required." });
                }

                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                {
                    return NotFound(new { Message = $"Error: Role with id {roleId} not found." });
                }

                var updateResult = await PermissionService.UpdateRolePermissionsAsync(role.Name, request.Permissions, _dbContext);
                if (!updateResult.success)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = updateResult.error });
                }

                return Ok(new { Message = $"Permissions updated for role {role.Name}.", roleId = role.Id, roleName = role.Name, permissions = request.Permissions });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error: Failed to update role permissions.", Details = ex.Message });
            }
        }
    }

    public class UpdateRolePermissionsRequest
    {
        [Required(ErrorMessage = "Error: Permissions array is required.")]
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class UpdateRoleRequest
    {
        [Required(ErrorMessage = "Error: Role name is required.")]
        public string RoleName { get; set; } = string.Empty;
    }
}