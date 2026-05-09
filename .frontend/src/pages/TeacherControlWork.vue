<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { getToken, getUserRoles } from '../auth'
import { useRouter } from 'vue-router'

const router = useRouter()
const roles = getUserRoles()

// Redirect students
if (roles.includes('student') && !roles.includes('teacher') && !roles.includes('admin')) {
  router.replace('/')
}

interface Option {
  id: string
  name: string
}

interface FileItem {
  id: string
  subjectId: string
  groupId: string
  fileName: string
  fileSize: number
  uploadedAt: string
}

const groups = ref<Option[]>([])
const subjects = ref<Option[]>([])
const files = ref<FileItem[]>([])

const selectedGroup = ref('')
const selectedSubject = ref('')
const selectedFile = ref<File | null>(null)

const MAX_FILE_SIZE_MB = 50
const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024

const fileInputRef = ref<HTMLInputElement | null>(null)
const loading = ref(true)
const uploading = ref(false)
const error = ref<string | null>(null)
const successMsg = ref<string | null>(null)

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

function groupName(id: string): string {
  return groups.value.find((g) => g.id === id)?.name ?? id
}

function subjectName(id: string): string {
  return subjects.value.find((s) => s.id === id)?.name ?? id
}

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const [groupsRes, subjectsRes, filesRes] = await Promise.all([
      fetch('/api/teacher/groups', { headers: authHeaders.value }),
      fetch('/api/teacher/subjects', { headers: authHeaders.value }),
      fetch('/api/control-work', { headers: authHeaders.value }),
    ])
    if (!groupsRes.ok || !subjectsRes.ok || !filesRes.ok)
      throw new Error('Ошибка загрузки данных')

    groups.value = await groupsRes.json()
    subjects.value = await subjectsRes.json()
    files.value = await filesRes.json()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Неизвестная ошибка'
  } finally {
    loading.value = false
  }
}

function onFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0] ?? null
  error.value = null

  if (file && file.size > MAX_FILE_SIZE_BYTES) {
    error.value = `Файл слишком большой (${formatSize(file.size)}). Максимум ${MAX_FILE_SIZE_MB} МБ`
    input.value = ''
    selectedFile.value = null
    return
  }

  selectedFile.value = file
}

function resetForm() {
  selectedGroup.value = ''
  selectedSubject.value = ''
  selectedFile.value = null
  if (fileInputRef.value) fileInputRef.value.value = ''
}

async function upload() {
  if (!selectedGroup.value || !selectedSubject.value || !selectedFile.value) return

  uploading.value = true
  error.value = null
  successMsg.value = null

  const formData = new FormData()
  formData.append('groupId', selectedGroup.value)
  formData.append('subjectId', selectedSubject.value)
  formData.append('file', selectedFile.value)

  try {
    const res = await fetch('/api/control-work/upload', {
      method: 'POST',
      headers: authHeaders.value,
      body: formData,
    })
    if (!res.ok) {
      const body = await res.json().catch(() => null)
      throw new Error(body?.error ?? `HTTP ${res.status}`)
    }
    successMsg.value = 'Файл загружен'
    resetForm()

    // Reload file list
    const filesRes = await fetch('/api/control-work', { headers: authHeaders.value })
    if (filesRes.ok) files.value = await filesRes.json()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Ошибка загрузки'
  } finally {
    uploading.value = false
  }
}

async function deleteFile(id: string) {
  if (!confirm('Удалить этот файл?')) return

  try {
    const res = await fetch(`/api/control-work/${id}`, {
      method: 'DELETE',
      headers: authHeaders.value,
    })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    files.value = files.value.filter((f) => f.id !== id)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Ошибка удаления'
  }
}

const filteredFiles = computed(() => {
  return files.value.filter((f) => {
    if (selectedGroup.value && f.groupId !== selectedGroup.value) return false
    if (selectedSubject.value && f.subjectId !== selectedSubject.value) return false
    return true
  })
})

const canUpload = computed(
  () => selectedGroup.value && selectedSubject.value && selectedFile.value && !uploading.value,
)

onMounted(loadData)
</script>

<template>
  <div class="container">
    <h1>Контрольные работы</h1>

    <div v-if="loading" class="loading">Загрузка...</div>
    <div v-else-if="error" class="error">{{ error }}</div>

    <template v-if="!loading">
      <form class="upload-form" @submit.prevent="upload">
        <div class="form-row">
          <div class="form-group">
            <label for="group">Группа</label>
            <select id="group" v-model="selectedGroup" required>
              <option value="" disabled>Выберите группу</option>
              <option v-for="g in groups" :key="g.id" :value="g.id">{{ g.name }}</option>
            </select>
          </div>
          <div class="form-group">
            <label for="subject">Дисциплина</label>
            <select id="subject" v-model="selectedSubject" required>
              <option value="" disabled>Выберите дисциплину</option>
              <option v-for="s in subjects" :key="s.id" :value="s.id">{{ s.name }}</option>
            </select>
          </div>
        </div>

        <div class="form-group">
          <label for="file">Файл</label>
          <input id="file" ref="fileInputRef" type="file" @change="onFileChange" required />
        </div>

        <button type="submit" class="btn-upload" :disabled="!canUpload">
          {{ uploading ? 'Загрузка...' : 'Загрузить' }}
        </button>

        <div v-if="successMsg" class="success">{{ successMsg }}</div>
      </form>

      <section class="files-section">
        <h2>Загруженные файлы</h2>
        <p v-if="filteredFiles.length === 0" class="empty">Нет файлов</p>
        <div v-else class="file-list">
          <div v-for="f in filteredFiles" :key="f.id" class="file-item">
            <div class="file-info">
              <span class="file-name">{{ f.fileName }}</span>
              <span class="file-meta">
                {{ groupName(f.groupId) }} &middot; {{ subjectName(f.subjectId) }} &middot;
                {{ formatSize(f.fileSize) }} &middot; {{ formatDate(f.uploadedAt) }}
              </span>
            </div>
            <button class="btn-delete" @click="deleteFile(f.id)" title="Удалить">&times;</button>
          </div>
        </div>
      </section>
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
.success {
  color: #38a169;
  margin-top: 0.5rem;
  font-size: 0.9rem;
}

.upload-form {
  padding: 1.25rem;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  margin-bottom: 2rem;
}
.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}
.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  margin-bottom: 1rem;
}
.form-group label {
  font-size: 0.85rem;
  font-weight: 600;
  color: #4a5568;
}
.form-group select,
.form-group input[type='file'] {
  padding: 0.5rem;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  font-size: 0.9rem;
  font-family: inherit;
  background: #fff;
}
.form-group select:focus {
  outline: none;
  border-color: #3182ce;
  box-shadow: 0 0 0 2px rgba(49, 130, 206, 0.15);
}
.btn-upload {
  padding: 0.55rem 1.5rem;
  background: #3182ce;
  color: #fff;
  border: none;
  border-radius: 6px;
  font-size: 0.9rem;
  font-family: inherit;
  cursor: pointer;
  transition: background 0.2s;
}
.btn-upload:hover:not(:disabled) {
  background: #2b6cb0;
}
.btn-upload:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.files-section {
  margin-top: 0.5rem;
}
.empty {
  color: #a0aec0;
  text-align: center;
  padding: 2rem 0;
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
.btn-delete {
  flex-shrink: 0;
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  background: transparent;
  color: #a0aec0;
  font-size: 1.2rem;
  cursor: pointer;
  transition: color 0.2s, border-color 0.2s;
}
.btn-delete:hover {
  color: #e53e3e;
  border-color: #e53e3e;
}
</style>
