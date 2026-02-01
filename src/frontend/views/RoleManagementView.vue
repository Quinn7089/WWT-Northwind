<template>
  <div class="role-management-view">
    <div class="page-header">
      <h1>
        <i class="fas fa-user-shield"></i>
        Role Management
      </h1>
      <p class="page-description">
        Create, rename, and manage user roles, plus assign users to each role.
      </p>
    </div>

    <div v-if="pageError" class="alert alert-error">
      <i class="fas fa-exclamation-circle"></i>
      {{ pageError }}
    </div>

    <div class="management-dashboard">
      <div class="action-bar">
        <div class="search-group">
          <p class="helper-text">
            Review existing roles and assign access to your users.
          </p>
        </div>
        <div class="action-group">
          <button 
            v-if="canAddRoles" 
            class="btn btn-primary" 
            @click="openCreateModal"
          >
            <i class="fas fa-plus"></i>
            New Role
          </button>
        </div>
      </div>

      <div class="role-list-section">
        <RoleTableSection
          :roles="roles"
          :loading="loadingRoles"
          :deleting-role-id="isDeletingRoleId"
          :can-edit-roles="canEditRoles"
          @edit="openEditModal"
          @view="openMembersModal"
          @viewPermissions="openPermissionsModal"
          @delete="confirmDelete"
        />
      </div>
    </div>

    <RoleFormModal
      :show="showRoleModal"
      :mode="roleModalMode"
      :initial-role-name="selectedRole?.name || ''"
      :submitting="submittingRole"
      :error-message="roleFormError"
      @close="closeRoleModal"
      @submit="submitRoleForm"
    />

    <RoleMembersModal
      :show="showMembersModal"
      :role="selectedRole"
      :search-term="memberSearch"
      :users="filteredUsers"
      :loading="membersLoading"
      :error-message="membersError"
      :busy-map="membershipBusy"
      @close="closeMembersModal"
      @update:searchTerm="updateMemberSearch"
      @add-user="addUserToRole"
      @remove-user="removeUserFromRole"
    />

    <RolePermissionsModal
      :show="showPermissionsModal"
      :role="selectedRole"
      :role-permissions="rolePermissions"
      :loading="loadingPermissions"
      :error-message="permissionsError"
      :saving="savingPermissions"
      :busy-map="permissionBusy"
      @close="closePermissionsModal"
      @toggle-permission="toggleRolePermission"
    />
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import RoleTableSection from '../components/roles/RoleTableSection.vue'
import RoleFormModal from '../components/roles/RoleFormModal.vue'
import RoleMembersModal from '../components/roles/RoleMembersModal.vue'
import RolePermissionsModal from '../components/roles/RolePermissionsModal.vue'
import { useAuth } from '../router/useAuth'
import { PERMISSIONS } from '../router/permissions'

const { hasPermission } = useAuth()
const canAddRoles = computed(() => hasPermission(PERMISSIONS.canAddRoles))
const canEditRoles = computed(() => hasPermission(PERMISSIONS.canEditRoles))

const roles = ref([])
const loadingRoles = ref(false)
const pageError = ref('')
const isDeletingRoleId = ref('')

const showRoleModal = ref(false)
const roleModalMode = ref('create')
const roleFormError = ref('')
const submittingRole = ref(false)
const selectedRole = ref(null)

const showMembersModal = ref(false)
const memberSearch = ref('')
const roleMembers = ref([])
const membersError = ref('')
const loadingMembers = ref(false)
const loadingAllUsers = ref(false)
const allUsers = ref([])
const membershipBusy = reactive({})

const showPermissionsModal = ref(false)
const rolePermissions = ref([])
const permissionsError = ref('')
const loadingPermissions = ref(false)
const savingPermissions = ref(false)
const permissionBusy = reactive({})

const fetchRoles = async () => {
  loadingRoles.value = true
  pageError.value = ''
  try {
    const response = await fetch('/Role', {
      credentials: 'include',
      headers: { Accept: 'application/json' }
    })

    if (!response.ok) {
      throw new Error(`Failed to load roles: ${response.status}`)
    }

    const data = await response.json()
    roles.value = Array.isArray(data) ? data : []
  } catch (error) {
    pageError.value = 'Failed to load roles. Please try again.'
  } finally {
    loadingRoles.value = false
  }
}

const fetchAllUsers = async () => {
  if (allUsers.value.length > 0) {
    return
  }

  loadingAllUsers.value = true
  try {
    const response = await fetch('/api/users', {
      credentials: 'include',
      headers: { Accept: 'application/json' }
    })

    if (!response.ok) {
      throw new Error(`Failed to load users: ${response.status}`)
    }

    const data = await response.json()
    allUsers.value = Array.isArray(data) ? data : []
  } catch (error) {
    membersError.value = 'Failed to load users.'
  } finally {
    loadingAllUsers.value = false
  }
}

