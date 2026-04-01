<template>
  <div class="space-y-6">
    <useHead><title>Ngữ pháp</title></useHead>

    <div>
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Ngữ pháp TOEIC</h1>
      <p class="text-sm text-gray-500 mt-0.5">Học từng chủ đề ngữ pháp, luyện mini quiz sau mỗi bài</p>
    </div>

    <!-- Filter -->
    <div class="flex flex-wrap gap-2">
      <button
        class="px-3 py-1.5 rounded-full text-sm font-medium transition-colors"
        :class="grammarStore.filter.category === null ? 'bg-primary text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'"
        @click="grammarStore.clearFilter()"
      >
        Tất cả
      </button>
      <button
        v-for="cat in categories"
        :key="cat.value"
        class="px-3 py-1.5 rounded-full text-sm font-medium transition-colors"
        :class="grammarStore.filter.category === cat.value ? 'bg-primary text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'"
        @click="grammarStore.setFilter({ category: cat.value })"
      >
        {{ cat.label }}
      </button>
    </div>

    <!-- Lessons grid -->
    <div v-if="grammarStore.isLoading" class="flex justify-center py-12">
      <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <NuxtLink
        v-for="lesson in grammarStore.filteredLessons"
        :key="lesson.id"
        :to="`/grammar/${lesson.id}`"
        class="bg-white dark:bg-slate-900 rounded-xl p-5 shadow-card hover:shadow-card-hover transition-all border border-transparent hover:border-primary-light group"
      >
        <div class="flex items-start justify-between mb-3">
          <span class="text-2xl">{{ categoryIcon(lesson.category) }}</span>
          <div class="flex gap-1">
            <span
              v-for="i in 3"
              :key="i"
              class="w-2 h-2 rounded-full"
              :class="i <= lesson.difficulty ? 'bg-primary' : 'bg-gray-200'"
            />
          </div>
        </div>
        <h3 class="font-semibold text-gray-900 dark:text-white group-hover:text-primary transition-colors text-sm">{{ lesson.title }}</h3>
        <p class="text-xs text-gray-400 mt-1 capitalize">{{ lesson.category.replace('_', ' ') }}</p>
        <div class="flex items-center justify-between mt-3">
          <span class="text-xs text-gray-400">{{ lesson.questions.length }} câu hỏi</span>
          <span class="text-xs text-gray-400">{{ lesson.estimatedMinutes }} phút</span>
        </div>
      </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { GrammarCategory } from '~/types'

definePageMeta({ middleware: 'auth' })

const grammarStore = useGrammarStore()
onMounted(() => grammarStore.fetchLessons())

const categories: Array<{ value: GrammarCategory; label: string }> = [
  { value: 'tenses', label: 'Thì' },
  { value: 'word_form', label: 'Từ loại' },
  { value: 'prepositions', label: 'Giới từ' },
  { value: 'conjunctions', label: 'Liên từ' },
  { value: 'articles', label: 'Mạo từ' },
  { value: 'subject_verb_agreement', label: 'Chủ vị' },
  { value: 'relative_clauses', label: 'Mệnh đề quan hệ' },
]

const iconMap: Record<GrammarCategory, string> = {
  tenses: '⏰',
  word_form: '🔤',
  prepositions: '📍',
  conjunctions: '🔗',
  articles: '📰',
  subject_verb_agreement: '⚖️',
  relative_clauses: '🔄',
  conditionals: '❓',
  passive_voice: '🔀',
  comparison: '📊',
}

function categoryIcon(cat: GrammarCategory) {
  return iconMap[cat] ?? '📝'
}
</script>
