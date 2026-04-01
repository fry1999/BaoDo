<template>
  <div class="space-y-6">
    <usehead>
      <title>Trang chủ</title>
    </usehead>

    <!-- Welcome section -->
    <div class="bg-gradient-to-r from-primary to-blue-700 rounded-2xl p-6 text-white">
      <p class="text-blue-100 text-sm mb-1">Chào mừng trở lại 👋</p>
      <h1 class="text-2xl font-bold mb-1">{{ authStore.user?.fullName ?? 'Học viên' }}</h1>
      <p class="text-blue-100 text-sm">Mục tiêu: <strong class="text-white">{{ authStore.user?.targetScore ?? 700 }}</strong> điểm TOEIC</p>
    </div>

    <!-- Stats row -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
      <div
        v-for="stat in stats"
        :key="stat.label"
        class="bg-white dark:bg-slate-900 rounded-xl p-4 shadow-card"
      >
        <p class="text-2xl mb-1">{{ stat.icon }}</p>
        <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ stat.value }}</p>
        <p class="text-xs text-gray-500">{{ stat.label }}</p>
      </div>
    </div>

    <!-- Quick actions -->
    <div>
      <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-3">Học ngay hôm nay</h2>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <NuxtLink
          v-for="action in quickActions"
          :key="action.to"
          :to="action.to"
          class="bg-white dark:bg-slate-900 rounded-xl p-5 shadow-card hover:shadow-card-hover transition-shadow border border-transparent hover:border-primary-light group"
        >
          <div class="text-3xl mb-3">{{ action.icon }}</div>
          <h3 class="font-semibold text-gray-900 dark:text-white group-hover:text-primary transition-colors">{{ action.title }}</h3>
          <p class="text-sm text-gray-500 mt-1">{{ action.description }}</p>
        </NuxtLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const authStore = useAuthStore()
const vocabStore = useVocabularyStore()

onMounted(() => vocabStore.fetchDueCards())

const stats = computed(() => [
  { icon: '🔥', value: authStore.user?.streakCount ?? 0, label: 'Ngày liên tiếp' },
  { icon: '📚', value: vocabStore.masteredCount, label: 'Từ đã thuộc' },
  { icon: '⚡', value: authStore.user?.xpTotal ?? 0, label: 'Điểm XP' },
  { icon: '🎯', value: vocabStore.dueCount, label: 'Từ cần ôn' },
])

const quickActions = [
  { to: '/vocabulary/study', icon: '📚', title: 'Ôn từ vựng', description: `${vocabStore.dueCount} từ đang chờ ôn lại` },
  { to: '/practice', icon: '🔍', title: 'Luyện tập', description: 'Luyện theo từng Part TOEIC' },
  { to: '/dictation', icon: '🎧', title: 'Dictation', description: 'Nghe và chép lại' },
  { to: '/grammar', icon: '📝', title: 'Ngữ pháp', description: 'Học và ôn ngữ pháp TOEIC' },
  { to: '/exam', icon: '📋', title: 'Thi thử', description: 'Đề thi đầy đủ 200 câu' },
]
</script>
