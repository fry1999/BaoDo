<template>
  <div class="space-y-5">
    <useHead><title>Admin — Đề thi</title></useHead>

    <div class="flex items-center justify-between">
      <h1 class="text-xl font-bold text-gray-900 dark:text-white">Quản lý đề thi</h1>
      <button
        class="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-lg text-sm font-medium hover:bg-primary-dark transition-colors"
        @click="openCreate"
      >
        + Tạo đề thi
      </button>
    </div>

    <!-- Filters -->
    <div class="flex gap-3">
      <select
        v-model="filterPublished"
        class="h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary"
        @change="fetchTests(1)"
      >
        <option value="">Tất cả trạng thái</option>
        <option value="true">Đã đăng</option>
        <option value="false">Nháp</option>
      </select>
      <select
        v-model="filterType"
        class="h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary"
        @change="fetchTests(1)"
      >
        <option value="">Tất cả loại</option>
        <option value="full">Full (200 câu)</option>
        <option value="mini">Mini</option>
        <option value="part">Part</option>
      </select>
    </div>

    <!-- Table -->
    <div class="bg-white dark:bg-slate-900 rounded-xl shadow-card overflow-hidden">
      <div v-if="isLoading" class="flex justify-center py-12">
        <div class="w-6 h-6 border-2 border-primary border-t-transparent rounded-full animate-spin" />
      </div>

      <table v-else class="w-full text-sm">
        <thead class="border-b border-gray-100 dark:border-slate-800 bg-gray-50 dark:bg-slate-800/50">
          <tr>
            <th class="text-left px-4 py-3 font-medium text-gray-500">Tên đề thi</th>
            <th class="text-left px-4 py-3 font-medium text-gray-500 hidden md:table-cell w-24">Loại</th>
            <th class="text-left px-4 py-3 font-medium text-gray-500 hidden lg:table-cell w-28">Số câu / TG</th>
            <th class="text-left px-4 py-3 font-medium text-gray-500 hidden md:table-cell w-24">Độ khó</th>
            <th class="text-left px-4 py-3 font-medium text-gray-500 w-24">Trạng thái</th>
            <th class="text-right px-4 py-3 font-medium text-gray-500 w-32">Thao tác</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50 dark:divide-slate-800">
          <tr v-if="tests.length === 0">
            <td colspan="6" class="px-4 py-8 text-center text-gray-400">Chưa có đề thi nào</td>
          </tr>
          <tr
            v-for="test in tests"
            :key="test.id"
            class="hover:bg-gray-50 dark:hover:bg-slate-800/50"
          >
            <td class="px-4 py-3">
              <p class="font-medium text-gray-900 dark:text-white">{{ test.title }}</p>
              <p class="text-xs text-gray-400 mt-0.5">{{ new Date(test.createdAt).toLocaleDateString('vi-VN') }}</p>
            </td>
            <td class="px-4 py-3 hidden md:table-cell">
              <span
                class="text-xs px-2 py-0.5 rounded-full font-medium"
                :class="{
                  'bg-blue-100 text-blue-700': test.type === 'full',
                  'bg-purple-100 text-purple-700': test.type === 'mini',
                  'bg-orange-100 text-orange-700': test.type === 'part',
                }"
              >
                {{ test.type }}
              </span>
            </td>
            <td class="px-4 py-3 hidden lg:table-cell text-gray-500">
              {{ test.totalQuestions }} câu / {{ test.durationMinutes }}p
            </td>
            <td class="px-4 py-3 hidden md:table-cell">
              <span
                class="text-xs px-2 py-0.5 rounded-full"
                :class="{
                  'bg-green-100 text-green-700': test.difficulty === 'easy',
                  'bg-yellow-100 text-yellow-700': test.difficulty === 'medium',
                  'bg-red-100 text-red-700': test.difficulty === 'hard',
                }"
              >
                {{ diffLabel(test.difficulty) }}
              </span>
            </td>
            <td class="px-4 py-3">
              <button
                class="text-xs px-2 py-0.5 rounded-full font-medium transition-colors"
                :class="test.isPublished
                  ? 'bg-green-100 text-green-700 hover:bg-green-200'
                  : 'bg-gray-100 text-gray-500 hover:bg-gray-200'"
                @click="togglePublish(test)"
              >
                {{ test.isPublished ? 'Đã đăng' : 'Nháp' }}
              </button>
            </td>
            <td class="px-4 py-3 text-right">
              <div class="flex items-center justify-end gap-1">
                <NuxtLink
                  :to="`/admin/tests/${test.id}`"
                  class="text-xs text-indigo-500 hover:text-indigo-700 font-medium px-2 py-1 rounded hover:bg-indigo-50"
                >
                  Câu hỏi
                </NuxtLink>
                <button
                  class="text-xs text-primary hover:text-primary-dark font-medium px-2 py-1 rounded hover:bg-primary-light"
                  @click="openEdit(test)"
                >
                  Sửa
                </button>
                <button
                  class="text-xs text-red-500 hover:text-red-700 font-medium px-2 py-1 rounded hover:bg-red-50"
                  @click="deleteTest(test.id)"
                >
                  Xoá
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>

      <div v-if="totalPages > 1" class="flex items-center justify-between px-4 py-3 border-t border-gray-100 dark:border-slate-800">
        <p class="text-xs text-gray-500">{{ total }} đề thi</p>
        <div class="flex gap-1">
          <button
            v-for="p in totalPages"
            :key="p"
            class="w-7 h-7 text-xs rounded-lg font-medium"
            :class="p === currentPage ? 'bg-primary text-white' : 'text-gray-500 hover:bg-gray-100'"
            @click="fetchTests(p)"
          >
            {{ p }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal form -->
    <Teleport to="body">
      <Transition name="fade">
        <div v-if="showForm" class="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div class="absolute inset-0 bg-black/40" @click="closeForm" />
          <div class="relative w-full max-w-md bg-white dark:bg-slate-900 rounded-2xl shadow-xl">
            <div class="border-b border-gray-200 dark:border-slate-800 px-6 py-4 flex items-center justify-between">
              <h2 class="font-semibold text-gray-900 dark:text-white">
                {{ editingId ? 'Sửa đề thi' : 'Tạo đề thi mới' }}
              </h2>
              <button class="text-gray-400 hover:text-gray-600 text-xl" @click="closeForm">✕</button>
            </div>

            <form class="p-6 space-y-4" @submit.prevent="saveTest">
              <div>
                <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Tên đề thi *</label>
                <input v-model="form.title" required type="text" class="w-full h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary" />
              </div>

              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Loại *</label>
                  <select v-model="form.type" required class="w-full h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary">
                    <option value="full">Full (200 câu)</option>
                    <option value="mini">Mini</option>
                    <option value="part">Part</option>
                  </select>
                </div>
                <div>
                  <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Độ khó *</label>
                  <select v-model="form.difficulty" required class="w-full h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary">
                    <option value="easy">Dễ</option>
                    <option value="medium">Trung bình</option>
                    <option value="hard">Khó</option>
                  </select>
                </div>
              </div>

              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Số câu</label>
                  <input v-model.number="form.totalQuestions" type="number" min="1" max="200" class="w-full h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary" />
                </div>
                <div>
                  <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Thời gian (phút)</label>
                  <input v-model.number="form.durationMinutes" type="number" min="1" max="300" class="w-full h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary" />
                </div>
              </div>

              <div class="flex gap-3 pt-2">
                <button type="button" class="flex-1 h-10 border border-gray-300 rounded-lg text-sm" @click="closeForm">Huỷ</button>
                <button type="submit" :disabled="isSaving" class="flex-1 h-10 bg-primary text-white rounded-lg text-sm font-semibold disabled:opacity-50">
                  {{ isSaving ? 'Đang lưu...' : 'Lưu' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })

const config = useRuntimeConfig()
const auth = useAuthStore()
const headers = computed(() => ({ Authorization: `Bearer ${auth.token}` }))

interface AdminTest {
  id: string; title: string; type: string; totalQuestions: number
  durationMinutes: number; difficulty: string; isPublished: boolean; createdAt: string
}

const tests = ref<AdminTest[]>([])
const isLoading = ref(false)
const isSaving = ref(false)
const total = ref(0)
const currentPage = ref(1)
const totalPages = ref(1)
const filterPublished = ref('')
const filterType = ref('')

const showForm = ref(false)
const editingId = ref<string | null>(null)
const form = reactive({ title: '', type: 'full', difficulty: 'medium', totalQuestions: 200, durationMinutes: 120 })

async function fetchTests(page = 1) {
  isLoading.value = true
  currentPage.value = page
  try {
    const params: Record<string, string | number> = { page, pageSize: 20 }
    if (filterPublished.value) params.published = filterPublished.value
    if (filterType.value) params.type = filterType.value
    const res = await $fetch<{ data: AdminTest[]; total: number; totalPages: number }>(
      `${config.public.apiBaseUrl}/api/admin/tests`,
      { headers: headers.value, query: params },
    )
    tests.value = res.data
    total.value = res.total
    totalPages.value = res.totalPages
  }
  finally { isLoading.value = false }
}

const diffLabel = (d: string) => ({ easy: 'Dễ', medium: 'TB', hard: 'Khó' }[d] ?? d)

function openCreate() {
  editingId.value = null
  Object.assign(form, { title: '', type: 'full', difficulty: 'medium', totalQuestions: 200, durationMinutes: 120 })
  showForm.value = true
}

function openEdit(t: AdminTest) {
  editingId.value = t.id
  Object.assign(form, { title: t.title, type: t.type, difficulty: t.difficulty, totalQuestions: t.totalQuestions, durationMinutes: t.durationMinutes })
  showForm.value = true
}

function closeForm() { showForm.value = false }

async function saveTest() {
  isSaving.value = true
  try {
    const body = { ...form }
    if (editingId.value) {
      await $fetch(`${config.public.apiBaseUrl}/api/admin/tests/${editingId.value}`, { method: 'PUT', headers: headers.value, body })
    }
    else {
      await $fetch(`${config.public.apiBaseUrl}/api/admin/tests`, { method: 'POST', headers: headers.value, body })
    }
    closeForm()
    await fetchTests(currentPage.value)
  }
  finally { isSaving.value = false }
}

async function togglePublish(test: AdminTest) {
  await $fetch(`${config.public.apiBaseUrl}/api/admin/tests/${test.id}/publish`, { method: 'PATCH', headers: headers.value })
  test.isPublished = !test.isPublished
}

async function deleteTest(id: string) {
  if (!confirm('Xoá đề thi này?')) return
  await $fetch(`${config.public.apiBaseUrl}/api/admin/tests/${id}`, { method: 'DELETE', headers: headers.value })
  await fetchTests(currentPage.value)
}

onMounted(() => fetchTests())
</script>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
