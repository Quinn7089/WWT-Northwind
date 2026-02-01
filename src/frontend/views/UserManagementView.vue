<template>
  <div class="user-management-view">
    <div class="page-header">
      <h1>
        <i class="fas fa-users-cog"></i>
        User Management
      </h1>
      <p class="page-description">
        Manage user accounts, permissions, and security settings for your application.
      </p>
    </div>

    <div class="management-dashboard">
      <!-- Management Actions -->
      <div class="action-bar">
        <div class="action-group">
          <button 
            v-if="canAddUsers" 
            class="btn btn-primary" 
            @click="showAddUserModal = true"
          >
            <i class="fas fa-user-plus"></i>
            Add New User
          </button>
        </div>
        
        <div class="search-group">
          <div class="search-box">
            <i class="fas fa-search"></i>
            <input 
              type="text" 
              placeholder="Search users..." 
              v-model="searchQuery"
              @input="handleSearch"
            />
          </div>
        </div>
      </div>

      <!-- User List Component -->
      <div class="user-list-section">
        <UserList 
          ref="userListRef"
          :search-query="searchQuery"
          :always-show="true"
          :hide-toggle="true"
          @user-edited="handleUserEdited"
        />
      </div>
    </div>

    <!-- Create User Form Component -->
    <AddUserForm 
      :show="showAddUserModal"
      @close="showAddUserModal = false"
      @user-created="handleUserCreated"
    />

    <!-- Edit User Form Component -->
    <EditUserForm
      :show="showEditUserModal"
      :user-id="selectedUserId"
      @close="showEditUserModal = false"
      @user-updated="handleUserUpdated"
    />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import UserList from '../components/UserList.vue'
import AddUserForm from '../components/AddUserForm.vue'
import EditUserForm from '../components/EditUserForm.vue'
import { useAuth } from '../router/useAuth'
import { PERMISSIONS } from '../router/permissions'

const router = useRouter()
const { hasPermission } = useAuth()

// Permission checks
const canAddUsers = computed(() => hasPermission(PERMISSIONS.canAddUsers))
const canEditUsers = computed(() => hasPermission(PERMISSIONS.canEditUsers))

// Reactive data
const userListRef = ref(null)
const searchQuery = ref('')
const showAddUserModal = ref(false)
const showEditUserModal = ref(false)
const selectedUserId = ref('')
const refreshing = ref(false)

// Methods
const handleSearch = () => {
  // The UserList component will handle the actual filtering
  // This could be extended to send the search to the backend
}

const handleUserEdited = (userId) => {
  // Handle user edit - open edit modal
  selectedUserId.value = userId
  showEditUserModal.value = true
}

const refreshData = async () => {
  refreshing.value = true
  try {
    // Refresh the user list data
    if (userListRef.value && userListRef.value.fetchUsers) {
      await userListRef.value.fetchUsers()
    }
    
    // Could also refresh other data here
    await new Promise(resolve => setTimeout(resolve, 500)) // Simulate API call
  } catch (error) {
  } finally {
    refreshing.value = false
  }
}

const handleUserCreated = async (userData) => {
  // Handle successful user creation
  // Show success notification
  showNotification('User created successfully!', 'success')
  // Refresh the user list to show the new user
  await refreshData()
}

const handleUserUpdated = async (userData) => {
  // Handle successful user update
  // Show success notification
  showNotification('User updated successfully!', 'success')
  // Refresh the user list to show the updated user
  await refreshData()
}

const showNotification = (message, type = 'success') => {
  const notification = document.createElement('div')
  notification.className = `notification notification-${type}`
  notification.innerHTML = `
    <i class="fas ${type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle'}"></i>
    <span>${message}</span>
  `
  
  document.body.appendChild(notification)
  
  setTimeout(() => {
    if (document.body.contains(notification)) {
      document.body.removeChild(notification)
    }
  }, 3000)
}

// Lifecycle
onMounted(() => {
  // Set page title
  document.title = 'User Management - Starter App'
})
</script>

<style scoped lang="scss">
@import '../styles/variables';

.user-management-view {
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
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }

  .page-description {
    color: rgb(var(--v-theme-medium));
    font-size: 1rem;
  }
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

  .action-group {
    display: flex;
    align-items: center;
    order: 2;
    flex: 1;
    justify-content: flex-end;
  }

  .search-group {
    display: flex;
    align-items: center;
    order: 1;
    flex: 2;
  }
}

.btn {
  border: none;
  padding: 0.6rem 1.2rem;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;

  &.btn-primary {
    background: rgb(var(--v-theme-primary));
    color: #fff;

    &:hover {
      background: #0056A4;
    }
  }

  &.btn-info {
    background: rgb(var(--v-theme-secondary));
    color: #fff;

    &:hover {
      background: rgb(var(--v-theme-primary));
    }
  }
}

.search-box {
  position: relative;
  display: flex;
  align-items: center;
  width: 100%;

  i {
    position: absolute;
    left: 12px;
    color: rgb(var(--v-theme-medium));
    z-index: 1;
  }

  input {
    padding: 0.6rem 1rem 0.6rem 2.5rem;
    border: 1px solid rgb(var(--v-theme-border));
    border-radius: 6px;
    background: rgb(var(--v-theme-background));
    color: rgb(var(--v-theme-on-background));
    width: 100%;
    transition: border-color 0.3s ease;

    &:focus {
      outline: none;
      border-color: rgb(var(--v-theme-primary));
    }

    &::placeholder {
      color: rgb(var(--v-theme-medium));
    }
  }
}

.user-list-section {
  background: rgb(var(--v-theme-surface));
  border-radius: 10px;
  border: 1px solid rgb(var(--v-theme-border));
  margin-top: 1.5rem;
  padding: 1.5rem;
  min-height: 200px;
}

/* Notifications */
:global(.notification) {
  position: fixed !important;
  top: 20px !important;
  right: 20px !important;
  padding: 1rem 1.5rem !important;
  border-radius: 6px !important;
  color: white !important;
  font-weight: 600 !important;
  z-index: 10000 !important;
  display: flex !important;
  align-items: center !important;
  gap: 0.5rem !important;
  min-width: 300px !important;
  animation: slideIn 0.3s ease !important;
  border: none !important;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15) !important;
}

:global(.notification.notification-success) {
  background-color: rgb(var(--v-theme-success)) !important;
  background: rgb(var(--v-theme-success)) !important;
  color: white !important;
}

:global(.notification.notification-error) {
  background-color: rgb(var(--v-theme-error)) !important;
  background: rgb(var(--v-theme-error)) !important;
  color: white !important;
}

@keyframes slideIn {
  from { transform: translateX(100%); opacity: 0; }
  to { transform: translateX(0); opacity: 1; }
}
</style>
