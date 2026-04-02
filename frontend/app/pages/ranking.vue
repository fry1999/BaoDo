<template>
  <div class="space-y-6">
    <useHead><title>Bảng Xếp Hạng</title></useHead>

    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Bảng Xếp Hạng</h1>
        <p class="text-sm text-gray-500 mt-0.5">So sánh thành tích với người học khác</p>
      </div>
      <span class="text-3xl">🏆</span>
    </div>

    <!-- Type tabs -->
    <div class="flex gap-2 bg-gray-100 dark:bg-slate-800 p-1 rounded-xl w-fit">
      <button
        v-for="tab in typeTabs"
        :key="tab.value"
        class="px-4 py-2 rounded-lg text-sm font-medium transition-all duration-150"
        :class="rankingStore.activeType === tab.value
          ? 'bg-white dark:bg-slate-700 text-gray-900 dark:text-white shadow-sm'
          : 'text-gray-500 dark:text-slate-400 hover:text-gray-700 dark:hover:text-white'"
        @click="switchType(tab.value)"
      >
        {{ tab.icon }} {{ tab.label }}
      </button>
    </div>

    <!-- Period chips (exam board only) -->
    <div v-if="rankingStore.activeType === 'exam'" class="flex gap-2">
      <button
        v-for="p in periods"
        :key="p.value"
        class="px-3 py-1.5 rounded-full text-xs font-medium border transition-all duration-150"
        :class="rankingStore.activePeriod === p.value
          ? 'bg-primary text-white border-primary'
          : 'bg-white dark:bg-slate-800 text-gray-600 dark:text-slate-300 border-gray-200 dark:border-slate-700 hover:border-primary hover:text-primary'"
        @click="rankingStore.switchPeriod(p.value)"
      >
        {{ p.label }}
      </button>
    </div>

    <!-- My rank banner (when logged in and ranked) -->
    <div
      v-if="authStore.isAuthenticated && rankingStore.myRank && rankingStore.myRank.rank > 0 && rankingStore.activeType === 'exam'"
      class="bg-primary/10 border border-primary/30 rounded-xl p-4 flex items-center gap-3"
    >
      <span class="text-2xl font-bold text-primary">#{{ rankingStore.myRank.rank }}</span>
      <div>
        <p class="text-sm font-semibold text-gray-900 dark:text-white">Thứ hạng của bạn</p>
        <p class="text-xs text-gray-500">Điểm cao nhất: {{ rankingStore.myRank.entry?.bestScore ?? 0 }}</p>
      </div>
    </div>

    <!-- Loading skeleton -->
    <div v-if="rankingStore.isLoading" class="space-y-3">
      <div
        v-for="i in 10"
        :key="i"
        class="bg-white dark:bg-slate-900 rounded-xl p-4 flex items-center gap-3 animate-pulse"
      >
        <div class="w-8 h-5 bg-gray-200 dark:bg-slate-700 rounded" />
        <div class="w-8 h-8 bg-gray-200 dark:bg-slate-700 rounded-full" />
        <div class="flex-1 space-y-2">
          <div class="h-4 w-32 bg-gray-200 dark:bg-slate-700 rounded" />
          <div class="h-3 w-20 bg-gray-200 dark:bg-slate-700 rounded" />
        </div>
        <div class="h-5 w-16 bg-gray-200 dark:bg-slate-700 rounded" />
      </div>
    </div>

    <!-- Error state -->
    <div
      v-else-if="rankingStore.error"
      class="bg-white dark:bg-slate-900 rounded-xl p-8 text-center border border-gray-100 dark:border-slate-800"
    >
      <p class="text-gray-400 text-sm">{{ rankingStore.error }}</p>
      <button class="mt-3 text-primary text-sm underline" @click="reload">Thử lại</button>
    </div>

    <!-- Empty state -->
    <div
      v-else-if="rankingStore.activeBoard.length === 0"
      class="bg-white dark:bg-slate-900 rounded-xl p-8 text-center border border-gray-100 dark:border-slate-800"
    >
      <span class="text-4xl">📊</span>
      <p class="mt-3 text-gray-500 text-sm">Chưa có dữ liệu xếp hạng</p>
    </div>

    <!-- Leaderboard table -->
    <div v-else class="bg-white dark:bg-slate-900 rounded-xl border border-gray-100 dark:border-slate-800 overflow-hidden">
      <div
        v-for="(entry, idx) in rankingStore.activeBoard"
        :key="entry.userId"
        class="flex items-center gap-3 px-4 py-3 transition-colors"
        :class="[
          idx < rankingStore.activeBoard.length - 1 ? 'border-b border-gray-50 dark:border-slate-800' : '',
          entry.userId === authStore.user?.id ? 'bg-primary/5 dark:bg-primary/10' : 'hover:bg-gray-50 dark:hover:bg-slate-800/50',
        ]"
      >
        <!-- Rank badge -->
        <div class="w-8 text-center flex-shrink-0">
          <span v-if="entry.rank === 1" class="text-xl">🥇</span>
          <span v-else-if="entry.rank === 2" class="text-xl">🥈</span>
          <span v-else-if="entry.rank === 3" class="text-xl">🥉</span>
          <span v-else class="text-sm font-bold text-gray-400">#{{ entry.rank }}</span>
        </div>

        <!-- Avatar -->
        <div
          class="w-9 h-9 rounded-full flex items-center justify-center text-white font-semibold text-sm flex-shrink-0"
          :style="{ backgroundColor: avatarColor(entry.fullName) }"
        >
          <img v-if="entry.avatarUrl" :src="entry.avatarUrl" :alt="entry.fullName" class="w-9 h-9 rounded-full object-cover">
          <span v-else>{{ entry.fullName.charAt(0).toUpperCase() }}</span>
        </div>

        <!-- Name & level -->
        <div class="flex-1 min-w-0">
          <div class="flex items-center gap-2">
            <p class="text-sm font-medium text-gray-900 dark:text-white truncate">
              {{ entry.fullName }}
              <span v-if="entry.userId === authStore.user?.id" class="text-xs text-primary font-normal ml-1">(bạn)</span>
            </p>
          </div>
          <div class="flex items-center gap-2 mt-0.5">
            <span class="text-xs text-gray-400">Lv.{{ entry.level }}</span>
            <span v-if="rankingStore.activeType === 'exam'" class="text-xs text-gray-400">· {{ entry.testsTaken }} bài thi</span>
          </div>
        </div>

        <!-- Score / XP -->
        <div class="text-right flex-shrink-0">
          <p v-if="rankingStore.activeType === 'exam'" class="text-sm font-bold text-gray-900 dark:text-white">
            {{ entry.bestScore }}
            <span class="text-xs font-normal text-gray-400">/990</span>
          </p>
          <p v-else class="text-sm font-bold text-primary">
            {{ entry.xpTotal.toLocaleString() }}
            <span class="text-xs font-normal text-gray-400">XP</span>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { RankingType } from '~/types'

