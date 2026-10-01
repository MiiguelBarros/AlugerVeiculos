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
import VehicleFormDialog from '@/Veiculos/components/VehicleFormDialog.vue'
import { vehicleService } from '@/Veiculos/services/vehicle.service'
import { useAuthStore } from '@/AutenticacaoAutorizacao/stores/auth.store'
import { getErrorMessage } from '@/Shared/utils/apiErrors'
import {
  fuelTypeLabels,
  recordStatusLabels,
  recordStatusSeverities,
  vehicleAvailabilityLabels,
  vehicleAvailabilitySeverities,
} from '@/Shared/utils/labels'
import type { RecordStatus } from '@/Shared/models/RecordStatus'
import type { Vehicle } from '@/Veiculos/models/Vehicle'
import type { VehicleAvailability } from '@/Veiculos/models/VehicleAvailability'
import TruncatedText from '@/Shared/components/TruncatedText.vue'

type AvailabilityFilter = VehicleAvailability | 'All'
type StatusFilter = RecordStatus | 'All'

const availabilityOptions: { label: string; value: AvailabilityFilter }[] = [
  { label: 'Todos', value: 'All' },
  { label: 'Disponíveis', value: 'Available' },
  { label: 'Alugados', value: 'Rented' },
]

const statusOptions: { label: string; value: StatusFilter }[] = [
  { label: 'Ativos', value: 'Active' },
  { label: 'Inativos', value: 'Inactive' },
  { label: 'Todos', value: 'All' },
]

const authStore = useAuthStore()
const confirm = useConfirm()
const toast = useToast()

const vehicles = ref<Vehicle[]>([])
const loading = ref(false)
const search = ref('')
const availabilityFilter = ref<AvailabilityFilter>('All')
const statusFilter = ref<StatusFilter>('Active')
const dialogVisible = ref(false)
const selectedVehicle = ref<Vehicle | null>(null)

const filteredVehicles = computed(() => {
  const term = search.value.trim().toLowerCase()

  if (!term)
    return vehicles.value

  return vehicles.value.filter((vehicle) =>
    [vehicle.licensePlate, vehicle.brand, vehicle.model].some((value) => value.toLowerCase().includes(term)),
  )
})

async function loadVehicles() {
  loading.value = true

  try {
    vehicles.value = await vehicleService.getAll({
      availability: availabilityFilter.value === 'All' ? undefined : availabilityFilter.value,
      status: statusFilter.value === 'All' ? undefined : statusFilter.value,
    })
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Não foi possível carregar os veículos', detail: getErrorMessage(error), life: 5000 })
  } finally {
    loading.value = false
  }
}

function openCreate() {
  selectedVehicle.value = null
  dialogVisible.value = true
}

function openEdit(vehicle: Vehicle) {
  selectedVehicle.value = vehicle
  dialogVisible.value = true
}

function confirmDeactivate(vehicle: Vehicle) {
  confirm.require({
    header: 'Desativar veículo',
    message: `Tem a certeza que pretende desativar o veículo ${vehicle.licensePlate}?`,
    icon: 'pi pi-exclamation-triangle',
    rejectProps: { label: 'Cancelar', severity: 'secondary', text: true },
    acceptProps: { label: 'Desativar', severity: 'danger' },
    accept: () => changeStatus(vehicle, 'deactivate'),
  })
}

async function changeStatus(vehicle: Vehicle, action: 'deactivate' | 'reactivate') {
  try {
    if (action === 'deactivate')
      await vehicleService.deactivate(vehicle.vehicleId)
    else
      await vehicleService.reactivate(vehicle.vehicleId)

    toast.add({
      severity: 'success',
      summary: action === 'deactivate' ? 'Veículo desativado' : 'Veículo reativado',
      detail: vehicle.licensePlate,
      life: 3000,
    })

    await loadVehicles()
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Não foi possível concluir a operação', detail: getErrorMessage(error), life: 5000 })
  }
}

watch([availabilityFilter, statusFilter], loadVehicles)
onMounted(loadVehicles)
</script>

