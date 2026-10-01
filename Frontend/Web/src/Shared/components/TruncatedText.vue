<script setup lang="ts">
withDefaults(defineProps<{ text: string; maxWidth?: string }>(), {
  maxWidth: '14rem',
})
</script>

<template>
  <span class="truncated-text" :style="{ maxWidth }" :title="text">{{ text }}</span>
</template>

<style scoped>
.truncated-text {
  display: block;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>