<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-white flex items-center justify-center p-4">
    <div class="w-full max-w-md">
      <!-- Logo -->
      <div class="text-center mb-8">
        <div class="w-14 h-14 bg-primary rounded-2xl flex items-center justify-center mx-auto mb-3 shadow-lg">
          <span class="text-white font-bold text-2xl">B</span>
        </div>
        <h1 class="text-2xl font-bold text-gray-900">Đăng nhập BaoDo</h1>
        <p class="text-gray-500 text-sm mt-1">Nền tảng luyện thi TOEIC thông minh</p>
      </div>

      <form class="bg-white rounded-2xl shadow-card p-6 space-y-4" @submit.prevent="handleLogin">
        <div v-if="authStore.error" class="bg-red-50 text-red-600 text-sm px-4 py-3 rounded-xl">
          {{ authStore.error }}
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1.5">Email</label>
          <input
            v-model="form.email"
            type="email"
            required
            placeholder="you@example.com"
            class="w-full h-10 px-3 border border-gray-300 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/20 transition-colors"
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1.5">Mật khẩu</label>
          <input
            v-model="form.password"
            type="password"
            required
            placeholder="••••••••"
            class="w-full h-10 px-3 border border-gray-300 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/20 transition-colors"
          />
        </div>

        <button
          type="submit"
          :disabled="authStore.isLoading"
          class="w-full h-11 bg-primary text-white rounded-lg font-semibold text-sm hover:bg-primary-dark transition-colors disabled:opacity-60"
        >
          {{ authStore.isLoading ? 'Đang đăng nhập...' : 'Đăng nhập' }}
        </button>

        <p class="text-center text-sm text-gray-500">
          Chưa có tài khoản?
          <NuxtLink to="/auth/register" class="text-primary font-medium hover:underline">Đăng ký</NuxtLink>
        </p>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: false })

const authStore = useAuthStore()
const route = useRoute()

const form = reactive({ email: '', password: '' })

async function handleLogin() {
  authStore.clearError()
  await authStore.login(form.email, form.password)
  const redirect = route.query.redirect as string
  if (redirect) await navigateTo(redirect)
}

onMounted(() => authStore.clearError())
</script>
