<template>
  <div class="max-w-3xl mx-auto space-y-5">
    <useHead><title>Luyện Part {{ part }}</title></useHead>

    <div>
      <NuxtLink to="/practice" class="text-sm text-gray-500 hover:text-primary flex items-center gap-1 mb-3">
        ← Chọn Part
      </NuxtLink>
      <h1 class="text-xl font-bold text-gray-900 dark:text-white">Part {{ part }} — Luyện tập</h1>
    </div>

    <!-- Loading -->
    <div v-if="isLoading" class="flex justify-center py-24">
      <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
    </div>

    <!-- No questions -->
    <div v-else-if="questions.length === 0" class="text-center py-16 text-gray-400">
      <p class="text-4xl mb-3">📝</p>
      <p>Chưa có câu hỏi nào cho Part này</p>
    </div>

    <!-- Session summary screen -->
    <template v-else-if="showSummary && sessionResult">
      <div class="bg-white dark:bg-slate-900 rounded-2xl shadow-card p-6 text-center">
        <p class="text-5xl mb-4">{{ sessionResult.percentage >= 80 ? '🎉' : sessionResult.percentage >= 60 ? '👍' : '💪' }}</p>
        <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Kết quả phiên luyện tập</h2>
        <p class="text-gray-500 mt-1 text-sm">Part {{ part }}</p>

        <!-- Score ring -->
        <div class="my-6 flex items-center justify-center gap-8">
          <div class="text-center">
            <p class="text-4xl font-bold" :class="sessionResult.percentage >= 80 ? 'text-green-500' : sessionResult.percentage >= 60 ? 'text-primary' : 'text-red-500'">
              {{ sessionResult.percentage }}%
            </p>
            <p class="text-sm text-gray-500 mt-1">Chính xác</p>
          </div>
          <div class="text-center">
            <p class="text-4xl font-bold text-gray-900 dark:text-white">{{ sessionResult.correct }}/{{ sessionResult.total }}</p>
            <p class="text-sm text-gray-500 mt-1">Câu đúng</p>
          </div>
        </div>

        <div class="flex gap-3 justify-center">
          <button
            class="px-5 py-2.5 border border-gray-200 rounded-xl text-sm font-medium hover:bg-gray-50"
            @click="restart"
          >
            Luyện lại
          </button>
          <NuxtLink
            to="/practice"
            class="px-5 py-2.5 bg-primary text-white rounded-xl text-sm font-semibold hover:bg-primary-dark"
          >
            Chọn Part khác
          </NuxtLink>
        </div>
      </div>

      <!-- Wrong answers review -->
      <div v-if="wrongItems.length > 0" class="space-y-3">
        <h3 class="font-semibold text-gray-900 dark:text-white">Câu sai ({{ wrongItems.length }})</h3>
        <div
          v-for="(item, idx) in wrongItems"
          :key="item.questionId"
          class="bg-white dark:bg-slate-900 rounded-xl shadow-card p-5 border-l-4 border-red-400"
        >
          <p class="text-xs text-gray-400 mb-2">Câu {{ idx + 1 }}</p>
          <p v-if="item.questionText" class="text-sm font-medium text-gray-900 dark:text-white mb-3">
            {{ item.questionText }}
          </p>
          <div class="space-y-1.5 mb-3">
            <div
              v-for="(opt, oi) in item.options"
              :key="oi"
              class="px-3 py-2 rounded-lg text-sm border"
              :class="{
                'border-green-500 bg-green-50 text-green-800 dark:bg-green-900/20 dark:text-green-300': oi === item.correctIndex,
                'border-red-300 bg-red-50 text-red-700 dark:bg-red-900/20 dark:text-red-300': oi === item.selectedIndex && oi !== item.correctIndex,
                'border-gray-100 dark:border-slate-800 text-gray-500': oi !== item.correctIndex && oi !== item.selectedIndex,
              }"
            >
              <span class="font-semibold mr-2 text-gray-400">{{ String.fromCharCode(65 + oi) }}.</span>
              {{ opt }}
              <span v-if="oi === item.correctIndex" class="ml-2 text-xs font-semibold text-green-600">(Đúng)</span>
              <span v-if="oi === item.selectedIndex && oi !== item.correctIndex" class="ml-2 text-xs font-semibold text-red-500">(Bạn chọn)</span>
            </div>
          </div>
          <div class="p-3 bg-yellow-50 dark:bg-yellow-900/20 rounded-lg text-sm text-yellow-800 dark:text-yellow-300">
            <span class="font-semibold">Giải thích: </span>{{ item.explanation }}
          </div>
        </div>
      </div>
    </template>

    <!-- Question -->
    <template v-else>
      <!-- Progress -->
      <div class="flex items-center gap-3">
        <div class="flex-1 h-2 bg-gray-200 rounded-full overflow-hidden">
          <div
            class="h-full bg-primary rounded-full transition-all duration-300"
            :style="{ width: `${((currentIndex + 1) / questions.length) * 100}%` }"
          />
        </div>
        <span class="text-sm text-gray-500 font-mono">{{ currentIndex + 1 }}/{{ questions.length }}</span>
      </div>

      <!-- Question card -->
      <div class="bg-white dark:bg-slate-900 rounded-2xl shadow-card p-6">
        <!-- Audio (Part 1-4) -->
        <div v-if="currentQ?.audioUrl" class="mb-4 flex items-center gap-3 p-3 bg-gray-50 dark:bg-slate-800 rounded-xl">
          <button
            class="w-9 h-9 bg-primary text-white rounded-full flex items-center justify-center hover:bg-primary-dark text-sm"
            @click="playAudio"
          >
            ▶
          </button>
          <span class="text-sm text-gray-500">Nhấn để nghe audio</span>
        </div>

        <!-- Image (Part 1) -->
        <div v-if="currentQ?.imageUrl" class="mb-4 rounded-xl overflow-hidden">
          <img :src="currentQ.imageUrl" class="w-full max-h-60 object-contain bg-gray-50" alt="Question image" />
        </div>

        <!-- Question text -->
        <p v-if="currentQ?.questionText" class="text-sm text-gray-900 dark:text-white font-medium mb-4">
          {{ currentQ.questionText }}
        </p>

        <!-- Options -->
        <div class="space-y-2">
          <button
            v-for="(option, oi) in currentQ?.options"
            :key="oi"
            class="w-full text-left px-4 py-3 rounded-xl text-sm border transition-all"
            :class="getOptionClass(oi)"
            :disabled="isAnswered"
            @click="selectAnswer(oi)"
          >
            <span class="font-semibold mr-2 text-gray-500">{{ String.fromCharCode(65 + oi) }}.</span>
            {{ option }}
          </button>
        </div>

        <!-- Explanation -->
        <Transition name="fade-in-up">
          <div v-if="isAnswered" class="mt-4 p-4 rounded-xl text-sm" :class="isCorrect ? 'bg-green-50 text-green-800 dark:bg-green-900/20 dark:text-green-300' : 'bg-red-50 text-red-800 dark:bg-red-900/20 dark:text-red-300'">
            <p class="font-semibold mb-1">{{ isCorrect ? '✓ Chính xác!' : '✗ Chưa đúng' }}</p>
            <p>{{ currentQ?.explanation }}</p>
          </div>
        </Transition>
      </div>

      <!-- Navigation -->
      <div class="flex justify-between">
        <button
          :disabled="currentIndex === 0"
          class="px-4 py-2 text-sm text-gray-500 border border-gray-200 rounded-xl hover:bg-gray-50 disabled:opacity-30"
          @click="prev"
        >
          ← Câu trước
        </button>
        <button
          v-if="currentIndex < questions.length - 1"
          :disabled="!isAnswered"
          class="px-4 py-2 text-sm bg-primary text-white rounded-xl hover:bg-primary-dark font-semibold disabled:opacity-50 transition-colors"
          @click="next"
        >
          Câu tiếp →
        </button>
        <button
          v-else
          :disabled="!isAnswered || isSubmitting"
          class="px-5 py-2 text-sm bg-green-600 text-white rounded-xl hover:bg-green-700 font-semibold disabled:opacity-50 transition-colors"
          @click="finishSession"
        >
          {{ isSubmitting ? 'Đang tính...' : 'Xem kết quả →' }}
        </button>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import type { Question, ToeicPart } from '~/types'

