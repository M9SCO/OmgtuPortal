<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { getToken } from '../auth'

interface Subject {
  id: string
  name: string
}

interface FileItem {
  id: string
  fileName: string
  fileSize: number
  contentType: string
  uploadedAt: string
}

const subjects = ref<Subject[]>([])
const files = ref<FileItem[]>([])
const selectedSubject = ref('')
const loading = ref(true)
const loadingFiles = ref(false)
const error = ref<string | null>(null)

const authHeaders = computed(() => ({
  Authorization: `Bearer ${getToken()}`,
}))

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} Б`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} КБ`
  return `${(bytes / (1024 * 1024)).toFixed(1)} МБ`
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleString('ru-RU')
}

async function loadSubjects() {
  loading.value = true
  error.value = null
  try {
    const res = await fetch('/api/contact-work/subjects', { headers: authHeaders.value })
    if (!res.ok) throw new Error('Ошибка загрузки дисциплин')
    subjects.value = await res.json()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Неизвестная ошибка'
  } finally {
    loading.value = false
  }
}

async function loadFiles() {
  if (!selectedSubject.value) {
    files.value = []
    return
  }
  loadingFiles.value = true
  error.value = null
  try {
    const res = await fetch(
      `/api/contact-work/files?subjectId=${encodeURIComponent(selectedSubject.value)}`,
      { headers: authHeaders.value },
    )
    if (!res.ok) throw new Error('Ошибка загрузки файлов')
    files.value = await res.json()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Неизвестная ошибка'
  } finally {
    loadingFiles.value = false
  }
}

function selectSubject(id: string) {
  selectedSubject.value = id
  loadFiles()
}

function downloadFile(file: FileItem) {
  const link = document.createElement('a')
  link.href = `/api/contact-work/download/${file.id}`
  const token = getToken()
  if (token) {
    // For authenticated downloads, use fetch + blob
    fetch(link.href, { headers: authHeaders.value })
      .then((res) => {
        if (!res.ok) throw new Error('Ошибка скачивания')
        return res.blob()
      })
      .then((blob) => {
        const url = URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = file.fileName
        a.click()
        URL.revokeObjectURL(url)
      })
      .catch(() => {
        error.value = 'Ошибка скачивания файла'
      })
  } else {
    // Auto-login mode — direct link works
    link.download = file.fileName
    link.click()
  }
}

const selectedSubjectName = computed(
  () => subjects.value.find((s) => s.id === selectedSubject.value)?.name ?? '',
)

onMounted(loadSubjects)
</script>

<template>
  <div class="container">
    <h1>Файлы по дисциплинам</h1>

    <div v-if="loading" class="loading">Загрузка...</div>
    <div v-else-if="error" class="error">{{ error }}</div>

    <template v-if="!loading">
      <div v-if="subjects.length === 0 && !error" class="empty">
        Нет доступных файлов по вашей группе
      </div>

      <template v-else>
        <div class="subjects">
          <button
            v-for="s in subjects"
            :key="s.id"
            class="subject-btn"
            :class="{ 'subject-btn--active': selectedSubject === s.id }"
            @click="selectSubject(s.id)"
          >
            {{ s.name }}
          </button>
        </div>

        <section v-if="selectedSubject" class="files-section">
          <h2>{{ selectedSubjectName }}</h2>

          <div v-if="loadingFiles" class="loading">Загрузка файлов...</div>
          <p v-else-if="files.length === 0" class="empty">Нет файлов</p>
          <div v-else class="file-list">
            <div v-for="f in files" :key="f.id" class="file-item" @click="downloadFile(f)">
              <div class="file-info">
                <span class="file-name">{{ f.fileName }}</span>
                <span class="file-meta">
                  {{ formatSize(f.fileSize) }} &middot; {{ formatDate(f.uploadedAt) }}
                </span>
              </div>
              <button class="btn-download" title="Скачать">&#8595;</button>
            </div>
          </div>
        </section>

        <div v-else class="hint">Выберите дисциплину для просмотра файлов</div>
      </template>
    </template>
  </div>
</template>

<style scoped>
.container {
  max-width: 720px;
  margin: 2rem auto;
  padding: 0 1rem;
  font-family: system-ui, sans-serif;
}
h1 {
  font-size: 1.5rem;
  margin: 0 0 1.5rem;
}
h2 {
  font-size: 1.15rem;
  margin: 0 0 1rem;
}
.loading {
  text-align: center;
  color: #718096;
}
.error {
  color: #e53e3e;
  margin-bottom: 1rem;
}
.empty {
  color: #a0aec0;
  text-align: center;
  padding: 2rem 0;
}
.hint {
  color: #a0aec0;
  text-align: center;
  padding: 2rem 0;
  font-size: 0.95rem;
}

.subjects {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 1.5rem;
}
.subject-btn {
  padding: 0.5rem 1rem;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: #fff;
  color: #4a5568;
  font-size: 0.9rem;
  font-family: inherit;
  cursor: pointer;
  transition: all 0.2s;
}
.subject-btn:hover {
  border-color: #3182ce;
  color: #3182ce;
}
.subject-btn--active {
  background: #3182ce;
  border-color: #3182ce;
  color: #fff;
}

.files-section {
  margin-top: 0.5rem;
}
.file-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
.file-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.75rem 1rem;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.15s;
}
.file-item:hover {
  background: #f7fafc;
}
.file-info {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  min-width: 0;
}
.file-name {
  font-weight: 500;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.file-meta {
  font-size: 0.78rem;
  color: #718096;
}
.btn-download {
  flex-shrink: 0;
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  background: transparent;
  color: #3182ce;
  font-size: 1.2rem;
  cursor: pointer;
  transition: color 0.2s, border-color 0.2s, background 0.2s;
}
.btn-download:hover {
  background: #ebf8ff;
  border-color: #3182ce;
}
</style>
