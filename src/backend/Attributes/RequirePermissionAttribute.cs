using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Starter_App.src.backend.Services;
using Starter_App.src.backend.Models;

namespace Starter_App.src.backend.Attributes
{

    public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _requiredPermissions;

        public RequirePermissionAttribute(params string[] permissions)
        {
            _requiredPermissions = permissions ?? throw new ArgumentNullException(nameof(permissions));

            if (_requiredPermissions.Length == 0)
                throw new ArgumentException("At least one permission is required", nameof(permissions));
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.Any(em => em is AllowAnonymousAttribute))
                return;

            var user = context.HttpContext.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var userPermissions = user.Claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToList();

            if (userPermissions.Count == 0 && userRoles.Count > 0)
            {
                var dbContext = context.HttpContext.RequestServices.GetService<AppStarterContext>();
                if (dbContext != null)
                {
                    userPermissions = PermissionService.GetPermissionsForRoles(userRoles, dbContext);
                }
            }

            bool hasPermission = _requiredPermissions.Any(requiredPerm =>
                userPermissions.Contains(requiredPerm));

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}

