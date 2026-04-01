<template>
  <Teleport to="body">
    <Transition name="fade-in-up">
      <div
        v-if="popup.visible"
        class="fixed z-[9999] w-72 bg-white dark:bg-slate-900 rounded-xl shadow-popup border border-gray-200 dark:border-slate-700 animate-fade-in-up"
        :style="{ top: `${popup.y - 8}px`, left: `${Math.min(popup.x, windowWidth - 300)}px`, transform: 'translateY(-100%)' }"
      >
        <!-- Loading -->
        <div v-if="popup.isLoading" class="p-4 flex items-center justify-center">
          <div class="w-5 h-5 border-2 border-primary border-t-transparent rounded-full animate-spin" />
        </div>

        <!-- Content -->
        <div v-else-if="popup.entry" class="p-4">
          <!-- Word + phonetic + audio -->
          <div class="flex items-start justify-between mb-2">
            <div>
              <span class="text-base font-bold text-gray-900 dark:text-white">{{ popup.entry.word }}</span>
              <span class="ml-2 text-sm text-gray-500 font-mono">{{ popup.entry.phonetic }}</span>
            </div>
            <button
              v-if="popup.entry.audioUrl"
              class="text-primary hover:text-primary-dark p-1 rounded-lg hover:bg-primary-light transition-colors"
              @click="playAudio"
            >
              🔊
            </button>
          </div>

          <div class="h-px bg-gray-100 dark:bg-slate-800 mb-3" />

          <!-- Meanings -->
          <div v-for="meaning in popup.entry.meanings.slice(0, 2)" :key="meaning.partOfSpeech" class="mb-2">
            <span class="text-xs text-gray-400 italic mr-1">({{ meaning.partOfSpeech }})</span>
            <span class="text-sm text-gray-900 dark:text-white font-medium">{{ meaning.definitionVi }}</span>
          </div>

          <!-- Collocations -->
          <div v-if="popup.entry.collocations.length" class="mt-2">
            <p class="text-xs text-gray-400 mb-1">Collocations</p>
            <div class="flex flex-wrap gap-1">
              <span
                v-for="col in popup.entry.collocations.slice(0, 3)"
                :key="col"
                class="text-xs bg-primary-light text-primary px-2 py-0.5 rounded-full"
              >
                {{ col }}
              </span>
            </div>
          </div>

          <div class="h-px bg-gray-100 dark:bg-slate-800 my-3" />

          <!-- Actions -->
          <div class="flex items-center gap-2">
            <button
              class="flex-1 text-xs bg-primary text-white px-3 py-1.5 rounded-lg hover:bg-primary-dark font-medium transition-colors"
              @click="dictionary.saveToSRS()"
            >
              + Lưu vào SRS
            </button>
            <button
              class="text-gray-400 hover:text-gray-600 p-1 rounded-lg hover:bg-gray-100"
              @click="dictionary.closePopup()"
            >
              ✕
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
defineOptions({ name: 'DictionaryPopup' })

const dictionary = useDictionary()
const { popup } = dictionary
const windowWidth = ref(1440)

onMounted(() => {
  windowWidth.value = window.innerWidth
  window.addEventListener('resize', () => { windowWidth.value = window.innerWidth })
  dictionary.init()
})

onUnmounted(() => dictionary.destroy())

function playAudio() {
  if (popup.entry?.audioUrl) new Audio(popup.entry.audioUrl).play()
}
</script>
