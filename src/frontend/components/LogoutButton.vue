<template>
  <button 
    @click="logout" 
    class="logout-button"
    :disabled="isLoading || !isLoggedIn"
    v-if="isLoggedIn"
  >
    {{ isLoading ? 'Logging out...' : 'Logout' }}
  </button>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const isLoggedIn = ref(false)
const isLoading = ref(false)

onMounted(() => {
  checkAuthStatus()
})

const checkAuthStatus = async () => {
  try {
    const response = await fetch('/Authentication/test', {
      credentials: 'include'
    })
    
    if (response.ok) {
      const data = await response.json()
      isLoggedIn.value = data.isAuthenticated
    } else {
      isLoggedIn.value = false
    }
  } catch (error) {
    isLoggedIn.value = false
  }
}

const logout = async () => {
  isLoading.value = true
  try {
    window.location.href = '/Authentication/logout'
  } catch (error) {
    isLoading.value = false
  }
}
</script>

<style scoped lang="scss">
.logout-button {
  background-color: rgba(var(--v-theme-on-primary), 0.15);
  color: rgb(var(--v-theme-on-primary));
  border: 1px solid rgba(var(--v-theme-on-primary), 0.2);
  border-radius: 6px;
  padding: 0.5rem 1rem;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  letter-spacing: 0.3px;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.08);

  &:hover:not(:disabled) {
    background-color: rgba(var(--v-theme-on-primary), 0.25);
    transform: translateY(-1px);
  }

  &:active:not(:disabled) {
    background-color: rgba(var(--v-theme-on-primary), 0.35);
    transform: translateY(0);
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
}

/* Mobile adjustments */
@media (max-width: 768px) {
  .logout-button {
    width: 100%;
    text-align: center;
    padding: 0.75rem 1rem;
  }
}
</style>
