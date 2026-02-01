<script setup>
import { RouterView } from 'vue-router'
import NavBar from './components/NavBar.vue'
import { useAuth } from './router/useAuth'
import { onMounted } from 'vue'

const { hasCertificateError, fetchCurrentUser } = useAuth()

onMounted(async () => {
  await fetchCurrentUser()
})
</script>

<template>
  <div class="app-container">
    <header class="header-full-width">
      <NavBar />
    </header>
    <div v-if="hasCertificateError" class="certificate-error-banner">
      <div class="certificate-error-content">
        <i class="fas fa-exclamation-triangle"></i>
        <span>The samlCert has expired, please notify your system admin right away</span>
      </div>
    </div>
    <main class="main-content">
      <RouterView />
    </main>
  </div>
</template>

<style scoped lang="scss">
.app-container {
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  font-family: 'Karla', Arial, Helvetica, sans-serif;
  background-color: rgb(var(--v-theme-background));
}

.header-full-width {
  width: 100%;
  margin: 0;
  padding: 0;
}

.main-content {  
  max-width: 1200px;
  width: 100%;
  margin: 0 auto;
  flex: 1;
  padding: 2rem 1rem;
}

.certificate-error-banner {
  width: 100%;
  background-color: #dc2626;
  color: #fff;
  padding: 1rem;
  text-align: center;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  z-index: 5;
}

.certificate-error-content {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  font-weight: 600;
  font-size: 1rem;

  i {
    font-size: 1.2rem;
  }
}
</style>
