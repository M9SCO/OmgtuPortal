<script setup lang="ts">
import { logout } from '../auth'
</script>

<template>
  <div class="container">
    <header class="header">
      <h1>Панель преподавателя</h1>
      <div class="actions">
        <router-link to="/" class="back-link">На главную</router-link>
        <button class="logout-btn" @click="logout()">Logout</button>
      </div>
    </header>
    <nav class="teacher-nav">
      <router-link to="/teacher/students" class="card">
        <span class="card-icon">&#128218;</span>
        <span class="card-title">Студенты</span>
        <span class="card-desc">Просмотр списков групп</span>
      </router-link>
    </nav>
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
  margin-bottom: 2rem;
}
.header h1 {
  font-size: 1.5rem;
  margin: 0;
}
.actions {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}
.back-link {
  color: #3182ce;
  text-decoration: none;
  font-size: 0.875rem;
}
.back-link:hover {
  text-decoration: underline;
}
.logout-btn {
  padding: 0.4rem 1rem;
  border: none;
  border-radius: 6px;
  background: #e53e3e;
  color: white;
  cursor: pointer;
  font-size: 0.875rem;
}
.logout-btn:hover {
  background: #c53030;
}
.teacher-nav {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 1rem;
}
.card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 1.5rem 1rem;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  text-decoration: none;
  color: inherit;
  transition: box-shadow 0.2s, border-color 0.2s;
}
.card:hover {
  border-color: #3182ce;
  box-shadow: 0 4px 12px rgba(49, 130, 206, 0.15);
}
.card-icon {
  font-size: 2rem;
}
.card-title {
  font-weight: 600;
  font-size: 1rem;
}
.card-desc {
  font-size: 0.8rem;
  color: #718096;
}
</style>
