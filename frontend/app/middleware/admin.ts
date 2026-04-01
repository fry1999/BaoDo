export default defineNuxtRouteMiddleware(() => {
  // Skip on server — Pinia persist only runs on client (localStorage)
  if (import.meta.server) return

  const auth = useAuthStore()

  if (!auth.isAuthenticated) {
    return navigateTo('/auth/login')
  }

  if (!auth.isAdmin) {
    return navigateTo('/')
  }
})
