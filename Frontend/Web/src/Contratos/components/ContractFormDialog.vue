<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import Select from 'primevue/select'
import DatePicker from 'primevue/datepicker'
import InputNumber from 'primevue/inputnumber'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { contractService } from '@/Contratos/services/contract.service'
import { clientService } from '@/Clientes/services/client.service'
import { vehicleService } from '@/Veiculos/services/vehicle.service'
import { getErrorMessage, getValidationErrors } from '@/Shared/utils/apiErrors'
import { formatCurrency } from '@/Shared/utils/currency'
import { addDays, today, toDateOnly } from '@/Shared/utils/dates'
import type { Contract } from '@/Contratos/models/Contract'
import type { ValidationErrors } from '@/Shared/models/ResponseDTO'
import type { Vehicle } from '@/Veiculos/models/Vehicle'

interface ContractForm {
  clientId: number | null
  vehicleId: number | null
  startDate: Date | null
  endDate: Date | null
  startMileage: number | null
  dailyRate: number | null
}

interface SelectOption {
  label: string
  value: number
}

const emit = defineEmits<{ saved: [contract: Contract] }>()
const visible = defineModel<boolean>('visible', { required: true })

const toast = useToast()

const form = reactive<ContractForm>(createEmptyForm())
const clientOptions = ref<SelectOption[]>([])
const vehicles = ref<Vehicle[]>([])
const fieldErrors = ref<ValidationErrors>({})
const errorMessage = ref('')
const loadingOptions = ref(false)
const saving = ref(false)

const maxAdvanceDays = 7
const minStartDate = today()
const maxStartDate = addDays(minStartDate, maxAdvanceDays)
const minEndDate = computed(() => addDays(form.startDate ?? minStartDate, 1))

const vehicleOptions = computed<SelectOption[]>(() =>
  vehicles.value.map((vehicle) => ({
    label: `${vehicle.licensePlate} · ${vehicle.brand} ${vehicle.model}`,
    value: vehicle.vehicleId,
  })),
)

const selectedVehicle = computed(() => vehicles.value.find((vehicle) => vehicle.vehicleId === form.vehicleId) ?? null)

const priceHint = computed(() => {
  if (!form.startDate || !form.endDate || form.dailyRate === null)
    return 'O total é calculado a partir do preço por dia e das datas.'

  const days = Math.round((form.endDate.getTime() - form.startDate.getTime()) / 86_400_000)

  return `Total: ${formatCurrency(form.dailyRate * days)} (${days} ${days === 1 ? 'dia' : 'dias'}).`
})

const mileageHint = computed(() => {
  if (!selectedVehicle.value)
    return 'Selecione um veículo para preencher a quilometragem.'

  if (selectedVehicle.value.lastMileage === null)
    return 'Este veículo ainda não tem quilometragem registada. Indique o valor do conta-quilómetros.'

  return `Última quilometragem registada: ${selectedVehicle.value.lastMileage} km.`
})

function createEmptyForm(): ContractForm {
  return { clientId: null, vehicleId: null, startDate: null, endDate: null, startMileage: null, dailyRate: null }
}

async function loadOptions() {
  loadingOptions.value = true

  try {
    const [clients, activeVehicles] = await Promise.all([
      clientService.getAll({ status: 'Active' }),
      vehicleService.getAll({ status: 'Active', availability: 'Available' }),
    ])

    clientOptions.value = clients.map((client) => ({
      label: `${client.fullName} · ${client.driverLicenseNumber}`,
      value: client.clientId,
    }))

    vehicles.value = activeVehicles
  } catch (error) {
    errorMessage.value = getErrorMessage(error)
  } finally {
    loadingOptions.value = false
  }
}

watch(visible, (isVisible) => {
  if (!isVisible)
    return

  fieldErrors.value = {}
  errorMessage.value = ''
  Object.assign(form, createEmptyForm())
  loadOptions()
})

watch(() => form.startDate, () => {
  if (form.endDate && form.endDate < minEndDate.value)
    form.endDate = null
})

watch(selectedVehicle, (vehicle) => {
  form.startMileage = vehicle?.lastMileage ?? null
})

