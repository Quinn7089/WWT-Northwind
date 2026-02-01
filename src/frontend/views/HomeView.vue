<script setup>
</script>

<template>
  <div class="home">
    <section class="intro">
      <h1>Welcome to the Starter App</h1>
      <p class="subtitle">
        A foundation built with <strong>ASP.NET Core 8.0</strong> and <strong>Vue.js 3</strong>,
        designed for building reliable, modern, full-stack applications.
      </p>
    </section>

    <section class="content">
      <p>
        This template follows WCTC’s clean, accessible design principles with a focus on usability,
        consistency, and strong visual hierarchy. It’s an ideal starting point for your web or software
        development projects.
      </p>
    </section>
  </div>
</template>

<style scoped lang="scss">
.home {
  max-width: 900px;
  margin: 0 auto;
  padding: 3rem 1.5rem;
  font-family: 'Karla', Arial, Helvetica, sans-serif;
  color: rgb(var(--v-theme-on-background));
  text-align: center;
  background: rgb(var(--v-theme-surface));
  border-radius: 8px;
  border: 1px solid rgb(var(--v-theme-border));
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.04);
}

.intro {
  border-bottom: 2px solid rgb(var(--v-theme-border));
  padding-bottom: 1.5rem;
  margin-bottom: 2rem;

  h1 {
    color: rgb(var(--v-theme-primary));
    font-size: 2rem;
    font-weight: 700;
    margin-bottom: 0.5rem;
  }

  .subtitle {
    color: rgb(var(--v-theme-medium));
    font-size: 1.1rem;
    line-height: 1.6;
    max-width: 600px;
    margin: 0 auto;
  }
}

.content {
  p {
    font-size: 1rem;
    line-height: 1.7;
    color: rgb(var(--v-theme-on-surface));
    max-width: 700px;
    margin: 0 auto;
  }
}
</style>
