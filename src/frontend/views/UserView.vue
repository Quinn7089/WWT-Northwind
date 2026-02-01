<template>
  <div class="user-dashboard">
    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <i class="fas fa-spinner fa-spin"></i>
      <span>Loading...</span>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="error-container">
      <i class="fas fa-exclamation-circle"></i>
      <span>{{ error }}</span>
      <button @click="loadUserProfile" class="retry-btn">
        <i class="fas fa-redo"></i>
        Retry
      </button>
    </div>

    <!-- Dashboard Content -->
    <div v-else-if="userProfile" class="dashboard-content">
      <!-- Single unified card with all information -->
      <div class="profile-card">
        <div class="card-header">
          <div class="header-content">
            <i class="fas fa-user-circle profile-icon"></i>
            <div class="header-text">
              <h1>{{ fullName || userProfile?.userName || 'Welcome' }}</h1>
              
              <div class="user-info">
                <p class="email-info">
                  <i class="fas fa-envelope"></i>
                  {{ userProfile.email }}
                </p>
                
                <p class="phone-info">
                  <i class="fas fa-phone"></i>
                  {{ userProfile.phoneNumber || 'Phone not provided' }}
                </p>
                
                <!-- User Roles -->
                <div v-if="userProfile?.roles && userProfile.roles.length > 0" class="user-roles">
                  <span 
                    v-for="role in userProfile.roles" 
                    :key="role"
                    class="role-badge"
                    :class="getRoleClass(role)"
                  >
                    {{ role }}
                  </span>
                </div>
                <div v-else-if="!isTempUser && userProfile?.role" class="user-roles">
                  <span 
                    class="role-badge"
                    :class="getRoleClass(userProfile.role)"
                  >
                    {{ userProfile.role }}
                  </span>
                </div>
              </div>

              <p v-if="isTempUser" class="temp-user-notice">
                <i class="fas fa-exclamation-triangle"></i>
                You are logged in but not yet registered in our system.
              </p>
            </div>
          </div>
          
          <!-- Action Buttons -->
          <div v-if="!isTempUser" class="header-actions">
            <button @click="startEditing" class="btn btn-secondary">
              <i class="fas fa-edit"></i>
              Edit
            </button>
          </div>
        </div>

        <!-- Temp User Message -->
        <div v-if="isTempUser" class="temp-user-message">
          <i class="fas fa-info-circle"></i>
          <div>
            <h3>Limited Access Account</h3>
            <p>You are currently using a temporary account. Contact your administrator to be added to the system for full access to all features.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Edit Profile Modal -->
    <div v-if="isEditing" class="modal-overlay" @click.self="cancelEditing">
      <div class="modal-content">
        <div class="modal-header">
          <h2>
            <i class="fas fa-edit"></i>
            Edit Information
          </h2>
          <button @click="cancelEditing" class="modal-close-btn">
            <i class="fas fa-times"></i>
          </button>
        </div>

        <form @submit.prevent="saveProfile" class="modal-body">
          <div class="form-row">
            <div class="form-group">
              <label for="firstName">
                <i class="fas fa-user"></i>
                First Name *
              </label>
              <input
                type="text"
                id="firstName"
                v-model="editForm.firstName"
                :class="['form-control', { error: errors.firstName }]"
                placeholder="Enter your first name"
                required
              />
              <span v-if="errors.firstName" class="error-text">{{ errors.firstName }}</span>
            </div>

            <div class="form-group">
              <label for="lastName">
                <i class="fas fa-user"></i>
                Last Name *
              </label>
              <input
                type="text"
                id="lastName"
                v-model="editForm.lastName"
                :class="['form-control', { error: errors.lastName }]"
                placeholder="Enter your last name"
                required
              />
              <span v-if="errors.lastName" class="error-text">{{ errors.lastName }}</span>
            </div>
          </div>

          <div class="form-group">
            <label for="email">
              <i class="fas fa-envelope"></i>
              Email Address
            </label>
            <input
              type="email"
              id="email"
              :value="userProfile.email"
              class="form-control"
              disabled
              readonly
            />
            <small class="form-text">Email address cannot be changed</small>
          </div>

          <div class="form-group">
            <label for="phoneNumber">
              <i class="fas fa-phone"></i>
              Phone Number
            </label>
            <input
              type="tel"
              id="phoneNumber"
              v-model="editForm.phoneNumber"
              class="form-control"
              placeholder="Enter your phone number (optional)"
            />
          </div>

          <div class="modal-footer">
            <button 
              type="button" 
              @click="cancelEditing" 
              class="btn btn-secondary"
              :disabled="saving"
            >
              <i class="fas fa-times"></i>
              Cancel
            </button>
            <button 
              type="submit" 
              class="btn btn-primary"
              :disabled="saving"
            >
              <i class="fas fa-spinner fa-spin" v-if="saving"></i>
              <i class="fas fa-save" v-else></i>
              {{ saving ? 'Saving...' : 'Save Changes' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, reactive } from 'vue'

// Reactive data
const userProfile = ref(null)
const loading = ref(true)
const error = ref('')
const isEditing = ref(false)
const saving = ref(false)
const isTempUser = ref(false)

// Edit form
const editForm = reactive({
  firstName: '',
  lastName: '',
  phoneNumber: ''
})

// Form errors
const errors = reactive({
  firstName: '',
  lastName: ''
})

// Computed properties
const fullName = computed(() => {
  if (userProfile.value?.firstName || userProfile.value?.lastName) {
    return `${userProfile.value.firstName || ''} ${userProfile.value.lastName || ''}`.trim()
  }
  return null
})

// Methods
const loadUserProfile = async () => {
  loading.value = true
  error.value = ''
  
  try {
    // First check authentication status to determine if user is temp
    const authResponse = await fetch('/Authentication/test', {
      credentials: 'include'
    })
    
    if (authResponse.ok) {
      const authData = await authResponse.json()
      if (authData.isAuthenticated) {
        const tempUserClaim = authData.claims?.find(claim => claim.type === 'IsTempUser')
        isTempUser.value = tempUserClaim?.value === 'true'
        
        if (isTempUser.value) {
          // For temp users, create a basic profile from auth data
          userProfile.value = {
            userName: authData.userName || 
                     (authData.claims?.find(claim => claim.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')?.value) ||
                     (authData.claims?.find(claim => claim.type === 'name')?.value) ||
                     'User',
            email: authData.userName || 'N/A',
            firstName: '',
            lastName: '',
            phoneNumber: '',
            emailConfirmed: false,
            role: 'Temporary User'
          }
          loading.value = false
          return
        }
      }
    }
    
    // For registered users, fetch full profile
    const response = await fetch('/api/profile', {
      credentials: 'include',
      headers: {
        'Accept': 'application/json'
      }
    })
    
    if (!response.ok) {
      throw new Error(`Failed to load profile: ${response.status}`)
    }
    
    const data = await response.json()
    userProfile.value = data
  } catch (err) {
    error.value = 'Failed to load profile. Please try again.'
  } finally {
    loading.value = false
  }
}

const startEditing = () => {
  if (isTempUser.value) return
  
  // Populate edit form with current values
  editForm.firstName = userProfile.value?.firstName || ''
  editForm.lastName = userProfile.value?.lastName || ''
  editForm.phoneNumber = userProfile.value?.phoneNumber || ''
  
  // Clear errors
  Object.keys(errors).forEach(key => errors[key] = '')
  
  isEditing.value = true
}

const cancelEditing = () => {
  isEditing.value = false
  // Clear form and errors
  Object.keys(editForm).forEach(key => editForm[key] = '')
  Object.keys(errors).forEach(key => errors[key] = '')
}

const validateForm = () => {
  // Clear previous errors
  Object.keys(errors).forEach(key => errors[key] = '')
  
  let isValid = true
  
  if (!editForm.firstName.trim()) {
    errors.firstName = 'First name is required'
    isValid = false
  }
  
  if (!editForm.lastName.trim()) {
    errors.lastName = 'Last name is required'
    isValid = false
  }
  
  return isValid
}

const saveProfile = async () => {
  if (!validateForm()) return
  
  saving.value = true
  
  try {
    const response = await fetch('/api/profile', {
      method: 'PUT',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      body: JSON.stringify({
        firstName: editForm.firstName.trim(),
        lastName: editForm.lastName.trim(),
        phoneNumber: editForm.phoneNumber.trim() || null
      })
    })
    
    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(errorData.error || 'Failed to update profile')
    }
    
    const updatedProfile = await response.json()
    userProfile.value = updatedProfile
    isEditing.value = false
    
    // Show success message
    showNotification('Information updated successfully!', 'success')
  } catch (err) {
    showNotification(err.message || 'Failed to update information', 'error')
  } finally {
    saving.value = false
  }
}

const getRoleClass = (role) => {
  if (!role) return 'role-default'
  const roleType = role.toLowerCase()
  if (roleType.includes('admin')) return 'role-admin'
  if (roleType.includes('manager')) return 'role-manager'
  if (roleType.includes('user')) return 'role-user'
  if (roleType.includes('temp')) return 'role-temp'
  return 'role-default'
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
  }, 4000)
}

