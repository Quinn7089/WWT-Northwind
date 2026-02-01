import rolePermissionsConfig from '../../shared/role-permissions.json'

export const ROLES = {
    TEMP: 'temp',
    USER: 'User',
    ADMIN: 'Admin',
    MANAGER: 'Manager'
}

export const PERMISSIONS = rolePermissionsConfig.permissions

export const ROUTE_PERMISSIONS = {
    '/': PERMISSIONS.canViewHome,
    '/user': PERMISSIONS.canViewHome,
    '/user-management': PERMISSIONS.canViewManageUsers,
    '/role-management': PERMISSIONS.canViewManageRoles,
}

export const ROLE_PERMISSIONS = {}
for (const [roleName, roleData] of Object.entries(rolePermissionsConfig.roles)) {
    ROLE_PERMISSIONS[roleName] = roleData.permissions
}

export function getPermissionsForRole(role) {
    return ROLE_PERMISSIONS[role] || []
}

export function roleHasPermission(role, permission) {
    const rolePerms = getPermissionsForRole(role)
    return rolePerms.includes(permission)
}

export function hasPermission(userPermissions, requiredPermission) {
    if (!userPermissions || !Array.isArray(userPermissions)) return false
    return userPermissions.includes(requiredPermission)
}

export function hasAnyPermission(userPermissions, requiredPermissions) {
    if (!userPermissions || !Array.isArray(userPermissions)) return false
    if (!Array.isArray(requiredPermissions)) requiredPermissions = [requiredPermissions]
    return requiredPermissions.some(perm => userPermissions.includes(perm))
}

export function canAccessRoute(userRole, routePath) {
    const requiredPermission = ROUTE_PERMISSIONS[routePath]
    
    if (!requiredPermission) return false
    
    if (routePath === '/' && requiredPermission === PERMISSIONS.canViewHome) {
        return true
    }
    
    return roleHasPermission(userRole, requiredPermission)
}

export function hasAnyRole(userRole, requiredRoles) {
    if (requiredRoles.includes('*')) return true
    return requiredRoles.includes(userRole)
}