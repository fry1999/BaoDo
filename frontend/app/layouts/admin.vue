<template>
  <div class="min-h-screen bg-gray-100 dark:bg-slate-950 flex">
    <!-- Admin Sidebar -->
    <aside class="w-64 bg-white dark:bg-slate-900 border-r border-gray-200 dark:border-slate-800 fixed inset-y-0 left-0 z-40 flex flex-col">
      <!-- Logo -->
      <div class="h-16 flex items-center px-6 border-b border-gray-200 dark:border-slate-800">
        <NuxtLink to="/" class="flex items-center gap-2">
          <span class="text-xl font-bold text-primary">BaoDo</span>
          <span class="text-xs bg-red-100 text-red-700 px-1.5 py-0.5 rounded font-medium">Admin</span>
        </NuxtLink>
      </div>

      <!-- Nav -->
      <nav class="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
        <NuxtLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors"
          :class="$route.path.startsWith(item.to)
            ? 'bg-primary-light text-primary'
            : 'text-gray-600 dark:text-slate-400 hover:bg-gray-100 dark:hover:bg-slate-800'"
        >
          <span class="text-base">{{ item.icon }}</span>
          {{ item.label }}
        </NuxtLink>
      </nav>

      <!-- Back to app -->
      <div class="p-3 border-t border-gray-200 dark:border-slate-800">
        <NuxtLink
          to="/"
          class="flex items-center gap-2 px-3 py-2 text-sm text-gray-500 hover:text-gray-700 rounded-lg hover:bg-gray-100"
        >
          ← Về trang học
        </NuxtLink>
      </div>
    </aside>

    <!-- Content -->
    <div class="flex-1 ml-64 flex flex-col min-h-screen">
      <header class="h-16 bg-white dark:bg-slate-900 border-b border-gray-200 dark:border-slate-800 flex items-center px-6">
        <h1 class="text-lg font-semibold text-gray-900 dark:text-white">Admin Panel</h1>
      </header>
      <main class="flex-1 p-6">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
defineOptions({ name: 'AdminLayout' })

const navItems = [
  { to: '/admin', icon: '📊', label: 'Dashboard' },
  { to: '/admin/users', icon: '👥', label: 'Người dùng' },
  { to: '/admin/questions', icon: '❓', label: 'Ngân hàng câu hỏi' },
  { to: '/admin/vocabulary', icon: '📚', label: 'Từ vựng' },
  { to: '/admin/grammar', icon: '📝', label: 'Ngữ pháp' },
  { to: '/admin/tests', icon: '📋', label: 'Đề thi' },
  { to: '/admin/dictation', icon: '🎧', label: 'Dictation' },
]
</script>
