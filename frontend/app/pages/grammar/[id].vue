<template>
  <div class="max-w-3xl mx-auto space-y-6">
    <useHead><title>{{ grammarStore.currentLesson?.title ?? 'Ngữ pháp' }}</title></useHead>

    <div v-if="grammarStore.isLoading" class="flex justify-center py-24">
      <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
    </div>

    <template v-else-if="grammarStore.currentLesson">
      <!-- Back + title -->
      <div>
        <NuxtLink to="/grammar" class="text-sm text-gray-500 hover:text-primary flex items-center gap-1 mb-3">
          ← Danh sách bài học
        </NuxtLink>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">{{ grammarStore.currentLesson.title }}</h1>
      </div>

      <!-- Lesson content -->
      <div class="bg-white dark:bg-slate-900 rounded-2xl shadow-card p-6 prose prose-sm dark:prose-invert max-w-none"
        v-html="renderedContent"
      />

      <!-- Quiz section -->
      <div class="bg-white dark:bg-slate-900 rounded-2xl shadow-card p-6">
        <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Mini Quiz</h2>

        <div class="space-y-5">
          <div v-for="(q, qi) in grammarStore.currentLesson.questions" :key="q.id">
            <p class="text-sm font-medium text-gray-900 dark:text-white mb-2">
              {{ qi + 1 }}. {{ q.questionText }}
            </p>
            <div class="space-y-2">
              <button
                v-for="(option, oi) in q.options"
                :key="oi"
                class="w-full text-left px-4 py-2.5 rounded-xl text-sm border transition-all"
                :class="getOptionClass(q.id, oi, q.correctIndex)"
                :disabled="grammarStore.quizSubmitted"
                @click="grammarStore.selectAnswer(q.id, oi)"
              >
                <span class="font-mono mr-2">{{ String.fromCharCode(65 + oi) }}.</span>
                {{ option }}
              </button>
            </div>
            <div v-if="grammarStore.quizSubmitted && grammarStore.quizAnswers[q.id] !== q.correctIndex" class="mt-2 text-sm text-blue-700 bg-blue-50 dark:bg-blue-900/20 dark:text-blue-300 px-3 py-2 rounded-lg">
              💡 {{ q.explanation }}
            </div>
          </div>
        </div>

        <div v-if="!grammarStore.quizSubmitted" class="mt-6">
          <button
            :disabled="!grammarStore.isQuizComplete"
            class="px-6 py-2.5 bg-primary text-white rounded-xl font-semibold text-sm hover:bg-primary-dark transition-colors disabled:opacity-50"
            @click="grammarStore.submitQuiz()"
          >
            Kiểm tra
          </button>
        </div>

        <div v-else class="mt-6 bg-gray-50 dark:bg-slate-800 rounded-xl p-4 text-center">
          <p class="text-2xl font-bold" :class="grammarStore.quizScore >= 70 ? 'text-green-600' : 'text-orange-500'">
            {{ grammarStore.quizScore }}%
          </p>
          <p class="text-sm text-gray-500 mt-1">{{ grammarStore.quizScore >= 70 ? 'Làm tốt lắm!' : 'Hãy xem lại bài nhé' }}</p>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const route = useRoute()
const grammarStore = useGrammarStore()

onMounted(() => grammarStore.fetchLesson(route.params.id as string))

const renderedContent = computed(() => grammarStore.currentLesson?.content ?? '')

function getOptionClass(questionId: string, optionIndex: number, correctIndex: number) {
  const selected = grammarStore.quizAnswers[questionId]
  if (!grammarStore.quizSubmitted) {
    return selected === optionIndex
      ? 'border-primary bg-primary-light text-primary'
      : 'border-gray-200 dark:border-slate-700 hover:border-primary hover:bg-primary-light/50'
  }
  if (optionIndex === correctIndex) return 'border-green-500 bg-green-50 text-green-800'
  if (selected === optionIndex) return 'border-red-400 bg-red-50 text-red-700'
  return 'border-gray-200 dark:border-slate-700 opacity-50'
}
</script>