const fetchRoleMembers = async (roleId) => {
  loadingMembers.value = true
  membersError.value = ''
  try {
    const response = await fetch(`/Role/${roleId}/users`, {
      credentials: 'include',
      headers: { Accept: 'application/json' }
    })

    if (!response.ok) {
      throw new Error(`Failed to load role members: ${response.status}`)
    }

    const data = await response.json()
    roleMembers.value = Array.isArray(data) ? data : []
  } catch (error) {
    membersError.value = 'Failed to load users for this role.'
  } finally {
    loadingMembers.value = false
  }
}

const openCreateModal = () => {
  roleModalMode.value = 'create'
  selectedRole.value = null
  roleFormError.value = ''
  submittingRole.value = false
  showRoleModal.value = true
}

const openEditModal = (role) => {
  roleModalMode.value = 'edit'
  selectedRole.value = role
  roleFormError.value = ''
  submittingRole.value = false
  showRoleModal.value = true
}

const closeRoleModal = () => {
  showRoleModal.value = false
  roleFormError.value = ''
  submittingRole.value = false
}

const submitRoleForm = async (roleName) => {
  if (!roleName) {
    roleFormError.value = 'Role name is required.'
    return
  }

  if (roleModalMode.value === 'edit' && !selectedRole.value) {
    roleFormError.value = 'Unable to locate the selected role.'
    return
  }

  roleFormError.value = ''
  submittingRole.value = true

  try {
    const payload = { roleName }
    const options = {
      method: roleModalMode.value === 'create' ? 'POST' : 'PUT',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json'
      },
      body: JSON.stringify(payload)
    }

    const url =
      roleModalMode.value === 'create'
        ? '/Role'
        : `/Role/${selectedRole.value?.id}`

    const response = await fetch(url, options)

    if (response.status === 409) {
      const conflict = await response.json()
      roleFormError.value = conflict?.Message || 'Role name already exists.'
      return
    }

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}))
      throw new Error(errorData?.Message || 'Request failed.')
    }

    await fetchRoles()
    closeRoleModal()
  } catch (error) {
    roleFormError.value =
      error?.message || 'An unexpected error occurred. Please try again.'
  } finally {
    submittingRole.value = false
  }
}

const confirmDelete = async (role) => {
  const confirmed = window.confirm(
    `Delete role "${role.name}"? This will remove the role from all users.`
  )
  if (!confirmed) return

  isDeletingRoleId.value = role.id
  try {
    const response = await fetch(`/Role/${role.id}`, {
      method: 'DELETE',
      credentials: 'include',
      headers: { Accept: 'application/json' }
    })

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}))
      throw new Error(errorData?.Message || 'Failed to delete role.')
    }

    await fetchRoles()
  } catch (error) {
    pageError.value = error?.message || 'Failed to delete role.'
  } finally {
    isDeletingRoleId.value = ''
  }
}

const openMembersModal = async (role) => {
  selectedRole.value = role
  memberSearch.value = ''
  roleMembers.value = []
  membersError.value = ''
  showMembersModal.value = true
  await Promise.all([fetchAllUsers(), fetchRoleMembers(role.id)])
}

const closeMembersModal = () => {
  showMembersModal.value = false
  selectedRole.value = null
  memberSearch.value = ''
  membersError.value = ''
  roleMembers.value = []
}

const memberIds = computed(() => new Set(roleMembers.value.map((u) => u.id)))

const filteredUsers = computed(() => {
  if (!Array.isArray(allUsers.value)) {
    return []
  }

  const query = memberSearch.value.trim().toLowerCase()
  const ids = memberIds.value
  const isSearching = query.length > 0

  return allUsers.value
    .filter((user) => {
      const isMember = ids.has(user.id)

      if (!isSearching) {
        return isMember
      }

      const haystack = [
        user.userName || '',
        user.email || '',
        user.studentId || ''
      ]
        .join(' ')
        .toLowerCase()

      return haystack.includes(query)
    })
    .map((user) => ({
      ...user,
      isMember: ids.has(user.id)
    }))
    .sort((a, b) => {
      const nameA = (a.userName || '').toLowerCase()
      const nameB = (b.userName || '').toLowerCase()
      return nameA.localeCompare(nameB)
    })
})

const membersLoading = computed(() => loadingMembers.value || loadingAllUsers.value)

const updateMemberSearch = (value) => {
  memberSearch.value = value
}

const addUserToRole = async (user) => {
  if (!selectedRole.value) return
  membershipBusy[user.id] = true
  try {
    const response = await fetch(
      `/Role/${selectedRole.value.id}/users/${user.id}`,
      {
        method: 'POST',
        credentials: 'include',
        headers: { Accept: 'application/json' }
      }
    )

    if (response.status === 409) {
      await fetchRoleMembers(selectedRole.value.id)
      return
    }

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}))
      throw new Error(errorData?.Message || 'Failed to add user to role.')
    }

    roleMembers.value.push({
      id: user.id,
      userName: user.userName,
      email: user.email,
      phoneNumber: user.phoneNumber,
      studentId: user.studentId
    })
  } catch (error) {
    membersError.value = error?.message || 'Failed to add user to role.'
  } finally {
    membershipBusy[user.id] = false
  }
}

