<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import Tag from 'primevue/tag'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useConfirm } from 'primevue/useconfirm'
import { useToast } from 'primevue/usetoast'
import ContractFormDialog from '@/Contratos/components/ContractFormDialog.vue'
import ReturnContractDialog from '@/Contratos/components/ReturnContractDialog.vue'
import TruncatedText from '@/Shared/components/TruncatedText.vue'
import { contractService } from '@/Contratos/services/contract.service'
import { useAuthStore } from '@/AutenticacaoAutorizacao/stores/auth.store'
import { getErrorMessage } from '@/Shared/utils/apiErrors'
import { formatDate, today, toDateOnly } from '@/Shared/utils/dates'
import { contractStatusLabels, contractStatusSeverities } from '@/Shared/utils/labels'
import type { Contract } from '@/Contratos/models/Contract'
import type { ContractStatus } from '@/Contratos/models/ContractStatus'

type StatusFilter = ContractStatus | 'All'

const statusOptions: { label: string; value: StatusFilter }[] = [
  { label: 'Todos', value: 'All' },
  { label: 'Agendados', value: 'Scheduled' },
  { label: 'Ativos', value: 'Active' },
  { label: 'Em atraso', value: 'Overdue' },
  { label: 'Concluídos', value: 'Completed' },
  { label: 'Cancelados', value: 'Cancelled' },
]

const authStore = useAuthStore()
const confirm = useConfirm()
const toast = useToast()

const contracts = ref<Contract[]>([])
const loading = ref(false)
const search = ref('')
const statusFilter = ref<StatusFilter>('All')
const createDialogVisible = ref(false)
const returnDialogVisible = ref(false)
const selectedContract = ref<Contract | null>(null)

const filteredContracts = computed(() => {
  const term = search.value.trim().toLowerCase()

  if (!term)
    return contracts.value

  return contracts.value.filter((contract) =>
    [contract.clientName, contract.vehicleLicensePlate, contract.vehicleBrand, contract.vehicleModel]
      .some((value) => value.toLowerCase().includes(term)),
  )
})

function canReturn(contract: Contract): boolean {
  return contract.status === 'Active' || contract.status === 'Overdue'
}

function canCancel(contract: Contract): boolean {
  if (!authStore.isManager)
    return false

  return contract.status === 'Scheduled' || (contract.status === 'Active' && contract.startDate === toDateOnly(today()))
}

async function loadContracts() {
  loading.value = true

  try {
    contracts.value = await contractService.getAll({
      status: statusFilter.value === 'All' ? undefined : statusFilter.value,
    })
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Não foi possível carregar os contratos', detail: getErrorMessage(error), life: 5000 })
  } finally {
    loading.value = false
  }
}

function openReturn(contract: Contract) {
  selectedContract.value = contract
  returnDialogVisible.value = true
}

function confirmCancel(contract: Contract) {
  confirm.require({
    header: 'Cancelar contrato',
    message: `Tem a certeza que pretende cancelar o contrato de ${contract.clientName} (${contract.vehicleLicensePlate})?`,
    icon: 'pi pi-exclamation-triangle',
    rejectProps: { label: 'Voltar', severity: 'secondary', text: true },
    acceptProps: { label: 'Cancelar contrato', severity: 'danger' },
    accept: () => cancel(contract),
  })
}

async function cancel(contract: Contract) {
  try {
    await contractService.cancel(contract.contractId)

    toast.add({
      severity: 'success',
      summary: 'Contrato cancelado',
      detail: `${contract.clientName} · ${contract.vehicleLicensePlate}`,
      life: 3000,
    })

    await loadContracts()
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Não foi possível cancelar o contrato', detail: getErrorMessage(error), life: 5000 })
  }
}

watch(statusFilter, loadContracts)
onMounted(loadContracts)
</script>

<template>
  <div>
    <div class="page-header">
      <div>
        <h1 class="page-title">Contratos</h1>
        <p class="page-subtitle">{{ filteredContracts.length }} contrato(s)</p>
      </div>

      <Button label="Novo contrato" icon="pi pi-plus" @click="createDialogVisible = true" />
    </div>

    <Card>
      <template #content>
        <div class="toolbar">
          <IconField>
            <InputIcon class="pi pi-search" />
            <InputText v-model="search" placeholder="Pesquisar cliente, matrícula ou veículo" class="search" />
          </IconField>

          <div class="pill-group" role="group" aria-label="Estado">
            <button
              v-for="option in statusOptions"
              :key="option.value"
              type="button"
              class="pill"
              :class="{ 'pill-active': statusFilter === option.value }"
              :aria-pressed="statusFilter === option.value"
              @click="statusFilter = option.value"
            >
              {{ option.label }}
            </button>
          </div>
        </div>

        <DataTable :value="filteredContracts" :loading="loading" data-key="contractId" paginator :rows="10" removable-sort>
          <template #empty>Nenhum contrato encontrado.</template>

          <Column field="clientName" header="Cliente" sortable>
            <template #body="{ data }">
              <TruncatedText class="client-name" :text="data.clientName" max-width="8rem" />
            </template>
          </Column>
          <Column field="vehicleLicensePlate" header="Veículo" sortable>
            <template #body="{ data }">
              <div class="vehicle">
                <span class="plate">{{ data.vehicleLicensePlate }}</span>
                <TruncatedText :text="`${data.vehicleBrand} ${data.vehicleModel}`" max-width="8rem" />
              </div>
            </template>
          </Column>
          <Column field="startDate" header="Início" sortable>
            <template #body="{ data }">{{ formatDate(data.startDate) }}</template>
          </Column>
          <Column field="endDate" header="Fim" sortable>
            <template #body="{ data }">{{ formatDate(data.endDate) }}</template>
          </Column>
          <Column field="returnedAt" header="Devolução" sortable>
            <template #body="{ data }">{{ formatDate(data.returnedAt) }}</template>
          </Column>
          <Column header="Quilometragem">
            <template #body="{ data }">
              {{ data.startMileage }} km
              <template v-if="data.endMileage !== null">→ {{ data.endMileage }} km</template>
            </template>
          </Column>
          <Column field="status" header="Estado" sortable>
            <template #body="{ data }">
              <Tag
                :value="contractStatusLabels[data.status as ContractStatus]"
                :severity="contractStatusSeverities[data.status as ContractStatus]"
              />
            </template>
          </Column>
          <Column>
            <template #body="{ data }">
              <div class="table-actions">
                <Button
                  v-if="canReturn(data)"
                  label="Devolver"
                  icon="pi pi-check"
                  size="small"
                  @click="openReturn(data)"
                />
                <Button
                  v-if="canCancel(data)"
                  icon="pi pi-times"
                  severity="danger"
                  text
                  rounded
                  title="Cancelar contrato"
                  aria-label="Cancelar contrato"
                  @click="confirmCancel(data)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <ContractFormDialog v-model:visible="createDialogVisible" @saved="loadContracts" />
    <ReturnContractDialog v-model:visible="returnDialogVisible" :contract="selectedContract" @saved="loadContracts" />
  </div>
</template>

<style scoped>
.search {
  width: 22rem;
}

.client-name {
  font-weight: 700;
}

.vehicle {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.plate {
  display: inline-block;
  padding: 0.125rem 0.5rem;
  border: 1px solid var(--app-border);
  border-radius: 6px;
  background: #ffffff;
  font-weight: 700;
  letter-spacing: 0.05em;
  white-space: nowrap;
}
</style>
