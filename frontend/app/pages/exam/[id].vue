<template>
  <div class="flex h-[calc(100vh-4rem)]">
    <useHead><title>Phòng thi</title></useHead>

    <!-- Pre-start screen -->
    <div v-if="!examStore.isStarted" class="w-full flex items-center justify-center p-4">
      <div class="max-w-md w-full bg-white dark:bg-slate-900 rounded-2xl shadow-card p-8 text-center">
        <p class="text-5xl mb-4">📋</p>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">{{ examStore.currentTest?.title }}</h1>
        <div class="flex justify-center gap-8 text-sm text-gray-500 mb-6">
          <span>{{ examStore.questions.length }} câu</span>
          <span>{{ examStore.currentTest?.durationMinutes }} phút</span>
        </div>
        <div class="bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-xl p-4 text-sm text-yellow-800 dark:text-yellow-300 text-left mb-6">
          <p class="font-semibold mb-1">Lưu ý trước khi thi:</p>
          <ul class="space-y-1 list-disc list-inside text-yellow-700 dark:text-yellow-400">
            <li>Đồng hồ sẽ bắt đầu ngay khi bạn nhấn "Bắt đầu"</li>
            <li>Audio chỉ phát một lần (như thi thật)</li>
            <li>Bài sẽ tự nộp khi hết giờ</li>
          </ul>
        </div>
        <button
          :disabled="examStore.isLoading"
          class="w-full py-3 bg-primary text-white rounded-xl font-bold text-base hover:bg-primary-dark transition-colors"
          @click="startExam"
        >
          Bắt đầu thi
        </button>
        <NuxtLink to="/exam" class="mt-3 block text-sm text-gray-400 hover:text-gray-600">← Quay lại</NuxtLink>
      </div>
    </div>

    <!-- Exam in progress -->
    <template v-else>
      <!-- Question panel -->
      <div class="flex-1 overflow-y-auto p-6">
        <div v-if="examStore.currentQuestion" class="max-w-2xl mx-auto bg-white dark:bg-slate-900 rounded-2xl shadow-card p-6">
          <div class="flex items-center gap-2 mb-4">
            <span class="text-xs bg-gray-100 dark:bg-slate-800 text-gray-600 dark:text-gray-400 px-2 py-1 rounded-lg font-mono">
              Part {{ examStore.currentQuestion.part }}
            </span>
            <span class="text-xs text-gray-400">Câu {{ examStore.currentQuestionIndex + 1 }}</span>
          </div>

          <!-- Audio -->
          <div v-if="examStore.currentQuestion.audioUrl" class="mb-4 p-3 bg-gray-50 dark:bg-slate-800 rounded-xl flex items-center gap-3">
            <button
              class="w-9 h-9 bg-primary text-white rounded-full flex items-center justify-center hover:bg-primary-dark text-sm"
              @click="playAudio"
            >
              ▶
            </button>
            <span class="text-sm text-gray-500">Audio phát 1 lần</span>
          </div>

          <!-- Image -->
          <div v-if="examStore.currentQuestion.imageUrl" class="mb-4 rounded-xl overflow-hidden">
            <img :src="examStore.currentQuestion.imageUrl" class="w-full max-h-60 object-contain" alt="" />
          </div>

          <!-- Question text -->
          <p v-if="examStore.currentQuestion.questionText" class="text-sm font-medium text-gray-900 dark:text-white mb-4">
            {{ examStore.currentQuestion.questionText }}
          </p>

          <!-- Options -->
          <div class="space-y-2">
            <button
              v-for="(opt, oi) in examStore.currentQuestion.options"
              :key="oi"
              class="w-full text-left px-4 py-3 rounded-xl text-sm border transition-all"
              :class="examStore.answers[examStore.currentQuestion.id] === oi
                ? 'border-primary bg-primary-light text-primary font-medium'
                : 'border-gray-200 dark:border-slate-700 hover:border-primary hover:bg-primary-light/30'"
              @click="examStore.selectAnswer(examStore.currentQuestion!.id, oi)"
            >
              <span class="font-semibold mr-2 text-gray-400">{{ String.fromCharCode(65 + oi) }}.</span>
              {{ opt }}
            </button>
          </div>

          <!-- Navigation -->
          <div class="flex justify-between mt-6">
            <button
              :disabled="examStore.currentQuestionIndex === 0"
              class="px-4 py-2 text-sm border border-gray-200 rounded-xl hover:bg-gray-50 disabled:opacity-30"
              @click="examStore.prevQuestion()"
            >
              ← Câu trước
            </button>
            <button
              v-if="examStore.currentQuestionIndex < examStore.questions.length - 1"
              class="px-4 py-2 text-sm bg-primary text-white rounded-xl hover:bg-primary-dark font-semibold"
              @click="examStore.nextQuestion()"
            >
              Câu tiếp →
            </button>
          </div>
        </div>
      </div>

      <!-- Question map (desktop) -->
      <aside class="w-60 bg-white dark:bg-slate-900 border-l border-gray-200 dark:border-slate-800 p-4 overflow-y-auto hidden lg:block">
        <p class="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">Bảng câu hỏi</p>
        <div class="grid grid-cols-5 gap-1">
          <button
            v-for="(q, i) in examStore.questions"
            :key="q.id"
            class="w-9 h-9 text-xs rounded-lg font-mono font-medium transition-colors"
            :class="{
              'bg-primary text-white': i === examStore.currentQuestionIndex,
              'bg-green-100 text-green-700 dark:bg-green-900/30': examStore.answers[q.id] !== undefined && i !== examStore.currentQuestionIndex,
              'bg-gray-100 dark:bg-slate-800 text-gray-600 dark:text-gray-400': examStore.answers[q.id] === undefined && i !== examStore.currentQuestionIndex,
            }"
            @click="examStore.navigateTo(i)"
          >
            {{ i + 1 }}
          </button>
        </div>
      </aside>
    </template>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'exam', middleware: 'auth' })

const route = useRoute()
const examStore = useExamStore()
const { start } = useExamTimer()

onMounted(() => examStore.loadTest(route.params.id as string))
onUnmounted(() => examStore.reset())

function startExam() {
  examStore.startExam()
  start()
}

function playAudio() {
  const q = examStore.currentQuestion
  if (q?.audioUrl) new Audio(q.audioUrl).play()
}
</script>
