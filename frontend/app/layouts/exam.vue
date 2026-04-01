<template>
  <div class="min-h-screen bg-white dark:bg-slate-950 flex flex-col">
    <!-- Minimal exam header -->
    <header class="border-b border-gray-200 dark:border-slate-800 bg-white dark:bg-slate-900 px-4 py-3 flex items-center justify-between sticky top-0 z-50">
      <div class="flex items-center gap-3">
        <span class="text-sm font-medium text-gray-600 dark:text-slate-400">
          {{ examStore.currentTest?.title }}
        </span>
        <span class="text-xs bg-gray-100 dark:bg-slate-800 text-gray-600 dark:text-slate-400 px-2 py-1 rounded-md">
          Câu {{ examStore.currentQuestionIndex + 1 }} / {{ examStore.questions.length }}
        </span>
      </div>

      <!-- Timer -->
      <div
        class="flex items-center gap-1.5 font-mono font-semibold text-lg transition-colors"
        :class="{
          'text-gray-900 dark:text-white': examStore.timerStatus === 'normal',
          'text-yellow-600': examStore.timerStatus === 'warning',
          'text-red-600 animate-pulse': examStore.timerStatus === 'danger',
        }"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        {{ examStore.formattedTime }}
      </div>

      <!-- Progress -->
      <div class="flex items-center gap-2 text-sm text-gray-500">
        <span>{{ examStore.answeredCount }}/{{ examStore.questions.length }}</span>
        <span class="text-gray-300">|</span>
        <button
          class="text-red-500 hover:text-red-700 text-sm font-medium"
          @click="confirmSubmit = true"
        >
          Nộp bài
        </button>
      </div>
    </header>

    <main class="flex-1">
      <slot />
    </main>

    <!-- Submit confirm modal -->
    <div v-if="confirmSubmit" class="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
      <div class="bg-white dark:bg-slate-900 rounded-2xl p-6 max-w-sm w-full shadow-popup">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">Nộp bài?</h3>
        <p class="text-gray-600 dark:text-slate-400 text-sm mb-6">
          Còn <strong>{{ examStore.unansweredCount }}</strong> câu chưa trả lời. Bạn chắc chắn muốn nộp?
        </p>
        <div class="flex gap-3">
          <button
            class="flex-1 px-4 py-2 border border-gray-200 rounded-lg text-sm font-medium hover:bg-gray-50"
            @click="confirmSubmit = false"
          >
            Tiếp tục làm
          </button>
          <button
            class="flex-1 px-4 py-2 bg-primary text-white rounded-lg text-sm font-medium hover:bg-primary-dark"
            @click="examStore.submitExam()"
          >
            Nộp bài
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
defineOptions({ name: 'ExamLayout' })

const examStore = useExamStore()
const confirmSubmit = ref(false)
</script>
