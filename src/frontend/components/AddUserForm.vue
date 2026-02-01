<template>
  <!-- Add User Modal -->
  <div v-if="show" class="modal-overlay" @click="closeModal">
    <div class="modal-content add-user-modal" @click.stop>
      <div class="modal-header">
        <h3>
          <i class="fas fa-user-plus"></i>
          Add New User
        </h3>
        <button class="close-btn" @click="closeModal">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <div class="modal-body">
        <form @submit.prevent="submitAddUser" class="add-user-form">
          <!-- Basic Information -->
          <div class="form-section">
            <h4 class="section-title">
              <i class="fas fa-user"></i>
              Basic Information
            </h4>

            <div class="form-row">
              <div class="form-group">
                <label for="email">Email Address *</label>
                <input
                  type="email"
                  id="email"
                  v-model="newUser.email"
                  :class="['form-control', { error: errors.email }]"
                  placeholder="Enter email address"
                  required
                />
                <span v-if="errors.email" class="error-text">{{ errors.email }}</span>
              </div>

              <div class="form-group">
                <label for="wctcId">WCTC ID *</label>
                <input
                  type="text"
                  id="wctcId"
                  v-model="newUser.wctcId"
                  :class="['form-control', { error: errors.wctcId }]"
                  placeholder="Enter 9-digit WCTC ID"
                  required
                  maxlength="9"
                  pattern="[0-9]{9}"
                />
                <span v-if="errors.wctcId" class="error-text">{{ errors.wctcId }}</span>
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label for="firstName">First Name *</label>
                <input
                  type="text"
                  id="firstName"
                  v-model="newUser.firstName"
                  :class="['form-control', { error: errors.firstName }]"
                  placeholder="Enter first name"
                  required
                />
                <span v-if="errors.firstName" class="error-text">{{ errors.firstName }}</span>
              </div>

              <div class="form-group">
                <label for="lastName">Last Name *</label>
                <input
                  type="text"
                  id="lastName"
                  v-model="newUser.lastName"
                  :class="['form-control', { error: errors.lastName }]"
                  placeholder="Enter last name"
                  required
                />
                <span v-if="errors.lastName" class="error-text">{{ errors.lastName }}</span>
              </div>
            </div>

            <div class="form-group">
              <label for="phoneNumber">Phone Number</label>
              <input
                type="tel"
                id="phoneNumber"
                v-model="newUser.phoneNumber"
                class="form-control"
                placeholder="Enter phone number (optional)"
              />
            </div>
          </div>

          <!-- Role & Permissions -->
          <div class="form-section">
            <h4 class="section-title">
              <i class="fas fa-users-cog"></i>
              Role & Permissions
            </h4>

            <div class="form-group">
              <label>User Roles</label>
              <div v-if="loadingRoles" class="loading-text">
                <i class="fas fa-spinner fa-spin"></i>
                Loading roles...
              </div>
              <div v-else class="roles-container">
                <div v-if="roles.length === 0" class="role-checkbox">
                  <label class="checkbox-label">
                    <input
                      type="checkbox"
                      value="User"
                      v-model="newUser.selectedRoles"
                      checked
                    />
                    <span class="checkmark"></span>
                    User
                  </label>
                </div>
                <div v-for="role in roles" :key="role.id" class="role-checkbox">
                  <label class="checkbox-label">
                    <input
                      type="checkbox"
                      :value="role.name"
                      v-model="newUser.selectedRoles"
                    />
                    <span class="checkmark"></span>
                    {{ role.name }}
                  </label>
                </div>
              </div>
              <span v-if="errors.role" class="error-text">{{ errors.role }}</span>
            </div>
          </div>
        </form>
      </div>

      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" @click="closeModal">
          <i class="fas fa-times"></i>
          Cancel
        </button>
        <button
          type="button"
          class="btn btn-primary"
          @click="submitAddUser"
          :disabled="isSubmitting"
        >
          <i class="fas fa-spinner fa-spin" v-if="isSubmitting"></i>
          <i class="fas fa-user-plus" v-else></i>
          {{ isSubmitting ? 'Creating...' : 'Create User' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, defineEmits, defineProps, onMounted } from 'vue'

const props = defineProps({ show: Boolean })
const emit = defineEmits(['close', 'user-created'])

const isSubmitting = ref(false)
const roles = ref([])
const loadingRoles = ref(false)

const newUser = reactive({
  email: '',
  wctcId: '',
  firstName: '',
  lastName: '',
  phoneNumber: '',
  selectedRoles: ['User'], // Changed to support multiple roles
  isActive: true
})

const errors = reactive({ email: '', wctcId: '', firstName: '', lastName: '', role: '' })

const closeModal = () => {
  resetForm()
  emit('close')
}

const resetForm = () => {
  Object.assign(newUser, {
    email: '',
    wctcId: '',
    firstName: '',
    lastName: '',
    phoneNumber: '',
    selectedRoles: ['User'],
    isActive: true
  })
  Object.keys(errors).forEach(key => (errors[key] = ''))
}

const fetchRoles = async () => {
  loadingRoles.value = true
  try {
    const res = await fetch('/Role', { credentials: 'include', headers: { Accept: 'application/json' } })
    if (res.ok) {
      const data = await res.json()
      roles.value = data || []
      // Set default role if available
      if (roles.value.length > 0) {
        const defaultRole = roles.value.find(role => role.name === 'User')
        if (defaultRole) {
          newUser.selectedRoles = ['User']
        } else {
          newUser.selectedRoles = [roles.value[0].name]
        }
      } else {
        newUser.selectedRoles = ['User']
      }
    }
  } finally {
    loadingRoles.value = false
  }
}

const validateForm = () => {
  Object.keys(errors).forEach(key => (errors[key] = ''))
  let valid = true
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  const wctcIdRegex = /^[0-9]{9}$/
  
  if (!newUser.email.trim()) (errors.email = 'Email is required', valid = false)
  else if (!emailRegex.test(newUser.email)) (errors.email = 'Enter a valid email', valid = false)
  
  if (!newUser.wctcId.trim()) (errors.wctcId = 'WCTC ID is required', valid = false)
  else if (!wctcIdRegex.test(newUser.wctcId.trim())) (errors.wctcId = 'WCTC ID must be exactly 9 digits', valid = false)
  
  if (!newUser.firstName.trim()) (errors.firstName = 'First name required', valid = false)
  if (!newUser.lastName.trim()) (errors.lastName = 'Last name required', valid = false)
  
  if (!newUser.selectedRoles || newUser.selectedRoles.length === 0) (errors.role = 'At least one role must be selected', valid = false)
  
  return valid
}

const submitAddUser = async () => {
  if (!validateForm()) return
  isSubmitting.value = true
  try {
    // Prepare the data to match backend expectations
    const userData = {
      email: newUser.email.trim(),
      studentId: newUser.wctcId.trim(),
      firstName: newUser.firstName.trim(),
      lastName: newUser.lastName.trim(),
      phoneNumber: newUser.phoneNumber?.trim() || null,
      roles: newUser.selectedRoles || ['User'], // Use selectedRoles array
      isActive: newUser.isActive
    }


    const response = await fetch('/api/users', {
      method: 'POST',
      credentials: 'include',
      headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
      body: JSON.stringify(userData)
    })

    if (response.ok) {
      const createdUser = await response.json()
      emit('user-created', createdUser)
      closeModal()
    } else {
      const errorData = await response.json()
      alert(`Failed to create user: ${errorData.error || 'Unknown error'}`)
    }
  } catch (error) {
    alert('An error occurred while creating the user. Please try again.')
  } finally {
    isSubmitting.value = false
  }
}

onMounted(fetchRoles)
</script>

<style scoped lang="scss">
/* Overlay */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 30, 60, 0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

/* Modal */
.modal-content {
  background: rgb(var(--v-theme-surface));
  border: 1px solid rgb(var(--v-theme-border));
  border-radius: 10px;
  width: 95%;
  max-width: 700px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.15);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: rgb(var(--v-theme-primary));
  color: rgb(var(--v-theme-on-primary));
  padding: 1rem 1.25rem;
  border-radius: 10px 10px 0 0;

  h3 {
    font-size: 1.2rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }

  .close-btn {
    background: none;
    border: none;
    color: rgb(var(--v-theme-on-primary));
    font-size: 1.2rem;
    cursor: pointer;
    transition: opacity 0.2s;

    &:hover {
      opacity: 0.85;
    }
  }
}

