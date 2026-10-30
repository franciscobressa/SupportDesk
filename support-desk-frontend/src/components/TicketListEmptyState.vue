<script setup lang="ts">
import AppIcon from './icons/AppIcon.vue'

defineProps<{
  hasActiveFilters: boolean
}>()

const emit = defineEmits<{
  'clear-filters': []
  create: []
}>()
</script>

<template>
  <div class="px-6 py-16 text-center">
    <span class="mx-auto grid size-12 place-items-center rounded-2xl bg-accent-soft text-accent-fg">
      <AppIcon name="chat" class="size-6" />
    </span>
    <h3 class="mt-4 text-base font-bold text-fg-strong">
      {{ hasActiveFilters ? 'No matching tickets' : 'Your queue is clear' }}
    </h3>
    <p class="mx-auto mt-1 max-w-sm text-sm text-fg-subtle">
      {{ hasActiveFilters ? 'Try adjusting your filters or search terms.' : 'Create a ticket to start tracking a customer request.' }}
    </p>
    <button
      v-if="hasActiveFilters"
      type="button"
      class="mt-4 text-sm font-semibold text-accent-fg hover:text-accent-fg-strong"
      @click="emit('clear-filters')"
    >
      Clear filters
    </button>
    <button v-else type="button" class="button-primary mx-auto mt-5" @click="emit('create')">
      <span aria-hidden="true">+</span>
      Create your first ticket
    </button>
  </div>
</template>
