import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import UserView from '../views/UserView.vue'
import UserManagementView from '../views/UserManagementView.vue'
import RoleManagementView from '../views/RoleManagementView.vue'
import { PERMISSIONS, ROUTE_PERMISSIONS } from './permissions'
import { useAuth } from './useAuth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
      meta: {
        title: 'Home',
        requiredPermission: PERMISSIONS.canViewHome
      }
    },
    {
      path: '/user',
      name: 'user',
      component: UserView,
      meta: {
        title: 'User',
        requiredPermission: PERMISSIONS.canViewHome
      }
    },
    {
      path: '/user-management',
      name: 'user-management',
      component: UserManagementView,
      meta: {
        title: 'User Management',
        requiresAuth: true,
        requiredPermission: PERMISSIONS.canViewManageUsers
      }
    },
    {
      path: '/role-management',
      name: 'role-management',
      component: RoleManagementView,
      meta: {
        title: 'Role Management',
        requiresAuth: true,
        requiredPermission: PERMISSIONS.canViewManageRoles
      }
    },
  ],
})

router.beforeEach(async (to, from, next) => {
  const { user, fetchCurrentUser, hasPermission } = useAuth()
  
  if (!user.value) {
    await fetchCurrentUser()
  }
  
  if (to.meta.requiresAuth && !user.value) {
    window.location.href = '/Authentication/loginWithSaml'
    return
  }
  
  const requiredPermission = to.meta.requiredPermission
  
  if (!requiredPermission) {
    return next()
  }
  
  if (user.value && hasPermission(requiredPermission)) {
    return next()
  }
  
  return next({ path: '/', query: { accessDenied: 'true' } })
})

export default router
