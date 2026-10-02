<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Select from 'primevue/select'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { userService } from '@/Utilizadores/services/user.service'
import { roleLabels, toOptions } from '@/Shared/utils/labels'
import { getErrorMessage, getValidationErrors } from '@/Shared/utils/apiErrors'
import type { CreateUser } from '@/Utilizadores/models/CreateUser'
import type { User } from '@/Utilizadores/models/User'
import type { ValidationErrors } from '@/Shared/models/ResponseDTO'

const emit = defineEmits<{ saved: [user: User] }>()
const visible = defineModel<boolean>('visible', { required: true })

const toast = useToast()
const roleOptions = toOptions(roleLabels)

const form = reactive<CreateUser>(createEmptyForm())
const fieldErrors = ref<ValidationErrors>({})
const saving = ref(false)

function createEmptyForm(): CreateUser {
  return { name: '', email: '', password: '', confirmPassword: '', role: null }
}

watch(visible, (isVisible) => {
  if (!isVisible)
    return

  fieldErrors.value = {}
  Object.assign(form, createEmptyForm())
})

async function submit() {
  saving.value = true
  fieldErrors.value = {}

  try {
    const user = await userService.create(form)

    toast.add({ severity: 'success', summary: 'Funcionário criado', detail: user.name, life: 3000 })

    emit('saved', user)
    visible.value = false
  } catch (error) {
    fieldErrors.value = getValidationErrors(error)

    if (Object.keys(fieldErrors.value).length === 0)
      toast.add({ severity: 'error', summary: 'Não foi possível guardar', detail: getErrorMessage(error), life: 5000 })
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <Dialog v-model:visible="visible" header="Novo funcionário" :style="{ width: '34rem' }" :draggable="false" modal>
    <form id="user-form" class="form" novalidate @submit.prevent="submit">
      <div class="field">
        <label for="name" class="field-label">Nome</label>
        <InputText id="name" v-model="form.name" :invalid="!!fieldErrors.name" fluid />
        <Message v-if="fieldErrors.name" severity="error" size="small" variant="simple">
          {{ fieldErrors.name[0] }}
        </Message>
      </div>

      <div class="form-row">
        <div class="field">
          <label for="email" class="field-label">Email</label>
          <InputText
            id="email"
            v-model="form.email"
            type="email"
            autocomplete="off"
            :invalid="!!fieldErrors.email"
            fluid
          />
          <Message v-if="fieldErrors.email" severity="error" size="small" variant="simple">
            {{ fieldErrors.email[0] }}
          </Message>
        </div>

        <div class="field">
          <label for="role" class="field-label">Função</label>
          <Select
            v-model="form.role"
            input-id="role"
            :options="roleOptions"
            option-label="label"
            option-value="value"
            placeholder="Selecione a função"
            :invalid="!!fieldErrors.role"
            fluid
          />
          <Message v-if="fieldErrors.role" severity="error" size="small" variant="simple">
            {{ fieldErrors.role[0] }}
          </Message>
        </div>
      </div>

      <div class="form-row">
        <div class="field">
          <label for="password" class="field-label">Password</label>
          <Password
            v-model="form.password"
            input-id="password"
            :input-props="{ autocomplete: 'new-password' }"
            :feedback="false"
            :invalid="!!fieldErrors.password"
            toggle-mask
            fluid
          />
          <Message v-if="fieldErrors.password" severity="error" size="small" variant="simple">
            {{ fieldErrors.password[0] }}
          </Message>
          <small v-else class="field-hint">Mínimo de 8 caracteres, com maiúscula, minúscula, número e símbolo.</small>
        </div>

        <div class="field">
          <label for="confirmPassword" class="field-label">Confirmar password</label>
          <Password
            v-model="form.confirmPassword"
            input-id="confirmPassword"
            :input-props="{ autocomplete: 'new-password' }"
            :feedback="false"
            :invalid="!!fieldErrors.confirmPassword"
            toggle-mask
            fluid
          />
          <Message v-if="fieldErrors.confirmPassword" severity="error" size="small" variant="simple">
            {{ fieldErrors.confirmPassword[0] }}
          </Message>
        </div>
      </div>
    </form>

    <template #footer>
      <Button label="Cancelar" severity="secondary" text @click="visible = false" />
      <Button type="submit" form="user-form" label="Criar funcionário" :loading="saving" />
    </template>
  </Dialog>
</template>
