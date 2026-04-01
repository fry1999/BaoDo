<template>
  <div class="max-w-lg mx-auto py-8 px-4">
    <useHead><title>Học từ vựng</title></useHead>

    <!-- Session complete -->
    <div v-if="isComplete" class="text-center py-16">
      <p class="text-5xl mb-4">🎊</p>
      <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">Hoàn thành phiên học!</h2>
      <div class="flex justify-center gap-8 my-6">
        <div class="text-center">
          <p class="text-2xl font-bold text-green-600">{{ vocabStore.studyStats.correct }}</p>
          <p class="text-sm text-gray-500">Đúng</p>
        </div>
        <div class="text-center">
          <p class="text-2xl font-bold text-red-500">{{ vocabStore.studyStats.wrong }}</p>
          <p class="text-sm text-gray-500">Sai</p>
        </div>
        <div class="text-center">
          <p class="text-2xl font-bold text-gray-400">{{ vocabStore.studyStats.skipped }}</p>
          <p class="text-sm text-gray-500">Bỏ qua</p>
        </div>
      </div>
      <div class="flex gap-3 justify-center">
        <NuxtLink to="/vocabulary" class="px-5 py-2.5 border border-gray-200 rounded-xl text-sm font-medium hover:bg-gray-50">
          Về trang từ vựng
        </NuxtLink>
        <button
          class="px-5 py-2.5 bg-primary text-white rounded-xl text-sm font-medium hover:bg-primary-dark"
          @click="vocabStore.fetchDueCards()"
        >
          Học tiếp
        </button>
      </div>
    </div>

    <!-- Loading -->
    <div v-else-if="vocabStore.isLoading" class="flex items-center justify-center py-24">
      <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
    </div>

    <!-- No cards -->
    <div v-else-if="!vocabStore.currentCard" class="text-center py-16">
      <p class="text-4xl mb-3">✅</p>
      <p class="font-semibold text-gray-700 dark:text-gray-300">Không có từ nào cần ôn hôm nay</p>
      <NuxtLink to="/vocabulary" class="mt-4 inline-block text-primary hover:underline text-sm">
        ← Về trang từ vựng
      </NuxtLink>
    </div>

    <!-- Study card -->
    <div v-else>
      <!-- Progress bar -->
      <div class="flex items-center gap-3 mb-6">
        <NuxtLink to="/vocabulary" class="text-gray-400 hover:text-gray-600">
          ✕
        </NuxtLink>
        <div class="flex-1 h-2 bg-gray-200 rounded-full overflow-hidden">
          <div
            class="h-full bg-primary rounded-full transition-all duration-500"
            :style="{ width: `${vocabStore.sessionProgress}%` }"
          />
        </div>
        <span class="text-sm text-gray-500 font-mono">
          {{ vocabStore.sessionIndex }}/{{ vocabStore.currentSession.length }}
        </span>
      </div>

      <!-- Flashcard -->
      <div class="flip-card w-full h-80 cursor-pointer" :class="{ flipped: vocabStore.isFlipped }" @click="vocabStore.flipCard()">
        <div class="flip-card-inner w-full h-full relative">
          <!-- Front -->
          <div class="flip-card-front absolute inset-0 bg-white dark:bg-slate-900 rounded-2xl shadow-card-hover border border-gray-100 dark:border-slate-800 flex flex-col items-center justify-center p-6">
            <span class="text-xs text-gray-400 mb-4 uppercase tracking-widest">{{ vocabStore.currentCard?.vocabulary?.topic }}</span>
            <h2 class="text-4xl font-bold text-gray-900 dark:text-white mb-2">
              {{ vocabStore.currentCard?.vocabulary?.word }}
            </h2>
            <p class="text-gray-500 font-mono text-sm">{{ vocabStore.currentCard?.vocabulary?.phonetic }}</p>
            <p class="text-xs text-gray-400 mt-6">Nhấn để lật thẻ</p>
          </div>

          <!-- Back -->
          <div class="flip-card-back absolute inset-0 bg-white dark:bg-slate-900 rounded-2xl shadow-card-hover border border-gray-100 dark:border-slate-800 flex flex-col p-6 overflow-y-auto">
            <div class="flex items-center gap-2 mb-3">
              <span class="text-xs bg-primary-light text-primary px-2 py-0.5 rounded-full font-medium">
                {{ vocabStore.currentCard?.vocabulary?.partOfSpeech }}
              </span>
            </div>
            <p class="text-xl font-bold text-gray-900 dark:text-white mb-1">{{ vocabStore.currentCard?.vocabulary?.meaningVi }}</p>
            <p class="text-sm text-gray-500 mb-3">{{ vocabStore.currentCard?.vocabulary?.meaningEn }}</p>

            <p v-if="vocabStore.currentCard?.vocabulary?.examples?.[0]" class="text-sm text-gray-600 dark:text-gray-400 italic border-l-2 border-primary-light pl-3 mb-3">
              "{{ vocabStore.currentCard?.vocabulary?.examples[0] }}"
            </p>

            <div v-if="vocabStore.currentCard?.vocabulary?.collocations?.length" class="flex flex-wrap gap-1">
              <span
                v-for="col in vocabStore.currentCard?.vocabulary?.collocations.slice(0, 3)"
                :key="col"
                class="text-xs bg-gray-100 dark:bg-slate-800 text-gray-600 dark:text-gray-400 px-2 py-0.5 rounded-full"
              >
                {{ col }}
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- SRS Rating buttons (show after flip) -->
      <Transition name="fade-in-up">
        <div v-if="vocabStore.isFlipped" class="grid grid-cols-4 gap-2 mt-6">
          <button
            v-for="rating in srsRatings"
            :key="rating.value"
            class="py-3 rounded-xl text-white text-sm font-semibold transition-transform hover:scale-105 active:scale-95"
            :class="rating.color"
            @click="vocabStore.rateCard(vocabStore.currentCard!.id, rating.value)"
          >
            {{ rating.label }}
          </button>
        </div>
      </Transition>

      <!-- Skip -->
      <div class="text-center mt-4">
        <button class="text-sm text-gray-400 hover:text-gray-600" @click="vocabStore.skipCard()">
          Bỏ qua
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { SRSRating } from '~/types'

definePageMeta({ middleware: 'auth' })

const vocabStore = useVocabularyStore()
const { ratingLabel, ratingColor } = useSRS()

const isComplete = computed(
  () => vocabStore.sessionIndex >= vocabStore.currentSession.length && vocabStore.currentSession.length > 0,
)

const srsRatings: Array<{ value: SRSRating; label: string; color: string }> = [
  { value: 1, label: 'Quên', color: 'bg-red-500 hover:bg-red-600' },
  { value: 2, label: 'Khó', color: 'bg-orange-500 hover:bg-orange-600' },
  { value: 3, label: 'Ổn', color: 'bg-blue-500 hover:bg-blue-600' },
  { value: 4, label: 'Dễ', color: 'bg-green-500 hover:bg-green-600' },
]

onMounted(() => {
  if (vocabStore.currentSession.length === 0) vocabStore.fetchDueCards()
})
</script>
