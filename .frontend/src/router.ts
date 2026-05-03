import { createRouter, createWebHistory } from 'vue-router'
import { useToast } from 'vue-toastification'
import { getUserRoles } from './auth'

import HomePage from './pages/HomePage.vue'
import TeacherDashboard from './pages/TeacherDashboard.vue'
import TeacherStudents from './pages/TeacherStudents.vue'

declare module 'vue-router' {
  interface RouteMeta {
    requiredRoles?: string[]
  }
}

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomePage,
    },
    {
      path: '/teacher',
      name: 'teacher',
      component: TeacherDashboard,
      meta: { requiredRoles: ['teacher', 'admin'] },
    },
    {
      path: '/teacher/students',
      name: 'teacher-students',
      component: TeacherStudents,
      meta: { requiredRoles: ['teacher', 'admin'] },
    },
  ],
})

router.beforeEach((to) => {
  const requiredRoles = to.meta.requiredRoles
  if (!requiredRoles || requiredRoles.length === 0) return true

  const userRoles = getUserRoles()
  const hasAccess = requiredRoles.some((role) => userRoles.includes(role))
  if (hasAccess) return true

  const toast = useToast()
  toast.error('Доступ запрещён: недостаточно прав')
  return { name: 'home' }
})

export default router
