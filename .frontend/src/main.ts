import { createApp } from 'vue'
import Toast from 'vue-toastification'
import 'vue-toastification/dist/index.css'
import './style.css'
import App from './App.vue'
import { initAuth } from './auth'

initAuth().then(async (authenticated) => {
  if (authenticated) {
    const { default: router } = await import('./router')

    const app = createApp(App)
    app.use(router)
    app.use(Toast, {
      position: 'top-right',
      timeout: 4000,
      closeOnClick: true,
      pauseOnHover: true,
    })
    app.mount('#app')
  }
})
