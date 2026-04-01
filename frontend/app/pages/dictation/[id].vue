<template>
  <div class="max-w-2xl mx-auto space-y-5">
    <useHead><title>{{ dictationStore.current?.title ?? 'Dictation' }}</title></useHead>

    <div v-if="dictationStore.isLoading" class="flex justify-center py-24">
      <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin" />
    </div>

    <template v-else-if="dictationStore.current">
      <!-- Header -->
      <div>
        <NuxtLink to="/dictation" class="text-sm text-gray-500 hover:text-primary flex items-center gap-1 mb-3">
          ← Danh sách dictation
        </NuxtLink>
        <h1 class="text-xl font-bold text-gray-900 dark:text-white">{{ dictationStore.current.title }}</h1>
        <p class="text-sm text-gray-500 mt-0.5">
          Đoạn {{ dictationStore.segmentIndex + 1 }} / {{ dictationStore.totalSegments }}
        </p>
      </div>

      <!-- Progress -->
      <div class="h-2 bg-gray-200 rounded-full overflow-hidden">
        <div
          class="h-full bg-primary rounded-full transition-all duration-500"
          :style="{ width: `${((dictationStore.segmentIndex + 1) / dictationStore.totalSegments) * 100}%` }"
        />
      </div>

      <!-- Audio player -->
      <div class="bg-white dark:bg-slate-900 rounded-2xl shadow-card p-5">
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center gap-3">
            <button
              class="w-10 h-10 bg-primary text-white rounded-full flex items-center justify-center hover:bg-primary-dark transition-colors"
              @click="replaySegment"
            >
              {{ isPlaying ? '⏸' : '▶' }}
            </button>
            <button
              class="px-3 py-1.5 text-xs border border-gray-200 rounded-lg font-medium hover:bg-gray-50"
              @click="replaySegment"
            >
              🔁 Nghe lại
            </button>
          </div>

          <!-- Speed control -->
          <div class="flex gap-1">
            <button
              v-for="rate in [0.75, 1]"
              :key="rate"
              class="px-2.5 py-1 text-xs rounded-lg font-medium transition-colors"
              :class="dictationStore.playbackRate === rate ? 'bg-primary text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'"
              @click="dictationStore.setPlaybackRate(rate)"
            >
              {{ rate }}x
            </button>
          </div>
        </div>
      </div>

      <!-- Input -->
      <div class="bg-white dark:bg-slate-900 rounded-2xl shadow-card p-5">
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Gõ những gì bạn nghe được:
        </label>
        <textarea
          v-model="dictationStore.userInput"
          :disabled="!!dictationStore.checkResult"
          rows="3"
          placeholder="Nhập nội dung..."
          class="w-full px-4 py-3 border border-gray-300 dark:border-slate-600 rounded-xl text-sm font-mono focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary/20 resize-none dark:bg-slate-800 dark:text-white transition-colors"
        />

        <!-- Check result -->
        <div v-if="dictationStore.checkResult" class="mt-4">
          <div class="flex items-center gap-2 mb-2">
            <span
              class="text-lg font-bold"
              :class="{
                'text-green-600': dictationStore.checkResult.accuracy >= 90,
                'text-yellow-600': dictationStore.checkResult.accuracy >= 70,
                'text-red-500': dictationStore.checkResult.accuracy < 70,
              }"
            >
              {{ dictationStore.checkResult.accuracy }}%
            </span>
            <span class="text-sm text-gray-500">chính xác</span>
          </div>

          <!-- Word-by-word highlight -->
          <div class="flex flex-wrap gap-1 text-sm font-mono">
            <span
              v-for="(w, i) in dictationStore.checkResult.words"
              :key="i"
              class="px-1.5 py-0.5 rounded"
              :class="{
                'dictation-correct': w.status === 'correct',
                'dictation-wrong': w.status === 'wrong',
                'dictation-missing': w.status === 'missing',
                'dictation-extra': w.status === 'extra',
              }"
            >
              {{ w.word }}
            </span>
          </div>

          <!-- Reveal transcript -->
          <div v-if="dictationStore.isRevealed" class="mt-3 text-sm text-gray-600 dark:text-gray-400 bg-gray-50 dark:bg-slate-800 rounded-xl p-3 italic">
            "{{ dictationStore.currentSegment?.text }}"
          </div>
        </div>

        <!-- Action buttons -->
        <div class="flex gap-2 mt-4">
          <template v-if="!dictationStore.checkResult">
            <button
              :disabled="!dictationStore.userInput.trim()"
              class="px-4 py-2 bg-primary text-white rounded-xl text-sm font-semibold hover:bg-primary-dark transition-colors disabled:opacity-50"
              @click="dictationStore.checkAnswer()"
            >
              Kiểm tra
            </button>
            <button
              class="px-4 py-2 border border-gray-200 text-gray-600 rounded-xl text-sm hover:bg-gray-50"
              @click="dictationStore.reveal()"
            >
              Xem đáp án
            </button>
          </template>
          <template v-else>
            <button
              v-if="!dictationStore.isRevealed"
              class="px-4 py-2 border border-gray-200 text-gray-600 rounded-xl text-sm hover:bg-gray-50"
              @click="dictationStore.reveal()"
            >
              Xem transcript
            </button>
            <button
              v-if="!dictationStore.isLastSegment"
              class="px-4 py-2 bg-primary text-white rounded-xl text-sm font-semibold hover:bg-primary-dark transition-colors"
              @click="dictationStore.nextSegment()"
            >
              Đoạn tiếp →
            </button>
            <div v-else class="text-sm text-green-600 font-medium flex items-center gap-1">
              🎉 Hoàn thành! Điểm trung bình: {{ dictationStore.sessionAccuracy }}%
            </div>
          </template>
        </div>
      </div>

      <!-- Navigation -->
      <div class="flex justify-between">
        <button
          :disabled="dictationStore.segmentIndex === 0"
          class="px-3 py-1.5 text-sm text-gray-500 hover:text-primary disabled:opacity-30"
          @click="dictationStore.prevSegment()"
        >
          ← Đoạn trước
        </button>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ middleware: 'auth' })

const route = useRoute()
const dictationStore = useDictationStore()
const isPlaying = ref(false)
let audio: HTMLAudioElement | null = null

onMounted(() => dictationStore.loadContent(route.params.id as string))

function replaySegment() {
  const segment = dictationStore.currentSegment
  const content = dictationStore.current
  if (!segment || !content) return

  if (content.audioUrl) {
    audio?.pause()
    audio = new Audio(content.audioUrl)
    audio.playbackRate = dictationStore.playbackRate
    audio.currentTime = segment.start
    audio.play()
    isPlaying.value = true

    const checkInterval = setInterval(() => {
      if (!audio || audio.currentTime >= segment.end) {
        audio?.pause()
        isPlaying.value = false
        clearInterval(checkInterval)
      }
    }, 100)
  }
}

onUnmounted(() => { audio?.pause(); audio = null })
</script>
