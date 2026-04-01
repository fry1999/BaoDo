import type { DictionaryEntry } from '~/types'

interface PopupState {
  visible: boolean
  entry: DictionaryEntry | null
  x: number
  y: number
  isLoading: boolean
}

export function useDictionary() {
  const config = useRuntimeConfig()
  const auth = useAuthStore()
  const vocabStore = useVocabularyStore()

  const popup = reactive<PopupState>({
    visible: false,
    entry: null,
    x: 0,
    y: 0,
    isLoading: false,
  })

  const cache = new Map<string, DictionaryEntry>()

  async function lookup(word: string) {
    const clean = word.trim().toLowerCase()
    if (!clean || clean.split(' ').length > 5) return

    popup.isLoading = true
    popup.entry = null
    popup.visible = true

    try {
      if (cache.has(clean)) {
        popup.entry = cache.get(clean)!
        return
      }
      const res = await $fetch<DictionaryEntry>(`${config.public.apiBaseUrl}/api/dictionary/lookup`, {
        query: { word: clean },
        headers: auth.token ? { Authorization: `Bearer ${auth.token}` } : {},
      })
      cache.set(clean, res)
      popup.entry = res
    } catch {
      popup.visible = false
    } finally {
      popup.isLoading = false
    }
  }

  function handleMouseUp(event: MouseEvent) {
    const selection = window.getSelection()
    const text = selection?.toString().trim() ?? ''
    if (!text || text.length > 100) return

    const rect = window.getSelection()?.getRangeAt(0).getBoundingClientRect()
    if (!rect) return

    popup.x = rect.left + window.scrollX
    popup.y = rect.top + window.scrollY - 8
    lookup(text)
  }

  function closePopup() {
    popup.visible = false
    popup.entry = null
  }

  async function saveToSRS() {
    if (!popup.entry || !auth.isAuthenticated) return
    await $fetch(`${config.public.apiBaseUrl}/api/vocabulary/save`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${auth.token}` },
      body: { word: popup.entry.word },
    })
    closePopup()
  }

  function init() {
    if (import.meta.client) {
      document.addEventListener('mouseup', handleMouseUp)
    }
  }

  function destroy() {
    if (import.meta.client) {
      document.removeEventListener('mouseup', handleMouseUp)
    }
  }

  return { popup, lookup, closePopup, saveToSRS, init, destroy }
}
