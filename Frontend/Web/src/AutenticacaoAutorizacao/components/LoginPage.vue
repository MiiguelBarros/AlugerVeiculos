<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useAuthStore } from '@/AutenticacaoAutorizacao/stores/auth.store'
import { getErrorMessage, getValidationErrors } from '@/Shared/utils/apiErrors'
import type { Login } from '@/AutenticacaoAutorizacao/models/Login'
import type { ValidationErrors } from '@/Shared/models/ResponseDTO'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const form = reactive<Login>({ email: '', password: '' })
const fieldErrors = ref<ValidationErrors>({})
const errorMessage = ref('')
const loading = ref(false)

async function submit() {
  loading.value = true
  fieldErrors.value = {}
  errorMessage.value = ''

  try {
    await authStore.login(form)

    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/'
    await router.push(redirect)
  } catch (error) {
    fieldErrors.value = getValidationErrors(error)

    if (Object.keys(fieldErrors.value).length === 0)
      errorMessage.value = getErrorMessage(error)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login">
    <div class="accent-bar" />

    <div class="login-body">
      <div class="brand">
        <span class="brand-mark"><i class="pi pi-car" /></span>
        <span class="brand-name">Aluguer de Veículos</span>
      </div>

      <Card class="login-card">
        <template #title>
          <h1 class="page-title login-title">Iniciar sessão</h1>
        </template>

        <template #content>
          <form class="login-form" novalidate @submit.prevent="submit">
            <Message v-if="errorMessage" severity="error">{{ errorMessage }}</Message>

            <div class="field">
              <label for="email" class="field-label">Email</label>
              <InputText
                id="email"
                v-model="form.email"
                type="email"
                autocomplete="username"
                :invalid="!!fieldErrors.email"
                fluid
              />
              <Message v-if="fieldErrors.email" severity="error" size="small" variant="simple">
                {{ fieldErrors.email[0] }}
              </Message>
            </div>

            <div class="field">
              <label for="password" class="field-label">Password</label>
              <Password
                v-model="form.password"
                input-id="password"
                :input-props="{ autocomplete: 'current-password' }"
                :feedback="false"
                :invalid="!!fieldErrors.password"
                toggle-mask
                fluid
              />
              <Message v-if="fieldErrors.password" severity="error" size="small" variant="simple">
                {{ fieldErrors.password[0] }}
              </Message>
            </div>

            <Button type="submit" label="Entrar" :loading="loading" fluid />
          </form>
        </template>
      </Card>
    </div>
  </div>
</template>

<style scoped>
.login {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--app-black);
}

.accent-bar {
  height: 4px;
  background: var(--app-orange);
}

.login-body {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 2rem;
  padding: 1rem;
}

.brand {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: #ffffff;
}

.brand-mark {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: var(--app-orange);
  font-size: 1.35rem;
}

.brand-name {
  font-family: var(--app-heading-font);
  font-size: 1.75rem;
  font-weight: 900;
  text-transform: uppercase;
}

.login-card {
  width: 100%;
  max-width: 420px;
}

.login-title {
  margin-bottom: 0.75rem;
  font-size: 2rem;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
}
</style>
