<template>
  <nav class="nav-wrapper">
    <div class="nav-inner">
      <div class="nav-links">
        <RouterLink v-if="canAccess('/')" to="/" class="nav-link">Home</RouterLink>
        <div 
          v-if="canAccess('/user-management') || canAccess('/role-management')"
          class="dropdown"
          @mouseenter="showDropdown = true"
          @mouseleave="showDropdown = false"
        >
          <button class="dropdown-toggle nav-link">
            Management
            <span class="dropdown-arrow">▼</span>
          </button>
          <div v-show="showDropdown" class="dropdown-menu">
            <RouterLink v-if="canAccess('/user-management')" to="/user-management" class="dropdown-item">Manage Users</RouterLink>
            <RouterLink v-if="canAccess('/role-management')" to="/role-management" class="dropdown-item">Manage Roles</RouterLink>
          </div>
        </div>
      </div>
      <UserMenu />
    </div>
  </nav>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import UserMenu from './UserMenu.vue'
import { useAuth } from '../router/useAuth'

const { canAccess, fetchCurrentUser } = useAuth()
const showDropdown = ref(false)

onMounted(async () => {
  await fetchCurrentUser()
})


</script>

<style scoped lang="scss">
.nav-wrapper {
  width: 100%;
  background-color: rgb(var(--v-theme-primary));
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
  position: sticky;
  top: 0;
  z-index: 10;
}

.nav-inner {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0.75rem 1.5rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
}

.nav-links {
  display: flex;
  flex-wrap: wrap;
  gap: 1.5rem;
  align-items: center;
}

.nav-link {
  color: rgb(var(--v-theme-on-primary));
  text-decoration: none;
  font-weight: 500;
  font-size: 1rem;
  padding: 0.5rem 0.75rem;
  border-radius: 6px;
  transition: background-color 0.3s ease, color 0.3s ease;

  &:hover {
    background-color: rgba(var(--v-theme-on-primary), 0.15);
  }

  &.router-link-active {
    background-color: rgba(var(--v-theme-on-primary), 0.25);
    font-weight: 600;
  }
}

/* Dropdown styles */
.dropdown {
  position: relative;
  display: inline-block;
}

.dropdown-toggle {
  background: none;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.dropdown-arrow {
  font-size: 0.75rem;
  transition: transform 0.3s ease;
}

.dropdown:hover .dropdown-arrow {
  transform: rotate(180deg);
}

.dropdown-menu {
  position: absolute;
  top: 100%;
  left: 0;
  background-color: rgb(var(--v-theme-surface));
  border: 1px solid rgba(var(--v-theme-on-surface), 0.12);
  border-radius: 6px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  min-width: 180px;
  z-index: 20;
}

.dropdown-item {
  display: block;
  color: rgb(var(--v-theme-on-surface));
  text-decoration: none;
  padding: 0.75rem 1rem;
  font-weight: 500;
  transition: background-color 0.3s ease;

  &:hover {
    background-color: rgba(var(--v-theme-primary), 0.1);
  }

  &.router-link-active {
    background-color: rgba(var(--v-theme-primary), 0.15);
    font-weight: 600;
  }

  &:first-child {
    border-top-left-radius: 6px;
    border-top-right-radius: 6px;
  }

  &:last-child {
    border-bottom-left-radius: 6px;
    border-bottom-right-radius: 6px;
  }
}

/* Mobile adjustments */
@media (max-width: 768px) {
  .nav-inner {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }

  .nav-links {
    width: 100%;
    justify-content: flex-start;
    gap: 1rem;
  }

  .nav-link {
    padding: 0.5rem 1rem;
  }

  .dropdown-menu {
    position: static;
    box-shadow: none;
    border: none;
    background-color: transparent;
    margin-top: 0;
    margin-left: 1rem;
  }

  .dropdown-item {
    padding: 0.5rem 0;
    color: rgb(var(--v-theme-on-primary));
    
    &:hover {
      background-color: rgba(var(--v-theme-on-primary), 0.15);
    }
  }
}
</style>
