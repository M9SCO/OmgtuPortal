<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getToken, logout } from './auth'

interface UserInfo {
  sub: string
  preferredUsername: string
  email: string
  givenName: string
  familyName: string
  roles: string[]
}

const user = ref<UserInfo | null>(null)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    const res = await fetch('/api/me', {
      headers: { Authorization: `Bearer ${getToken()}` },
    })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    user.value = await res.json()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Unknown error'
  }
})
</script>

<template>
  <div class="container">
    <div v-if="error" class="error">Error: {{ error }}</div>
    <div v-else-if="user" class="profile">
      <h1>{{ user.givenName }} {{ user.familyName }}</h1>
      <p>{{ user.email }}</p>
      <p class="roles">{{ user.roles?.join(', ') }}</p>
      <button @click="logout()">Logout</button>
    </div>
    <div v-else class="loading">Loading...</div>
  </div>
</template>

<style scoped>
.container {
  max-width: 480px;
  margin: 4rem auto;
  text-align: center;
  font-family: system-ui, sans-serif;
}
.error {
  color: #e53e3e;
}
.roles {
  color: #718096;
  font-size: 0.875rem;
}
button {
  margin-top: 1rem;
  padding: 0.5rem 1.5rem;
  border: none;
  border-radius: 6px;
  background: #e53e3e;
  color: white;
  cursor: pointer;
  font-size: 1rem;
}
button:hover {
  background: #c53030;
}
</style>
