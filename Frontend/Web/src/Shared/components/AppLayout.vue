<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, RouterView, useRouter } from 'vue-router'
import Button from 'primevue/button'
import { useAuthStore } from '@/AutenticacaoAutorizacao/stores/auth.store'
import { roleLabels } from '@/Shared/utils/labels'
import type { Role } from '@/Shared/models/Role'

interface MenuItem {
  label: string
  icon: string
  to: string
  roles: Role[]
}

const menuItems: MenuItem[] = [
  { label: 'Contratos', icon: 'pi pi-file', to: '/contratos', roles: ['Manager', 'Employee'] },
  { label: 'Veículos', icon: 'pi pi-car', to: '/veiculos', roles: ['Manager', 'Employee'] },
  { label: 'Clientes', icon: 'pi pi-users', to: '/clientes', roles: ['Manager', 'Employee'] },
  { label: 'Funcionários', icon: 'pi pi-id-card', to: '/utilizadores', roles: ['Manager'] },
]

const authStore = useAuthStore()
const router = useRouter()

const visibleItems = computed(() => menuItems.filter((item) => authStore.hasRole(item.roles)))

async function logout() {
  await authStore.logout()
  await router.push({ name: 'login' })
}
</script>

<template>
  <div class="layout">
    <div class="accent-bar" />

    <header class="topbar">
      <div class="brand">
        <span class="brand-mark"><i class="pi pi-car" /></span>
        <span class="brand-name">Aluguer de Veículos</span>
      </div>

      <nav class="nav">
        <RouterLink
          v-for="item in visibleItems"
          :key="item.to"
          :to="item.to"
          class="nav-link"
          active-class="nav-link-active"
        >
          <i :class="item.icon" />
          <span>{{ item.label }}</span>
        </RouterLink>
      </nav>

      <div v-if="authStore.user" class="user">
        <div class="user-info">
          <span class="user-name">{{ authStore.user.name }}</span>
          <span class="user-role">{{ roleLabels[authStore.user.role] }}</span>
        </div>
        <Button label="Sair" icon="pi pi-sign-out" class="logout" text @click="logout" />
      </div>
    </header>

    <main class="content">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
.layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.accent-bar {
  height: 4px;
  background: var(--app-orange);
}

.topbar {
  display: flex;
  align-items: center;
  gap: 2.5rem;
  padding: 0 2rem;
  height: 68px;
  background: var(--app-black);
  color: #ffffff;
}

.brand {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.brand-mark {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  background: var(--app-orange);
  font-size: 1.1rem;
}

.brand-name {
  font-family: var(--app-heading-font);
  font-size: 1.35rem;
  font-weight: 900;
  text-transform: uppercase;
  white-space: nowrap;
}

.nav {
  flex: 1;
  display: flex;
  gap: 0.5rem;
}

.nav-link {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 360px;
  color: #ffffff;
  font-weight: 700;
  text-decoration: none;
  transition: background 0.15s;
}

.nav-link:hover {
  background: rgba(255, 255, 255, 0.12);
}

.nav-link-active,
.nav-link-active:hover {
  background: #ffffff;
  color: var(--app-black);
}

.user {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-info {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  line-height: 1.2;
}

.user-name {
  font-weight: 700;
}

.user-role {
  font-size: 0.75rem;
  color: #b9bdc1;
}

.logout {
  color: #ffffff;
}

.content {
  flex: 1;
  width: 100%;
  max-width: 1280px;
  margin: 0 auto;
  padding: 2rem;
  box-sizing: border-box;
}
</style>
