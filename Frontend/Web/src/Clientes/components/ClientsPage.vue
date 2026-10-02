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
import ClientFormDialog from '@/Clientes/components/ClientFormDialog.vue'
import { clientService } from '@/Clientes/services/client.service'
import { useAuthStore } from '@/AutenticacaoAutorizacao/stores/auth.store'
import { getErrorMessage } from '@/Shared/utils/apiErrors'
import { recordStatusLabels, recordStatusSeverities } from '@/Shared/utils/labels'
import type { RecordStatus } from '@/Shared/models/RecordStatus'
import type { Client } from '@/Clientes/models/Client'
import TruncatedText from '@/Shared/components/TruncatedText.vue'

type StatusFilter = RecordStatus | 'All'

const statusOptions: { label: string; value: StatusFilter }[] = [
  { label: 'Ativos', value: 'Active' },
  { label: 'Inativos', value: 'Inactive' },
  { label: 'Todos', value: 'All' },
]

const authStore = useAuthStore()
const confirm = useConfirm()
const toast = useToast()

const clients = ref<Client[]>([])
const loading = ref(false)
const search = ref('')
const statusFilter = ref<StatusFilter>('Active')
const dialogVisible = ref(false)
const selectedClient = ref<Client | null>(null)

const filteredClients = computed(() => {
  const term = search.value.trim().toLowerCase()

  if (!term)
    return clients.value

  return clients.value.filter((client) =>
    [client.fullName, client.email, client.phone, client.driverLicenseNumber].some((value) => value.toLowerCase().includes(term)),
  )
})

async function loadClients() {
  loading.value = true

  try {
    clients.value = await clientService.getAll({
      status: statusFilter.value === 'All' ? undefined : statusFilter.value,
    })
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Não foi possível carregar os clientes', detail: getErrorMessage(error), life: 5000 })
  } finally {
    loading.value = false
  }
}

function openCreate() {
  selectedClient.value = null
  dialogVisible.value = true
}

function openEdit(client: Client) {
  selectedClient.value = client
  dialogVisible.value = true
}

function confirmDeactivate(client: Client) {
  confirm.require({
    header: 'Desativar cliente',
    message: `Tem a certeza que pretende desativar o cliente ${client.fullName}?`,
    icon: 'pi pi-exclamation-triangle',
    rejectProps: { label: 'Cancelar', severity: 'secondary', text: true },
    acceptProps: { label: 'Desativar', severity: 'danger' },
    accept: () => changeStatus(client, 'deactivate'),
  })
}

async function changeStatus(client: Client, action: 'deactivate' | 'reactivate') {
  try {
    if (action === 'deactivate')
      await clientService.deactivate(client.clientId)
    else
      await clientService.reactivate(client.clientId)

    toast.add({
      severity: 'success',
      summary: action === 'deactivate' ? 'Cliente desativado' : 'Cliente reativado',
      detail: client.fullName,
      life: 3000,
    })

    await loadClients()
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Não foi possível concluir a operação', detail: getErrorMessage(error), life: 5000 })
  }
}

watch(statusFilter, loadClients)
onMounted(loadClients)
</script>

<template>
  <div>
    <div class="page-header">
      <div>
        <h1 class="page-title">Clientes</h1>
        <p class="page-subtitle">{{ filteredClients.length }} cliente(s)</p>
      </div>

      <Button label="Novo cliente" icon="pi pi-plus" @click="openCreate" />
    </div>

    <Card>
      <template #content>
        <div class="toolbar">
          <IconField>
            <InputIcon class="pi pi-search" />
            <InputText v-model="search" placeholder="Pesquisar nome, email, telefone ou carta" class="search" />
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

        <DataTable :value="filteredClients" :loading="loading" data-key="clientId" paginator :rows="10" removable-sort>
          <template #empty>Nenhum cliente encontrado.</template>

          <Column field="fullName" header="Nome" sortable>
            <template #body="{ data }">
              <TruncatedText class="client-name" :text="data.fullName" max-width="16rem" />
            </template>
          </Column>
          <Column field="email" header="Email" sortable>
            <template #body="{ data }">
              <TruncatedText :text="data.email" max-width="18rem" />
            </template>
          </Column>
          <Column field="phone" header="Telefone" />
          <Column field="driverLicenseNumber" header="Carta de condução" sortable />
          <Column field="status" header="Estado" sortable>
            <template #body="{ data }">
              <Tag
                :value="recordStatusLabels[data.status as RecordStatus]"
                :severity="recordStatusSeverities[data.status as RecordStatus]"
              />
            </template>
          </Column>
          <Column>
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
                <template v-if="authStore.isManager">
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
                </template>
              </div>
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <ClientFormDialog v-model:visible="dialogVisible" :client="selectedClient" @saved="loadClients" />
  </div>
</template>

<style scoped>
.search {
  width: 22rem;
}

.client-name {
  font-weight: 700;
}
</style>
