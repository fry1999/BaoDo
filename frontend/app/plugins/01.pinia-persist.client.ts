import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import { type Pinia } from 'pinia'

// Runs FIRST (prefix 01) — must register before any store is accessed
export default defineNuxtPlugin((nuxtApp) => {
  const pinia = nuxtApp.$pinia as Pinia
  pinia.use(piniaPluginPersistedstate)
})
