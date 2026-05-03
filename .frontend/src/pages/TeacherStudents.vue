<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getToken } from '../auth'

interface StudentGroup {
  id: string
  name: string
}

const groups = ref<StudentGroup[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
  try {
    const res = await fetch('/api/teacher/groups', {
      headers: { Authorization: `Bearer ${getToken()}` },
    })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    groups.value = await res.json()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Unknown error'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="container">
    <header class="header">
      <h1>Студенты</h1>
      <router-link to="/teacher" class="back-link">Назад</router-link>
    </header>
    <div v-if="loading" class="loading">Загрузка...</div>
    <div v-else-if="error" class="error">{{ error }}</div>
    <ul v-else class="group-list">
      <li v-for="group in groups" :key="group.id" class="group-item">
        {{ group.name }}
      </li>
    </ul>
  </div>
</template>

<style scoped>
.container {
  max-width: 640px;
  margin: 2rem auto;
  padding: 0 1rem;
  font-family: system-ui, sans-serif;
}
.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
}
.header h1 {
  font-size: 1.5rem;
  margin: 0;
}
.back-link {
  color: #3182ce;
  text-decoration: none;
  font-size: 0.875rem;
}
.back-link:hover {
  text-decoration: underline;
}
.loading {
  text-align: center;
  color: #718096;
}
.error {
  color: #e53e3e;
  text-align: center;
}
.group-list {
  list-style: none;
  padding: 0;
  margin: 0;
}
.group-item {
  padding: 0.75rem 1rem;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  margin-bottom: 0.5rem;
  transition: background 0.15s;
}
.group-item:hover {
  background: #f7fafc;
}
</style>
