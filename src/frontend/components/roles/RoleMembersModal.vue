<template>
  <div v-if="show" class="role-members-modal__overlay" @click="emitClose">
    <div class="role-members-modal role-members-modal--large" @click.stop>
      <div class="role-members-modal__header">
        <h3>
          <i class="fas fa-user-friends"></i>
          {{ role?.name || 'Role' }} Members
        </h3>
        <button @click="emitClose">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <div class="role-members-modal__body">
        <div class="role-members-modal__search-row">
          <div class="role-members-modal__form-group">
            <label for="userSearch">Search Users</label>
            <input
              id="userSearch"
              type="text"
              class="role-members-modal__control"
              v-model="localSearch"
              placeholder="Search by username, email, or student ID"
              autocomplete="off"
            />
          </div>
        </div>

        <div v-if="errorMessage" class="role-members-modal__alert role-members-modal__alert--error">
          <i class="fas fa-exclamation-circle"></i>
          {{ errorMessage }}
        </div>

        <div v-if="loading" class="role-members-modal__placeholder">
          <div class="role-members-modal__spinner"></div>
          <p>Loading users...</p>
        </div>

        <div v-else-if="users.length === 0" class="role-members-modal__placeholder">
          <i class="fas fa-users"></i>
          <p>No users match your search.</p>
        </div>

        <table v-else class="role-members-table">
          <thead>
            <tr>
              <th>User</th>
              <th>Email</th>
              <th>Student ID</th>
              <th>In Role</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in users" :key="user.id">
              <td>{{ user.userName }}</td>
              <td>{{ user.email || 'N/A' }}</td>
              <td>{{ user.studentId || '—' }}</td>
              <td>
                <span
                  :class="[
                    'role-members-table__badge',
                    user.isMember
                      ? 'role-members-table__badge--success'
                      : 'role-members-table__badge--muted'
                  ]"
                >
                  {{ user.isMember ? 'Yes' : 'No' }}
                </span>
              </td>
              <td class="role-members-table__actions">
                <button
                  v-if="user.isMember"
                  class="role-members-table__action-btn role-members-table__action-btn--danger"
                  @click="$emit('remove-user', user)"
                  :disabled="!!busyMap[user.id]"
                >
                  <i class="fas fa-user-minus"></i>
                  <span v-if="busyMap[user.id]">Removing...</span>
                  <span v-else>Remove</span>
                </button>
                <button
                  v-else
                  class="role-members-table__action-btn"
                  @click="$emit('add-user', user)"
                  :disabled="!!busyMap[user.id]"
                >
                  <i class="fas fa-user-plus"></i>
                  <span v-if="busyMap[user.id]">Adding...</span>
                  <span v-else>Add</span>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="role-members-modal__footer">
        <button class="role-members-modal__button role-members-modal__button--secondary" @click="emitClose">
          Close
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  show: {
    type: Boolean,
    default: false
  },
  role: {
    type: Object,
    default: null
  },
  searchTerm: {
    type: String,
    default: ''
  },
  users: {
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
  busyMap: {
    type: Object,
    default: () => ({})
  }
})

const emit = defineEmits(['close', 'update:searchTerm', 'add-user', 'remove-user'])

const localSearch = ref(props.searchTerm)

watch(
  () => props.show,
  (visible) => {
    if (visible) {
      localSearch.value = props.searchTerm
    }
  }
)

watch(
  () => props.searchTerm,
  (value) => {
    if (props.show) {
      localSearch.value = value
    }
  }
)

watch(localSearch, (value) => {
  emit('update:searchTerm', value)
})

const emitClose = () => emit('close')
</script>

<style scoped lang="scss">
.role-members-modal__overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 30, 60, 0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.role-members-modal {
  background: rgb(var(--v-theme-surface));
  border-radius: 10px;
  width: 100%;
  max-width: 520px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.25);
  display: flex;
  flex-direction: column;
  max-height: 90vh;
  overflow: hidden;

  &--large {
    max-width: 900px;
  }
}

.role-members-modal__header {
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

.role-members-modal__body {
  padding: 1.5rem;
  overflow-y: auto;
}

.role-members-modal__footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid rgb(var(--v-theme-border));
}

.role-members-modal__button {
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

.role-members-modal__button--secondary {
  background: rgb(var(--v-theme-background));
  color: rgb(var(--v-theme-on-background));
  border: 1px solid rgb(var(--v-theme-border));

  &:hover:not(:disabled) {
    background: rgb(var(--v-theme-border));
    transform: translateY(-1px);
  }
}

.role-members-modal__search-row {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  margin-bottom: 1.5rem;
}

.role-members-modal__form-group {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;

  label {
    font-weight: 600;
    color: rgb(var(--v-theme-on-background));
    margin-bottom: 0.4rem;
    font-size: 0.9rem;
  }
}

.role-members-modal__control {
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

  &::placeholder {
    color: rgb(var(--v-theme-medium));
  }
}

.role-members-modal__alert {
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

.role-members-modal__placeholder {
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

.role-members-modal__spinner {
  width: 42px;
  height: 42px;
  border-radius: 50%;
  border: 4px solid rgba(var(--v-theme-primary), 0.15);
  border-top-color: rgb(var(--v-theme-primary));
  animation: role-members-modal-spin 1s linear infinite;
}

@keyframes role-members-modal-spin {
  to {
    transform: rotate(360deg);
  }
}

.role-members-table {
  width: 100%;
  border-collapse: collapse;

  th {
    background-color: rgb(var(--v-theme-primary));
    color: rgb(var(--v-theme-on-primary));
    padding: 0.75rem;
    text-align: left;
    font-weight: 600;
  }

  td {
    padding: 0.75rem;
    border-bottom: 1px solid rgb(var(--v-theme-border));
    color: rgb(var(--v-theme-on-surface));
  }

  tr:hover {
    background: rgb(var(--v-theme-surface-bright));
  }
}

.role-members-table__actions {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
  align-items: center;
}

.role-members-table__action-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.45rem 0.75rem;
  border-radius: 6px;
  border: 1px solid rgb(var(--v-theme-border));
  background: rgb(var(--v-theme-surface-bright));
  font-weight: 600;
  color: rgb(var(--v-theme-on-surface));
  cursor: pointer;
  transition: background 0.2s ease, transform 0.1s ease, opacity 0.2s ease;

  i {
    font-size: 0.9rem;
  }

  &:hover:not(:disabled) {
    background: rgba(var(--v-theme-surface-bright), 0.8);
    transform: translateY(-1px);
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
}

.role-members-table__action-btn--danger {
  color: rgb(var(--v-theme-error));
  border-color: rgba(var(--v-theme-error), 0.3);
  background: rgba(var(--v-theme-error), 0.1);

  &:hover:not(:disabled) {
    background: rgba(var(--v-theme-error), 0.15);
  }
}

.role-members-table__badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
}

.role-members-table__badge--success {
  background: rgba(var(--v-theme-success), 0.15);
  color: rgb(var(--v-theme-success));
}

.role-members-table__badge--muted {
  background: rgba(var(--v-theme-outline), 0.15);
  color: rgb(var(--v-theme-on-surface-variant));
}

@media (max-width: 768px) {
  .role-members-modal--large {
    max-width: 100%;
    height: 90vh;
  }
}
</style>
