<template>
  <div class="container">
    <h1>Панель преподавателя</h1>
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
h1 {
  font-size: 1.5rem;
  margin: 0 0 2rem;
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
