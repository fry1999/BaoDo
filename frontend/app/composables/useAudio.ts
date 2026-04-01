export function useAudio(src: MaybeRef<string>) {
  const audio = ref<HTMLAudioElement | null>(null)
  const isPlaying = ref(false)
  const duration = ref(0)
  const currentTime = ref(0)
  const playbackRate = ref(1)

  function init() {
    if (!import.meta.client) return
    const srcVal = unref(src)
    if (!srcVal) return

    audio.value = new Audio(srcVal)

    audio.value.addEventListener('loadedmetadata', () => {
      duration.value = audio.value?.duration ?? 0
    })
    audio.value.addEventListener('timeupdate', () => {
      currentTime.value = audio.value?.currentTime ?? 0
    })
    audio.value.addEventListener('ended', () => {
      isPlaying.value = false
    })
  }

  function play() {
    audio.value?.play()
    isPlaying.value = true
  }

  function pause() {
    audio.value?.pause()
    isPlaying.value = false
  }

  function toggle() {
    isPlaying.value ? pause() : play()
  }

  function seekTo(seconds: number) {
    if (audio.value) audio.value.currentTime = seconds
  }

  function playSegment(start: number, end: number) {
    seekTo(start)
    play()
    const checkInterval = setInterval(() => {
      if ((audio.value?.currentTime ?? 0) >= end) {
        pause()
        clearInterval(checkInterval)
      }
    }, 100)
  }

  function setRate(rate: number) {
    playbackRate.value = rate
    if (audio.value) audio.value.playbackRate = rate
  }

  onMounted(() => init())
  onUnmounted(() => {
    pause()
    audio.value = null
  })

  const progressPercent = computed(() =>
    duration.value > 0 ? (currentTime.value / duration.value) * 100 : 0,
  )

  return { isPlaying, duration, currentTime, progressPercent, play, pause, toggle, seekTo, playSegment, setRate, playbackRate }
}