definePageMeta({ middleware: 'auth' })

const route = useRoute()
const part = computed(() => Number(route.params.part) as ToeicPart)
const config = useRuntimeConfig()
const auth = useAuthStore()

interface SessionResultItem {
  questionId: string
  selectedIndex: number
  isCorrect: boolean
  correctIndex: number
  explanation: string
  questionText?: string
  options: string[]
}

interface SessionResult {
  correct: number
  total: number
  percentage: number
  items: SessionResultItem[]
}

const questions = ref<Question[]>([])
const currentIndex = ref(0)
const selectedAnswer = ref<number | null>(null)
const isLoading = ref(false)
const isSubmitting = ref(false)
const showSummary = ref(false)
const sessionResult = ref<SessionResult | null>(null)

// Track all answers for session submit
const sessionAnswers = ref<Array<{ questionId: string; selectedIndex: number }>>([])

const currentQ = computed(() => questions.value[currentIndex.value] ?? null)
const isAnswered = computed(() => selectedAnswer.value !== null)
const isCorrect = computed(() => selectedAnswer.value === currentQ.value?.correctIndex)

const wrongItems = computed(() =>
  sessionResult.value?.items.filter(i => !i.isCorrect) ?? [],
)

onMounted(async () => {
  isLoading.value = true
  try {
    questions.value = await $fetch<Question[]>(`${config.public.apiBaseUrl}/api/practice`, {
      query: { part: part.value, limit: 20 },
      headers: { Authorization: `Bearer ${auth.token}` },
    })
    sessionAnswers.value = []
  }
  finally {
    isLoading.value = false
  }
})