// Lifecycle
onMounted(() => {
  loadUserProfile()
})
</script>

<style scoped lang="scss">
.user-dashboard {
  width: 100%;
  padding: 2rem;
  font-family: 'Karla', Arial, Helvetica, sans-serif;

  @media (max-width: 768px) {
    padding: 1rem;
  }
}

/* Dashboard Content Layout */
.dashboard-content {
  max-width: 800px;
  margin: 0 auto;

  .profile-card {
    background: rgb(var(--v-theme-surface));
    border: 1px solid rgb(var(--v-theme-border));
    border-radius: 12px;
    overflow: hidden;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);

    .card-header {
      background: linear-gradient(135deg, rgb(var(--v-theme-primary)) 0%, rgb(var(--v-theme-secondary)) 100%);
      color: white;
      padding: 2rem;
      border-bottom: 1px solid rgb(var(--v-theme-border));
      display: flex;
      justify-content: space-between;
      align-items: flex-start;

      @media (max-width: 768px) {
        flex-direction: column;
        gap: 1.5rem;
        text-align: center;
        padding: 1.5rem;
      }

      .header-content {
        display: flex;
        align-items: center;
        gap: 1.5rem;
        flex: 1;

        @media (max-width: 640px) {
          flex-direction: column;
          text-align: center;
          gap: 1rem;
        }

        .profile-icon {
          font-size: 3rem;
          opacity: 0.9;

          @media (max-width: 640px) {
            font-size: 2.5rem;
          }
        }

        .header-text {
          flex: 1;

          h1 {
            margin: 0 0 1rem 0;
            font-size: 2rem;
            font-weight: 600;
            color: white;

            @media (max-width: 640px) {
              font-size: 1.8rem;
            }
          }

          .user-info {
            margin-bottom: 1rem;

            .email-info, .phone-info {
              display: flex;
              align-items: center;
              gap: 0.75rem;
              margin: 0.5rem 0;
              font-size: 0.95rem;
              color: white;
              opacity: 0.95;

              @media (max-width: 640px) {
                justify-content: center;
              }

              i {
                width: 16px;
                text-align: center;
                opacity: 0.8;
              }
            }

            .user-roles {
              margin-top: 1rem;
              display: flex;
              flex-wrap: wrap;
              gap: 0.5rem;

              @media (max-width: 640px) {
                justify-content: center;
              }
            }
          }

          .temp-user-notice {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            margin: 1rem 0 0 0;
            font-size: 0.9rem;
            opacity: 0.9;
            padding: 0.75rem;
            background: rgba(255, 215, 0, 0.2);
            border: 1px solid rgba(255, 215, 0, 0.3);
            border-radius: 6px;

            @media (max-width: 640px) {
              justify-content: center;
            }

            i {
              color: #ffd700;
            }
          }
        }
      }

      .header-actions {
        margin-left: 1rem;

        @media (max-width: 768px) {
          margin-left: 0;
          width: 100%;
          display: flex;
          justify-content: center;
        }

        .btn {
          background: rgba(255, 255, 255, 0.2);
          color: white;
          border: 1px solid rgba(255, 255, 255, 0.3);

          &:hover:not(:disabled) {
            background: rgba(255, 255, 255, 0.3);
          }
        }
      }
    }
  }
}

