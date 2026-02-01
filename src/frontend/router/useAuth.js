import { ref, computed } from 'vue'
import { canAccessRoute, hasAnyRole, ROLES, getPermissionsForRole, hasPermission as checkPermission, hasAnyPermission as checkAnyPermission, ROUTE_PERMISSIONS } from '../router/permissions'

const user = ref(null)
const isAuthenticated = ref(false)

export function useAuth() {
    const fetchCurrentUser = async () => {
        try {
            const response = await fetch('/Authentication/test', {
                credentials: 'include'
            })

            if (!response.ok) {
                user.value = null
                isAuthenticated.value = false
                return
            }

            const data = await response.json()

            if (data.isAuthenticated) {
                isAuthenticated.value = true

                const isTempUser = data.claims?.find(c => c.type === 'IsTempUser')?.value === 'true'
                const hasCertificateError = data.claims?.find(c => c.type === 'CertificateError')?.value === 'true'
                const roleClaim = data.claims?.find(c => c.type && c.type.includes('role'))
                
                const userRole = isTempUser ? ROLES.TEMP : (roleClaim?.value || ROLES.USER)
                
                const permissionClaims = data.claims?.filter(c => c.type === 'Permission') || []
                let userPermissions = []
                
                if (permissionClaims.length > 0) {
                    userPermissions = permissionClaims.map(c => c.value)
                } else {
                    userPermissions = getPermissionsForRole(userRole)
                }

                user.value = {
                    userName: data.userName,
                    role: userRole,
                    permissions: userPermissions,
                    isTempUser: isTempUser,
                    hasCertificateError: hasCertificateError,
                    claims: data.claims
                }
            } else {
                user.value = null
                isAuthenticated.value = false
            }
        } catch (error) {
            user.value = null
            isAuthenticated.value = false
        }
    }

    const hasPermission = (permission) => {
        if (!user.value || !user.value.permissions) return false
        return checkPermission(user.value.permissions, permission)
    }

    const canAccess = (routePath) => {
        if (!user.value) return false
        
        const requiredPermission = ROUTE_PERMISSIONS[routePath]
        
        if (!requiredPermission) return false
        
        return hasPermission(requiredPermission)
    }

    const hasAnyPermission = (requiredPermissions) => {
        if (!user.value || !user.value.permissions) return false
        const perms = Array.isArray(requiredPermissions) ? requiredPermissions : [requiredPermissions]
        return checkAnyPermission(user.value.permissions, perms)
    }

    const hasRole = (requiredRoles) => {
        if (!user.value) return false
        const roles = Array.isArray(requiredRoles) ? requiredRoles : [requiredRoles]
        return hasAnyRole(user.value.role, roles)
    }

    const isTempUser = computed(() => user.value?.isTempUser || false)
    const hasCertificateError = computed(() => user.value?.hasCertificateError || false)
    const userRole = computed(() => user.value?.role || null)
    const userPermissions = computed(() => user.value?.permissions || [])

    return {
        user,
        isAuthenticated,
        fetchCurrentUser,
        canAccess,
        hasPermission,
        hasAnyPermission,
        hasRole,
        isTempUser,
        hasCertificateError,
        userRole,
        userPermissions
    }
}