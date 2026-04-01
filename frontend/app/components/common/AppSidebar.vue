<template>
  <!-- Desktop sidebar -->
  <aside
    class="hidden lg:flex flex-col w-60 bg-white dark:bg-slate-900 border-r border-gray-200 dark:border-slate-800 fixed inset-y-0 left-0 z-40"
  >
    <!-- Logo -->
    <div class="h-16 flex items-center px-5 border-b border-gray-200 dark:border-slate-800">
      <NuxtLink to="/" class="flex items-center gap-2.5">
        <div class="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
          <span class="text-white font-bold text-sm">B</span>
        </div>
        <span class="text-xl font-bold text-gray-900 dark:text-white">BaoDo</span>
      </NuxtLink>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 px-3 py-4 space-y-0.5 overflow-y-auto custom-scrollbar">
      <div class="px-3 py-1.5 text-xs font-semibold text-gray-400 uppercase tracking-wider mb-1">Học tập</div>

      <NuxtLink
        v-for="item in mainNav"
        :key="item.to"
        :to="item.to"
        class="flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all duration-150"
        :class="isActive(item.to)
          ? 'bg-primary-light text-primary border-l-2 border-primary'
          : 'text-gray-600 dark:text-slate-400 hover:bg-gray-100 dark:hover:bg-slate-800 hover:text-gray-900 dark:hover:text-white'"
      >
        <span class="text-base w-5 text-center flex-shrink-0">{{ item.icon }}</span>
        <span>{{ item.label }}</span>
        <span
          v-if="item.badge"
          class="ml-auto text-xs bg-primary text-white px-1.5 py-0.5 rounded-full"
        >
          {{ item.badge }}
        </span>
      </NuxtLink>

      <div class="px-3 py-1.5 text-xs font-semibold text-gray-400 uppercase tracking-wider mt-4 mb-1">Thi thử</div>

      <NuxtLink
        v-for="item in examNav"
        :key="item.to"
        :to="item.to"
        class="flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all duration-150"
        :class="isActive(item.to)
          ? 'bg-primary-light text-primary border-l-2 border-primary'
          : 'text-gray-600 dark:text-slate-400 hover:bg-gray-100 dark:hover:bg-slate-800'"
      >
        <span class="text-base w-5 text-center flex-shrink-0">{{ item.icon }}</span>
        <span>{{ item.label }}</span>
      </NuxtLink>
    </nav>

    <!-- User profile -->
    <div class="p-3 border-t border-gray-200 dark:border-slate-800">
      <div class="flex items-center gap-3 p-2 rounded-xl hover:bg-gray-100 dark:hover:bg-slate-800 cursor-pointer">
        <div class="w-8 h-8 bg-primary rounded-full flex items-center justify-center text-white text-sm font-semibold flex-shrink-0">
          {{ authStore.userInitials }}
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-sm font-medium text-gray-900 dark:text-white truncate">{{ authStore.user?.fullName }}</p>
          <p class="text-xs text-gray-500 capitalize">{{ authStore.currentPlan }}</p>
        </div>
        <NuxtLink v-if="authStore.isAdmin" to="/admin" class="text-xs text-primary font-medium">Admin</NuxtLink>
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
defineOptions({ name: 'AppSidebar' })

const route = useRoute()
const authStore = useAuthStore()
const vocabStore = useVocabularyStore()

const mainNav = computed(() => [
  { to: '/', icon: '🏠', label: 'Trang chủ' },
  { to: '/vocabulary', icon: '📚', label: 'Từ vựng', badge: vocabStore.dueCount > 0 ? vocabStore.dueCount : undefined },
  { to: '/grammar', icon: '📝', label: 'Ngữ pháp' },
  { to: '/dictation', icon: '🎧', label: 'Dictation' },
  { to: '/practice', icon: '🔍', label: 'Luyện tập' },
])

const examNav = [
  { to: '/exam', icon: '📋', label: 'Thi thử' },
]

function isActive(path: string) {
  if (path === '/') return route.path === '/'
  return route.path.startsWith(path)
}
</script>
