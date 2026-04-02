<template>
  <div class="space-y-5">
    <useHead><title>Admin — Passages</title></useHead>

    <div class="flex items-center justify-between">
      <h1 class="text-xl font-bold text-gray-900 dark:text-white">Quản lý Passage</h1>
      <button
        class="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-lg text-sm font-medium hover:bg-primary-dark transition-colors"
        @click="openCreate"
      >
        + Thêm passage
      </button>
    </div>

    <!-- Filter -->
    <div class="flex gap-3">
      <select
        v-model="filterPart"
        class="h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary"
        @change="fetchPassages(1)"
      >
        <option value="">Tất cả Part</option>
        <option value="6">Part 6</option>
        <option value="7">Part 7</option>
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
            <th class="text-left px-4 py-3 font-medium text-gray-500 w-20">Part</th>
            <th class="text-left px-4 py-3 font-medium text-gray-500 w-24">Loại</th>
            <th class="text-left px-4 py-3 font-medium text-gray-500">Tiêu đề / Nội dung</th>
            <th class="text-right px-4 py-3 font-medium text-gray-500 w-28">Thao tác</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50 dark:divide-slate-800">
          <tr v-if="passages.length === 0">
            <td colspan="4" class="px-4 py-8 text-center text-gray-400">Không có passage nào</td>
          </tr>
          <tr
            v-for="p in passages"
            :key="p.id"
            class="hover:bg-gray-50 dark:hover:bg-slate-800/50"
          >
            <td class="px-4 py-3">
              <span class="text-xs bg-gray-100 dark:bg-slate-800 px-2 py-1 rounded font-mono">Part {{ p.part }}</span>
            </td>
            <td class="px-4 py-3">
              <span class="text-xs text-gray-500">{{ p.passageType }}</span>
            </td>
            <td class="px-4 py-3 max-w-md">
              <p class="font-medium text-gray-900 dark:text-white truncate">{{ p.title || '(no title)' }}</p>
              <p class="text-xs text-gray-400 truncate mt-0.5">{{ p.content.substring(0, 80) }}...</p>
            </td>
            <td class="px-4 py-3 text-right">
              <div class="flex items-center justify-end gap-1">
                <button
                  class="text-xs text-primary hover:text-primary-dark font-medium px-2 py-1 rounded hover:bg-primary-light"
                  @click="openEdit(p)"
                >
                  Sửa
                </button>
                <button
                  class="text-xs text-red-500 hover:text-red-700 font-medium px-2 py-1 rounded hover:bg-red-50"
                  @click="deletePassage(p.id)"
                >
                  Xoá
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="flex items-center justify-between px-4 py-3 border-t border-gray-100 dark:border-slate-800">
        <p class="text-xs text-gray-500">{{ total }} passages</p>
        <div class="flex gap-1">
          <button
            v-for="p in totalPages"
            :key="p"
            class="w-7 h-7 text-xs rounded-lg font-medium"
            :class="p === currentPage ? 'bg-primary text-white' : 'text-gray-500 hover:bg-gray-100'"
            @click="fetchPassages(p)"
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
          <div class="relative w-full max-w-2xl bg-white dark:bg-slate-900 rounded-2xl shadow-xl max-h-[90vh] overflow-y-auto">
            <div class="sticky top-0 bg-white dark:bg-slate-900 border-b border-gray-200 dark:border-slate-800 px-6 py-4 flex items-center justify-between">
              <h2 class="font-semibold text-gray-900 dark:text-white">
                {{ editingId ? 'Sửa passage' : 'Thêm passage' }}
              </h2>
              <button class="text-gray-400 hover:text-gray-600 text-xl" @click="closeForm">✕</button>
            </div>

            <form class="p-6 space-y-4" @submit.prevent="savePassage">
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Part *</label>
                  <select v-model="form.part" required class="w-full h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary">
                    <option value="6">Part 6</option>
                    <option value="7">Part 7</option>
                  </select>
                </div>
                <div>
                  <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Loại passage *</label>
                  <select v-model="form.passageType" required class="w-full h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary">
                    <option value="single">Single</option>
                    <option value="double">Double</option>
                    <option value="triple">Triple</option>
                  </select>
                </div>
              </div>

              <div>
                <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Tiêu đề</label>
                <input v-model="form.title" type="text" class="w-full h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary" />
              </div>

              <div>
                <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Nội dung *</label>
                <textarea
                  v-model="form.content"
                  rows="8"
                  required
                  class="w-full px-3 py-2 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary resize-none"
                />
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

interface Passage { id: string; part: number; passageType: string; title?: string; content: string }

const passages = ref<Passage[]>([])
const isLoading = ref(false)
const isSaving = ref(false)
const total = ref(0)
const currentPage = ref(1)
const totalPages = ref(1)
const filterPart = ref('')

const showForm = ref(false)
const editingId = ref<string | null>(null)
const form = reactive({ part: 6, passageType: 'single', title: '', content: '' })

async function fetchPassages(page = 1) {
  isLoading.value = true
  currentPage.value = page
  try {
    const params: Record<string, string | number> = { page, pageSize: 20 }
    if (filterPart.value) params.part = filterPart.value
    const res = await $fetch<{ data: Passage[]; total: number; totalPages: number }>(
      `${config.public.apiBaseUrl}/api/admin/passages`,
      { headers: headers.value, query: params },
    )
    passages.value = res.data
    total.value = res.total
    totalPages.value = res.totalPages
  }
  finally { isLoading.value = false }
}

function openCreate() {
  editingId.value = null
  form.part = 6; form.passageType = 'single'; form.title = ''; form.content = ''
  showForm.value = true
}

function openEdit(p: Passage) {
  editingId.value = p.id
  form.part = p.part; form.passageType = p.passageType
  form.title = p.title ?? ''; form.content = p.content
  showForm.value = true
}

function closeForm() { showForm.value = false }

async function savePassage() {
  isSaving.value = true
  try {
    const body = { part: form.part, passageType: form.passageType, title: form.title || null, content: form.content }
    if (editingId.value) {
      await $fetch(`${config.public.apiBaseUrl}/api/admin/passages/${editingId.value}`, { method: 'PUT', headers: headers.value, body })
    }
    else {
      await $fetch(`${config.public.apiBaseUrl}/api/admin/passages`, { method: 'POST', headers: headers.value, body })
    }
    closeForm()
    await fetchPassages(currentPage.value)
  }
  finally { isSaving.value = false }
}

async function deletePassage(id: string) {
  if (!confirm('Xoá passage này? Các câu hỏi liên kết sẽ mất passage.')) return
  await $fetch(`${config.public.apiBaseUrl}/api/admin/passages/${id}`, { method: 'DELETE', headers: headers.value })
  await fetchPassages(currentPage.value)
}

onMounted(() => fetchPassages())
</script>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
