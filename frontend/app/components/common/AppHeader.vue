<template>
  <!-- Mobile top bar -->
  <header class="lg:hidden h-14 bg-white dark:bg-slate-900 border-b border-gray-200 dark:border-slate-800 flex items-center justify-between px-4 sticky top-0 z-30">
    <NuxtLink to="/" class="flex items-center gap-2">
      <div class="w-7 h-7 bg-primary rounded-lg flex items-center justify-center">
        <span class="text-white font-bold text-xs">B</span>
      </div>
      <span class="font-bold text-gray-900 dark:text-white">BaoDo</span>
    </NuxtLink>

    <div class="flex items-center gap-2">
      <!-- Streak indicator -->
      <div class="flex items-center gap-1 bg-orange-50 dark:bg-orange-900/20 text-streak text-sm font-semibold px-2 py-1 rounded-lg">
        🔥 {{ authStore.user?.streakCount ?? 0 }}
      </div>

      <!-- XP -->
      <div class="flex items-center gap-1 bg-yellow-50 dark:bg-yellow-900/20 text-xp text-sm font-semibold px-2 py-1 rounded-lg">
        ⚡ {{ authStore.user?.xpTotal ?? 0 }}
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
defineOptions({ name: 'AppHeader' })
const authStore = useAuthStore()
</script>
