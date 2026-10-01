import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/AutenticacaoAutorizacao/stores/auth.store'
import type { Role } from '@/Shared/models/Role'

declare module 'vue-router' {
  interface RouteMeta {
    public?: boolean
    roles?: Role[]
  }
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/AutenticacaoAutorizacao/components/LoginPage.vue'),
      meta: { public: true },
    },
    {
      path: '/',
      component: () => import('@/Shared/components/AppLayout.vue'),
      children: [
        {
          path: 'veiculos',
          name: 'vehicles',
          component: () => import('@/Veiculos/components/VehiclesPage.vue'),
          meta: { roles: ['Manager', 'Employee'] },
        },
        {
          path: 'clientes',
          name: 'clients',
          component: () => import('@/Clientes/components/ClientsPage.vue'),
          meta: { roles: ['Manager', 'Employee'] },
        },
      ],
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/',
    },
  ],
})

router.beforeEach(async (to) => {
  const authStore = useAuthStore()
  await authStore.initialize()

  if (to.meta.public)
    return authStore.isAuthenticated && to.name === 'login' ? { path: '/' } : true

  if (!authStore.isAuthenticated)
    return { name: 'login', query: { redirect: to.fullPath } }

  if (to.meta.roles && !authStore.hasRole(to.meta.roles))
    return { path: '/' }

  return true
})

export default router
