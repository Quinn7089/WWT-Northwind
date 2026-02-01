<template>
  <div>
    <div v-if="loading" class="role-table-section__placeholder">
      <div class="role-table-section__spinner"></div>
      <p>Loading roles...</p>
    </div>

    <div v-else-if="roles.length === 0" class="role-table-section__placeholder role-table-section__placeholder--empty">
      <i class="fas fa-users-cog"></i>
      <p>No roles found. Create a role to get started.</p>
    </div>

    <table v-else class="role-table-section__table">
      <thead>
        <tr>
          <th>Role Name</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="role in roles" :key="role.id">
          <td>{{ role.name }}</td>
          <td class="role-table-section__actions">
            <button 
              v-if="canEditRoles" 
              class="role-table-section__action-btn" 
              @click="$emit('edit', role)"
            >
              <i class="fas fa-edit"></i>
              Edit
            </button>
            <button class="role-table-section__action-btn" @click="$emit('view', role)">
              <i class="fas fa-user-friends"></i>
              View Users
            </button>
            <button 
              v-if="canEditRoles"
              class="role-table-section__action-btn" 
              @click="$emit('viewPermissions', role)"
            >
              <i class="fas fa-key"></i>
              View Permissions
            </button>
            <button
              v-if="canEditRoles"
              class="role-table-section__action-btn role-table-section__action-btn--danger"
              @click="$emit('delete', role)"
              :disabled="deletingRoleId === role.id"
            >
              <i class="fas fa-trash"></i>
              <span v-if="deletingRoleId === role.id">Removing...</span>
              <span v-else>Delete</span>
            </button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup>
const props = defineProps({
  roles: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  },
  deletingRoleId: {
    type: String,
    default: ''
  },
  canEditRoles: {
    type: Boolean,
    default: false
  }
})

defineEmits(['edit', 'view', 'viewPermissions', 'delete'])
</script>

<style scoped lang="scss">
.role-table-section__table {
  width: 100%;
  border-collapse: collapse;
  background: rgb(var(--v-theme-surface));
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);

  th {
    background-color: rgb(var(--v-theme-primary));
    color: rgb(var(--v-theme-on-primary));
    padding: 0.75rem;
    text-align: left;
    font-weight: 600;
    font-size: 0.9rem;
  }

  td {
    padding: 0.75rem;
    border-bottom: 1px solid rgb(var(--v-theme-border));
    color: rgb(var(--v-theme-on-surface));
    font-size: 0.95rem;
  }

  tr:hover {
    background-color: rgba(var(--v-theme-primary), 0.04);
  }
}

.role-table-section__actions {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  flex-wrap: wrap;
}

.role-table-section__action-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.45rem 0.75rem;
  border-radius: 6px;
  border: 1px solid rgb(var(--v-theme-secondary));
  background: transparent;
  font-weight: 600;
  color: rgb(var(--v-theme-secondary));
  cursor: pointer;
  transition: all 0.2s ease;
  font-size: 0.9rem;

  i {
    font-size: 0.9rem;
  }

  &:hover:not(:disabled) {
    background: rgb(var(--v-theme-secondary));
    color: rgb(var(--v-theme-on-secondary));
    transform: translateY(-1px);
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
}

.role-table-section__action-btn--danger {
  color: rgb(var(--v-theme-error));
  border-color: rgba(var(--v-theme-error), 0.3);
  background: rgba(var(--v-theme-error), 0.1);

  &:hover:not(:disabled) {
    background: rgba(var(--v-theme-error), 0.15);
  }
}

.role-table-section__placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
  padding: 2rem 1rem;
  color: rgb(var(--v-theme-on-surface-variant));
  text-align: center;

  i {
    font-size: 2rem;
    color: rgb(var(--v-theme-primary));
  }
}

.role-table-section__spinner {
  width: 42px;
  height: 42px;
  border-radius: 50%;
  border: 4px solid rgba(var(--v-theme-primary), 0.15);
  border-top-color: rgb(var(--v-theme-primary));
  animation: role-table-section-spin 1s linear infinite;
}

@keyframes role-table-section-spin {
  to {
    transform: rotate(360deg);
  }
}
</style>

