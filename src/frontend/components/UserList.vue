<template>
  <div class="user-list-container">
    <div class="header-section">
      <h2 v-if="!hideHeader">User Management</h2>
      <button 
        v-if="!hideToggle"
        @click="toggleUserTable" 
        :class="['toggle-btn', showUsers ? 'btn-hide' : 'btn-show']"
        :disabled="loading"
      >
        {{ loading ? 'Loading...' : (showUsers ? 'Hide Users' : 'Show Users') }}
      </button>
    </div>
    
    <div v-if="error" class="error-message">
      <i class="fas fa-exclamation-triangle"></i>
      {{ error }}
    </div>
    
    <transition name="fade">
      <div v-if="showUsers || alwaysShow" class="table-container">
        <div class="table-header">
          <h3 v-if="!hideHeader">{{searchQuery.trim().length === 0 ? `User List (${filteredUsers.length} users)` : `User List (${filteredUsers.length} users matching "${searchQuery}")`}}</h3>
          <div class="items-per-page">
            <label for="items-select">Show:</label>
            <select id="items-select" v-model="itemsPerPage" @change="resetPage" class="items-select">
              <option value="10">10</option>
              <option value="20">20</option>
              <option value="30">30</option> 
            </select>
            <span>per page</span>
          </div>
        </div>
        <div v-if="loading" class="loading-spinner">
          <div class="spinner"></div>
          <p>Loading users...</p>
        </div>
        
        <table v-else-if="filteredUsers.length > 0" class="user-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>WCTC ID</th>
              <th>Username</th>
              <th>Email</th>
              <th>Phone</th>
              <th>Roles</th>
              <th>Status</th>
              <th v-if="showActions">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in paginated" :key="user.id" class="user-row">
              <td class="status">{{ user.firstName }} {{user.lastName}}</td>
              <td class="user-id">{{ user.studentId || 'N/A' }}</td>
              <td class="username">{{ user.userName }}</td>
              <td class="email">{{ user.email }}</td>
              <td class="phone">{{ user.phoneNumber || 'N/A' }}</td>
              <td class="roles">
                <div v-if="user.roles && user.roles.length > 0" class="roles-container">
                  <span 
                    v-for="role in user.roles.sort(a,b => (a.toLowerCase.localeCompare(b.toLowerCase)))" 
                    :key="role" 
                    class="role-badge"
                    :class="getRoleClass(role)"
                  >
                    {{ role }}
                  </span>
                </div>
                <span v-else class="no-roles">No roles assigned</span>
              </td>
              <td class="status">
                <span :class="['badge', user.isActive ? 'badge-success' : 'badge-danger']">
                  {{ user.isActive ? 'Active' : 'Deactivated' }}
                </span>
              </td>
              <td v-if="showActions" class="actions">
                <button @click="editUser(user.id)" class="btn-edit" title="Edit User">
                  <i class="fas fa-edit"></i>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
        
        <div v-else class="no-users">
          <i class="fas fa-users"></i>
          <p>{{ searchQuery ? `No users found matching "${searchQuery}"` : 'No users found.' }}</p>
        </div>
        <div class="pagination-controls">
          <button @click="changePage(-1)" class="pagination-btn" :disabled="currentPage === 1">
            <i class="fas fa-chevron-left"></i>
          </button>
          <span class="page-info">Page {{ currentPage }}</span>
          <button @click="changePage(1)" class="pagination-btn" :disabled="currentPage * itemsPerPage >= filteredUsers.length">
            <i class="fas fa-chevron-right"></i>
          </button>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, defineEmits, defineExpose } from 'vue'

// Props
const props = defineProps({
  searchQuery: {
    type: String,
    default: ''
  },
  statusFilter: {
    type: String,
    default: 'all'
  },
  hideHeader: {
    type: Boolean,
    default: false
  },
  hideToggle: {
    type: Boolean,
    default: false
  },
  alwaysShow: {
    type: Boolean,
    default: false
  },
  showActions: {
    type: Boolean,
    default: true
  }
})

// Emits
const emit = defineEmits(['user-stats-updated', 'user-edited'])

// Reactive data
const users = ref([])
//const showUsers = ref(props.alwaysShow)
const showUsers = ref(true);
const loading = ref(false)
const error = ref('')
const currentPage = ref(1);
var itemsPerPage = 10;

