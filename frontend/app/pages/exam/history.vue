<template>
  <div class="space-y-6">
    <useHead><title>Lịch sử thi</title></useHead>

    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Lịch sử thi</h1>
        <p class="text-sm text-gray-500 mt-0.5">Xem lại các bài thi đã làm</p>
      </div>
      <NuxtLink
        to="/exam"
        class="px-4 py-2 bg-primary text-white rounded-xl text-sm font-semibold hover:bg-primary-dark transition-colors"
      >
        Thi thêm
      </NuxtLink>
    </div>

    <div v-if="isLoading" class="flex justify-center py-12">
      <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
    </div>

    <div v-else-if="history.length === 0" class="bg-white dark:bg-slate-900 rounded-2xl shadow-card p-12 text-center border border-gray-100 dark:border-slate-800">
      <p class="text-4xl mb-3">📋</p>
      <p class="text-gray-500 font-medium">Bạn chưa làm bài thi nào</p>
      <NuxtLink to="/exam" class="mt-4 inline-block px-5 py-2.5 bg-primary text-white rounded-xl text-sm font-semibold hover:bg-primary-dark">
        Bắt đầu thi ngay
      </NuxtLink>
    </div>

    <div v-else class="space-y-3">
      <div
        v-for="item in history"
        :key="item.id"
        class="bg-white dark:bg-slate-900 rounded-xl shadow-card border border-gray-100 dark:border-slate-800 p-4 flex items-center gap-4 hover:border-primary-light transition-colors"
      >
        <!-- Score circle -->
        <div
          class="w-14 h-14 rounded-full flex-shrink-0 flex flex-col items-center justify-center text-white font-bold text-sm"
          :class="scoreClass(item.totalScore)"
        >
          <span class="text-lg leading-none">{{ item.totalScore }}</span>
        </div>

        <!-- Info -->
        <div class="flex-1 min-w-0">
          <p class="font-semibold text-gray-900 dark:text-white truncate">{{ item.testTitle }}</p>
          <div class="flex items-center gap-3 mt-1 text-xs text-gray-400">
            <span>{{ new Date(item.completedAt).toLocaleDateString('vi-VN', { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' }) }}</span>
          </div>
          <!-- Listening / Reading breakdown -->
          <div class="flex gap-4 mt-2 text-xs">
            <span class="flex items-center gap-1">
              <span class="text-blue-500">🎧</span>
              <span class="text-gray-600 dark:text-gray-400">Listening: <strong class="text-gray-900 dark:text-white">{{ item.listeningScaled }}</strong></span>
            </span>
            <span class="flex items-center gap-1">
              <span class="text-green-500">📖</span>
              <span class="text-gray-600 dark:text-gray-400">Reading: <strong class="text-gray-900 dark:text-white">{{ item.readingScaled }}</strong></span>
            </span>
          </div>
        </div>

        <!-- Action -->
        <NuxtLink
          :to="`/exam/result/${item.id}`"
          class="flex-shrink-0 px-3 py-2 border border-gray-200 dark:border-slate-700 rounded-xl text-xs font-medium text-gray-600 dark:text-gray-400 hover:border-primary hover:text-primary transition-colors"
        >
          Xem chi tiết →
        </NuxtLink>
      </div>

      <!-- Load more -->
      <div v-if="hasMore" class="text-center pt-2">
        <button
          :disabled="isLoadingMore"
          class="px-5 py-2.5 border border-gray-200 rounded-xl text-sm font-medium text-gray-600 hover:border-primary hover:text-primary disabled:opacity-50 transition-colors"
          @click="loadMore"
        >
          {{ isLoadingMore ? 'Đang tải...' : 'Tải thêm' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ExamHistoryItem } from '~/types'

definePageMeta({ middleware: 'auth' })

const config = useRuntimeConfig()
const auth = useAuthStore()

const history = ref<ExamHistoryItem[]>([])
const isLoading = ref(false)
const isLoadingMore = ref(false)
const currentPage = ref(1)
const hasMore = ref(false)
const PAGE_SIZE = 10

function scoreClass(score: number) {
  if (score >= 800) return 'bg-green-500'
  if (score >= 600) return 'bg-primary'
  if (score >= 400) return 'bg-yellow-500'
  return 'bg-red-500'
}

async function fetchHistory(page: number, append = false) {
  if (append) isLoadingMore.value = true
  else isLoading.value = true
  try {
    const data = await $fetch<ExamHistoryItem[]>(
      `${config.public.apiBaseUrl}/api/exam/history`,
      { headers: { Authorization: `Bearer ${auth.token}` }, query: { page, pageSize: PAGE_SIZE } },
    )
    if (append) history.value.push(...data)
    else history.value = data
    hasMore.value = data.length === PAGE_SIZE
  }
  finally {
    isLoading.value = false
    isLoadingMore.value = false
  }
}

async function loadMore() {
  currentPage.value++
  await fetchHistory(currentPage.value, true)
}

onMounted(() => fetchHistory(1))
</script>
