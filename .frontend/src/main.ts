import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { initAuth } from './auth'

initAuth().then((authenticated) => {
  if (authenticated) {
    createApp(App).mount('#app')
  }
})
