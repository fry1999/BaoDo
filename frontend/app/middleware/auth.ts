export default defineNuxtRouteMiddleware((to) => {
  // Skip on server — Pinia persist only runs on client (localStorage)
  if (import.meta.server) return

  const auth = useAuthStore()

  if (!auth.isAuthenticated) {
    return navigateTo(`/auth/login?redirect=${encodeURIComponent(to.fullPath)}`)
  }
})
