<template>
  <v-btn
    variant="outlined"
    @click="toggleTheme"
    :title="isDark ? 'Switch to Light Mode' : 'Switch to Dark Mode'"
    class="theme-toggle-btn"
    size="small"
  >
    <i :class="isDark ? 'fas fa-sun' : 'fas fa-moon'" class="theme-icon"></i>
  </v-btn>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useTheme } from 'vuetify'

const theme = useTheme()
const isDark = ref(false)

// Load saved theme preference from localStorage
const loadThemePreference = () => {
  const savedTheme = localStorage.getItem('app-theme')
  if (savedTheme) {
    theme.global.name.value = savedTheme
    isDark.value = savedTheme === 'dark'
  } else {
    // Default to light theme if no preference is saved
    theme.global.name.value = 'light'
    isDark.value = false
  }
}

// Save theme preference to localStorage
const saveThemePreference = (themeName) => {
  localStorage.setItem('app-theme', themeName)
}

// Toggle between light and dark themes
const toggleTheme = () => {
  const newTheme = isDark.value ? 'light' : 'dark'
  theme.global.name.value = newTheme
  isDark.value = !isDark.value
  saveThemePreference(newTheme)
}

// Initialize theme on component mount
onMounted(() => {
  loadThemePreference()
})
</script>

<style scoped lang="scss">
.theme-toggle-btn {
  transition: all 0.3s ease;
  border: 2px solid rgba(255, 255, 255, 0.3) !important;
  background-color: rgba(255, 255, 255, 0.1) !important;
  color: white !important;
  
  &:hover {
    transform: scale(1.1);
    background-color: rgba(255, 255, 255, 0.2) !important;
    border-color: rgba(255, 255, 255, 0.5) !important;
  }

  .theme-icon {
    color: white !important;
    font-size: 16px;
  }
}

// Add rotation animation for the sun icon when switching
.theme-toggle-btn:active {
  .fa-sun {
    animation: rotate 0.3s ease-in-out;
  }
}

@keyframes rotate {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(180deg);
  }
}
</style>