<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
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
import UserFormDialog from '@/Utilizadores/components/UserFormDialog.vue'
import TruncatedText from '@/Shared/components/TruncatedText.vue'
import { userService } from '@/Utilizadores/services/user.service'
import { useAuthStore } from '@/AutenticacaoAutorizacao/stores/auth.store'
import { getErrorMessage } from '@/Shared/utils/apiErrors'
import { formatDate } from '@/Shared/utils/dates'
import { recordStatusLabels, recordStatusSeverities, roleLabels } from '@/Shared/utils/labels'
import type { RecordStatus } from '@/Shared/models/RecordStatus'
import type { Role } from '@/Shared/models/Role'
import type { User } from '@/Utilizadores/models/User'

type StatusFilter = RecordStatus | 'All'

const statusOptions: { label: string; value: StatusFilter }[] = [
  { label: 'Ativos', value: 'Active' },
  { label: 'Inativos', value: 'Inactive' },
  { label: 'Todos', value: 'All' },
]

const authStore = useAuthStore()
const confirm = useConfirm()
const toast = useToast()

const users = ref<User[]>([])
const loading = ref(false)
const search = ref('')
const statusFilter = ref<StatusFilter>('Active')
const dialogVisible = ref(false)

const filteredUsers = computed(() => {
  const term = search.value.trim().toLowerCase()

  return users.value.filter((user) => {
    const matchesStatus = statusFilter.value === 'All' || user.status === statusFilter.value
    const matchesSearch = !term || [user.name, user.email].some((value) => value.toLowerCase().includes(term))

    return matchesStatus && matchesSearch
  })
})

function isCurrentUser(user: User): boolean {
  return user.userId === authStore.user?.id
}

async function loadUsers() {
  loading.value = true

  try {
    users.value = await userService.getAll()
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Não foi possível carregar os funcionários', detail: getErrorMessage(error), life: 5000 })
  } finally {
    loading.value = false
  }
}

function confirmDeactivate(user: User) {
  confirm.require({
    header: 'Desativar funcionário',
    message: `Tem a certeza que pretende desativar ${user.name}? As sessões ativas deste funcionário serão terminadas.`,
    icon: 'pi pi-exclamation-triangle',
    rejectProps: { label: 'Cancelar', severity: 'secondary', text: true },
    acceptProps: { label: 'Desativar', severity: 'danger' },
    accept: () => changeStatus(user, 'deactivate'),
  })
}

async function changeStatus(user: User, action: 'deactivate' | 'reactivate') {
  try {
    if (action === 'deactivate')
      await userService.deactivate(user.userId)
    else
      await userService.reactivate(user.userId)

    toast.add({
      severity: 'success',
      summary: action === 'deactivate' ? 'Funcionário desativado' : 'Funcionário reativado',
      detail: user.name,
      life: 3000,
    })

    await loadUsers()
  } catch (error) {
    toast.add({ severity: 'error', summary: 'Não foi possível concluir a operação', detail: getErrorMessage(error), life: 5000 })
  }
}

onMounted(loadUsers)
</script>

<template>
  <div>
    <div class="page-header">
      <div>
        <h1 class="page-title">Funcionários</h1>
        <p class="page-subtitle">{{ filteredUsers.length }} funcionário(s)</p>
      </div>

      <Button label="Novo funcionário" icon="pi pi-plus" @click="dialogVisible = true" />
    </div>

    <Card>
      <template #content>
        <div class="toolbar">
          <IconField>
            <InputIcon class="pi pi-search" />
            <InputText v-model="search" placeholder="Pesquisar nome ou email" class="search" />
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

        <DataTable :value="filteredUsers" :loading="loading" data-key="userId" paginator :rows="10" removable-sort>
          <template #empty>Nenhum funcionário encontrado.</template>

          <Column field="name" header="Nome" sortable>
            <template #body="{ data }">
              <div class="user-name">
                <TruncatedText :text="data.name" max-width="16rem" />
                <Tag v-if="isCurrentUser(data)" value="Você" severity="contrast" />
              </div>
            </template>
          </Column>
          <Column field="email" header="Email" sortable>
            <template #body="{ data }">
              <TruncatedText :text="data.email" max-width="18rem" />
            </template>
          </Column>
          <Column field="role" header="Função" sortable>
            <template #body="{ data }">{{ roleLabels[data.role as Role] }}</template>
          </Column>
          <Column field="createdAt" header="Criado em" sortable>
            <template #body="{ data }">{{ formatDate(data.createdAt.slice(0, 10)) }}</template>
          </Column>
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
              <div v-if="!isCurrentUser(data)" class="table-actions">
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

    <UserFormDialog v-model:visible="dialogVisible" @saved="loadUsers" />
  </div>
</template>

<style scoped>
.search {
  width: 22rem;
}

.user-name {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 700;
}
</style>
