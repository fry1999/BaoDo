<template>
  <div class="max-w-3xl mx-auto space-y-6 py-6 px-4">
    <useHead><title>Kết quả thi</title></useHead>

    <div v-if="isLoading" class="flex justify-center py-24">
      <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
    </div>

    <template v-else-if="result">
      <!-- Score hero -->
      <div class="bg-gradient-to-r from-primary to-blue-700 rounded-2xl p-6 text-white text-center">
        <p class="text-sm text-blue-200 mb-2">Điểm TOEIC của bạn</p>
        <p class="text-6xl font-bold mb-3 animate-score-count">{{ result.totalScore }}</p>
        <div class="flex justify-center gap-8 text-sm">
          <div>
            <p class="text-blue-200">Listening</p>
            <p class="font-bold text-xl">{{ result.listeningScaled }}</p>
          </div>
          <div class="w-px bg-white/30" />
          <div>
            <p class="text-blue-200">Reading</p>
            <p class="font-bold text-xl">{{ result.readingScaled }}</p>
          </div>
        </div>
      </div>

      <!-- Part breakdown -->
      <div class="bg-white dark:bg-slate-900 rounded-2xl shadow-card p-6">
        <h2 class="font-semibold text-gray-900 dark:text-white mb-4">Kết quả theo Part</h2>
        <div class="space-y-3">
          <div v-for="ps in result.partBreakdown" :key="ps.part" class="flex items-center gap-3">
            <span class="text-sm text-gray-600 dark:text-gray-400 w-16">Part {{ ps.part }}</span>
            <div class="flex-1 h-2.5 bg-gray-100 dark:bg-slate-800 rounded-full overflow-hidden">
              <div
                class="h-full rounded-full transition-all duration-700"
                :class="{
                  'bg-green-500': ps.percentage >= 80,
                  'bg-yellow-500': ps.percentage >= 60,
                  'bg-red-500': ps.percentage < 60,
                }"
                :style="{ width: `${ps.percentage}%` }"
              />
            </div>
            <span class="text-sm font-medium text-gray-900 dark:text-white w-20 text-right">
              {{ ps.correct }}/{{ ps.total }} ({{ ps.percentage }}%)
            </span>
          </div>
        </div>
      </div>

      <!-- AI Analysis -->
      <div v-if="result.aiAnalysis" class="bg-white dark:bg-slate-900 rounded-2xl shadow-card p-6">
        <div class="flex items-center gap-2 mb-4">
          <span class="text-xl">🤖</span>
          <h2 class="font-semibold text-gray-900 dark:text-white">Phân tích AI & Lộ trình ôn tập</h2>
        </div>

        <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">{{ result.aiAnalysis.summary }}</p>

        <!-- Weak points -->
        <div class="space-y-2 mb-5">
          <h3 class="text-sm font-semibold text-gray-700 dark:text-gray-300">Điểm cần cải thiện</h3>
          <div
            v-for="wp in result.aiAnalysis.weakParts"
            :key="wp.part"
            class="flex items-start gap-3 p-3 rounded-xl"
            :class="{
              'bg-red-50 dark:bg-red-900/20': wp.priorityLevel === 'high',
              'bg-yellow-50 dark:bg-yellow-900/20': wp.priorityLevel === 'medium',
              'bg-blue-50 dark:bg-blue-900/20': wp.priorityLevel === 'low',
            }"
          >
            <span class="text-sm font-medium w-16 flex-shrink-0">Part {{ wp.part }}</span>
            <p class="text-sm text-gray-600 dark:text-gray-400">{{ wp.recommendation }}</p>
          </div>
        </div>

        <!-- Study plan -->
        <div>
          <h3 class="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-2">
            Lộ trình {{ result.aiAnalysis.studyPlan.length }} tuần
          </h3>
          <div class="space-y-2">
            <div
              v-for="week in result.aiAnalysis.studyPlan"
              :key="week.week"
              class="border border-gray-100 dark:border-slate-800 rounded-xl p-4"
            >
              <p class="text-sm font-semibold text-gray-900 dark:text-white mb-1">Tuần {{ week.week }}: {{ week.focus }}</p>
              <ul class="text-sm text-gray-500 space-y-0.5">
                <li v-for="task in week.tasks" :key="task" class="flex items-center gap-2">
                  <span class="w-1 h-1 bg-gray-400 rounded-full flex-shrink-0" />
                  {{ task }}
                </li>
              </ul>
            </div>
          </div>
        </div>

        <div class="mt-4 p-4 bg-primary-light rounded-xl text-center">
          <p class="text-sm text-primary font-medium">
            Xác suất đạt {{ authStore.user?.targetScore }} điểm: <strong>{{ result.aiAnalysis.successProbability }}%</strong>
          </p>
        </div>
      </div>

      <!-- AI loading -->
      <div v-else class="bg-white dark:bg-slate-900 rounded-2xl shadow-card p-6 text-center">
        <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin mx-auto mb-3" />
        <p class="text-sm text-gray-500">Đang phân tích kết quả bằng AI...</p>
      </div>

      <!-- Actions -->
      <div class="flex gap-3">
        <NuxtLink to="/exam" class="flex-1 text-center px-4 py-3 border border-gray-200 rounded-xl text-sm font-medium hover:bg-gray-50">
          Thi lại
        </NuxtLink>
        <NuxtLink to="/" class="flex-1 text-center px-4 py-3 bg-primary text-white rounded-xl text-sm font-semibold hover:bg-primary-dark">
          Về trang chủ
        </NuxtLink>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import type { UserTestResult } from '~/types'

definePageMeta({ middleware: 'auth' })

const route = useRoute()
const authStore = useAuthStore()
const config = useRuntimeConfig()
const auth = useAuthStore()

const result = ref<UserTestResult | null>(null)
const isLoading = ref(false)

onMounted(async () => {
  isLoading.value = true
  try {
    result.value = await $fetch<UserTestResult>(
      `${config.public.apiBaseUrl}/api/exam/results/${route.params.id}`,
      { headers: { Authorization: `Bearer ${auth.token}` } },
    )
  } finally {
    isLoading.value = false
  }
})
</script>
