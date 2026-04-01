<template>
  <div class="space-y-6">
    <useHead><title>Admin Dashboard</title></useHead>

    <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Dashboard</h1>

    <div v-if="adminStore.isLoading" class="flex justify-center py-12">
      <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
    </div>

    <template v-else-if="adminStore.stats">
      <!-- KPI cards -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <div v-for="kpi in kpis" :key="kpi.label" class="bg-white dark:bg-slate-900 rounded-xl p-4 shadow-card">
          <p class="text-2xl mb-1">{{ kpi.icon }}</p>
          <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ kpi.value }}</p>
          <p class="text-xs text-gray-500">{{ kpi.label }}</p>
        </div>
      </div>

      <!-- Subscription breakdown -->
      <div class="bg-white dark:bg-slate-900 rounded-xl p-5 shadow-card">
        <h2 class="font-semibold text-gray-900 dark:text-white mb-4">Phân bổ Subscription</h2>
        <div class="space-y-3">
          <div v-for="plan in plans" :key="plan.key" class="flex items-center gap-3">
            <span class="text-sm text-gray-600 dark:text-gray-400 w-14">{{ plan.label }}</span>
            <div class="flex-1 h-2.5 bg-gray-100 dark:bg-slate-800 rounded-full overflow-hidden">
              <div
                class="h-full rounded-full"
                :class="plan.color"
                :style="{ width: `${planPercent(plan.key)}%` }"
              />
            </div>
            <span class="text-sm font-medium text-gray-900 dark:text-white w-12 text-right">
              {{ adminStore.stats!.subscriptionCount[plan.key] }}
            </span>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import type { SubscriptionPlan } from '~/types'

definePageMeta({ layout: 'admin', middleware: 'admin' })

const adminStore = useAdminStore()
onMounted(() => adminStore.fetchStats())

const kpis = computed(() => [
  { icon: '👥', value: adminStore.stats?.totalUsers ?? 0, label: 'Tổng người dùng' },
  { icon: '🆕', value: adminStore.stats?.newUsersToday ?? 0, label: 'Mới hôm nay' },
  { icon: '💰', value: `${((adminStore.stats?.revenueThisMonth ?? 0) / 1_000_000).toFixed(1)}M`, label: 'Doanh thu tháng' },
  { icon: '📝', value: adminStore.stats?.testsCompletedToday ?? 0, label: 'Bài thi hôm nay' },
])

const plans: Array<{ key: SubscriptionPlan; label: string; color: string }> = [
  { key: 'free', label: 'Free', color: 'bg-gray-400' },
  { key: 'basic', label: 'Basic', color: 'bg-blue-400' },
  { key: 'pro', label: 'Pro', color: 'bg-purple-500' },
]

function planPercent(plan: SubscriptionPlan) {
  const total = adminStore.stats?.totalUsers ?? 1
  const count = adminStore.stats?.subscriptionCount[plan] ?? 0
  return Math.round((count / total) * 100)
}
</script>