/* Loading and Error States */
.loading-container, .error-container {
  text-align: center;
  padding: 4rem 2rem;
  background: rgb(var(--v-theme-surface));
  border: 1px solid rgb(var(--v-theme-border));
  border-radius: 12px;
  color: rgb(var(--v-theme-on-surface));
  margin: 2rem auto;
  max-width: 500px;

  i {
    font-size: 2.5rem;
    margin-bottom: 1rem;
    display: block;
    color: rgb(var(--v-theme-primary));
  }

  span {
    font-size: 1.1rem;
    display: block;
    margin-bottom: 1.5rem;
  }

  .retry-btn {
    background: rgb(var(--v-theme-primary));
    color: white;
    border: none;
    padding: 0.75rem 1.5rem;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.9rem;
    transition: background-color 0.3s;

    &:hover {
      background: rgb(var(--v-theme-secondary));
    }

    i {
      margin-right: 0.5rem;
      font-size: 0.9rem;
      display: inline;
      margin-bottom: 0;
    }
  }
}



/* Temp User Message */
.temp-user-message {
  margin: 0 2rem 2rem 2rem;
  padding: 1.5rem;
  background: #fff8e1;
  border: 1px solid #ffcc02;
  border-radius: 8px;
  display: flex;
  gap: 1rem;

  @media (max-width: 640px) {
    margin: 0 1.5rem 1.5rem 1.5rem;
    flex-direction: column;
    text-align: center;
    gap: 0.75rem;
  }

  i {
    color: #f57c00;
    font-size: 1.5rem;
    margin-top: 0.25rem;

    @media (max-width: 640px) {
      margin-top: 0;
    }
  }

  div {
    h3 {
      margin: 0 0 0.5rem 0;
      color: #e65100;
      font-size: 1.1rem;
    }

    p {
      margin: 0;
      color: #bf360c;
      line-height: 1.5;
    }
  }
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.modal-content {
  background: rgb(var(--v-theme-surface));
  border-radius: 12px;
  max-width: 600px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem 2rem;
  border-bottom: 1px solid rgb(var(--v-theme-border));
  background: rgb(var(--v-theme-background));
  border-radius: 12px 12px 0 0;

  h2 {
    margin: 0;
    color: rgb(var(--v-theme-on-background));
    font-size: 1.4rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;

    i {
      color: rgb(var(--v-theme-primary));
    }
  }

  .modal-close-btn {
    background: none;
    border: none;
    font-size: 1.2rem;
    color: rgb(var(--v-theme-medium));
    cursor: pointer;
    padding: 0.5rem;
    border-radius: 4px;
    transition: background-color 0.3s;

    &:hover {
      background: rgb(var(--v-theme-border));
    }
  }
}

.modal-body {
  padding: 2rem;

  .form-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1rem;
    margin-bottom: 1rem;

    @media (max-width: 640px) {
      grid-template-columns: 1fr;
    }
  }

  .form-group {
    margin-bottom: 1rem;

    label {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-weight: 600;
      color: rgb(var(--v-theme-on-surface));
      margin-bottom: 0.5rem;

      i {
        color: rgb(var(--v-theme-primary));
        width: 16px;
      }
    }

    .form-control {
      width: 100%;
      padding: 0.75rem;
      border: 2px solid rgb(var(--v-theme-border));
      border-radius: 6px;
      font-size: 0.9rem;
      transition: border-color 0.3s, box-shadow 0.3s;
      background: rgb(var(--v-theme-background));
      color: rgb(var(--v-theme-on-background));

      &:focus {
        outline: none;
        border-color: rgb(var(--v-theme-primary));
        box-shadow: 0 0 0 3px rgba(var(--v-theme-primary), 0.1);
      }

      &.error {
        border-color: #d32f2f;
        box-shadow: 0 0 0 3px rgba(211, 47, 47, 0.1);
      }
    }

    .error-text {
      color: #d32f2f;
      font-size: 0.8rem;
      margin-top: 0.25rem;
      display: block;
    }

    .form-text {
      color: rgb(var(--v-theme-medium));
      font-size: 0.8rem;
      margin-top: 0.25rem;
      display: block;
    }
  }
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding: 0 2rem 2rem;

  @media (max-width: 640px) {
    flex-direction: column;
  }
}

