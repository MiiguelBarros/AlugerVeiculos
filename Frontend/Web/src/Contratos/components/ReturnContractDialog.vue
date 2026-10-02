<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import Dialog from 'primevue/dialog'
import DatePicker from 'primevue/datepicker'
import InputNumber from 'primevue/inputnumber'
import Button from 'primevue/button'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { contractService } from '@/Contratos/services/contract.service'
import { getErrorMessage, getValidationErrors } from '@/Shared/utils/apiErrors'
import { formatCurrency } from '@/Shared/utils/currency'
import { formatDate, parseDateOnly, today, toDateOnly } from '@/Shared/utils/dates'
import type { Contract } from '@/Contratos/models/Contract'
import type { ValidationErrors } from '@/Shared/models/ResponseDTO'

interface ReturnForm {
  returnedAt: Date | null
  endMileage: number | null
}

const props = defineProps<{ contract: Contract | null }>()
const emit = defineEmits<{ saved: [contract: Contract] }>()
const visible = defineModel<boolean>('visible', { required: true })

const toast = useToast()

const form = reactive<ReturnForm>({ returnedAt: null, endMileage: null })
const fieldErrors = ref<ValidationErrors>({})
const errorMessage = ref('')
const saving = ref(false)

const maxReturnDate = today()
const minReturnDate = computed(() => (props.contract ? parseDateOnly(props.contract.startDate) : undefined))

watch(visible, (isVisible) => {
  if (!isVisible)
    return

  fieldErrors.value = {}
  errorMessage.value = ''
  form.returnedAt = today()
  form.endMileage = null
})

async function submit() {
  if (!props.contract)
    return

  saving.value = true
  fieldErrors.value = {}
  errorMessage.value = ''

  try {
    const contract = await contractService.returnContract(props.contract.contractId, {
      returnedAt: form.returnedAt ? toDateOnly(form.returnedAt) : null,
      endMileage: form.endMileage,
    })

    toast.add({
      severity: 'success',
      summary: 'Devolução registada',
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
  <Dialog v-model:visible="visible" header="Registar devolução" :style="{ width: '32rem' }" :draggable="false" modal>
    <form id="return-form" class="form" novalidate @submit.prevent="submit">
      <dl v-if="contract" class="summary">
        <div>
          <dt>Cliente</dt>
          <dd>{{ contract.clientName }}</dd>
        </div>
        <div>
          <dt>Veículo</dt>
          <dd>{{ contract.vehicleLicensePlate }} · {{ contract.vehicleBrand }} {{ contract.vehicleModel }}</dd>
        </div>
        <div>
          <dt>Período</dt>
          <dd>{{ formatDate(contract.startDate) }} a {{ formatDate(contract.endDate) }}</dd>
        </div>
        <div>
          <dt>Quilometragem inicial</dt>
          <dd>{{ contract.startMileage }} km</dd>
        </div>
        <div>
          <dt>Preço por dia</dt>
          <dd>{{ formatCurrency(contract.dailyRate) }}</dd>
        </div>
        <div>
          <dt>Total</dt>
          <dd>{{ formatCurrency(contract.totalPrice) }} ({{ contract.numberOfDays }} dia(s))</dd>
        </div>
      </dl>

      <Message v-if="errorMessage" severity="error">{{ errorMessage }}</Message>

      <div class="form-row">
        <div class="field">
          <label for="returnedAt" class="field-label">Data de devolução</label>
          <DatePicker
            v-model="form.returnedAt"
            input-id="returnedAt"
            date-format="dd/mm/yy"
            :min-date="minReturnDate"
            :max-date="maxReturnDate"
            :invalid="!!fieldErrors.returnedAt"
            show-icon
            fluid
          />
          <Message v-if="fieldErrors.returnedAt" severity="error" size="small" variant="simple">
            {{ fieldErrors.returnedAt[0] }}
          </Message>
        </div>

        <div class="field">
          <label for="endMileage" class="field-label">Quilometragem final</label>
          <InputNumber
            v-model="form.endMileage"
            input-id="endMileage"
            suffix=" km"
            :min="0"
            :use-grouping="false"
            :invalid="!!fieldErrors.endMileage"
            fluid
          />
          <Message v-if="fieldErrors.endMileage" severity="error" size="small" variant="simple">
            {{ fieldErrors.endMileage[0] }}
          </Message>
        </div>
      </div>
    </form>

    <template #footer>
      <Button label="Cancelar" severity="secondary" text @click="visible = false" />
      <Button type="submit" form="return-form" label="Registar devolução" :loading="saving" />
    </template>
  </Dialog>
</template>

<style scoped>
.summary {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem 1rem;
  margin: 0;
  padding: 1rem;
  border-radius: 12px;
  background: var(--app-grey);
}

.summary dt {
  font-size: 0.75rem;
  font-weight: 700;
  color: #6b7075;
}

.summary dd {
  margin: 0.125rem 0 0;
  font-weight: 700;
}
</style>
