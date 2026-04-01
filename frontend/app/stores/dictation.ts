import { defineStore } from 'pinia'
import type { DictationContent, DictationSegment, DictationCheckResult } from '~/types'

interface DictationState {
  library: DictationContent[]
  current: DictationContent | null
  segmentIndex: number
  userInput: string
  checkResult: DictationCheckResult | null
  isRevealed: boolean
  sessionResults: Array<{ segmentIndex: number; accuracy: number }>
  isLoading: boolean
  playbackRate: number
}

export const useDictationStore = defineStore('dictation', {
  state: (): DictationState => ({
    library: [],
    current: null,
    segmentIndex: 0,
    userInput: '',
    checkResult: null,
    isRevealed: false,
    sessionResults: [],
    isLoading: false,
    playbackRate: 1,
  }),

  getters: {
    currentSegment: (state): DictationSegment | null =>
      state.current?.segments[state.segmentIndex] ?? null,

    totalSegments: (state): number => state.current?.segments.length ?? 0,

    sessionAccuracy: (state): number => {
      if (state.sessionResults.length === 0) return 0
      const sum = state.sessionResults.reduce((acc, r) => acc + r.accuracy, 0)
      return Math.round(sum / state.sessionResults.length)
    },

    isLastSegment: (state): boolean =>
      state.segmentIndex >= (state.current?.segments.length ?? 0) - 1,
  },

  actions: {
    async fetchLibrary() {
      this.isLoading = true
      try {
        const config = useRuntimeConfig()
        const auth = useAuthStore()
        const res = await $fetch<DictationContent[]>(`${config.public.apiBaseUrl}/api/dictation`, {
          headers: { Authorization: `Bearer ${auth.token}` },
        })
        this.library = res
      } finally {
        this.isLoading = false
      }
    },

    async loadContent(id: string) {
      this.isLoading = true
      try {
        const config = useRuntimeConfig()
        const auth = useAuthStore()
        const res = await $fetch<DictationContent>(`${config.public.apiBaseUrl}/api/dictation/${id}`, {
          headers: { Authorization: `Bearer ${auth.token}` },
        })
        this.current = res
        this.segmentIndex = 0
        this.userInput = ''
        this.checkResult = null
        this.isRevealed = false
        this.sessionResults = []
      } finally {
        this.isLoading = false
      }
    },

    checkAnswer() {
      const segment = this.currentSegment
      if (!segment) return

      const result = checkDictationAccuracy(this.userInput, segment.text)
      this.checkResult = result

      this.sessionResults.push({ segmentIndex: this.segmentIndex, accuracy: result.accuracy })
    },

    reveal() {
      this.isRevealed = true
    },

    nextSegment() {
      if (!this.isLastSegment) {
        this.segmentIndex++
        this.userInput = ''
        this.checkResult = null
        this.isRevealed = false
      }
    },

    prevSegment() {
      if (this.segmentIndex > 0) {
        this.segmentIndex--
        this.userInput = ''
        this.checkResult = null
        this.isRevealed = false
      }
    },

    setPlaybackRate(rate: number) {
      this.playbackRate = rate
    },

    reset() {
      this.current = null
      this.segmentIndex = 0
      this.userInput = ''
      this.checkResult = null
      this.isRevealed = false
      this.sessionResults = []
    },
  },
})

function checkDictationAccuracy(input: string, answer: string): DictationCheckResult {
  const normalize = (s: string) => s.toLowerCase().replace(/[^a-z0-9\s']/g, '').trim()
  const inputWords = normalize(input).split(/\s+/).filter(Boolean)
  const answerWords = normalize(answer).split(/\s+/).filter(Boolean)

  let correct = 0
  const words = answerWords.map((word, i) => {
    const inputWord = inputWords[i]
    if (!inputWord) return { word, status: 'missing' as const }
    if (inputWord === word) {
      correct++
      return { word, status: 'correct' as const }
    }
    return { word, status: 'wrong' as const }
  })

  const extra = inputWords.slice(answerWords.length).map((w) => ({ word: w, status: 'extra' as const }))
  const accuracy = answerWords.length > 0 ? Math.round((correct / answerWords.length) * 100) : 0

  return { accuracy, words: [...words, ...extra] }
}