const paginated = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return filteredUsers.value.sort((a, b) => a.lastName.localeCompare(b.lastName)).slice(start, end)
})

// Computed
const filteredUsers = computed(() => {
  let filtered = users.value

  // Apply search filter
  if (props.searchQuery) {
    currentPage.value = 1; // don't use resetPage function, as it makes the list do fetchUsers() calls over and over again when query isn't empty
    const query = props.searchQuery.toLowerCase()
    filtered = filtered.filter(user => {
      // Search in basic user fields
      const basicFieldsMatch = 
        user.userName?.toLowerCase().includes(query) ||
        user.email?.toLowerCase().includes(query) ||
        user.phoneNumber?.toLowerCase().includes(query) ||
        user.firstName?.toLowerCase().includes(query) ||
        user.lastName?.toLowerCase().includes(query)
      
      // Search in roles
      const rolesMatch = user.roles?.some(role => 
        role?.toLowerCase().includes(query)
      )
      
      return basicFieldsMatch || rolesMatch
    })
  }

  // Apply status filter
  if (props.statusFilter && props.statusFilter !== 'all') {
    if (props.statusFilter === 'active') {
      filtered = filtered.filter(user => user.isActive)
    } else if (props.statusFilter === 'inactive') {
      filtered = filtered.filter(user => !user.isActive)
    }
  }

  return filtered
})

// Watchers
watch(users, (newUsers) => {
  // Calculate and emit user statistics
  const stats = {
    totalUsers: newUsers.length,
    confirmedUsers: newUsers.filter(u => u.emailConfirmed).length,
    twoFactorUsers: newUsers.filter(u => u.twoFactorEnabled).length,
    activeUsers: newUsers.filter(u => u.isActive).length,
    deactivatedUsers: newUsers.filter(u => !u.isActive).length
  }
  emit('user-stats-updated', stats)
}, { immediate: true })



// Methods
const toggleUserTable = async () => {
  if (!showUsers.value && users.value.length === 0) {
    await fetchUsers()
  }
  showUsers.value = !showUsers.value
}

const fetchUsers = async () => {
  loading.value = true
  error.value = ''
  
  try {
    const response = await fetch('/api/users', {
      credentials: 'include',
      headers: {
        'Accept': 'application/json',
      }
      
    })
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
      
    }
    
    const data = await response.json()
    users.value = data
  } catch (err) {
    error.value = 'Failed to load users. Please try again.'
  } finally {
    loading.value = false
  }
}

const editUser = (userId) => {
  emit('user-edited', userId)
}

const getRoleClass = (role) => {
  const roleLower = role?.toLowerCase() || ''
  switch (roleLower) {
    case 'admin':
      return 'role-admin'
    case 'manager':
      return 'role-manager'
    case 'user':
      return 'role-user'
    default:
      return 'role-default'
  }
}

const changePage = (direction) => { // -1 to go back a page, 1 to go forward
    if(direction == 1) {
      if(currentPage.value * itemsPerPage < filteredUsers.value.length){
      currentPage.value++;
      }
    }
    else if(direction == -1) {
      if(currentPage.value > 1){
      currentPage.value--;
      }
    }
}
const resetPage = () => {
  currentPage.value = 1;
  fetchUsers();
}

const showNotification = (message, type = 'success') => {
  const notification = document.createElement('div')
  notification.className = `notification notification-${type}`
  notification.textContent = message
  document.body.appendChild(notification)
  
  setTimeout(() => {
    if (document.body.contains(notification)) {
      document.body.removeChild(notification)
    }
  }, 3000)
}

// Expose methods for parent components
defineExpose({
  fetchUsers,
  refreshData: fetchUsers
})

// Lifecycle
onMounted(() => {
  if (props.alwaysShow) {
    fetchUsers()
  }
})
</script>

<style scoped lang="scss">
.user-list-container {
  width: 100%;
  font-family: 'Karla', 'Segoe UI', Arial, sans-serif;
  color: rgb(var(--v-theme-on-surface));
}

/* Header Section */
.header-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  border-bottom: 2px solid rgb(var(--v-theme-primary));
  padding-bottom: 0.5rem;

  h2 {
    color: rgb(var(--v-theme-primary));
    font-weight: 700;
    font-size: 1.4rem;
    margin: 0;
  }
}