function selectAnswer(index: number) {
  if (isAnswered.value) return
  selectedAnswer.value = index
}

function getOptionClass(oi: number) {
  if (!isAnswered.value) {
    return selectedAnswer.value === oi
      ? 'border-primary bg-primary-light text-primary'
      : 'border-gray-200 dark:border-slate-700 hover:border-primary hover:bg-primary-light/30'
  }
  if (oi === currentQ.value?.correctIndex) return 'border-green-500 bg-green-50 text-green-800 dark:bg-green-900/20 dark:text-green-300'
  if (oi === selectedAnswer.value) return 'border-red-400 bg-red-50 text-red-700 dark:bg-red-900/20 dark:text-red-300'
  return 'border-gray-200 dark:border-slate-700 opacity-50'
}

function recordAnswer() {
  if (currentQ.value && selectedAnswer.value !== null) {
    const existing = sessionAnswers.value.find(a => a.questionId === currentQ.value!.id)
    if (!existing) {
      sessionAnswers.value.push({ questionId: currentQ.value.id, selectedIndex: selectedAnswer.value })
    }
  }
}

function next() {
  recordAnswer()
  currentIndex.value++
  selectedAnswer.value = null
}

function prev() {
  currentIndex.value--
  selectedAnswer.value = null
}

async function finishSession() {
  recordAnswer()
  isSubmitting.value = true
  try {
    const payload = sessionAnswers.value
    sessionResult.value = await $fetch<SessionResult>(`${config.public.apiBaseUrl}/api/practice/session`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${auth.token}` },
      body: payload,
    })
    showSummary.value = true
  }
  finally {
    isSubmitting.value = false
  }
}

function restart() {
  showSummary.value = false
  sessionResult.value = null
  sessionAnswers.value = []
  currentIndex.value = 0
  selectedAnswer.value = null
  // Reload questions
  isLoading.value = true
  $fetch<Question[]>(`${config.public.apiBaseUrl}/api/practice`, {
    query: { part: part.value, limit: 20 },
    headers: { Authorization: `Bearer ${auth.token}` },
  }).then(data => {
    questions.value = data
    isLoading.value = false
  })
}

function playAudio() {
  if (currentQ.value?.audioUrl) new Audio(currentQ.value.audioUrl).play()
}
</script>