async function submit() {
  saving.value = true
  fieldErrors.value = {}
  errorMessage.value = ''

  try {
    const contract = await contractService.create({
      clientId: form.clientId,
      vehicleId: form.vehicleId,
      startDate: form.startDate ? toDateOnly(form.startDate) : null,
      endDate: form.endDate ? toDateOnly(form.endDate) : null,
      startMileage: form.startMileage,
      dailyRate: form.dailyRate,
    })

    toast.add({
      severity: 'success',
      summary: 'Contrato criado',
      detail: `${contract.clientName} · ${contract.vehicleLicensePlate}`,
      life: 3000,
    })

    emit('saved', contract)
    visible.value = false
  } catch (error) {
    fieldErrors.value = getValidationErrors(error)

    if (Object.keys(fieldErrors.value).length === 0)
      errorMessage.value = getErrorMessage(error)
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <Dialog v-model:visible="visible" header="Novo contrato" :style="{ width: '36rem' }" :draggable="false" modal>
    <form id="contract-form" class="form" novalidate @submit.prevent="submit">
      <Message v-if="errorMessage" severity="error">{{ errorMessage }}</Message>

      <div class="field">
        <label for="clientId" class="field-label">Cliente</label>
        <Select
          v-model="form.clientId"
          input-id="clientId"
          :options="clientOptions"
          option-label="label"
          option-value="value"
          placeholder="Selecione o cliente"
          empty-message="Não existem clientes ativos."
          empty-filter-message="Nenhum cliente encontrado."
          :loading="loadingOptions"
          :invalid="!!fieldErrors.clientId"
          filter
          fluid
        />
        <Message v-if="fieldErrors.clientId" severity="error" size="small" variant="simple">
          {{ fieldErrors.clientId[0] }}
        </Message>
      </div>

      <div class="field">
        <label for="vehicleId" class="field-label">Veículo</label>
        <Select
          v-model="form.vehicleId"
          input-id="vehicleId"
          :options="vehicleOptions"
          option-label="label"
          option-value="value"
          placeholder="Selecione o veículo"
          empty-message="Não existem veículos disponíveis."
          empty-filter-message="Nenhum veículo encontrado."
          :loading="loadingOptions"
          :invalid="!!fieldErrors.vehicleId"
          filter
          fluid
        />
        <Message v-if="fieldErrors.vehicleId" severity="error" size="small" variant="simple">
          {{ fieldErrors.vehicleId[0] }}
        </Message>
      </div>

      <div class="form-row">
        <div class="field">
          <label for="startDate" class="field-label">Data de início</label>
          <DatePicker
            v-model="form.startDate"
            input-id="startDate"
            date-format="dd/mm/yy"
            :min-date="minStartDate"
            :max-date="maxStartDate"
            :invalid="!!fieldErrors.startDate"
            show-icon
            fluid
          />
          <Message v-if="fieldErrors.startDate" severity="error" size="small" variant="simple">
            {{ fieldErrors.startDate[0] }}
          </Message>
          <small v-else class="field-hint">Até {{ maxAdvanceDays }} dias a partir de hoje.</small>
        </div>

        <div class="field">
          <label for="endDate" class="field-label">Data de fim</label>
          <DatePicker
            v-model="form.endDate"
            input-id="endDate"
            date-format="dd/mm/yy"
            :min-date="minEndDate"
            :invalid="!!fieldErrors.endDate"
            show-icon
            fluid
          />
          <Message v-if="fieldErrors.endDate" severity="error" size="small" variant="simple">
            {{ fieldErrors.endDate[0] }}
          </Message>
        </div>
      </div>

      <div class="form-row">
        <div class="field">
          <label for="startMileage" class="field-label">Quilometragem inicial</label>
          <InputNumber
            v-model="form.startMileage"
            input-id="startMileage"
            suffix=" km"
            :min="selectedVehicle?.lastMileage ?? 0"
            :use-grouping="false"
            :invalid="!!fieldErrors.startMileage"
            fluid
          />
          <Message v-if="fieldErrors.startMileage" severity="error" size="small" variant="simple">
            {{ fieldErrors.startMileage[0] }}
          </Message>
          <small v-else class="field-hint">{{ mileageHint }}</small>
        </div>

        <div class="field">
          <label for="dailyRate" class="field-label">Preço por dia</label>
          <InputNumber
            v-model="form.dailyRate"
            input-id="dailyRate"
            mode="currency"
            currency="EUR"
            locale="pt-PT"
            :min="0"
            :invalid="!!fieldErrors.dailyRate"
            fluid
          />
          <Message v-if="fieldErrors.dailyRate" severity="error" size="small" variant="simple">
            {{ fieldErrors.dailyRate[0] }}
          </Message>
          <small v-else class="field-hint">{{ priceHint }}</small>
        </div>
      </div>
    </form>

    <template #footer>
      <Button label="Cancelar" severity="secondary" text @click="visible = false" />
      <Button type="submit" form="contract-form" label="Criar contrato" :loading="saving" />
    </template>
  </Dialog>
</template>