/* Read-only Section */

.role-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 600;
  background: rgba(255, 255, 255, 0.2);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.3);

  &.role-admin {
    background: rgba(255, 255, 255, 0.9);
    color: #c62828;
  }

  &.role-manager {
    background: rgba(255, 255, 255, 0.9);
    color: #1565c0;
  }

  &.role-user {
    background: rgba(255, 255, 255, 0.9);
    color: #2e7d32;
  }

  &.role-temp {
    background: rgba(255, 255, 255, 0.9);
    color: #f57c00;
  }

  &.role-default {
    background: rgba(255, 255, 255, 0.2);
    color: white;
  }
}

/* Buttons */
.btn {
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 6px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.9rem;
  text-decoration: none;

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }

  &.btn-primary {
    background: rgb(var(--v-theme-primary));
    color: white;

    &:hover:not(:disabled) {
      background: #0056A4;
    }
  }

  &.btn-secondary {
    background: rgb(var(--v-theme-background));
    color: rgb(var(--v-theme-on-background));
    border: 1px solid rgb(var(--v-theme-border));

    &:hover:not(:disabled) {
      background: rgb(var(--v-theme-border));
    }
  }
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
  background-color: #2e7d32 !important;
  background: #2e7d32 !important;
  color: white !important;
}

:global(.notification.notification-error) {
  background-color: #d32f2f !important;
  background: #d32f2f !important;
  color: white !important;
}

@keyframes slideIn {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}
</style>
