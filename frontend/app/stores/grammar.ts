import { defineStore } from 'pinia'
import type { GrammarLesson, GrammarQuestion, GrammarCategory } from '~/types'

interface GrammarState {
  lessons: GrammarLesson[]
  currentLesson: GrammarLesson | null
  quizAnswers: Record<string, number>
  quizSubmitted: boolean
  isLoading: boolean
  filter: { category: GrammarCategory | null; difficulty: number | null }
}

export const useGrammarStore = defineStore('grammar', {
  state: (): GrammarState => ({
    lessons: [],
    currentLesson: null,
    quizAnswers: {},
    quizSubmitted: false,
    isLoading: false,
    filter: { category: null, difficulty: null },
  }),

  getters: {
    filteredLessons: (state) =>
      state.lessons.filter((l) => {
        if (state.filter.category && l.category !== state.filter.category) return false
        if (state.filter.difficulty && l.difficulty !== state.filter.difficulty) return false
        return true
      }),

    quizScore: (state): number => {
      if (!state.currentLesson) return 0
      let correct = 0
      for (const q of state.currentLesson.questions) {
        if (state.quizAnswers[q.id] === q.correctIndex) correct++
      }
      return Math.round((correct / state.currentLesson.questions.length) * 100)
    },

    isQuizComplete: (state): boolean =>
      !!state.currentLesson &&
      state.currentLesson.questions.every((q) => state.quizAnswers[q.id] !== undefined),
  },

  actions: {
    async fetchLessons() {
      this.isLoading = true
      try {
        const config = useRuntimeConfig()
        const auth = useAuthStore()
        const res = await $fetch<GrammarLesson[]>(`${config.public.apiBaseUrl}/api/grammar`, {
          headers: { Authorization: `Bearer ${auth.token}` },
        })
        this.lessons = res
      } finally {
        this.isLoading = false
      }
    },

    async fetchLesson(id: string) {
      this.isLoading = true
      try {
        const config = useRuntimeConfig()
        const auth = useAuthStore()
        const res = await $fetch<GrammarLesson>(`${config.public.apiBaseUrl}/api/grammar/${id}`, {
          headers: { Authorization: `Bearer ${auth.token}` },
        })
        this.currentLesson = res
        this.quizAnswers = {}
        this.quizSubmitted = false
      } finally {
        this.isLoading = false
      }
    },

    selectAnswer(questionId: string, index: number) {
      if (!this.quizSubmitted) this.quizAnswers[questionId] = index
    },

    submitQuiz() {
      this.quizSubmitted = true
    },

    setFilter(filter: Partial<GrammarState['filter']>) {
      this.filter = { ...this.filter, ...filter }
    },

    clearFilter() {
      this.filter = { category: null, difficulty: null }
    },
  },
})
