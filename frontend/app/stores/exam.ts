import { defineStore } from 'pinia'
import type { Test, Question, UserTestAnswer, UserTestResult, ToeicPart } from '~/types'

interface ExamState {
  currentTest: Test | null
  questions: Question[]
  answers: Record<string, number>
  timeRemaining: number
  isStarted: boolean
  isSubmitted: boolean
  currentQuestionIndex: number
  result: UserTestResult | null
  isLoading: boolean
}

export const useExamStore = defineStore('exam', {
  state: (): ExamState => ({
    currentTest: null,
    questions: [],
    answers: {},
    timeRemaining: 0,
    isStarted: false,
    isSubmitted: false,
    currentQuestionIndex: 0,
    result: null,
    isLoading: false,
  }),

  getters: {
    currentQuestion: (state): Question | null => state.questions[state.currentQuestionIndex] ?? null,
    answeredCount: (state) => Object.keys(state.answers).length,
    unansweredCount: (state) => state.questions.length - Object.keys(state.answers).length,
    progress: (state) =>
      state.questions.length > 0
        ? Math.round((Object.keys(state.answers).length / state.questions.length) * 100)
        : 0,
    formattedTime: (state) => {
      const h = Math.floor(state.timeRemaining / 3600)
      const m = Math.floor((state.timeRemaining % 3600) / 60)
      const s = state.timeRemaining % 60
      return h > 0
        ? `${h}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
        : `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
    },
    timerStatus: (state): 'normal' | 'warning' | 'danger' => {
      if (state.timeRemaining < 600) return 'danger'
      if (state.timeRemaining < 1800) return 'warning'
      return 'normal'
    },
    questionsByPart: (state): Record<ToeicPart, Question[]> => {
      const result = {} as Record<ToeicPart, Question[]>
      for (const q of state.questions) {
        if (!result[q.part]) result[q.part] = []
        result[q.part].push(q)
      }
      return result
    },
  },

  actions: {
    async loadTest(testId: string) {
      this.isLoading = true
      try {
        const config = useRuntimeConfig()
        const auth = useAuthStore()
        const res = await $fetch<{ test: Test; questions: Question[] }>(
          `${config.public.apiBaseUrl}/api/exam/${testId}`,
          { headers: { Authorization: `Bearer ${auth.token}` } },
        )
        this.currentTest = res.test
        this.questions = res.questions
        this.timeRemaining = res.test.durationMinutes * 60
        this.answers = {}
        this.isStarted = false
        this.isSubmitted = false
        this.currentQuestionIndex = 0
      } finally {
        this.isLoading = false
      }
    },

    startExam() {
      this.isStarted = true
    },

    selectAnswer(questionId: string, optionIndex: number) {
      this.answers[questionId] = optionIndex
    },

    navigateTo(index: number) {
      this.currentQuestionIndex = index
    },

    nextQuestion() {
      if (this.currentQuestionIndex < this.questions.length - 1)
        this.currentQuestionIndex++
    },

    prevQuestion() {
      if (this.currentQuestionIndex > 0)
        this.currentQuestionIndex--
    },

    tickTimer() {
      if (this.timeRemaining > 0) this.timeRemaining--
      else this.submitExam()
    },

    async submitExam() {
      if (this.isSubmitted) return
      this.isSubmitted = true

      const config = useRuntimeConfig()
      const auth = useAuthStore()

      const payload: UserTestAnswer[] = this.questions.map((q) => ({
        questionId: q.id,
        selectedIndex: this.answers[q.id] ?? -1,
        isCorrect: this.answers[q.id] === q.correctIndex,
        timeSpentSeconds: 0,
      }))

      const res = await $fetch<UserTestResult>(`${config.public.apiBaseUrl}/api/exam/${this.currentTest?.id}/submit`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${auth.token}` },
        body: { answers: payload },
      })
      this.result = res
      await navigateTo(`/exam/result/${res.id}`)
    },

    reset() {
      this.currentTest = null
      this.questions = []
      this.answers = {}
      this.timeRemaining = 0
      this.isStarted = false
      this.isSubmitted = false
      this.currentQuestionIndex = 0
      this.result = null
    },
  },
})
