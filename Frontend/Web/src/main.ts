import '@fontsource/roboto-condensed/700.css'
import '@fontsource/roboto-condensed/900.css'
import 'primeicons/primeicons.css'
import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'
import pt from 'primelocale/pt.json'

import App from './App.vue'
import router from './router'
import AppPreset from './Shared/theme/AppPreset'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, {
  theme: {
    preset: AppPreset,
    options: { darkModeSelector: '.app-dark' },
  },
  locale: { ...pt.pt, firstDayOfWeek: 1 },
})
app.use(ToastService)
app.use(ConfirmationService)

app.mount('#app')