.modal-body {
  padding: 1.5rem;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid rgb(var(--v-theme-border));
}

/* Form sections */
.form-section {
  margin-bottom: 1.5rem;

  .section-title {
    color: rgb(var(--v-theme-primary));
    font-weight: 700;
    font-size: 1.05rem;
    margin-bottom: 0.75rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    border-bottom: 2px solid rgb(var(--v-theme-border));
    padding-bottom: 0.25rem;
  }
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;

  @media (max-width: 600px) {
    grid-template-columns: 1fr;
  }
}

.form-group {
  label {
    display: block;
    font-weight: 600;
    color: rgb(var(--v-theme-on-background));
    margin-bottom: 0.4rem;
    font-size: 0.9rem;
  }

  .form-control {
    width: 100%;
    padding: 0.65rem 0.75rem;
    border: 1px solid rgb(var(--v-theme-border));
    border-radius: 6px;
    font-size: 1rem;
    transition: border-color 0.25s, box-shadow 0.25s;
    background: rgb(var(--v-theme-surface));
    color: rgb(var(--v-theme-on-surface));

    &:focus {
      border-color: rgb(var(--v-theme-primary));
      box-shadow: 0 0 0 3px rgba(0, 62, 122, 0.1);
      outline: none;
    }

    &.error {
      border-color: rgb(var(--v-theme-error));
    }

    &::placeholder {
      color: rgb(var(--v-theme-medium));
    }
  }

  .error-text {
    color: rgb(var(--v-theme-error));
    font-size: 0.8rem;
    margin-top: 2px;
  }
}