/* Toggle Button */
.toggle-btn {
  background-color: rgb(var(--v-theme-secondary));
  color: white;
  border: none;
  border-radius: 6px;
  padding: 0.5rem 1.25rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;

  &:hover {
    background-color: rgb(var(--v-theme-primary));
  }

  &.btn-hide {
    background-color: rgba(var(--v-theme-on-surface), 0.6);

    &:hover {
      background-color: rgba(var(--v-theme-on-surface), 0.8);
    }
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
}

/* Error Message */
.error-message {
  background-color: rgba(var(--v-theme-error), 0.1);
  color: rgb(var(--v-theme-error));
  padding: 0.75rem 1rem;
  border-radius: 6px;
  margin-bottom: 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;

  i {
    font-size: 1rem;
  }
}

/* Table Container */
.table-container {
  .table-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
    flex-wrap: wrap;
    gap: 1rem;

    h3 {
      color: rgb(var(--v-theme-primary));
      font-weight: 600;
      margin: 0;
      font-size: 1.1rem;
      flex: 1;
    }

    .items-per-page {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.9rem;
      color: rgb(var(--v-theme-on-surface));

      label {
        font-weight: 500;
        color: rgba(var(--v-theme-on-surface), 0.8);
      }

      span {
        color: rgba(var(--v-theme-on-surface), 0.8);
      }

      .items-select {
        padding: 0.4rem 0.8rem;
        border: 2px solid rgba(var(--v-theme-primary), 0.3);
        border-radius: 6px;
        background-color: rgb(var(--v-theme-surface));
        color: rgb(var(--v-theme-on-surface));
        font-size: 0.9rem;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.2s ease;
        outline: none;
        min-width: 60px;

        &:hover {
          border-color: rgba(var(--v-theme-primary), 0.5);
        }

        &:focus {
          border-color: rgb(var(--v-theme-primary));
          box-shadow: 0 0 0 3px rgba(var(--v-theme-primary), 0.1);
        }

        option {
          background-color: rgb(var(--v-theme-surface));
          color: rgb(var(--v-theme-on-surface));
          padding: 0.5rem;
        }
      }
    }
  }
}

/* Loading Spinner */
.loading-spinner {
  text-align: center;
  padding: 1.5rem;

  .spinner {
    width: 40px;
    height: 40px;
    border: 4px solid rgba(var(--v-theme-primary), 0.15);
    border-top: 4px solid rgb(var(--v-theme-primary));
    border-radius: 50%;
    animation: spin 1s linear infinite;
    margin: 0 auto 0.5rem;
  }

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }

  p {
    color: rgb(var(--v-theme-on-surface));
  }
}

/* User Table */
.user-table {
  width: 100%;
  border-collapse: collapse;
  background: rgb(var(--v-theme-surface));
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);

  th {
    background-color: rgb(var(--v-theme-primary));
    color: rgb(var(--v-theme-on-primary));
    padding: 0.75rem;
    text-align: left;
    font-weight: 600;
    font-size: 0.9rem;
  }

  td {
    padding: 0.75rem;
    border-bottom: 1px solid rgb(var(--v-theme-border));
    font-size: 0.95rem;
    color: rgb(var(--v-theme-on-surface));
  }

  .user-row {
    transition: background-color 0.2s ease;

    &:hover {
      background-color: rgba(var(--v-theme-primary), 0.04);
    }
  }

  .user-id {
    font-family: monospace;
    color: rgba(var(--v-theme-on-surface), 0.7);
    font-size: 0.85rem;
  }

  .username {
    font-weight: 600;
  }

  .email {
    color: rgb(var(--v-theme-secondary));
  }

  .roles {
    max-width: 200px;
  }

  .actions {
    text-align: center;
    vertical-align: middle;
    width: 80px;
  }
}

/* Badges */
.badge {
  padding: 0.25rem 0.6rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 600;
  text-transform: capitalize;

  &.badge-success {
    background-color: rgba(var(--v-theme-success), 0.1);
    color: rgb(var(--v-theme-success));
  }

  &.badge-warning {
    background-color: rgba(var(--v-theme-warning), 0.1);
    color: rgb(var(--v-theme-warning));
  }

  &.badge-danger {
    background-color: rgba(var(--v-theme-error), 0.1);
    color: rgb(var(--v-theme-error));
  }

  &.badge-secondary {
    background-color: rgba(var(--v-theme-on-surface), 0.08);
    color: rgba(var(--v-theme-on-surface), 0.7);
  }
}

