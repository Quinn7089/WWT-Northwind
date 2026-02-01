using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Starter_App.src.backend.Models;
using Starter_App.src.backend.Models.DTOs;
using Starter_App.src.backend.Attributes;
using Starter_App.src.backend.Services;

public class ManageUserController(UserManager<AspNetUser> userManager, IConfiguration config, RoleManager<AspNetRole> roleManager, AppStarterContext dataContext) : Controller
{
    private readonly AppStarterContext _dataContext = dataContext;
    private readonly UserManager<AspNetUser> _userManager = userManager;
    private readonly IConfiguration _config = config;
    private readonly RoleManager<AspNetRole> _roleManager = roleManager;

    public ActionResult UserList() => View(_dataContext.Users.ToList());

    // API endpoint for Vue.js component
    [RequirePermission(Permissions.CanViewManageUsers)]
    [HttpGet]
    [Route("api/users")]
    public async Task<IActionResult> GetUsersApi()
    {
        try
        {
            var users = await _dataContext.AspNetUsers
            .Select(u => new {
                id = u.Id,
                studentId = u.StudentId,
                firstName = u.FirstName,
                lastName = u.LastName,
                userName = u.UserName,
                email = u.Email,
                emailConfirmed = u.EmailConfirmed,
                phoneNumber = u.PhoneNumber,
                lockoutEnabled = u.LockoutEnabled,
                lockoutEnd = u.LockoutEnd,
                isActive = u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow,
                roles = (from userRoles in _dataContext.UserRoles join roles in _dataContext.Roles on userRoles.RoleId equals roles.Id where userRoles.UserId == u.Id select roles.Name).ToList()
            })
            .ToListAsync();
            
            return Json(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve users", details = ex.Message });
        }
    }

    // API endpoint for getting a single user
    [RequirePermission(Permissions.CanViewManageUsers)]
    [HttpGet]
    [Route("api/users/{id}")]
    public async Task<IActionResult> GetUserApi(string id)
    {

        try
        {
            var user = await _dataContext.AspNetUsers.Where(u => u.Id == id)
            .Select(u => new {
                id = u.Id,
                studentId = u.StudentId,
                firstName = u.FirstName,
                lastName = u.LastName,
                userName = u.UserName,
                email = u.Email,
                emailConfirmed = u.EmailConfirmed,
                phoneNumber = u.PhoneNumber,
                lockoutEnabled = u.LockoutEnabled,
                lockoutEnd = u.LockoutEnd,
                isActive = u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow,
                roles = (from userRoles in _dataContext.UserRoles join roles in _dataContext.Roles on userRoles.RoleId equals roles.Id where userRoles.UserId == id select roles.Name).ToList()
            })
            .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Json(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve user", details = ex.Message });
        }
    }

    // API endpoint for deactivating a user
    [RequirePermission(Permissions.CanEditUsers)]
    [HttpPatch]
    [Route("api/users/{id}/deactivate")]
    public async Task<IActionResult> DeactivateUserApi(string id)
    {
        try
        {
            var user = await _dataContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Set lockout end to max value to effectively deactivate the user
            user.LockoutEnd = DateTimeOffset.MaxValue;
            user.LockoutEnabled = true;
            
            await _dataContext.SaveChangesAsync();

            return Ok(new { message = "User deactivated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to deactivate user", details = ex.Message });
        }
    }

    // API endpoint for reactivating a user
    [RequirePermission(Permissions.CanEditUsers)]
    [HttpPatch]
    [Route("api/users/{id}/reactivate")]
    public async Task<IActionResult> ReactivateUserApi(string id)
    {
        try
        {
            var user = await _dataContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Clear lockout to reactivate the user
            user.LockoutEnd = null;
            
            await _dataContext.SaveChangesAsync();

            return Ok(new { message = "User reactivated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to reactivate user", details = ex.Message });
        }
    }

    // API endpoint for updating a user
    [RequirePermission(Permissions.CanEditUsers)]
    [HttpPut]
    [Route("api/users/{id}")]
    public async Task<IActionResult> UpdateUserApi(string id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { error = "Request body is required" });
            }

            var user = await _dataContext.AspNetUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.UserName))
            {
                return BadRequest(new { error = "Email and UserName are required" });
            }

            // Check if email is already taken by another user
            var existingUserWithEmail = await _dataContext.AspNetUsers
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Id != id);
            if (existingUserWithEmail != null)
            {
                return Conflict(new { error = "Email is already taken by another user" });
            }

            // Check if username is already taken by another user
            var existingUserWithUserName = await _dataContext.AspNetUsers
                .FirstOrDefaultAsync(u => u.UserName == request.UserName && u.Id != id);
            if (existingUserWithUserName != null)
            {
                return Conflict(new { error = "Username is already taken by another user" });
            }

            // Update user properties
            user.StudentId = request.StudentId;
            user.Email = request.Email;
            user.UserName = request.UserName;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            
            // Only update these properties if they are provided
            if (request.EmailConfirmed.HasValue)
            {
                user.EmailConfirmed = request.EmailConfirmed.Value;
            }
            
            // Update active status via lockout
            if (request.IsActive)
            {
                user.LockoutEnd = null; // User is active
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.MaxValue; // User is deactivated
            }

            await _dataContext.SaveChangesAsync();

            // Update roles if provided
            if (request.Roles != null && request.Roles.Any())
            {
                await AssignUserRolesAsync(user.Id, request.Roles);
            }

            // Get the assigned roles
            var assignedRoles = await GetUserRolesAsync(user.Id);

            // Return updated user data
            var updatedUserData = new
            {
                id = user.Id,
                studentId = user.StudentId,
                firstName = user.FirstName,
                lastName = user.LastName,
                userName = user.UserName,
                email = user.Email,
                emailConfirmed = user.EmailConfirmed,
                phoneNumber = user.PhoneNumber,
                lockoutEnabled = user.LockoutEnabled,
                lockoutEnd = user.LockoutEnd,
                isActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
                roles = assignedRoles
            };

            return Json(updatedUserData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update user", details = ex.Message });
        }
    }

    // API endpoint for creating a new user
    [RequirePermission(Permissions.CanAddUsers)]
    [HttpPost]
    [Route("api/users")]
    public async Task<IActionResult> CreateUserApi([FromBody] CreateUserRequest request)
    {
        try
        {
            // Validate the request
            if (request == null)
            {
                return BadRequest(new { error = "Request body is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            {
                return BadRequest(new { error = "Email, FirstName, and LastName are required" });
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Conflict(new { error = "A user with this email already exists" });
            }

            // Generate username from email
            var userName = request.Email;

            // Create the new user
            var newUser = new AspNetUser
            {
                StudentId = request.StudentId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = userName,
                Email = request.Email,
                EmailConfirmed = false, // Email confirmation will be required
                PhoneNumber = request.PhoneNumber,
                LockoutEnabled = true,
                LockoutEnd = request.IsActive ? null : DateTimeOffset.MaxValue, // Set lockout if not active
            };

            // Generate a temporary password (in a real application, you might want to send this via email)
            var tempPassword = GenerateRandomPassword();

            // create the user using UserManager
            var result = await _userManager.CreateAsync(newUser, tempPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { error = "Failed to create user", details = errors });
            }

            // Assign roles
            var rolesToAssign = request.Roles != null && request.Roles.Any() ? request.Roles : new List<string> { "User" };
            await AssignUserRolesAsync(newUser.Id, rolesToAssign);

            // Get the assigned roles
            var assignedRoles = await GetUserRolesAsync(newUser.Id);

            // Return the created user data
            var createdUser = new
            {
                id = newUser.Id,
                studentId = newUser.StudentId,
                firstName = newUser.FirstName,
                lastName = newUser.LastName,
                userName = newUser.UserName,
                email = newUser.Email,
                emailConfirmed = newUser.EmailConfirmed,
                phoneNumber = newUser.PhoneNumber,
                lockoutEnabled = newUser.LockoutEnabled,
                lockoutEnd = newUser.LockoutEnd,
                isActive = newUser.LockoutEnd == null || newUser.LockoutEnd <= DateTimeOffset.UtcNow,
                roles = assignedRoles,
                temporaryPassword = tempPassword // Include temporary password in response
            };

            return CreatedAtAction(nameof(GetUsersApi), new { id = newUser.Id }, createdUser);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create user", details = ex.Message });
        }
    }

    private string GenerateRandomPassword()
    {
        // Generate a random password that meets ASP.NET Identity requirements
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var password = new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        
        // Ensure it has at least one uppercase, lowercase, digit, and special character
        return "Temp" + password.Substring(4) + "1!";
    }

    // assign user roles (multiple roles support)
    private async Task<bool> AssignUserRolesAsync(string userId, List<string> roleNames)
    {
        try
        {
            // Remove existing roles for this user
            var existingUserRoles = await _dataContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            if (existingUserRoles.Count > 0)
            {
                _dataContext.UserRoles.RemoveRange(existingUserRoles);
                await _dataContext.SaveChangesAsync();
            }


            var roles = await _dataContext.Roles
            .Where(r => roleNames.Contains(r.Name))
            .Select(r => new {r.Id, r.Name})
            .ToDictionaryAsync(r => r.Name, r => r.Id);

            // Add new roles
            foreach (var roleName in roleNames)
            {
                if (roles.TryGetValue(roleName, out var roleId))
                {
                    _dataContext.UserRoles.Add(new IdentityUserRole<String>
                    {
                        UserId = userId,
                        RoleId = roleId
                    });
                }
            }
            await _dataContext.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // assign single user role (for backward compatibility)
    private async Task<bool> AssignUserRoleAsync(string userId, string roleName)
    {
        return await AssignUserRolesAsync(userId, new List<string> { roleName });
    }

    // get user's roles via EF
    private async Task<List<string>> GetUserRolesAsync(string userId)
    {
        try
        {
            var roles = await _dataContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(
                _dataContext.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name
            ).ToListAsync();

            return roles;
        }
        catch (Exception)
        {
            return new List<string>();
        }
    }

    // API endpoint for getting current user's profile
    [RequirePermission(Permissions.CanViewHome)]
    [HttpGet]
    [Route("api/profile")]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        try
        {
            var currentUserName = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserName))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var user = await _dataContext.AspNetUsers.FirstOrDefaultAsync(u => u.UserName == currentUserName);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Get the assigned roles
            var assignedRoles = await GetUserRolesAsync(user.Id);

            var userProfile = new
            {
                id = user.Id,
                userName = user.UserName,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                phoneNumber = user.PhoneNumber,
                emailConfirmed = user.EmailConfirmed,
                roles = assignedRoles
            };

            return Json(userProfile);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve user profile", details = ex.Message });
        }
    }

    // API endpoint for updating current user's profile
    [RequirePermission(Permissions.CanViewHome)]
    [HttpPut]
    [Route("api/profile")]
    public async Task<IActionResult> UpdateCurrentUserProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var currentUserName = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserName))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var user = await _dataContext.AspNetUsers.FirstOrDefaultAsync(u => u.UserName == currentUserName);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Update only the allowed profile fields
            user.FirstName = request.FirstName?.Trim() ?? user.FirstName;
            user.LastName = request.LastName?.Trim() ?? user.LastName;
            user.PhoneNumber = request.PhoneNumber?.Trim();

            await _dataContext.SaveChangesAsync();

            // Get the updated user with roles in a single query
            var updatedUserWithRoles = await (
                from u in _dataContext.Users
                where u.Id == user.Id
                select new
                {
                    id = u.Id,
                    userName = u.UserName,
                    email = u.Email,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    phoneNumber = u.PhoneNumber,
                    emailConfirmed = u.EmailConfirmed,
                    roles = _dataContext.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(_dataContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                        .ToList()
                }
            ).FirstOrDefaultAsync();

            return Json(updatedUserWithRoles);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update user profile", details = ex.Message });
        }
    }
}
