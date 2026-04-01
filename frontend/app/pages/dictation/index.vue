<template>
  <div class="space-y-6">
    <useHead><title>Dictation</title></useHead>

    <div>
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Luyện Dictation</h1>
      <p class="text-sm text-gray-500 mt-0.5">Nghe và chép lại — cải thiện kỹ năng nghe chi tiết</p>
    </div>

    <div v-if="dictationStore.isLoading" class="flex justify-center py-12">
      <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <NuxtLink
        v-for="item in dictationStore.library"
        :key="item.id"
        :to="`/dictation/${item.id}`"
        class="bg-white dark:bg-slate-900 rounded-xl p-5 shadow-card hover:shadow-card-hover transition-all border border-transparent hover:border-primary-light group"
      >
        <div class="flex items-start justify-between mb-3">
          <span class="text-2xl">{{ item.source === 'youtube' ? '▶️' : '🎧' }}</span>
          <span
            class="text-xs px-2 py-0.5 rounded-full font-medium"
            :class="{
              'bg-green-100 text-green-700': item.level === 'beginner',
              'bg-yellow-100 text-yellow-700': item.level === 'intermediate',
              'bg-red-100 text-red-700': item.level === 'advanced',
            }"
          >
            {{ item.level }}
          </span>
        </div>
        <h3 class="font-semibold text-gray-900 dark:text-white group-hover:text-primary transition-colors text-sm">{{ item.title }}</h3>
        <p class="text-xs text-gray-400 mt-1">{{ item.topic }}</p>
        <div class="flex items-center justify-between mt-3">
          <span class="text-xs text-gray-400">{{ item.segments.length }} đoạn</span>
          <span class="text-xs text-gray-400">{{ Math.round(item.duration / 60) }} phút</span>
        </div>
      </NuxtLink>

      <!-- Empty state -->
      <div v-if="dictationStore.library.length === 0" class="col-span-full text-center py-12 text-gray-400">
        <p class="text-4xl mb-3">🎧</p>
        <p>Chưa có bài dictation nào</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'auth' })
const dictationStore = useDictationStore()
onMounted(() => dictationStore.fetchLibrary())
</script>
