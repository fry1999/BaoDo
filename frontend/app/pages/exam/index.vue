<template>
  <div class="space-y-6">
    <useHead><title>Thi thử</title></useHead>

    <div>
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Thi thử TOEIC</h1>
      <p class="text-sm text-gray-500 mt-0.5">Đề thi đầy đủ 200 câu, thời gian 120 phút</p>
    </div>

    <div v-if="isLoading" class="flex justify-center py-12">
      <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <div
        v-for="test in tests"
        :key="test.id"
        class="bg-white dark:bg-slate-900 rounded-xl p-5 shadow-card border border-gray-100 dark:border-slate-800"
      >
        <div class="flex items-start justify-between mb-3">
          <span class="text-2xl">📋</span>
          <span
            class="text-xs px-2 py-0.5 rounded-full font-medium"
            :class="{
              'bg-green-100 text-green-700': test.difficulty === 'easy',
              'bg-yellow-100 text-yellow-700': test.difficulty === 'medium',
              'bg-red-100 text-red-700': test.difficulty === 'hard',
            }"
          >
            {{ test.difficulty }}
          </span>
        </div>
        <h3 class="font-semibold text-gray-900 dark:text-white">{{ test.title }}</h3>
        <div class="flex gap-4 mt-2 text-xs text-gray-400">
          <span>{{ test.totalQuestions }} câu</span>
          <span>{{ test.durationMinutes }} phút</span>
        </div>
        <NuxtLink
          :to="`/exam/${test.id}`"
          class="mt-4 w-full block text-center px-4 py-2.5 bg-primary text-white rounded-xl text-sm font-semibold hover:bg-primary-dark transition-colors"
        >
          Bắt đầu thi
        </NuxtLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Test } from '~/types'

definePageMeta({ middleware: 'auth' })

const config = useRuntimeConfig()
const auth = useAuthStore()
const tests = ref<Test[]>([])
const isLoading = ref(false)

onMounted(async () => {
  isLoading.value = true
  try {
    tests.value = await $fetch<Test[]>(`${config.public.apiBaseUrl}/api/exam`, {
      headers: { Authorization: `Bearer ${auth.token}` },
    })
  } finally {
    isLoading.value = false
  }
})
</script>
