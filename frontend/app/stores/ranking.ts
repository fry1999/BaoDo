import { defineStore } from 'pinia'
import type { RankingEntry, UserRankResult, RankingPeriod, RankingType } from '~/types'

interface RankingState {
  examBoard: RankingEntry[]
  xpBoard: RankingEntry[]
  myRank: UserRankResult | null
  activePeriod: RankingPeriod
  activeType: RankingType
  isLoading: boolean
  error: string | null
}

export const useRankingStore = defineStore('ranking', {
  state: (): RankingState => ({
    examBoard: [],
    xpBoard: [],
    myRank: null,
    activePeriod: 'weekly',
    activeType: 'exam',
    isLoading: false,
    error: null,
  }),

  getters: {
    activeBoard: (state): RankingEntry[] =>
      state.activeType === 'exam' ? state.examBoard : state.xpBoard,
  },

  actions: {
    async fetchExamLeaderboard(period: RankingPeriod = 'weekly') {
      this.isLoading = true
      this.error = null
      try {
        const config = useRuntimeConfig()
        this.examBoard = await $fetch<RankingEntry[]>(
          `${config.public.apiBaseUrl}/api/ranking/exam?period=${period}&limit=50`,
        )
        this.activePeriod = period
      }
      catch (e: unknown) {
        this.error = e instanceof Error ? e.message : 'Không thể tải bảng xếp hạng'
      }
      finally {
        this.isLoading = false
      }
    },

    async fetchXpLeaderboard() {
      this.isLoading = true
      this.error = null
      try {
        const config = useRuntimeConfig()
        this.xpBoard = await $fetch<RankingEntry[]>(
          `${config.public.apiBaseUrl}/api/ranking/xp?limit=50`,
        )
      }
      catch (e: unknown) {
        this.error = e instanceof Error ? e.message : 'Không thể tải bảng xếp hạng XP'
      }
      finally {
        this.isLoading = false
      }
    },

    async fetchMyRank(period: RankingPeriod = 'weekly') {
      try {
        const config = useRuntimeConfig()
        const auth = useAuthStore()
        if (!auth.token) return
        this.myRank = await $fetch<UserRankResult>(
          `${config.public.apiBaseUrl}/api/ranking/me?period=${period}`,
          { headers: { Authorization: `Bearer ${auth.token}` } },
        )
      }
      catch {
        this.myRank = null
      }
    },

    setActiveType(type: RankingType) {
      this.activeType = type
    },

    async switchPeriod(period: RankingPeriod) {
      this.activePeriod = period
      await Promise.all([
        this.fetchExamLeaderboard(period),
        this.fetchMyRank(period),
      ])
    },
  },
})