const removeUserFromRole = async (user) => {
  if (!selectedRole.value) return
  membershipBusy[user.id] = true
  try {
    const response = await fetch(
      `/Role/${selectedRole.value.id}/users/${user.id}`,
      {
        method: 'DELETE',
        credentials: 'include',
        headers: { Accept: 'application/json' }
      }
    )

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}))
      throw new Error(errorData?.Message || 'Failed to remove user from role.')
    }

    roleMembers.value = roleMembers.value.filter((member) => member.id !== user.id)
  } catch (error) {
    membersError.value = error?.message || 'Failed to remove user from role.'
  } finally {
    membershipBusy[user.id] = false
  }
}

const openPermissionsModal = async (role) => {
  selectedRole.value = role
  rolePermissions.value = []
  permissionsError.value = ''
  showPermissionsModal.value = true
  await fetchRolePermissions(role.id)
}

const closePermissionsModal = () => {
  showPermissionsModal.value = false
  selectedRole.value = null
  rolePermissions.value = []
  permissionsError.value = ''
  Object.keys(permissionBusy).forEach(key => delete permissionBusy[key])
}

const fetchRolePermissions = async (roleId) => {
  loadingPermissions.value = true
  permissionsError.value = ''
  try {
    const response = await fetch(`/Role/${roleId}/permissions`, {
      credentials: 'include',
      headers: { Accept: 'application/json' }
    })

    if (!response.ok) {
      throw new Error(`Failed to load role permissions: ${response.status}`)
    }

    const data = await response.json()
    rolePermissions.value = data.permissions || []
  } catch (error) {
    permissionsError.value = 'Failed to load permissions for this role.'
  } finally {
    loadingPermissions.value = false
  }
}

const toggleRolePermission = async (permission, isChecked) => {
  if (!selectedRole.value) return
  
  permissionBusy[permission] = true
  savingPermissions.value = true
  
  try {
    if (isChecked) {
      if (!rolePermissions.value.includes(permission)) {
        rolePermissions.value.push(permission)
      }
    } else {
      rolePermissions.value = rolePermissions.value.filter(p => p !== permission)
    }

    const response = await fetch(`/Role/${selectedRole.value.id}/permissions`, {
      method: 'PUT',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json'
      },
      body: JSON.stringify({
        permissions: rolePermissions.value
      })
    })

    if (!response.ok) {
      if (isChecked) {
        rolePermissions.value = rolePermissions.value.filter(p => p !== permission)
      } else {
        if (!rolePermissions.value.includes(permission)) {
          rolePermissions.value.push(permission)
        }
      }
      
      const errorData = await response.json().catch(() => ({}))
      throw new Error(errorData?.Message || 'Failed to update role permissions.')
    }

    await fetchRolePermissions(selectedRole.value.id)
  } catch (error) {
    permissionsError.value = error?.message || 'Failed to update permission.'
  } finally {
    permissionBusy[permission] = false
    savingPermissions.value = false
  }
}

onMounted(() => {
  fetchRoles()
})
</script>

<style scoped lang="scss">
@import '../styles/variables';

.role-management-view {
  max-width: $max-width;
  margin: 0 auto;
  color: rgb(var(--v-theme-on-background));
}

.page-header {
  border-bottom: 2px solid rgb(var(--v-theme-border));
  padding-bottom: 1rem;
  margin-bottom: 2rem;

  h1 {
    color: rgb(var(--v-theme-primary));
    font-weight: 700;
    font-size: 1.8rem;
    display: block;
    gap: 0.5rem;
    margin-bottom: 0.5rem;
  }

  .page-description {
    color: rgb(var(--v-theme-medium));
    font-size: 1rem;
    margin: 0;
  }
}

.alert {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  border-radius: 8px;
  padding: 0.75rem 1rem;
  margin-bottom: 1rem;
  font-weight: 500;

  &.alert-error {
    background: rgba(220, 38, 38, 0.12);
    color: #991b1b;
    border: 1px solid rgba(220, 38, 38, 0.25);
  }
}

.management-dashboard {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.action-bar {
  background: rgb(var(--v-theme-surface));
  border: 1px solid rgb(var(--v-theme-border));
  border-radius: 8px;
  padding: 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
}

.action-group {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  order: 2;
  flex: 1;
  justify-content: flex-end;
}

.search-group {
  order: 1;
  flex: 2;
  display: flex;
  align-items: center;
}

.helper-text {
  margin: 0;
  color: rgb(var(--v-theme-medium));
  font-size: 1rem;
}

.btn {
  border: none;
  padding: 0.6rem 1.2rem;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1rem;

  i {
    font-size: 0.95rem;
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }

  &.btn-primary {
    background: rgb(var(--v-theme-primary));
    color: #fff;

    &:hover {
      background: #0056A4;
    }
  }
}

.role-list-section {
  background: rgb(var(--v-theme-surface));
  border-radius: 10px;
  border: 1px solid rgb(var(--v-theme-border));
  padding: 1.5rem;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
}

@media (max-width: 768px) {
  .action-bar {
    justify-content: center;
  }

  .action-group {
    width: 100%;
    justify-content: center;
  }

  .search-group {
    order: 2;
    justify-content: center;
    text-align: center;
  }
}
</style>

