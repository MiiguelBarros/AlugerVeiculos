<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Select from 'primevue/select'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { vehicleService } from '@/Veiculos/services/vehicle.service'
import { fuelTypeLabels, toOptions } from '@/Shared/utils/labels'
import { getErrorMessage, getValidationErrors } from '@/Shared/utils/apiErrors'
import type { Vehicle } from '@/Veiculos/models/Vehicle'
import type { VehicleRequest } from '@/Veiculos/models/VehicleRequest'
import type { ValidationErrors } from '@/Shared/models/ResponseDTO'

const props = defineProps<{ vehicle: Vehicle | null }>()
const emit = defineEmits<{ saved: [vehicle: Vehicle] }>()
const visible = defineModel<boolean>('visible', { required: true })

const toast = useToast()
const fuelTypeOptions = toOptions(fuelTypeLabels)

const form = reactive<VehicleRequest>(createEmptyForm())
const fieldErrors = ref<ValidationErrors>({})
const saving = ref(false)

const isEdit = computed(() => props.vehicle !== null)

function createEmptyForm(): VehicleRequest {
  return { brand: '', model: '', licensePlate: '', year: null, fuelType: null }
}

watch(visible, (isVisible) => {
  if (!isVisible)
    return

  fieldErrors.value = {}
  Object.assign(form, props.vehicle
    ? {
        brand: props.vehicle.brand,
        model: props.vehicle.model,
        licensePlate: props.vehicle.licensePlate,
        year: props.vehicle.year,
        fuelType: props.vehicle.fuelType,
      }
    : createEmptyForm())
})

async function submit() {
  saving.value = true
  fieldErrors.value = {}

  try {
    const vehicle = props.vehicle
      ? await vehicleService.update(props.vehicle.vehicleId, form)
      : await vehicleService.create(form)

    toast.add({
      severity: 'success',
      summary: isEdit.value ? 'Veículo atualizado' : 'Veículo registado',
      detail: vehicle.licensePlate,
      life: 3000,
    })

    emit('saved', vehicle)
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
    :header="isEdit ? 'Editar veículo' : 'Novo veículo'"
    :style="{ width: '34rem' }"
    :draggable="false"
    modal
  >
    <form id="vehicle-form" class="form" novalidate @submit.prevent="submit">
      <div class="form-row">
        <div class="field">
          <label for="brand" class="field-label">Marca</label>
          <InputText id="brand" v-model="form.brand" :invalid="!!fieldErrors.brand" fluid />
          <Message v-if="fieldErrors.brand" severity="error" size="small" variant="simple">
            {{ fieldErrors.brand[0] }}
          </Message>
        </div>

        <div class="field">
          <label for="model" class="field-label">Modelo</label>
          <InputText id="model" v-model="form.model" :invalid="!!fieldErrors.model" fluid />
          <Message v-if="fieldErrors.model" severity="error" size="small" variant="simple">
            {{ fieldErrors.model[0] }}
          </Message>
        </div>
      </div>

      <div class="form-row">
        <div class="field">
          <label for="licensePlate" class="field-label">Matrícula</label>
          <InputText
            id="licensePlate"
            v-model="form.licensePlate"
            placeholder="AA-00-AA"
            :invalid="!!fieldErrors.licensePlate"
            fluid
          />
          <Message v-if="fieldErrors.licensePlate" severity="error" size="small" variant="simple">
            {{ fieldErrors.licensePlate[0] }}
          </Message>
        </div>

        <div class="field">
          <label for="year" class="field-label">Ano de fabrico</label>
          <InputNumber
            v-model="form.year"
            input-id="year"
            :use-grouping="false"
            :invalid="!!fieldErrors.year"
            fluid
          />
          <Message v-if="fieldErrors.year" severity="error" size="small" variant="simple">
            {{ fieldErrors.year[0] }}
          </Message>
        </div>
      </div>

      <div class="field">
        <label for="fuelType" class="field-label">Tipo de combustível</label>
        <Select
          v-model="form.fuelType"
          input-id="fuelType"
          :options="fuelTypeOptions"
          option-label="label"
          option-value="value"
          placeholder="Selecione o combustível"
          :invalid="!!fieldErrors.fuelType"
          fluid
        />
        <Message v-if="fieldErrors.fuelType" severity="error" size="small" variant="simple">
          {{ fieldErrors.fuelType[0] }}
        </Message>
      </div>
    </form>

    <template #footer>
      <Button label="Cancelar" severity="secondary" text @click="visible = false" />
      <Button type="submit" form="vehicle-form" :label="isEdit ? 'Guardar' : 'Registar'" :loading="saving" />
    </template>
  </Dialog>
</template>