<template>
  <div>
    <div class="page-header">
      <div>
        <h1 class="page-title">Veículos</h1>
        <p class="page-subtitle">{{ filteredVehicles.length }} veículo(s)</p>
      </div>

      <Button v-if="authStore.isManager" label="Novo veículo" icon="pi pi-plus" @click="openCreate" />
    </div>

    <Card>
      <template #content>
        <div class="toolbar">
          <IconField>
            <InputIcon class="pi pi-search" />
            <InputText v-model="search" placeholder="Pesquisar matrícula, marca ou modelo" class="search" />
          </IconField>

          <div class="toolbar-filters">
            <div class="pill-group" role="group" aria-label="Disponibilidade">
              <button
                v-for="option in availabilityOptions"
                :key="option.value"
                type="button"
                class="pill"
                :class="{ 'pill-active': availabilityFilter === option.value }"
                :aria-pressed="availabilityFilter === option.value"
                @click="availabilityFilter = option.value"
              >
                {{ option.label }}
              </button>
            </div>

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
        </div>

        <DataTable :value="filteredVehicles" :loading="loading" data-key="vehicleId" paginator :rows="10" removable-sort>
          <template #empty>Nenhum veículo encontrado.</template>

          <Column field="licensePlate" header="Matrícula" sortable>
            <template #body="{ data }">
              <span class="plate">{{ data.licensePlate }}</span>
            </template>
          </Column>
          <Column field="brand" header="Marca" sortable>
            <template #body="{ data }">
                <TruncatedText :text="data.brand" max-width="10rem" />
            </template>
          </Column>
          <Column field="model" header="Modelo" sortable>
            <template #body="{ data }">
                <TruncatedText :text="data.model" max-width="10rem" />
            </template>
          </Column>
          <Column field="year" header="Ano" sortable />
          <Column field="fuelType" header="Combustível" sortable>
            <template #body="{ data }">{{ fuelTypeLabels[data.fuelType as Vehicle['fuelType']] }}</template>
          </Column>
          <Column field="lastMileage" header="Quilometragem" sortable>
            <template #body="{ data }">{{ data.lastMileage === null ? '—' : `${data.lastMileage} km` }}</template>
          </Column>
          <Column field="availability" header="Disponibilidade" sortable>
            <template #body="{ data }">
              <Tag
                :value="vehicleAvailabilityLabels[data.availability as VehicleAvailability]"
                :severity="vehicleAvailabilitySeverities[data.availability as VehicleAvailability]"
              />
            </template>
          </Column>
          <Column field="status" header="Estado" sortable>
            <template #body="{ data }">
              <Tag
                :value="recordStatusLabels[data.status as RecordStatus]"
                :severity="recordStatusSeverities[data.status as RecordStatus]"
              />
            </template>
          </Column>
          <Column v-if="authStore.isManager">
            <template #body="{ data }">
              <div class="table-actions">
                <Button
                  icon="pi pi-pencil"
                  severity="secondary"
                  text
                  rounded
                  title="Editar"
                  aria-label="Editar"
                  @click="openEdit(data)"
                />
                <Button
                  v-if="data.status === 'Active'"
                  icon="pi pi-ban"
                  severity="danger"
                  text
                  rounded
                  title="Desativar"
                  aria-label="Desativar"
                  @click="confirmDeactivate(data)"
                />
                <Button
                  v-else
                  icon="pi pi-replay"
                  severity="success"
                  text
                  rounded
                  title="Reativar"
                  aria-label="Reativar"
                  @click="changeStatus(data, 'reactivate')"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <VehicleFormDialog v-model:visible="dialogVisible" :vehicle="selectedVehicle" @saved="loadVehicles" />
  </div>
</template>

<style scoped>
.search {
  width: 22rem;
}

.plate {
  display: inline-block;
  padding: 0.125rem 0.5rem;
  border: 1px solid var(--app-border);
  border-radius: 6px;
  background: #ffffff;
  font-weight: 700;
  letter-spacing: 0.05em;
}
</style>
