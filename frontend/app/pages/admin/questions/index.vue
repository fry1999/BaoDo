<template>
  <div class="space-y-5">
    <useHead><title>Admin — Ngân hàng câu hỏi</title></useHead>

    <div class="flex items-center justify-between">
      <h1 class="text-xl font-bold text-gray-900 dark:text-white">Ngân hàng câu hỏi</h1>
      <button
        class="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-lg text-sm font-medium hover:bg-primary-dark transition-colors"
        @click="openCreate"
      >
        + Thêm câu hỏi
      </button>
    </div>

    <!-- Filters -->
    <div class="flex flex-wrap gap-3">
      <select
        v-model="filters.part"
        class="h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary"
        @change="fetchQuestions(1)"
      >
        <option value="">Tất cả Part</option>
        <option v-for="p in 7" :key="p" :value="p">Part {{ p }}</option>
      </select>

      <select
        v-model="filters.difficulty"
        class="h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary"
        @change="fetchQuestions(1)"
      >
        <option value="">Tất cả độ khó</option>
        <option value="easy">Dễ</option>
        <option value="medium">Trung bình</option>
        <option value="hard">Khó</option>
      </select>

      <select
        v-model="filters.published"
        class="h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary"
        @change="fetchQuestions(1)"
      >
        <option value="">Tất cả trạng thái</option>
        <option value="true">Đã đăng</option>
        <option value="false">Nháp</option>
      </select>

      <input
        v-model="filters.search"
        type="text"
        placeholder="Tìm nội dung câu hỏi..."
        class="flex-1 min-w-48 h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary"
        @input="debouncedSearch"
      />
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
            <th class="text-left px-4 py-3 font-medium text-gray-500">Nội dung</th>
            <th class="text-left px-4 py-3 font-medium text-gray-500 hidden md:table-cell w-28">Độ khó</th>
            <th class="text-left px-4 py-3 font-medium text-gray-500 w-24">Trạng thái</th>
            <th class="text-right px-4 py-3 font-medium text-gray-500 w-28">Thao tác</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50 dark:divide-slate-800">
          <tr v-if="questions.length === 0">
            <td colspan="5" class="px-4 py-8 text-center text-gray-400">Không có câu hỏi nào</td>
          </tr>
          <tr
            v-for="q in questions"
            :key="q.id"
            class="hover:bg-gray-50 dark:hover:bg-slate-800/50"
          >
            <td class="px-4 py-3">
              <span class="text-xs bg-gray-100 dark:bg-slate-800 px-2 py-1 rounded font-mono font-medium">
                Part {{ q.part }}
              </span>
            </td>
            <td class="px-4 py-3 max-w-xs">
              <p class="text-gray-900 dark:text-white truncate">{{ q.questionText || '(Audio/Image)' }}</p>
              <p v-if="q.audioUrl" class="text-xs text-blue-500 mt-0.5">🎵 Audio</p>
              <p v-if="q.imageUrl" class="text-xs text-green-500 mt-0.5">🖼 Image</p>
            </td>
            <td class="px-4 py-3 hidden md:table-cell">
              <span
                class="text-xs px-2 py-0.5 rounded-full font-medium"
                :class="{
                  'bg-green-100 text-green-700': q.difficulty === 'easy',
                  'bg-yellow-100 text-yellow-700': q.difficulty === 'medium',
                  'bg-red-100 text-red-700': q.difficulty === 'hard',
                }"
              >
                {{ difficultyLabel(q.difficulty) }}
              </span>
            </td>
            <td class="px-4 py-3">
              <button
                class="text-xs px-2 py-0.5 rounded-full font-medium transition-colors"
                :class="q.isPublished
                  ? 'bg-green-100 text-green-700 hover:bg-green-200'
                  : 'bg-gray-100 text-gray-500 hover:bg-gray-200'"
                @click="togglePublish(q)"
              >
                {{ q.isPublished ? 'Đã đăng' : 'Nháp' }}
              </button>
            </td>
            <td class="px-4 py-3 text-right">
              <div class="flex items-center justify-end gap-1">
                <button
                  class="text-xs text-primary hover:text-primary-dark font-medium px-2 py-1 rounded hover:bg-primary-light"
                  @click="openEdit(q)"
                >
                  Sửa
                </button>
                <button
                  class="text-xs text-red-500 hover:text-red-700 font-medium px-2 py-1 rounded hover:bg-red-50"
                  @click="deleteQuestion(q.id)"
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
        <p class="text-xs text-gray-500">{{ total }} câu hỏi</p>
        <div class="flex gap-1">
          <button
            v-for="p in totalPages"
            :key="p"
            class="w-7 h-7 text-xs rounded-lg font-medium transition-colors"
            :class="p === currentPage ? 'bg-primary text-white' : 'text-gray-500 hover:bg-gray-100'"
            @click="fetchQuestions(p)"
          >
            {{ p }}
          </button>
        </div>
      </div>
    </div>

    <!-- Slide-over form -->
    <Teleport to="body">
      <Transition name="slide-over">
        <div v-if="showForm" class="fixed inset-0 z-50 flex justify-end">
          <div class="absolute inset-0 bg-black/40" @click="closeForm" />
          <div class="relative w-full max-w-xl bg-white dark:bg-slate-900 h-full overflow-y-auto shadow-xl">
            <div class="sticky top-0 bg-white dark:bg-slate-900 border-b border-gray-200 dark:border-slate-800 px-6 py-4 flex items-center justify-between z-10">
              <h2 class="font-semibold text-gray-900 dark:text-white">
                {{ editingId ? 'Sửa câu hỏi' : 'Thêm câu hỏi' }}
              </h2>
              <button class="text-gray-400 hover:text-gray-600 text-xl" @click="closeForm">✕</button>
            </div>

            <form class="p-6 space-y-4" @submit.prevent="saveQuestion">
              <!-- Part + Difficulty -->
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Part *</label>
                  <select v-model="form.part" required class="w-full h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary">
                    <option v-for="p in 7" :key="p" :value="p">Part {{ p }}</option>
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

              <!-- Passage -->
              <div>
                <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Passage (Part 6–7)</label>
                <select v-model="form.passageId" class="w-full h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary">
                  <option :value="null">Không có passage</option>
                  <option v-for="p in passages" :key="p.id" :value="p.id">
                    Part {{ p.part }} — {{ p.title || p.content.substring(0, 50) + '...' }}
                  </option>
                </select>
              </div>

              <!-- Question text -->
              <div>
                <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Nội dung câu hỏi</label>
                <textarea
                  v-model="form.questionText"
                  rows="3"
                  class="w-full px-3 py-2 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary resize-none"
                  placeholder="Để trống nếu là câu hỏi Audio/Image"
                />
              </div>

              <!-- Audio / Image / Transcript -->
              <div class="grid grid-cols-1 gap-3">
                <div>
                  <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Audio URL</label>
                  <input v-model="form.audioUrl" type="url" class="w-full h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary" />
                </div>
                <div>
                  <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Image URL</label>
                  <input v-model="form.imageUrl" type="url" class="w-full h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary" />
                </div>
                <div>
                  <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Transcript (Part 1–4)</label>
                  <textarea v-model="form.transcript" rows="2" class="w-full px-3 py-2 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary resize-none" />
                </div>
              </div>

              <!-- Options -->
              <div>
                <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-2">Đáp án (chọn đáp án đúng) *</label>
                <div class="space-y-2">
                  <div v-for="(_, oi) in form.options" :key="oi" class="flex items-center gap-2">
                    <button
                      type="button"
                      class="w-6 h-6 rounded-full flex-shrink-0 border-2 transition-colors"
                      :class="form.correctIndex === oi ? 'border-green-500 bg-green-500' : 'border-gray-300'"
                      @click="form.correctIndex = oi"
                    >
                      <span v-if="form.correctIndex === oi" class="block w-full text-center text-white text-xs leading-none">✓</span>
                    </button>
                    <span class="text-sm font-semibold text-gray-500 w-4">{{ String.fromCharCode(65 + oi) }}</span>
                    <input
                      v-model="form.options[oi]"
                      type="text"
                      required
                      :placeholder="`Đáp án ${String.fromCharCode(65 + oi)}`"
                      class="flex-1 h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary"
                    />
                  </div>
                </div>
              </div>

              <!-- Explanation -->
              <div>
                <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Giải thích *</label>
                <textarea
                  v-model="form.explanation"
                  rows="3"
                  required
                  class="w-full px-3 py-2 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary resize-none"
                />
              </div>

              <!-- Tags -->
              <div>
                <label class="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Tags (phân cách bằng dấu phẩy)</label>
                <input
                  v-model="tagsInput"
                  type="text"
                  class="w-full h-9 px-3 border border-gray-300 dark:border-slate-700 rounded-lg text-sm dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary"
                  placeholder="word_form, tense, preposition"
                />
              </div>

              <div class="flex gap-3 pt-2">
                <button
                  type="button"
                  class="flex-1 h-10 border border-gray-300 rounded-lg text-sm font-medium hover:bg-gray-50"
                  @click="closeForm"
                >
                  Huỷ
                </button>
                <button
                  type="submit"
                  :disabled="isSaving"
                  class="flex-1 h-10 bg-primary text-white rounded-lg text-sm font-semibold hover:bg-primary-dark disabled:opacity-50 transition-colors"
                >
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
import { useDebounceFn } from '@vueuse/core'

