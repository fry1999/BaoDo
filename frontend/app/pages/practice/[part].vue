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

const questions = ref<Question[]>([])
const currentIndex = ref(0)
const selectedAnswer = ref<number | null>(null)
const isLoading = ref(false)

const currentQ = computed(() => questions.value[currentIndex.value] ?? null)
const isAnswered = computed(() => selectedAnswer.value !== null)
const isCorrect = computed(() => selectedAnswer.value === currentQ.value?.correctIndex)

onMounted(async () => {
  isLoading.value = true
  try {
    questions.value = await $fetch<Question[]>(`${config.public.apiBaseUrl}/api/practice`, {
      query: { part: part.value, limit: 20 },
      headers: { Authorization: `Bearer ${auth.token}` },
    })
  } finally {
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
  if (oi === currentQ.value?.correctIndex) return 'border-green-500 bg-green-50 text-green-800'
  if (oi === selectedAnswer.value) return 'border-red-400 bg-red-50 text-red-700'
  return 'border-gray-200 dark:border-slate-700 opacity-50'
}

function next() {
  currentIndex.value++
  selectedAnswer.value = null
}

function prev() {
  currentIndex.value--
  selectedAnswer.value = null
}

function playAudio() {
  if (currentQ.value?.audioUrl) new Audio(currentQ.value.audioUrl).play()
}
</script>
