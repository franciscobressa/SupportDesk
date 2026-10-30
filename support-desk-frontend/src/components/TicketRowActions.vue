<script setup lang="ts">
import { computed } from 'vue'
import type { Ticket } from '../types/tickets'
import AppIcon from './icons/AppIcon.vue'

const props = defineProps<{
  ticket: Ticket
  // "table": compact icon buttons in a desktop row. "card": larger touch targets with a labelled View button.
  layout: 'table' | 'card'
}>()

const emit = defineEmits<{
  delete: [ticket: Ticket]
}>()

const detailsLocation = computed(() => ({ name: 'ticket-details', params: { id: props.ticket.id } }))
const editLocation = computed(() => ({ ...detailsLocation.value, query: { edit: '1' } }))
</script>

<template>
  <div v-if="layout === 'table'" class="inline-flex items-center gap-1">
    <RouterLink
      :to="detailsLocation"
      class="button-icon size-8 border-transparent opacity-70 hover:border-line hover:bg-surface hover:opacity-100"
      :aria-label="`View ${ticket.title}`"
    >
      <AppIcon name="eye" class="size-4.25" />
    </RouterLink>
    <RouterLink
      :to="editLocation"
      class="button-icon size-8 border-transparent opacity-70 hover:border-line hover:bg-surface hover:opacity-100"
      :aria-label="`Edit ${ticket.title}`"
    >
      <AppIcon name="pencil" class="size-4.25" />
    </RouterLink>
    <button
      type="button"
      class="button-icon size-8 border-transparent text-fg-faint opacity-70 hover:border-danger-line hover:bg-danger-soft hover:text-danger-fg hover:opacity-100"
      :aria-label="`Delete ${ticket.title}`"
      @click="emit('delete', ticket)"
    >
      <AppIcon name="trash" class="size-4.25" />
    </button>
  </div>

  <div v-else class="mt-4 flex items-center justify-between gap-3 border-t border-line-subtle pt-3">
    <RouterLink :to="detailsLocation" class="button-secondary min-h-10 flex-1 justify-center">
      View ticket
    </RouterLink>
    <RouterLink
      :to="editLocation"
      class="button-icon size-10 shrink-0 border-transparent text-fg-subtle hover:border-line hover:bg-surface-hover hover:text-accent-fg-strong"
      :aria-label="`Edit ${ticket.title}`"
    >
      <AppIcon name="pencil" class="size-4.25" />
    </RouterLink>
    <button
      type="button"
      class="button-icon size-10 shrink-0 border-transparent text-fg-subtle hover:border-danger-line hover:bg-danger-soft hover:text-danger-fg"
      :aria-label="`Delete ${ticket.title}`"
      @click="emit('delete', ticket)"
    >
      <AppIcon name="trash" class="size-4.25" />
    </button>
  </div>
</template>