definePageMeta({ layout: 'admin', middleware: 'admin' })

const config = useRuntimeConfig()
const auth = useAuthStore()
const headers = computed(() => ({ Authorization: `Bearer ${auth.token}` }))

interface Question {
  id: string
  part: number
  difficulty: string
  questionText?: string
  audioUrl?: string
  imageUrl?: string
  transcript?: string
  options: string[]
  correctIndex: number
  explanation: string
  tags: string[]
  passageId?: string
  isPublished: boolean
}

interface Passage { id: string; part: number; title?: string; content: string }

const questions = ref<Question[]>([])
const passages = ref<Passage[]>([])
const isLoading = ref(false)
const isSaving = ref(false)
const total = ref(0)
const currentPage = ref(1)
const totalPages = ref(1)

const filters = reactive({ part: '', difficulty: '', published: '', search: '' })

const showForm = ref(false)
const editingId = ref<string | null>(null)
const tagsInput = ref('')

const form = reactive({
  part: 1,
  difficulty: 'medium',
  questionText: '',
  audioUrl: '',
  imageUrl: '',
  transcript: '',
  options: ['', '', '', ''],
  correctIndex: 0,
  explanation: '',
  passageId: null as string | null,
})

async function fetchQuestions(page = 1) {
  isLoading.value = true
  currentPage.value = page
  try {
    const params: Record<string, string | number> = { page, pageSize: 20 }
    if (filters.part) params.part = filters.part
    if (filters.difficulty) params.difficulty = filters.difficulty
    if (filters.published) params.published = filters.published
    if (filters.search) params.search = filters.search

    const res = await $fetch<{ data: Question[]; total: number; totalPages: number }>(
      `${config.public.apiBaseUrl}/api/admin/questions`,
      { headers: headers.value, query: params },
    )
    questions.value = res.data
    total.value = res.total
    totalPages.value = res.totalPages
  }
  finally {
    isLoading.value = false
  }
}

