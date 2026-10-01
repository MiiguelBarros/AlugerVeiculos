<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { clientService } from '@/Clientes/services/client.service'
import { getErrorMessage, getValidationErrors } from '@/Shared/utils/apiErrors'
import type { Client } from '@/Clientes/models/Client'
import type { ClientRequest } from '@/Clientes/models/ClientRequest'
import type { ValidationErrors } from '@/Shared/models/ResponseDTO'

const props = defineProps<{ client: Client | null }>()
const emit = defineEmits<{ saved: [client: Client] }>()
const visible = defineModel<boolean>('visible', { required: true })

const toast = useToast()

const form = reactive<ClientRequest>(createEmptyForm())
const fieldErrors = ref<ValidationErrors>({})
const saving = ref(false)

const isEdit = computed(() => props.client !== null)

function createEmptyForm(): ClientRequest {
  return { fullName: '', email: '', phone: '', driverLicenseNumber: '' }
}

watch(visible, (isVisible) => {
  if (!isVisible)
    return

  fieldErrors.value = {}
  Object.assign(form, props.client
    ? {
        fullName: props.client.fullName,
        email: props.client.email,
        phone: props.client.phone,
        driverLicenseNumber: props.client.driverLicenseNumber,
      }
    : createEmptyForm())
})

async function submit() {
  saving.value = true
  fieldErrors.value = {}

  try {
    const client = props.client
      ? await clientService.update(props.client.clientId, form)
      : await clientService.create(form)

    toast.add({
      severity: 'success',
      summary: isEdit.value ? 'Cliente atualizado' : 'Cliente registado',
      detail: client.fullName,
      life: 3000,
    })

    emit('saved', client)
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
  <Dialog
    v-model:visible="visible"
    :header="isEdit ? 'Editar cliente' : 'Novo cliente'"
    :style="{ width: '34rem' }"
    :draggable="false"
    modal
  >
    <form id="client-form" class="form" novalidate @submit.prevent="submit">
      <div class="field">
        <label for="fullName" class="field-label">Nome completo</label>
        <InputText id="fullName" v-model="form.fullName" :invalid="!!fieldErrors.fullName" fluid />
        <Message v-if="fieldErrors.fullName" severity="error" size="small" variant="simple">
          {{ fieldErrors.fullName[0] }}
        </Message>
      </div>

      <div class="field">
        <label for="email" class="field-label">Email</label>
        <InputText id="email" v-model="form.email" type="email" :invalid="!!fieldErrors.email" fluid />
        <Message v-if="fieldErrors.email" severity="error" size="small" variant="simple">
          {{ fieldErrors.email[0] }}
        </Message>
      </div>

      <div class="form-row">
        <div class="field">
          <label for="phone" class="field-label">Telefone</label>
          <InputText
            id="phone"
            v-model="form.phone"
            inputmode="numeric"
            maxlength="9"
            placeholder="912345678"
            :invalid="!!fieldErrors.phone"
            fluid
          />
          <Message v-if="fieldErrors.phone" severity="error" size="small" variant="simple">
            {{ fieldErrors.phone[0] }}
          </Message>
        </div>

        <div class="field">
          <label for="driverLicenseNumber" class="field-label">Carta de condução</label>
          <InputText
            id="driverLicenseNumber"
            v-model="form.driverLicenseNumber"
            placeholder="P-123456 7"
            :invalid="!!fieldErrors.driverLicenseNumber"
            fluid
          />
          <Message v-if="fieldErrors.driverLicenseNumber" severity="error" size="small" variant="simple">
            {{ fieldErrors.driverLicenseNumber[0] }}
          </Message>
        </div>
      </div>
    </form>

    <template #footer>
      <Button label="Cancelar" severity="secondary" text @click="visible = false" />
      <Button type="submit" form="client-form" :label="isEdit ? 'Guardar' : 'Registar'" :loading="saving" />
    </template>
  </Dialog>
</template>
