using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Starter_App.src.backend.Models;

namespace Starter_App.src.backend.Services
{
    public static class Permissions
    {
        public const string CanViewHome = "canViewHome";
        public const string CanViewManageUsers = "canViewManageUsers";
        public const string CanViewManageRoles = "canViewManageRoles";

        public const string CanAddUsers = "canAddUsers";
        public const string CanEditUsers = "canEditUsers";

        public const string CanAddRoles = "canAddRoles";
        public const string CanEditRoles = "canEditRoles";
    }

    public static class Roles
    {
        public const string Temp = "temp";
        public const string User = "User";
        public const string Admin = "Admin";
        public const string Manager = "Manager";
    }

    public static class PermissionService
    {
        public static async Task<List<string>> GetPermissionsForRoleAsync(string roleName, AppStarterContext dbContext)
        {
            if (string.IsNullOrEmpty(roleName) || dbContext == null)
                return new List<string>();

            var roleNameLower = roleName.ToLower();
            var role = await dbContext.AspNetRoles
                .FirstOrDefaultAsync(r => r.Name != null && r.Name.ToLower() == roleNameLower);
            
            if (role == null || string.IsNullOrEmpty(role.Permissions))
                return new List<string>();

            try
            {
                var permissions = JsonSerializer.Deserialize<List<string>>(role.Permissions);
                return permissions ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public static List<string> GetPermissionsForRole(string roleName, AppStarterContext dbContext)
        {
            if (string.IsNullOrEmpty(roleName) || dbContext == null)
                return new List<string>();

            var roleNameLower = roleName.ToLower();
            var role = dbContext.AspNetRoles
                .FirstOrDefault(r => r.Name != null && r.Name.ToLower() == roleNameLower);
            
            if (role == null || string.IsNullOrEmpty(role.Permissions))
                return new List<string>();

            try
            {
                var permissions = JsonSerializer.Deserialize<List<string>>(role.Permissions);
                return permissions ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public static async Task<List<string>> GetPermissionsForRolesAsync(IEnumerable<string> roleNames, AppStarterContext dbContext)
        {
            if (roleNames == null || !roleNames.Any() || dbContext == null)
                return new List<string>();

            var roleNamesList = roleNames.Select(rn => rn?.ToLowerInvariant()).Where(rn => !string.IsNullOrEmpty(rn)).ToList();
            if (!roleNamesList.Any())
                return new List<string>();

            var allPermissions = new HashSet<string>();

            var roles = await dbContext.AspNetRoles
                .Where(r => r.Name != null && roleNamesList.Contains(r.Name.ToLower()))
                .Select(r => new { r.Name, r.Permissions })
                .ToListAsync();

            foreach (var role in roles)
            {
                if (!string.IsNullOrEmpty(role.Permissions))
                {
                    try
                    {
                        var permissions = JsonSerializer.Deserialize<List<string>>(role.Permissions);
                        if (permissions != null)
                        {
                            foreach (var permission in permissions)
                            {
                                allPermissions.Add(permission);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return allPermissions.ToList();
        }

        public static List<string> GetPermissionsForRoles(IEnumerable<string> roleNames, AppStarterContext dbContext)
        {
            if (roleNames == null || !roleNames.Any() || dbContext == null)
                return new List<string>();

            var roleNamesList = roleNames.Select(rn => rn?.ToLowerInvariant()).Where(rn => !string.IsNullOrEmpty(rn)).ToList();
            if (!roleNamesList.Any())
                return new List<string>();

            var allPermissions = new HashSet<string>();

            var roles = dbContext.AspNetRoles
                .Where(r => r.Name != null && roleNamesList.Contains(r.Name.ToLower()))
                .Select(r => new { r.Name, r.Permissions })
                .ToList();

            foreach (var role in roles)
            {
                if (!string.IsNullOrEmpty(role.Permissions))
                {
                    try
                    {
                        var permissions = JsonSerializer.Deserialize<List<string>>(role.Permissions);
                        if (permissions != null)
                        {
                            foreach (var permission in permissions)
                            {
                                allPermissions.Add(permission);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return allPermissions.ToList();
        }

        public static bool RoleHasPermission(string roleName, string permission, AppStarterContext dbContext)
        {
            var permissions = GetPermissionsForRole(roleName, dbContext);
            return permissions.Contains(permission);
        }

        public static bool AnyRoleHasPermission(IEnumerable<string> roleNames, string permission, AppStarterContext dbContext)
        {
            if (roleNames == null || !roleNames.Any())
                return false;

            var allPermissions = GetPermissionsForRoles(roleNames, dbContext);
            return allPermissions.Contains(permission);
        }

        public static async Task<(bool success, string error)> UpdateRolePermissionsAsync(string roleName, List<string> permissions, AppStarterContext dbContext)
        {
            if (dbContext == null)
            {
                return (false, "DbContext is required to update role permissions.");
            }

            try
            {
                var roleNameLower = roleName.ToLower();
                var role = await dbContext.AspNetRoles
                    .FirstOrDefaultAsync(r => r.Name != null && r.Name.ToLower() == roleNameLower);

                if (role == null)
                {
                    return (false, $"Role '{roleName}' not found in database.");
                }

                var permissionsJson = JsonSerializer.Serialize(permissions ?? new List<string>(), new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                role.Permissions = permissionsJson;
                await dbContext.SaveChangesAsync();

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Error updating role permissions in database: {ex.Message}");
            }
        }
    }
}