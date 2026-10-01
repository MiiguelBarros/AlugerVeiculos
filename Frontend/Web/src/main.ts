import './assets/main.css'
import 'primeicons/primeicons.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import pt from 'primelocale/pt.json'

import App from './App.vue'
import router from './router'
import JapPreset from './Shared/theme/JapPreset'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: JapPreset,
    options: { darkModeSelector: '.app-dark' },
  },
  locale: { ...pt.pt, firstDayOfWeek: 1 },
})
app.use(ToastService)
app.use(ConfirmationService)

app.mount('#app')
