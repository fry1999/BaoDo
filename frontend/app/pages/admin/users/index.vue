<template>
  <div class="space-y-5">
    <useHead><title>Admin — Người dùng</title></useHead>

    <div class="flex items-center justify-between">
      <h1 class="text-xl font-bold text-gray-900 dark:text-white">Quản lý người dùng</h1>
    </div>

    <!-- Search -->
    <div class="flex gap-3">
      <input
        v-model="search"
        type="text"
        placeholder="Tìm theo email, tên..."
        class="flex-1 h-10 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/20 dark:bg-slate-800 dark:text-white"
        @input="debouncedSearch"
      />
    </div>

    <!-- Table -->
    <div class="bg-white dark:bg-slate-900 rounded-xl shadow-card overflow-hidden">
      <div v-if="adminStore.isLoading" class="flex justify-center py-12">
        <div class="w-6 h-6 border-2 border-primary border-t-transparent rounded-full animate-spin" />
      </div>

      <table v-else class="w-full text-sm">
        <thead class="border-b border-gray-100 dark:border-slate-800">
          <tr>
            <th class="text-left px-4 py-3 font-medium text-gray-500 dark:text-gray-400">Người dùng</th>
            <th class="text-left px-4 py-3 font-medium text-gray-500 dark:text-gray-400 hidden md:table-cell">Subscription</th>
            <th class="text-left px-4 py-3 font-medium text-gray-500 dark:text-gray-400 hidden lg:table-cell">Ngày tham gia</th>
            <th class="text-right px-4 py-3 font-medium text-gray-500 dark:text-gray-400">Thao tác</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50 dark:divide-slate-800">
          <tr v-for="user in adminStore.users?.data" :key="user.id" class="hover:bg-gray-50 dark:hover:bg-slate-800/50">
            <td class="px-4 py-3">
              <div class="flex items-center gap-3">
                <div class="w-8 h-8 bg-primary rounded-full flex items-center justify-center text-white text-xs font-semibold">
                  {{ user.fullName?.[0]?.toUpperCase() }}
                </div>
                <div>
                  <p class="font-medium text-gray-900 dark:text-white">{{ user.fullName }}</p>
                  <p class="text-xs text-gray-500">{{ user.email }}</p>
                </div>
              </div>
            </td>
            <td class="px-4 py-3 hidden md:table-cell">
              <span
                class="px-2 py-0.5 rounded-full text-xs font-medium"
                :class="{
                  'bg-gray-100 text-gray-600': user.subscription === 'free',
                  'bg-blue-100 text-blue-700': user.subscription === 'basic',
                  'bg-purple-100 text-purple-700': user.subscription === 'pro',
                }"
              >
                {{ user.subscription }}
              </span>
            </td>
            <td class="px-4 py-3 text-gray-500 text-xs hidden lg:table-cell">
              {{ new Date(user.createdAt).toLocaleDateString('vi-VN') }}
            </td>
            <td class="px-4 py-3 text-right">
              <button
                class="text-xs text-red-500 hover:text-red-700 font-medium px-2 py-1 rounded hover:bg-red-50"
                @click="adminStore.banUser(user.id)"
              >
                Ban
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Pagination -->
      <div v-if="adminStore.users && adminStore.users.totalPages > 1" class="flex items-center justify-between px-4 py-3 border-t border-gray-100 dark:border-slate-800">
        <p class="text-xs text-gray-500">
          {{ adminStore.users.total }} người dùng
        </p>
        <div class="flex gap-1">
          <button
            v-for="p in adminStore.users.totalPages"
            :key="p"
            class="w-7 h-7 text-xs rounded-lg font-medium transition-colors"
            :class="p === adminStore.usersPage ? 'bg-primary text-white' : 'text-gray-500 hover:bg-gray-100'"
            @click="adminStore.fetchUsers(p, search)"
          >
            {{ p }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useDebounceFn } from '@vueuse/core'

definePageMeta({ layout: 'admin', middleware: 'admin' })

const adminStore = useAdminStore()
const search = ref('')

onMounted(() => adminStore.fetchUsers())

const debouncedSearch = useDebounceFn(() => {
  adminStore.fetchUsers(1, search.value)
}, 400)
</script>
