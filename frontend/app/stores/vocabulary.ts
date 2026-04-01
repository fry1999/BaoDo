import { defineStore } from 'pinia'
import type { Vocabulary, UserVocabularyCard, SRSRating } from '~/types'

interface VocabularyState {
  cards: UserVocabularyCard[]
  currentSession: UserVocabularyCard[]
  sessionIndex: number
  isFlipped: boolean
  isLoading: boolean
  studyStats: { correct: number; wrong: number; skipped: number }
}

export const useVocabularyStore = defineStore('vocabulary', {
  state: (): VocabularyState => ({
    cards: [],
    currentSession: [],
    sessionIndex: 0,
    isFlipped: false,
    isLoading: false,
    studyStats: { correct: 0, wrong: 0, skipped: 0 },
  }),

  getters: {
    currentCard: (state): UserVocabularyCard | null => state.currentSession[state.sessionIndex] ?? null,
    isDue: (state) => (card: UserVocabularyCard) => new Date(card.nextReviewAt) <= new Date(),
    dueCount: (state) => state.cards.filter((c) => new Date(c.nextReviewAt) <= new Date()).length,
    masteredCount: (state) => state.cards.filter((c) => c.status === 'mastered').length,
    sessionProgress: (state) =>
      state.currentSession.length > 0
        ? Math.round((state.sessionIndex / state.currentSession.length) * 100)
        : 0,
  },

  actions: {
    async fetchDueCards() {
      this.isLoading = true
      try {
        const config = useRuntimeConfig()
        const auth = useAuthStore()
        const res = await $fetch<UserVocabularyCard[]>(`${config.public.apiBaseUrl}/api/vocabulary/due`, {
          headers: { Authorization: `Bearer ${auth.token}` },
        })
        this.cards = res
        this.currentSession = [...res].slice(0, 20)
        this.sessionIndex = 0
        this.studyStats = { correct: 0, wrong: 0, skipped: 0 }
      } finally {
        this.isLoading = false
      }
    },

    async rateCard(cardId: string, rating: SRSRating) {
      const config = useRuntimeConfig()
      const auth = useAuthStore()
      await $fetch(`${config.public.apiBaseUrl}/api/vocabulary/cards/${cardId}/rate`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${auth.token}` },
        body: { rating },
      })

      if (rating >= 3) this.studyStats.correct++
      else this.studyStats.wrong++

      this.isFlipped = false
      this.sessionIndex++
    },

    flipCard() {
      this.isFlipped = !this.isFlipped
    },

    skipCard() {
      this.studyStats.skipped++
      this.isFlipped = false
      this.sessionIndex++
    },

    resetSession() {
      this.sessionIndex = 0
      this.isFlipped = false
      this.studyStats = { correct: 0, wrong: 0, skipped: 0 }
    },
  },
})
