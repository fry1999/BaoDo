<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-white flex items-center justify-center p-4">
    <div class="w-full max-w-md">
      <div class="text-center mb-8">
        <div class="w-14 h-14 bg-primary rounded-2xl flex items-center justify-center mx-auto mb-3 shadow-lg">
          <span class="text-white font-bold text-2xl">B</span>
        </div>
        <h1 class="text-2xl font-bold text-gray-900">Tạo tài khoản BaoDo</h1>
        <p class="text-gray-500 text-sm mt-1">Bắt đầu hành trình TOEIC của bạn</p>
      </div>

      <form class="bg-white rounded-2xl shadow-card p-6 space-y-4" @submit.prevent="handleRegister">
        <div v-if="authStore.error" class="bg-red-50 text-red-600 text-sm px-4 py-3 rounded-xl">
          {{ authStore.error }}
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1.5">Họ và tên</label>
          <input
            v-model="form.fullName"
            type="text"
            required
            placeholder="Nguyễn Văn A"
            class="w-full h-10 px-3 border border-gray-300 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/20 transition-colors"
          />
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
            placeholder="Ít nhất 8 ký tự"
            minlength="8"
            class="w-full h-10 px-3 border border-gray-300 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/20 transition-colors"
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1.5">Điểm mục tiêu TOEIC</label>
          <select
            v-model="form.targetScore"
            class="w-full h-10 px-3 border border-gray-300 rounded-lg text-sm focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/20 bg-white"
          >
            <option v-for="score in targetScores" :key="score" :value="score">{{ score }} điểm</option>
          </select>
        </div>

        <button
          type="submit"
          :disabled="authStore.isLoading"
          class="w-full h-11 bg-primary text-white rounded-lg font-semibold text-sm hover:bg-primary-dark transition-colors disabled:opacity-60"
        >
          {{ authStore.isLoading ? 'Đang tạo tài khoản...' : 'Tạo tài khoản' }}
        </button>

        <p class="text-center text-sm text-gray-500">
          Đã có tài khoản?
          <NuxtLink to="/auth/login" class="text-primary font-medium hover:underline">Đăng nhập</NuxtLink>
        </p>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: false })

const authStore = useAuthStore()
const form = reactive({ fullName: '', email: '', password: '', targetScore: 700 })
const targetScores = [450, 550, 650, 700, 750, 800, 850, 900, 990]

async function handleRegister() {
  authStore.clearError()
  await authStore.register(form.email, form.password, form.fullName)
}

onMounted(() => authStore.clearError())
</script>
