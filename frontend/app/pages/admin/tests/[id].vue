<template>
  <div class="space-y-5">
    <useHead><title>Admin — Gán câu hỏi cho đề</title></useHead>

    <div class="flex items-center gap-3">
      <NuxtLink to="/admin/tests" class="text-sm text-gray-500 hover:text-primary flex items-center gap-1">
        ← Danh sách đề thi
      </NuxtLink>
    </div>

    <div v-if="isLoadingTest" class="flex justify-center py-12">
      <div class="w-6 h-6 border-2 border-primary border-t-transparent rounded-full animate-spin" />
    </div>

    <template v-else-if="test">
      <div class="flex items-start justify-between">
        <div>
          <h1 class="text-xl font-bold text-gray-900 dark:text-white">{{ test.title }}</h1>
          <div class="flex items-center gap-3 mt-1 text-sm text-gray-500">
            <span>{{ test.type }}</span>
            <span>·</span>
            <span>{{ testQuestions.length }}/{{ test.totalQuestions }} câu</span>
            <span>·</span>
            <span>{{ test.durationMinutes }} phút</span>
          </div>
        </div>
        <div class="flex items-center gap-2">
          <span
            class="text-xs px-2 py-1 rounded-full font-medium"
            :class="test.isPublished ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'"
          >
            {{ test.isPublished ? 'Đã đăng' : 'Nháp' }}
          </span>
        </div>
      </div>

      <div class="grid grid-cols-1 xl:grid-cols-2 gap-5">
        <!-- Left: Questions in test -->
        <div class="bg-white dark:bg-slate-900 rounded-xl shadow-card overflow-hidden">
          <div class="px-4 py-3 border-b border-gray-100 dark:border-slate-800 flex items-center justify-between">
            <h2 class="font-semibold text-gray-900 dark:text-white text-sm">
              Câu hỏi trong đề ({{ testQuestions.length }})
            </h2>
            <button
              v-if="reorderDirty"
              class="text-xs bg-primary text-white px-3 py-1 rounded-lg hover:bg-primary-dark"
              @click="saveReorder"
            >
              Lưu thứ tự
            </button>
          </div>

          <div v-if="isLoadingTest" class="flex justify-center py-8">
            <div class="w-5 h-5 border-2 border-primary border-t-transparent rounded-full animate-spin" />
          </div>

          <div v-else-if="testQuestions.length === 0" class="px-4 py-8 text-center text-gray-400 text-sm">
            Chưa có câu hỏi. Thêm từ ngân hàng bên phải.
          </div>

          <div v-else class="divide-y divide-gray-50 dark:divide-slate-800 max-h-[60vh] overflow-y-auto">
            <div
              v-for="(item, idx) in testQuestions"
              :key="item.question.id"
              class="flex items-center gap-3 px-4 py-3 hover:bg-gray-50 dark:hover:bg-slate-800/50 group"
            >
              <!-- Reorder buttons -->
              <div class="flex flex-col gap-0.5 flex-shrink-0">
                <button
                  :disabled="idx === 0"
                  class="text-gray-300 hover:text-gray-600 disabled:opacity-0 text-xs leading-none"
                  @click="moveUp(idx)"
                >
                  ▲
                </button>
                <button
                  :disabled="idx === testQuestions.length - 1"
                  class="text-gray-300 hover:text-gray-600 disabled:opacity-0 text-xs leading-none"
                  @click="moveDown(idx)"
                >
                  ▼
                </button>
              </div>
              <span class="text-xs text-gray-400 font-mono w-6 text-center flex-shrink-0">{{ item.sortOrder }}</span>
              <span class="text-xs bg-gray-100 dark:bg-slate-800 px-1.5 py-0.5 rounded font-mono flex-shrink-0">P{{ item.question.part }}</span>
              <p class="flex-1 text-sm text-gray-700 dark:text-gray-300 truncate">
                {{ item.question.questionText || `[Audio/Image — ${item.question.difficulty}]` }}
              </p>
              <button
                class="text-red-400 hover:text-red-600 text-xs opacity-0 group-hover:opacity-100 transition-opacity px-1.5 py-1 rounded hover:bg-red-50 flex-shrink-0"
                @click="removeQuestion(item.question.id)"
              >
                ✕
              </button>
            </div>
          </div>
        </div>

        <!-- Right: Question bank -->
        <div class="bg-white dark:bg-slate-900 rounded-xl shadow-card overflow-hidden">
          <div class="px-4 py-3 border-b border-gray-100 dark:border-slate-800">
            <h2 class="font-semibold text-gray-900 dark:text-white text-sm mb-3">Ngân hàng câu hỏi</h2>
            <div class="flex gap-2">
              <select
                v-model="bankPart"
                class="h-8 px-2 border border-gray-300 dark:border-slate-700 rounded-lg text-xs dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary"
                @change="fetchBank(1)"
              >
                <option value="">Tất cả Part</option>
                <option v-for="p in 7" :key="p" :value="p">Part {{ p }}</option>
              </select>
              <input
                v-model="bankSearch"
                type="text"
                placeholder="Tìm câu hỏi..."
                class="flex-1 h-8 px-2 border border-gray-300 dark:border-slate-700 rounded-lg text-xs dark:bg-slate-800 dark:text-white focus:outline-none focus:border-primary"
                @input="debouncedBankSearch"
              />
            </div>
          </div>

          <div v-if="isLoadingBank" class="flex justify-center py-8">
            <div class="w-5 h-5 border-2 border-primary border-t-transparent rounded-full animate-spin" />
          </div>

          <div v-else class="divide-y divide-gray-50 dark:divide-slate-800 max-h-[60vh] overflow-y-auto">
            <div v-if="bankQuestions.length === 0" class="px-4 py-6 text-center text-gray-400 text-sm">
              Không có câu hỏi
            </div>
            <div
              v-for="q in bankQuestions"
              :key="q.id"
              class="flex items-center gap-3 px-4 py-3 hover:bg-gray-50 dark:hover:bg-slate-800/50 group"
            >
              <span class="text-xs bg-gray-100 dark:bg-slate-800 px-1.5 py-0.5 rounded font-mono flex-shrink-0">P{{ q.part }}</span>
              <p class="flex-1 text-sm text-gray-700 dark:text-gray-300 truncate">
                {{ q.questionText || `[Audio/Image — ${q.difficulty}]` }}
              </p>
              <span
                v-if="isInTest(q.id)"
                class="text-xs text-green-600 font-medium flex-shrink-0"
              >
                ✓ Đã có
              </span>
              <button
                v-else
                class="text-xs text-primary hover:text-primary-dark font-medium opacity-0 group-hover:opacity-100 transition-opacity px-2 py-1 rounded hover:bg-primary-light flex-shrink-0"
                @click="addQuestion(q.id)"
              >
                + Thêm
              </button>
            </div>
          </div>

          <!-- Bank pagination -->
          <div v-if="bankTotalPages > 1" class="flex items-center justify-between px-4 py-3 border-t border-gray-100 dark:border-slate-800">
            <p class="text-xs text-gray-400">{{ bankTotal }} câu</p>
            <div class="flex gap-1">
              <button
                v-for="p in Math.min(bankTotalPages, 5)"
                :key="p"
                class="w-6 h-6 text-xs rounded font-medium"
                :class="p === bankPage ? 'bg-primary text-white' : 'text-gray-500 hover:bg-gray-100'"
                @click="fetchBank(p)"
              >
                {{ p }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { useDebounceFn } from '@vueuse/core'

definePageMeta({ layout: 'admin', middleware: 'admin' })

const route = useRoute()
const testId = route.params.id as string
const config = useRuntimeConfig()
const auth = useAuthStore()
const headers = computed(() => ({ Authorization: `Bearer ${auth.token}` }))

interface TestInfo {
  id: string; title: string; type: string; totalQuestions: number; durationMinutes: number; difficulty: string; isPublished: boolean
}
interface TestQuestionItem { sortOrder: number; question: { id: string; part: number; difficulty: string; questionText?: string } }
interface BankQuestion { id: string; part: number; difficulty: string; questionText?: string }

const test = ref<TestInfo | null>(null)
const testQuestions = ref<TestQuestionItem[]>([])
const isLoadingTest = ref(false)
const reorderDirty = ref(false)

const bankQuestions = ref<BankQuestion[]>([])
const isLoadingBank = ref(false)
const bankPart = ref('')
const bankSearch = ref('')
const bankPage = ref(1)
const bankTotal = ref(0)
const bankTotalPages = ref(1)

async function loadTest() {
  isLoadingTest.value = true
  try {
    // Load test metadata from admin tests list (reuse existing admin tests endpoint)
    const res = await $fetch<{ data: TestInfo[] }>(
      `${config.public.apiBaseUrl}/api/admin/tests`,
      { headers: headers.value, query: { pageSize: 100 } },
    )
    test.value = res.data.find(t => t.id === testId) ?? null
    await loadTestQuestions()
  }
  finally { isLoadingTest.value = false }
}

async function loadTestQuestions() {
  const res = await $fetch<TestQuestionItem[]>(
    `${config.public.apiBaseUrl}/api/admin/tests/${testId}/questions`,
    { headers: headers.value },
  )
  testQuestions.value = res
  reorderDirty.value = false
}

async function fetchBank(page = 1) {
  isLoadingBank.value = true
  bankPage.value = page
  try {
    const params: Record<string, string | number> = { page, pageSize: 20, published: 'true' }
    if (bankPart.value) params.part = bankPart.value
    if (bankSearch.value) params.search = bankSearch.value
    const res = await $fetch<{ data: BankQuestion[]; total: number; totalPages: number }>(
      `${config.public.apiBaseUrl}/api/admin/questions`,
      { headers: headers.value, query: params },
    )
    bankQuestions.value = res.data
    bankTotal.value = res.total
    bankTotalPages.value = res.totalPages
  }
  finally { isLoadingBank.value = false }
}

const debouncedBankSearch = useDebounceFn(() => fetchBank(1), 400)

function isInTest(qId: string) {
  return testQuestions.value.some(item => item.question.id === qId)
}

async function addQuestion(questionId: string) {
  await $fetch(`${config.public.apiBaseUrl}/api/admin/tests/${testId}/questions`, {
    method: 'POST', headers: headers.value, body: { questionId },
  })
  await loadTestQuestions()
}

async function removeQuestion(questionId: string) {
  await $fetch(`${config.public.apiBaseUrl}/api/admin/tests/${testId}/questions/${questionId}`, {
    method: 'DELETE', headers: headers.value,
  })
  await loadTestQuestions()
}

function moveUp(idx: number) {
  if (idx === 0) return
  const arr = testQuestions.value
  ;[arr[idx - 1], arr[idx]] = [arr[idx], arr[idx - 1]]
  arr.forEach((item, i) => (item.sortOrder = i + 1))
  reorderDirty.value = true
}

function moveDown(idx: number) {
  const arr = testQuestions.value
  if (idx === arr.length - 1) return
  ;[arr[idx], arr[idx + 1]] = [arr[idx + 1], arr[idx]]
  arr.forEach((item, i) => (item.sortOrder = i + 1))
  reorderDirty.value = true
}

async function saveReorder() {
  const items = testQuestions.value.map(item => ({
    questionId: item.question.id,
    sortOrder: item.sortOrder,
  }))
  await $fetch(`${config.public.apiBaseUrl}/api/admin/tests/${testId}/questions/reorder`, {
    method: 'PUT', headers: headers.value, body: items,
  })
  reorderDirty.value = false
}

onMounted(() => {
  loadTest()
  fetchBank()
})
</script>
