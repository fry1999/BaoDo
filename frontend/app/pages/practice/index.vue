<template>
  <div class="space-y-6">
    <useHead><title>Luyện tập</title></useHead>

    <div>
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Luyện tập</h1>
      <p class="text-sm text-gray-500 mt-0.5">Chọn Part để luyện tập trọng tâm</p>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <NuxtLink
        v-for="part in parts"
        :key="part.number"
        :to="`/practice/${part.number}`"
        class="bg-white dark:bg-slate-900 rounded-xl p-5 shadow-card hover:shadow-card-hover transition-all border border-transparent hover:border-primary-light group flex items-center gap-4"
      >
        <div
          class="w-12 h-12 rounded-xl flex items-center justify-center flex-shrink-0 text-xl"
          :class="part.bgColor"
        >
          {{ part.icon }}
        </div>
        <div class="flex-1 min-w-0">
          <h3 class="font-semibold text-gray-900 dark:text-white group-hover:text-primary transition-colors">
            Part {{ part.number }}: {{ part.title }}
          </h3>
          <p class="text-xs text-gray-500 mt-0.5">{{ part.description }}</p>
        </div>
        <span class="text-xs text-gray-400 font-medium bg-gray-100 dark:bg-slate-800 px-2 py-1 rounded-lg">
          {{ part.questions }} câu
        </span>
      </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const parts = [
  { number: 1, icon: '🖼️', title: 'Photographs', description: 'Mô tả hình ảnh', questions: 6, bgColor: 'bg-purple-100 dark:bg-purple-900/30' },
  { number: 2, icon: '💬', title: 'Question-Response', description: 'Câu hỏi và phản hồi', questions: 25, bgColor: 'bg-blue-100 dark:bg-blue-900/30' },
  { number: 3, icon: '🗣️', title: 'Conversations', description: 'Hội thoại', questions: 39, bgColor: 'bg-cyan-100 dark:bg-cyan-900/30' },
  { number: 4, icon: '📢', title: 'Talks', description: 'Bài nói đơn', questions: 30, bgColor: 'bg-teal-100 dark:bg-teal-900/30' },
  { number: 5, icon: '✍️', title: 'Incomplete Sentences', description: 'Điền vào chỗ trống', questions: 30, bgColor: 'bg-orange-100 dark:bg-orange-900/30' },
  { number: 6, icon: '📄', title: 'Text Completion', description: 'Hoàn thành đoạn văn', questions: 16, bgColor: 'bg-yellow-100 dark:bg-yellow-900/30' },
  { number: 7, icon: '📖', title: 'Reading Comprehension', description: 'Đọc hiểu', questions: 54, bgColor: 'bg-red-100 dark:bg-red-900/30' },
]
</script>
