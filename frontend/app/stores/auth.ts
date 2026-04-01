import { defineStore } from 'pinia'
import type { UserProfile, SubscriptionPlan } from '~/types'

interface AuthState {
  user: UserProfile | null
  token: string | null
  isLoading: boolean
  error: string | null
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    user: null,
    token: null,
    isLoading: false,
    error: null,
  }),

  getters: {
    isAuthenticated: (state) => !!state.token && !!state.user,
    isAdmin: (state) => state.user?.role?.toLowerCase() === 'admin' || state.user?.role?.toLowerCase() === 'content_editor',
    isSuperAdmin: (state) => state.user?.role?.toLowerCase() === 'admin',
    currentPlan: (state): SubscriptionPlan => state.user?.subscription ?? 'free',
    isPro: (state) => state.user?.subscription === 'pro',
    isBasicOrAbove: (state) => state.user?.subscription === 'basic' || state.user?.subscription === 'pro',
    userInitials: (state) => {
      if (!state.user?.fullName) return 'U'
      return state.user.fullName.split(' ').map((n) => n[0]).join('').toUpperCase().slice(0, 2)
    },
  },

  actions: {
    async login(email: string, password: string) {
      this.isLoading = true
      this.error = null
      try {
        const config = useRuntimeConfig()
        const res = await $fetch<{ token: string; user: UserProfile }>(`${config.public.apiBaseUrl}/api/auth/login`, {
          method: 'POST',
          body: { email, password },
        })
        this.token = res.token
        this.user = res.user
        await navigateTo('/')
      } catch (err: unknown) {
        this.error = (err as Error).message || 'Đăng nhập thất bại'
        throw err
      } finally {
        this.isLoading = false
      }
    },

    async register(email: string, password: string, fullName: string) {
      this.isLoading = true
      this.error = null
      try {
        const config = useRuntimeConfig()
        const res = await $fetch<{ token: string; user: UserProfile }>(`${config.public.apiBaseUrl}/api/auth/register`, {
          method: 'POST',
          body: { email, password, fullName },
        })
        this.token = res.token
        this.user = res.user
        await navigateTo('/')
      } catch (err: unknown) {
        this.error = (err as Error).message || 'Đăng ký thất bại'
        throw err
      } finally {
        this.isLoading = false
      }
    },

    async fetchProfile() {
      if (!this.token) return
      try {
        const config = useRuntimeConfig()
        const res = await $fetch<UserProfile>(`${config.public.apiBaseUrl}/api/auth/profile`, {
          headers: { Authorization: `Bearer ${this.token}` },
        })
        this.user = res
      } catch {
        this.logout()
      }
    },

    logout() {
      this.user = null
      this.token = null
      navigateTo('/auth/login')
    },

    clearError() {
      this.error = null
    },
  },

  persist: {
    storage: import.meta.client ? localStorage : undefined,
    pick: ['token', 'user'],
  },
})
