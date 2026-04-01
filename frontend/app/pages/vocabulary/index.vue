<template>
  <div class="space-y-6">
    <useHead><title>Từ vựng</title></useHead>

    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Từ vựng</h1>
        <p class="text-sm text-gray-500 mt-0.5">{{ vocabStore.dueCount }} từ cần ôn hôm nay</p>
      </div>
      <NuxtLink
        v-if="vocabStore.dueCount > 0"
        to="/vocabulary/study"
        class="px-5 py-2.5 bg-primary text-white rounded-xl font-semibold text-sm hover:bg-primary-dark transition-colors shadow-sm"
      >
        Học ngay →
      </NuxtLink>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-3 gap-4">
      <div class="bg-white dark:bg-slate-900 rounded-xl p-4 shadow-card text-center">
        <p class="text-2xl font-bold text-primary">{{ vocabStore.dueCount }}</p>
        <p class="text-xs text-gray-500 mt-0.5">Cần ôn hôm nay</p>
      </div>
      <div class="bg-white dark:bg-slate-900 rounded-xl p-4 shadow-card text-center">
        <p class="text-2xl font-bold text-green-600">{{ vocabStore.masteredCount }}</p>
        <p class="text-xs text-gray-500 mt-0.5">Đã thuộc</p>
      </div>
      <div class="bg-white dark:bg-slate-900 rounded-xl p-4 shadow-card text-center">
        <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ vocabStore.cards.length }}</p>
        <p class="text-xs text-gray-500 mt-0.5">Tổng số thẻ</p>
      </div>
    </div>

    <!-- Status info when nothing to review -->
    <div
      v-if="vocabStore.dueCount === 0 && !vocabStore.isLoading"
      class="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-xl p-6 text-center"
    >
      <p class="text-4xl mb-3">🎉</p>
      <p class="font-semibold text-green-800 dark:text-green-300">Hoàn thành ôn tập hôm nay!</p>
      <p class="text-sm text-green-600 dark:text-green-400 mt-1">Quay lại vào ngày mai để ôn tiếp.</p>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const vocabStore = useVocabularyStore()
onMounted(() => vocabStore.fetchDueCards())
</script>
