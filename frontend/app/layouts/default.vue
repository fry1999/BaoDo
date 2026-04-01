<template>
  <div class="min-h-screen bg-gray-50 dark:bg-slate-950 flex">
    <!-- Sidebar -->
    <AppSidebar />

    <!-- Main content -->
    <div class="flex-1 flex flex-col min-w-0 lg:ml-60">
      <!-- Top bar (mobile) -->
      <AppHeader />

      <!-- Page content -->
      <main class="flex-1 p-4 md:p-6 lg:p-8">
        <slot />
      </main>
    </div>

    <!-- Inline Dictionary Popup -->
    <DictionaryPopup />
  </div>
</template>

<script setup lang="ts">
defineOptions({ name: 'DefaultLayout' })
</script>