/* Roles */
.roles-container {
  display: flex;
  flex-wrap: wrap;
  gap: 0.3rem;
}

.role-badge {
  padding: 0.2rem 0.5rem;
  border-radius: 10px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: capitalize;
  white-space: nowrap;
  border: 1px solid transparent;

  &.role-admin {
    background-color: rgba(var(--v-theme-error), 0.15);
    color: rgb(var(--v-theme-error));
    border-color: rgba(var(--v-theme-error), 0.4);
  }

  &.role-manager {
    background-color: rgba(var(--v-theme-secondary), 0.15);
    color: rgb(var(--v-theme-secondary));
    border-color: rgba(var(--v-theme-secondary), 0.4);
  }

  &.role-user {
    background-color: rgba(var(--v-theme-success), 0.15);
    color: rgb(var(--v-theme-success));
    border-color: rgba(var(--v-theme-success), 0.4);
  }

  &.role-default {
    background-color: rgba(var(--v-theme-on-surface), 0.12);
    color: rgb(var(--v-theme-on-surface));
    border-color: rgba(var(--v-theme-on-surface), 0.3);
  }
}

.no-roles {
  color: rgba(var(--v-theme-on-surface), 0.6);
  font-style: italic;
  font-size: 0.85rem;
}

/* Action Buttons */
.actions {
  text-align: center;
  vertical-align: middle;
  width: 80px;

  button {
    padding: 0.5rem;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    transition: all 0.2s ease;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    background: transparent;
  }

  .btn-edit {
    color: rgb(var(--v-theme-secondary)) !important;
    border: 1px solid rgb(var(--v-theme-secondary));
    background: transparent;

    &:hover {
      background-color: rgb(var(--v-theme-secondary)) !important;
      color: rgb(var(--v-theme-on-secondary)) !important;
      transform: scale(1.05);
    }

    i {
      font-size: 0.9rem;
      color: inherit;
    }
  }
}

/* No Users Placeholder */
.no-users {
  text-align: center;
  padding: 2rem;
  color: rgba(var(--v-theme-on-surface), 0.7);

  i {
    font-size: 2.5rem;
    color: rgba(var(--v-theme-on-surface), 0.5);
    margin-bottom: 0.5rem;
  }
}


/* Pagination Controls */
.pagination-controls {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  margin-top: 1rem;
  padding: 1rem 0;

  .pagination-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    border: 2px solid rgb(var(--v-theme-primary));
    background: transparent;
    color: rgb(var(--v-theme-primary));
    border-radius: 50%;
    cursor: pointer;
    transition: all 0.2s ease;
    font-size: 0.9rem;

    &:hover:not(:disabled) {
      background-color: rgb(var(--v-theme-primary));
      color: white;
      transform: scale(1.05);
    }

    &:disabled {
      opacity: 0.3;
      cursor: not-allowed;
      transform: none;
    }

    i {
      font-size: 0.8rem;
    }
  }

  .page-info {
    font-weight: 600;
    color: rgb(var(--v-theme-primary));
    min-width: 80px;
    text-align: center;
    font-size: 0.95rem;
  }
}

/* Notifications */
:global(.notification) {
  position: fixed;
  top: 1rem;
  right: 1rem;
  padding: 1rem 1.25rem;
  border-radius: 6px;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
  font-weight: 500;
  color: white;
  animation: slideIn 0.3s ease;
  z-index: 9999;

  &.notification-success {
    background-color: #2e7d32;
  }

  &.notification-error {
    background-color: #c62828;
  }
}

@keyframes slideIn {
  from { transform: translateX(100%); opacity: 0; }
  to { transform: translateX(0); opacity: 1; }
}

/* Responsive Design */
@media (max-width: 768px) {
  .header-section {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }

  .table-container {
    .table-header {
      flex-direction: column;
      align-items: flex-start;
      gap: 0.75rem;

      .items-per-page {
        align-self: flex-end;
      }
    }
  }

  .user-table {
    font-size: 0.85rem;

    th, td {
      padding: 0.5rem;
    }
  }

  .user-id {
    display: none;
  }

  .roles-container {
    flex-direction: column;
    gap: 0.2rem;
  }

  .role-badge {
    font-size: 0.7rem;
    padding: 0.15rem 0.4rem;
  }

  .actions {
    flex-wrap: wrap;
    justify-content: flex-start;
  }
}
</style>
