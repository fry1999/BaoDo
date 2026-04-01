export function useExamTimer() {
  const examStore = useExamStore()
  let intervalId: ReturnType<typeof setInterval> | null = null

  function start() {
    if (intervalId) return
    intervalId = setInterval(() => {
      examStore.tickTimer()
    }, 1000)
  }

  function pause() {
    if (intervalId) {
      clearInterval(intervalId)
      intervalId = null
    }
  }

  function stop() {
    pause()
  }

  onUnmounted(() => stop())

  return {
    start,
    pause,
    stop,
    timeRemaining: computed(() => examStore.timeRemaining),
    formattedTime: computed(() => examStore.formattedTime),
    timerStatus: computed(() => examStore.timerStatus),
  }
}