async function fetchPassages() {
  const res = await $fetch<{ data: Passage[] }>(
    `${config.public.apiBaseUrl}/api/admin/passages`,
    { headers: headers.value, query: { pageSize: 100 } },
  )
  passages.value = res.data
}

const debouncedSearch = useDebounceFn(() => fetchQuestions(1), 400)

function difficultyLabel(d: string) {
  return { easy: 'Dễ', medium: 'TB', hard: 'Khó' }[d] ?? d
}

function resetForm() {
  form.part = 1
  form.difficulty = 'medium'
  form.questionText = ''
  form.audioUrl = ''
  form.imageUrl = ''
  form.transcript = ''
  form.options = ['', '', '', '']
  form.correctIndex = 0
  form.explanation = ''
  form.passageId = null
  tagsInput.value = ''
}

function openCreate() {
  editingId.value = null
  resetForm()
  showForm.value = true
}

function openEdit(q: Question) {
  editingId.value = q.id
  form.part = q.part
  form.difficulty = q.difficulty
  form.questionText = q.questionText ?? ''
  form.audioUrl = q.audioUrl ?? ''
  form.imageUrl = q.imageUrl ?? ''
  form.transcript = q.transcript ?? ''
  form.options = [...q.options]
  form.correctIndex = q.correctIndex
  form.explanation = q.explanation
  form.passageId = q.passageId ?? null
  tagsInput.value = q.tags.join(', ')
  showForm.value = true
}

function closeForm() {
  showForm.value = false
}

async function saveQuestion() {
  isSaving.value = true
  try {
    const body = {
      part: form.part,
      difficulty: form.difficulty,
      questionText: form.questionText || null,
      audioUrl: form.audioUrl || null,
      imageUrl: form.imageUrl || null,
      transcript: form.transcript || null,
      options: form.options,
      correctIndex: form.correctIndex,
      explanation: form.explanation,
      tags: tagsInput.value.split(',').map(t => t.trim()).filter(Boolean),
      passageId: form.passageId || null,
    }
    if (editingId.value) {
      await $fetch(`${config.public.apiBaseUrl}/api/admin/questions/${editingId.value}`, {
        method: 'PUT', headers: headers.value, body,
      })
    }
    else {
      await $fetch(`${config.public.apiBaseUrl}/api/admin/questions`, {
        method: 'POST', headers: headers.value, body,
      })
    }
    closeForm()
    await fetchQuestions(currentPage.value)
  }
  finally {
    isSaving.value = false
  }
}

async function togglePublish(q: Question) {
  await $fetch(`${config.public.apiBaseUrl}/api/admin/questions/${q.id}/publish`, {
    method: 'PATCH', headers: headers.value,
  })
  q.isPublished = !q.isPublished
}

async function deleteQuestion(id: string) {
  if (!confirm('Xoá câu hỏi này?')) return
  await $fetch(`${config.public.apiBaseUrl}/api/admin/questions/${id}`, {
    method: 'DELETE', headers: headers.value,
  })
  await fetchQuestions(currentPage.value)
}

onMounted(() => {
  fetchQuestions()
  fetchPassages()
})
</script>

<style scoped>
.slide-over-enter-active,
.slide-over-leave-active {
  transition: opacity 0.2s ease;
}
.slide-over-enter-active > div:last-child,
.slide-over-leave-active > div:last-child {
  transition: transform 0.25s ease;
}
.slide-over-enter-from,
.slide-over-leave-to {
  opacity: 0;
}
.slide-over-enter-from > div:last-child,
.slide-over-leave-to > div:last-child {
  transform: translateX(100%);
}
</style>
