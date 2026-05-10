<script setup lang="ts">
import { getUserRoles, logout } from '../auth'

const roles = getUserRoles()

interface NavItem {
  label: string
  to: string
  roles?: string[]
}

const navItems: NavItem[] = [
  { label: 'Главная', to: '/' },
  { label: 'Мои файлы', to: '/files', roles: ['student', 'admin'] },
  { label: 'Преподаватель', to: '/teacher', roles: ['teacher', 'admin'] },
]

const visibleItems = navItems.filter((item) => {
  if (!item.roles) return true
  return item.roles.some((r) => roles.includes(r))
})
</script>

<template>
  <header class="app-header">
    <nav class="nav-items">
      <router-link
        v-for="item in visibleItems"
        :key="item.to"
        :to="item.to"
        class="nav-item"
        exact-active-class="nav-item--active"
      >
        {{ item.label }}
      </router-link>
    </nav>
    <button class="logout-btn" @click="logout()">Выход</button>
  </header>
</template>

<style scoped>
.app-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 1.5rem;
  height: 3.5rem;
  border-bottom: 1px solid var(--border);
  background: var(--bg);
  font-family: var(--sans);
  position: sticky;
  top: 0;
  z-index: 100;
}

.nav-items {
  display: flex;
  gap: 0.25rem;
  align-items: center;
  height: 100%;
}

.nav-item {
  display: flex;
  align-items: center;
  padding: 0 0.75rem;
  height: 100%;
  text-decoration: none;
  color: var(--text);
  font-size: 0.9rem;
  font-weight: 500;
  border-bottom: 2px solid transparent;
  transition: color 0.2s, border-color 0.2s;
}

.nav-item:hover {
  color: var(--text-h);
}

.nav-item--active {
  color: var(--accent);
  border-bottom-color: var(--accent);
}

.logout-btn {
  padding: 0.35rem 1rem;
  border: 1px solid var(--border);
  border-radius: 6px;
  background: transparent;
  color: var(--text);
  cursor: pointer;
  font-size: 0.85rem;
  font-family: var(--sans);
  transition: color 0.2s, border-color 0.2s;
}

.logout-btn:hover {
  color: #e53e3e;
  border-color: #e53e3e;
}
</style>
