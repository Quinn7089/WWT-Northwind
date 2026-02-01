<template>
  <div class="user-menu" v-if="isLoggedIn">
    <button 
      @click="toggleMenu" 
      class="user-menu-button"
      :class="{ 'menu-open': isMenuOpen }"
    >
      <i class="fas fa-user"></i>
      <span class="username-text">{{ username || 'User' }}</span>
      <i class="fas fa-chevron-down dropdown-arrow" :class="{ 'rotated': isMenuOpen }"></i>
    </button>
    
    <div class="user-menu-dropdown" v-show="isMenuOpen">
      <RouterLink to="/user" class="menu-item profile-info" @click="closeMenu">
        <i class="fas fa-user-circle"></i>
        <span>User Profile</span>
      </RouterLink>
      
      <div class="menu-divider"></div>
      
      <button 
        @click="toggleTheme" 
        class="menu-item theme-toggle-item"
      >
        <i :class="isDark ? 'fas fa-sun' : 'fas fa-moon'"></i>
        <span>{{ isDark ? 'Light Mode' : 'Dark Mode' }}</span>
      </button>
      
      <div class="menu-divider"></div>
      
      <button 
        @click="logout" 
        class="menu-item logout-item"
        :disabled="isLoading"
      >
        <i class="fas fa-sign-out-alt"></i>
        <span>{{ isLoading ? 'Logging out...' : 'Logout' }}</span>
      </button>
    </div>
    
    <!-- Overlay to close menu when clicking outside -->
    <div 
      v-if="isMenuOpen" 
      class="menu-overlay" 
      @click="closeMenu"
    ></div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useTheme } from 'vuetify'

const theme = useTheme()
const isDark = ref(false)
const isLoggedIn = ref(false)
const isLoading = ref(false)
const isMenuOpen = ref(false)
const username = ref('')

onMounted(() => {
  checkAuthStatus()
  loadThemePreference()
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})

// Theme toggle functionality
const loadThemePreference = () => {
  const savedTheme = localStorage.getItem('app-theme')
  if (savedTheme) {
    theme.global.name.value = savedTheme
    isDark.value = savedTheme === 'dark'
  } else {
    theme.global.name.value = 'light'
    isDark.value = false
  }
}

const saveThemePreference = (themeName) => {
  localStorage.setItem('app-theme', themeName)
}

const toggleTheme = () => {
  const newTheme = isDark.value ? 'light' : 'dark'
  theme.global.name.value = newTheme
  isDark.value = !isDark.value
  saveThemePreference(newTheme)
  // Don't close the menu when toggling theme, let user see the change
}

const checkAuthStatus = async () => {
  try {
    const response = await fetch('/Authentication/test', {
      credentials: 'include'
    })
    
    if (response.ok) {
      const data = await response.json()
      isLoggedIn.value = data.isAuthenticated
      
      // If authenticated, extract username from the response like UserView does
      if (data.isAuthenticated) {
        username.value = data.userName || 
                       (data.claims?.find(claim => claim.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')?.value) ||
                       (data.claims?.find(claim => claim.type === 'name')?.value) ||
                       'User'
      } else {
        username.value = ''
      }
    } else {
      isLoggedIn.value = false
      username.value = ''
    }
  } catch (error) {
    isLoggedIn.value = false
    username.value = ''
  }
}

const toggleMenu = () => {
  isMenuOpen.value = !isMenuOpen.value
}

const closeMenu = () => {
  isMenuOpen.value = false
}

const handleClickOutside = (event) => {
  const userMenu = event.target.closest('.user-menu')
  if (!userMenu && isMenuOpen.value) {
    closeMenu()
  }
}

const logout = async () => {
  isLoading.value = true
  closeMenu()
  try {
    window.location.href = '/Authentication/logout'
  } catch (error) {
    isLoading.value = false
  }
}
</script>

<style scoped lang="scss">
.user-menu {
  position: relative;
  display: inline-block;
}

.user-menu-button {
  background-color: rgba(var(--v-theme-on-primary), 0.15);
  color: rgb(var(--v-theme-on-primary));
  border: 1px solid rgba(var(--v-theme-on-primary), 0.2);
  border-radius: 6px;
  padding: 0.5rem 1rem;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  
  &:hover {
    background-color: rgba(var(--v-theme-on-primary), 0.25);
  }
  
  &.menu-open {
    background-color: rgba(var(--v-theme-on-primary), 0.25);
  }
}

.username-text {
  font-weight: 500;
  max-width: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.dropdown-arrow {
  transition: transform 0.3s ease;
  font-size: 0.8rem;
  
  &.rotated {
    transform: rotate(180deg);
  }
}

.user-menu-dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: 0.5rem;
  background-color: rgb(var(--v-theme-surface));
  border: 1px solid rgba(var(--v-theme-outline), 0.2);
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  min-width: 200px;
  overflow: hidden;
  z-index: 1000;
  animation: slideDown 0.2s ease;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.menu-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  color: rgb(var(--v-theme-on-surface));
  text-decoration: none;
  transition: background-color 0.2s ease;
  border: none;
  background: none;
  width: 100%;
  text-align: left;
  font-size: 0.95rem;
  cursor: pointer;
  
  &:hover {
    background-color: rgba(var(--v-theme-primary), 0.08);
  }
  
  &.profile-info {
    font-weight: 500;
    color: rgb(var(--v-theme-on-surface));
    text-decoration: none;
    
    &:hover {
      background-color: rgba(var(--v-theme-primary), 0.08);
    }
    
    &.router-link-active {
      background-color: rgba(var(--v-theme-primary), 0.15);
      font-weight: 600;
    }
  }
  
  &.logout-item {
    color: rgb(var(--v-theme-error));
    
    &:hover:not(:disabled) {
      background-color: rgba(var(--v-theme-error), 0.08);
    }
    
    &:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
  }
  
  &.theme-toggle-item {
    color: rgb(var(--v-theme-secondary));
    
    &:hover {
      background-color: rgba(var(--v-theme-secondary), 0.08);
    }
    
    i {
      transition: transform 0.3s ease;
    }
    
    &:active i {
      transform: scale(1.2);
    }
  }
}

.menu-divider {
  height: 1px;
  background-color: rgba(var(--v-theme-outline), 0.12);
  margin: 0.25rem 0;
}

.menu-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 999;
  background: transparent;
}

/* Mobile adjustments */
@media (max-width: 768px) {
  .user-menu-button {
    padding: 0.6rem 1rem;
  }
  
  .user-menu-dropdown {
    right: 0;
    min-width: 180px;
  }
}
</style>