// Runs SECOND (prefix 02) — persist plugin already registered, store has state from localStorage
export default defineNuxtPlugin(async () => {
  const auth = useAuthStore()
  // Refresh profile from server if token exists but user data missing
  if (auth.token && !auth.user) {
    await auth.fetchProfile()
  }
})