/* Buttons */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  font-weight: 600;
  padding: 0.6rem 1.2rem;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  transition: background 0.3s ease, transform 0.15s ease;

  &.btn-primary {
    background: rgb(var(--v-theme-primary));
    color: #fff;

    &:hover {
      background: rgb(var(--v-theme-secondary));
      transform: translateY(-1px);
    }

    &:disabled {
      opacity: 0.6;
      cursor: not-allowed;
      transform: none;
    }
  }

  &.btn-secondary {
    background: rgb(var(--v-theme-background));
    color: rgb(var(--v-theme-on-background));
    border: 1px solid rgb(var(--v-theme-border));

    &:hover {
      background: rgb(var(--v-theme-border));
      transform: translateY(-1px);
    }
  }
}

/* Role Selection Styles */
.loading-text {
  color: rgb(var(--v-theme-medium));
  font-style: italic;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.roles-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 0.75rem;
  margin-top: 0.5rem;
  
  @media (max-width: 600px) {
    grid-template-columns: 1fr;
  }
}

.role-radio {
  .radio-label {
    display: flex;
    align-items: center;
    cursor: pointer;
    padding: 0.5rem;
    border: 1px solid rgb(var(--v-theme-border));
    border-radius: 6px;
    transition: all 0.2s ease;
    background: #fff;
    font-weight: 500;
    
    &:hover {
      border-color: rgb(var(--v-theme-primary));
      background: rgba(var(--v-theme-primary), 0.05);
    }
    
    input[type="radio"] {
      position: absolute;
      opacity: 0;
      cursor: pointer;
      
      &:checked + .radiomark {
        background: rgb(var(--v-theme-primary));
        border-color: rgb(var(--v-theme-primary));
        
        &::after {
          display: block;
        }
      }
    }
    
    .radiomark {
      position: relative;
      height: 18px;
      width: 18px;
      background: #fff;
      border: 2px solid rgb(var(--v-theme-border));
      border-radius: 50%;
      margin-right: 0.5rem;
      transition: all 0.2s ease;
      
      &::after {
        content: "";
        position: absolute;
        display: none;
        left: 5px;
        top: 5px;
        width: 6px;
        height: 6px;
        border-radius: 50%;
        background: white;
      }
    }
  }
}

.role-checkbox {
  .checkbox-label {
    display: flex;
    align-items: center;
    cursor: pointer;
    padding: 0.5rem;
    border: 1px solid rgb(var(--v-theme-border));
    border-radius: 6px;
    transition: all 0.2s ease;
    background: rgb(var(--v-theme-surface));
    color: rgb(var(--v-theme-on-surface));
    font-weight: 500;
    
    &:hover {
      border-color: rgb(var(--v-theme-primary));
      background: rgba(var(--v-theme-primary), 0.05);
    }
    
    input[type="checkbox"] {
      position: absolute;
      opacity: 0;
      cursor: pointer;
      
      &:checked + .checkmark {
        background: rgb(var(--v-theme-primary));
        border-color: rgb(var(--v-theme-primary));
        
        &::after {
          display: block;
        }
      }
    }
    
    .checkmark {
      position: relative;
      height: 18px;
      width: 18px;
      background: rgb(var(--v-theme-surface));
      border: 2px solid rgb(var(--v-theme-border));
      border-radius: 3px;
      margin-right: 0.5rem;
      transition: all 0.2s ease;
      
      &::after {
        content: "";
        position: absolute;
        display: none;
        left: 5px;
        top: 2px;
        width: 5px;
        height: 8px;
        border: solid white;
        border-width: 0 2px 2px 0;
        transform: rotate(45deg);
      }
    }
  }
}
</style>
