import { defineStore } from 'pinia'
import type { AdminStats, UserProfile, PaginatedResponse } from '~/types'

interface AdminState {
  stats: AdminStats | null
  users: PaginatedResponse<UserProfile> | null
  isLoading: boolean
  usersPage: number
  usersSearch: string
}

export const useAdminStore = defineStore('admin', {
  state: (): AdminState => ({
    stats: null,
    users: null,
    isLoading: false,
    usersPage: 1,
    usersSearch: '',
  }),

  actions: {
    async fetchStats() {
      this.isLoading = true
      try {
        const config = useRuntimeConfig()
        const auth = useAuthStore()
        const res = await $fetch<AdminStats>(`${config.public.apiBaseUrl}/api/admin/stats`, {
          headers: { Authorization: `Bearer ${auth.token}` },
        })
        this.stats = res
      } finally {
        this.isLoading = false
      }
    },

    async fetchUsers(page = 1, search = '') {
      this.isLoading = true
      this.usersPage = page
      this.usersSearch = search
      try {
        const config = useRuntimeConfig()
        const auth = useAuthStore()
        const res = await $fetch<PaginatedResponse<UserProfile>>(
          `${config.public.apiBaseUrl}/api/admin/users`,
          {
            headers: { Authorization: `Bearer ${auth.token}` },
            query: { page, pageSize: 20, search },
          },
        )
        this.users = res
      } finally {
        this.isLoading = false
      }
    },

    async banUser(userId: string) {
      const config = useRuntimeConfig()
      const auth = useAuthStore()
      await $fetch(`${config.public.apiBaseUrl}/api/admin/users/${userId}/ban`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${auth.token}` },
      })
      await this.fetchUsers(this.usersPage, this.usersSearch)
    },

    async grantPro(userId: string, days: number) {
      const config = useRuntimeConfig()
      const auth = useAuthStore()
      await $fetch(`${config.public.apiBaseUrl}/api/admin/users/${userId}/grant-pro`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${auth.token}` },
        body: { days },
      })
    },
  },
})
