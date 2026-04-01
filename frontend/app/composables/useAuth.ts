export function useAuth() {
  const store = useAuthStore()

  return {
    user: computed(() => store.user),
    isAuthenticated: computed(() => store.isAuthenticated),
    isAdmin: computed(() => store.isAdmin),
    isPro: computed(() => store.isPro),
    isLoading: computed(() => store.isLoading),
    error: computed(() => store.error),
    login: store.login,
    register: store.register,
    logout: store.logout,
    fetchProfile: store.fetchProfile,
    clearError: store.clearError,
  }
}
