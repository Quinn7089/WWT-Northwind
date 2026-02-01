<template>
  <div v-if="show" class="role-permissions-modal__overlay" @click="emitClose">
    <div class="role-permissions-modal" @click.stop>
      <div class="role-permissions-modal__header">
        <h3>
          <i class="fas fa-key"></i>
          {{ role?.name || 'Role' }} Permissions
        </h3>
        <button @click="emitClose">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <div class="role-permissions-modal__body">
        <div v-if="errorMessage" class="role-permissions-modal__alert role-permissions-modal__alert--error">
          <i class="fas fa-exclamation-circle"></i>
          {{ errorMessage }}
        </div>

        <div v-if="loading" class="role-permissions-modal__placeholder">
          <div class="role-permissions-modal__spinner"></div>
          <p>Loading permissions...</p>
        </div>

        <div v-else class="role-permissions-modal__permissions-list">
          <div
            v-for="permission in allPermissions"
            :key="permission.value"
            class="role-permissions-modal__permission-item"
          >
            <label class="role-permissions-modal__permission-label">
              <input
                type="checkbox"
                :checked="hasPermission(permission.value)"
                @change="togglePermission(permission.value, $event.target.checked)"
                :disabled="saving || !!busyMap[permission.value]"
                class="role-permissions-modal__checkbox"
              />
              <span class="role-permissions-modal__permission-text">
                <span class="role-permissions-modal__permission-name">{{ formatPermissionName(permission.value) }}</span>
                <span class="role-permissions-modal__permission-key">{{ permission.value }}</span>
              </span>
              <span v-if="busyMap[permission.value]" class="role-permissions-modal__busy-indicator">
                <i class="fas fa-spinner fa-spin"></i>
              </span>
            </label>
          </div>
        </div>
      </div>

      <div class="role-permissions-modal__footer">
        <button class="role-permissions-modal__button role-permissions-modal__button--secondary" @click="emitClose">
          Close
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { PERMISSIONS } from '../../router/permissions'

const props = defineProps({
  show: {
    type: Boolean,
    default: false
  },
  role: {
    type: Object,
    default: null
  },
  rolePermissions: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  },
  errorMessage: {
    type: String,
    default: ''
  },
  saving: {
    type: Boolean,
    default: false
  },
  busyMap: {
    type: Object,
    default: () => ({})
  }
})

const emit = defineEmits(['close', 'toggle-permission'])

const allPermissions = computed(() => {
  return Object.entries(PERMISSIONS).map(([key, value]) => ({
    key,
    value
  }))
})

const hasPermission = (permissionValue) => {
  return props.rolePermissions.includes(permissionValue)
}

const formatPermissionName = (permissionValue) => {
  return permissionValue
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, str => str.toUpperCase())
    .trim()
}

const togglePermission = (permissionValue, isChecked) => {
  emit('toggle-permission', permissionValue, isChecked)
}

const emitClose = () => emit('close')
</script>

<style scoped lang="scss">
.role-permissions-modal__overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 30, 60, 0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.role-permissions-modal {
  background: rgb(var(--v-theme-surface));
  border-radius: 10px;
  width: 100%;
  max-width: 600px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.25);
  display: flex;
  flex-direction: column;
  max-height: 90vh;
  overflow: hidden;
}

.role-permissions-modal__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.25rem;
  background: rgb(var(--v-theme-primary));
  color: rgb(var(--v-theme-on-primary));
  border-radius: 10px 10px 0 0;

  h3 {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    margin: 0;
    color: rgb(var(--v-theme-on-primary));
    font-size: 1.2rem;
  }

  button {
    border: none;
    background: transparent;
    color: rgb(var(--v-theme-on-primary));
    font-size: 1.2rem;
    cursor: pointer;
    transition: opacity 0.2s ease;

    &:hover {
      opacity: 0.85;
    }
  }
}

.role-permissions-modal__body {
  padding: 1.5rem;
  overflow-y: auto;
}

.role-permissions-modal__footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid rgb(var(--v-theme-border));
}

.role-permissions-modal__button {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  font-weight: 600;
  padding: 0.6rem 1.2rem;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  transition: background 0.3s ease, transform 0.15s ease;

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
    transform: none;
  }
}

.role-permissions-modal__button--secondary {
  background: rgb(var(--v-theme-background));
  color: rgb(var(--v-theme-on-background));
  border: 1px solid rgb(var(--v-theme-border));

  &:hover:not(:disabled) {
    background: rgb(var(--v-theme-border));
    transform: translateY(-1px);
  }
}

.role-permissions-modal__alert {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  border-radius: 8px;
  padding: 0.75rem 1rem;
  margin-bottom: 1rem;
  font-weight: 500;

  &--error {
    background: rgba(var(--v-theme-error), 0.12);
    color: rgb(var(--v-theme-error));
    border: 1px solid rgba(var(--v-theme-error), 0.25);
  }
}

.role-permissions-modal__placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
  padding: 2rem 1rem;
  color: rgb(var(--v-theme-on-surface-variant));
  text-align: center;
}

.role-permissions-modal__spinner {
  width: 42px;
  height: 42px;
  border-radius: 50%;
  border: 4px solid rgba(var(--v-theme-primary), 0.15);
  border-top-color: rgb(var(--v-theme-primary));
  animation: role-permissions-modal-spin 1s linear infinite;
}

@keyframes role-permissions-modal-spin {
  to {
    transform: rotate(360deg);
  }
}

.role-permissions-modal__permissions-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.role-permissions-modal__permission-item {
  padding: 1rem;
  border: 1px solid rgb(var(--v-theme-border));
  border-radius: 8px;
  background: rgb(var(--v-theme-surface));
  transition: border-color 0.2s ease, background-color 0.2s ease;

  &:hover {
    border-color: rgb(var(--v-theme-outline));
    background: rgb(var(--v-theme-surface-bright));
  }
}

.role-permissions-modal__permission-label {
  display: flex;
  align-items: center;
  gap: 1rem;
  cursor: pointer;
  position: relative;
}

.role-permissions-modal__checkbox {
  width: 20px;
  height: 20px;
  cursor: pointer;
  accent-color: rgb(var(--v-theme-primary));
  flex-shrink: 0;

  &:disabled {
    cursor: not-allowed;
    opacity: 0.6;
  }
}

.role-permissions-modal__permission-text {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  flex: 1;
}

.role-permissions-modal__permission-name {
  font-weight: 600;
  color: rgb(var(--v-theme-on-surface));
  font-size: 1rem;
}

.role-permissions-modal__permission-key {
  font-size: 0.85rem;
  color: rgb(var(--v-theme-on-surface));
  opacity: 0.7;
  font-family: 'Courier New', monospace;
}

.role-permissions-modal__busy-indicator {
  display: flex;
  align-items: center;
  color: rgb(var(--v-theme-primary));
  font-size: 0.9rem;
}

@media (max-width: 768px) {
  .role-permissions-modal {
    max-width: 100%;
    height: 90vh;
  }
}
</style>