definePageMeta({ middleware: 'auth' })

const rankingStore = useRankingStore()
const authStore = useAuthStore()

const typeTabs = [
  { value: 'exam' as RankingType, icon: '📋', label: 'Điểm Thi' },
  { value: 'xp' as RankingType, icon: '⭐', label: 'XP' },
]

const periods = [
  { value: 'weekly' as const, label: 'Tuần này' },
  { value: 'monthly' as const, label: 'Tháng này' },
  { value: 'all' as const, label: 'Tất cả' },
]

async function switchType(type: RankingType) {
  rankingStore.setActiveType(type)
  if (type === 'xp' && rankingStore.xpBoard.length === 0) {
    await rankingStore.fetchXpLeaderboard()
  }
  else if (type === 'exam' && rankingStore.examBoard.length === 0) {
    await rankingStore.fetchExamLeaderboard(rankingStore.activePeriod)
  }
}

async function reload() {
  if (rankingStore.activeType === 'exam') {
    await Promise.all([
      rankingStore.fetchExamLeaderboard(rankingStore.activePeriod),
      rankingStore.fetchMyRank(rankingStore.activePeriod),
    ])
  }
  else {
    await rankingStore.fetchXpLeaderboard()
  }
}

const avatarColors = [
  '#6366f1', '#8b5cf6', '#ec4899', '#f43f5e',
  '#f97316', '#eab308', '#22c55e', '#14b8a6', '#3b82f6',
]

function avatarColor(name: string): string {
  let hash = 0
  for (const ch of name) hash = (hash * 31 + ch.charCodeAt(0)) & 0xffffffff
  return avatarColors[Math.abs(hash) % avatarColors.length]
}

onMounted(async () => {
  await Promise.all([
    rankingStore.fetchExamLeaderboard(rankingStore.activePeriod),
    rankingStore.fetchMyRank(rankingStore.activePeriod),
  ])
})
</script>
