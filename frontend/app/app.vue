<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
</template>

<script setup lang="ts">
useHead({
  titleTemplate: (title) => title ? `${title} — BaoDo` : 'BaoDo — Luyện Thi TOEIC',
})
</script>
