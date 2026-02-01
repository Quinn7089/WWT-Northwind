<template>
  <div v-if="show" class="role-form-modal__overlay" @click="emitClose">
    <div class="role-form-modal" @click.stop>
      <div class="role-form-modal__header">
        <h3>{{ headingText }}</h3>
        <button @click="emitClose">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <div class="role-form-modal__body">
        <form @submit.prevent="handleSubmit">
          <div class="role-form-modal__group">
            <label for="roleName">Role Name</label>
            <input
              id="roleName"
              type="text"
              class="role-form-modal__control"
              :class="{ 'is-error': errorMessage }"
              v-model="localName"
              placeholder="Enter role name"
              autocomplete="off"
              required
            />
            <span v-if="errorMessage" class="role-form-modal__error">{{ errorMessage }}</span>
          </div>
        </form>
      </div>

      <div class="role-form-modal__footer">
        <button class="role-form-modal__button role-form-modal__button--secondary" @click="emitClose" :disabled="submitting">
          Cancel
        </button>
        <button class="role-form-modal__button role-form-modal__button--primary" @click="handleSubmit" :disabled="submitting">
          <i class="fas fa-spinner fa-spin" v-if="submitting"></i>
          {{ headingText }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, watch } from 'vue'

const props = defineProps({
  show: {
    type: Boolean,
    default: false
  },
  mode: {
    type: String,
    default: 'create'
  },
  initialRoleName: {
    type: String,
    default: ''
  },
  submitting: {
    type: Boolean,
    default: false
  },
  errorMessage: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['close', 'submit'])

const localName = ref(props.initialRoleName)

watch(
  () => props.show,
  (visible) => {
    if (visible) {
      localName.value = props.initialRoleName
    }
  }
)

watch(
  () => props.initialRoleName,
  (newName) => {
    if (props.show) {
      localName.value = newName
    }
  }
)

const headingText = computed(() =>
  props.mode === 'create' ? 'Create Role' : 'Edit Role'
)

const emitClose = () => emit('close')

const handleSubmit = () => {
  emit('submit', localName.value.trim())
}
</script>

<style scoped lang="scss">
.role-form-modal__overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 30, 60, 0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.role-form-modal {
  background: rgb(var(--v-theme-surface));
  border-radius: 10px;
  width: 100%;
  max-width: 520px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.25);
  display: flex;
  flex-direction: column;
  max-height: 90vh;
  overflow: hidden;
}

.role-form-modal__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.25rem;
  background: rgb(var(--v-theme-primary));
  color: rgb(var(--v-theme-on-primary));
  border-radius: 10px 10px 0 0;

  h3 {
    margin: 0;
    color: rgb(var(--v-theme-on-primary));
    font-size: 1.2rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
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

.role-form-modal__body {
  padding: 1.5rem;
  overflow-y: auto;
}

.role-form-modal__footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-top: 1px solid rgb(var(--v-theme-border));
}

.role-form-modal__group {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  margin-bottom: 1.25rem;

  label {
    font-weight: 600;
    color: rgb(var(--v-theme-on-background));
    margin-bottom: 0.4rem;
    font-size: 0.9rem;
  }
}

.role-form-modal__control {
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

  &.is-error {
    border-color: rgb(var(--v-theme-error));
  }

  &::placeholder {
    color: rgb(var(--v-theme-medium));
  }
}

.role-form-modal__error {
  color: rgb(var(--v-theme-error));
  font-size: 0.8rem;
  margin-top: 2px;
}

.role-form-modal__button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.4rem;
  font-weight: 600;
  padding: 0.6rem 1.2rem;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  transition: background 0.3s ease, transform 0.15s ease;

  i {
    font-size: 0.95rem;
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
    transform: none;
  }
}

.role-form-modal__button--primary {
  background: rgb(var(--v-theme-primary));
  color: #fff;

  &:hover:not(:disabled) {
    background: rgb(var(--v-theme-secondary));
    transform: translateY(-1px);
  }
}

.role-form-modal__button--secondary {
  background: rgb(var(--v-theme-background));
  color: rgb(var(--v-theme-on-background));
  border: 1px solid rgb(var(--v-theme-border));

  &:hover:not(:disabled) {
    background: rgb(var(--v-theme-border));
    transform: translateY(-1px);
  }
}
</style>
