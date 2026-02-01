<template>
  <!-- Edit User Modal -->
  <div v-if="show" class="modal-overlay" @click="closeModal">
    <div class="modal-content edit-user-modal" @click.stop>
      <div class="modal-header">
        <h3>
          <i class="fas fa-user-edit"></i>
          Edit User
        </h3>
        <button class="close-btn" @click="closeModal">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <div class="modal-body">
        <div v-if="loading" class="loading-spinner">
          <div class="spinner"></div>
          <p>Loading user data...</p>
        </div>

        <div v-else-if="error" class="error-message">
          <i class="fas fa-exclamation-triangle"></i>
          {{ error }}
        </div>

        <form v-else @submit.prevent="submitEditUser" class="edit-user-form">
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
                  v-model="editUser.email"
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
                  v-model="editUser.wctcId"
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
                <label for="userName">Username *</label>
                <input
                  type="text"
                  id="userName"
                  v-model="editUser.userName"
                  :class="['form-control', { error: errors.userName }]"
                  placeholder="Enter username"
                  required
                />
                <span v-if="errors.userName" class="error-text">{{ errors.userName }}</span>
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label for="firstName">First Name *</label>
                <input
                  type="text"
                  id="firstName"
                  v-model="editUser.firstName"
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
                  v-model="editUser.lastName"
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
                v-model="editUser.phoneNumber"
                class="form-control"
                placeholder="Enter phone number (optional)"
              />
            </div>
          </div>

          <!-- Account Status -->
          <div class="form-section">
            <h4 class="section-title">
              <i class="fas fa-shield-alt"></i>
              Account Status
            </h4>

            <div class="form-group">
              <label for="isActive">Account Status</label>
              <div class="toggle-container">
                <label class="toggle-switch">
                  <input
                    type="checkbox"
                    id="isActive"
                    v-model="editUser.isActive"
                  />
                  <span class="slider"></span>
                </label>
                <span class="toggle-label">
                  {{ editUser.isActive ? 'Active' : 'Inactive' }}
                </span>
              </div>
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
                      v-model="editUser.selectedRoles"
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
                      v-model="editUser.selectedRoles"
                    />
                    <span class="checkmark"></span>
                    {{ role.name }}
                  </label>
                </div>
              </div>
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
          @click="submitEditUser"
          :disabled="isSubmitting || loading"
        >
          <i class="fas fa-spinner fa-spin" v-if="isSubmitting"></i>
          <i class="fas fa-save" v-else></i>
          {{ isSubmitting ? 'Saving...' : 'Save Changes' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, defineEmits, defineProps, watch, onMounted } from 'vue'

const props = defineProps({
  show: Boolean,
  userId: String
})

const emit = defineEmits(['close', 'user-updated'])

const loading = ref(false)
const isSubmitting = ref(false)
const error = ref('')
const roles = ref([])
const loadingRoles = ref(false)

const editUser = reactive({
  id: '',
  email: '',
  wctcId: '',
  userName: '',
  firstName: '',
  lastName: '',
  phoneNumber: '',
  isActive: true,
  selectedRoles: ['User'] // Changed to support multiple roles
})

const errors = reactive({
  email: '',
  wctcId: '',
  userName: '',
  firstName: '',
  lastName: ''
})

const closeModal = () => {
  resetForm()
  emit('close')
}

const resetForm = () => {
  Object.assign(editUser, {
    email: '',
    wctcId: '',
    userName: '',
    firstName: '',
    lastName: '',
    phoneNumber: '',
    isActive: true,
    selectedRoles: ['User']
  })
  Object.keys(errors).forEach(key => (errors[key] = ''))
  error.value = ''
}

const fetchUser = async (userId) => {
  if (!userId) return
  
  loading.value = true
  error.value = ''
  
  try {
    const response = await fetch(`/api/users/${userId}`, {
      credentials: 'include',
      headers: {
        'Accept': 'application/json'
      }
    })
    
    if (!response.ok) {
      throw new Error(`Failed to fetch user: ${response.status}`)
    }
    
    const userData = await response.json()
    
    // Populate the form with user data
    Object.assign(editUser, {
      id: userData.id,
      email: userData.email || '',
      wctcId: userData.studentId || '',
      userName: userData.userName || '',
      firstName: userData.firstName || '',
      lastName: userData.lastName || '',
      phoneNumber: userData.phoneNumber || '',
      isActive: userData.isActive !== undefined ? userData.isActive : true,
      selectedRoles: userData.roles || ['User'] // Use roles array instead of single role
    })
  } catch (err) {
    error.value = 'Failed to load user data. Please try again.'
  } finally {
    loading.value = false
  }
}

const fetchRoles = async () => {
  loadingRoles.value = true
  try {
    const response = await fetch('/Role', {
      credentials: 'include',
      headers: {
        'Accept': 'application/json'
      }
    })
    
    if (response.ok) {
      const data = await response.json()
      roles.value = data || []
    }
  } catch (err) {
  } finally {
    loadingRoles.value = false
  }
}

const validateForm = () => {
  Object.keys(errors).forEach(key => (errors[key] = ''))
  let valid = true
  
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  const wctcIdRegex = /^[0-9]{9}$/
  
  if (!editUser.email.trim()) {
    errors.email = 'Email is required'
    valid = false
  } else if (!emailRegex.test(editUser.email)) {
    errors.email = 'Enter a valid email address'
    valid = false
  }
  
  if (!editUser.wctcId.trim()) {
    errors.wctcId = 'WCTC ID is required'
    valid = false
  } else if (!wctcIdRegex.test(editUser.wctcId.trim())) {
    errors.wctcId = 'WCTC ID must be exactly 9 digits'
    valid = false
  }
  
  if (!editUser.userName.trim()) {
    errors.userName = 'Username is required'
    valid = false
  }
  
  if (!editUser.firstName.trim()) {
    errors.firstName = 'First name is required'
    valid = false
  }
  
  if (!editUser.lastName.trim()) {
    errors.lastName = 'Last name is required'
    valid = false
  }
  
  return valid
}

const submitEditUser = async () => {
  if (!validateForm()) return
  
  isSubmitting.value = true
  
  try {
    const response = await fetch(`/api/users/${editUser.id}`, {
      method: 'PUT',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      body: JSON.stringify({
        id: editUser.id,
        studentId: editUser.wctcId,
        email: editUser.email,
        userName: editUser.userName,
        firstName: editUser.firstName,
        lastName: editUser.lastName,
        phoneNumber: editUser.phoneNumber,
        isActive: editUser.isActive,
        roles: editUser.selectedRoles // Send roles array instead of single role
      })
    })
    
    if (response.ok) {
      const updatedUser = await response.json()
      emit('user-updated', updatedUser)
      closeModal()
    } else {
      throw new Error(`Failed to update user: ${response.status}`)
    }
  } catch (err) {
    error.value = 'Failed to update user. Please try again.'
  } finally {
    isSubmitting.value = false
  }
}

// Watch for userId prop changes to fetch user data
watch(() => props.userId, (newUserId) => {
  if (newUserId && props.show) {
    fetchUser(newUserId)
  }
}, { immediate: true })

// Watch for show prop changes to fetch user data when modal opens
watch(() => props.show, (isShown) => {
  if (isShown && props.userId) {
    fetchUser(props.userId)
  } else if (!isShown) {
    resetForm()
  }
})

onMounted(() => {
  fetchRoles()
})
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

/* Loading Spinner */
.loading-spinner {
  text-align: center;
  padding: 2rem;

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
    
    &:has(.checkbox-input) {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      cursor: pointer;
    }
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

  .checkbox-input {
    width: 18px;
    height: 18px;
    accent-color: rgb(var(--v-theme-primary));
    cursor: pointer;
  }

  .error-text {
    color: rgb(var(--v-theme-error));
    font-size: 0.8rem;
    margin-top: 2px;
  }
}

/* Toggle Switch Styles */
.toggle-container {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-top: 0.25rem;
}

.toggle-switch {
  position: relative;
  display: inline-block;
  width: 50px;
  height: 24px;

  input {
    opacity: 0;
    width: 0;
    height: 0;

    &:checked + .slider {
      background-color: rgb(var(--v-theme-primary));

      &:before {
        transform: translateX(26px);
      }
    }

    &:focus + .slider {
      box-shadow: 0 0 1px rgb(var(--v-theme-primary));
    }
  }

  .slider {
    position: absolute;
    cursor: pointer;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(var(--v-theme-on-surface), 0.3);
    transition: 0.3s;
    border-radius: 24px;

    &:before {
      position: absolute;
      content: "";
      height: 18px;
      width: 18px;
      left: 3px;
      bottom: 3px;
      background-color: white;
      transition: 0.3s;
      border-radius: 50%;
    }
  }
}

.toggle-label {
  font-weight: 600;
  color: rgb(var(--v-theme-on-background));
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
    color: rgb(var(--v-theme-on-primary));

    &:hover {
      background: #0056A4;
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
</style>