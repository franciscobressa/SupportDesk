<script setup lang="ts">
import { computed } from 'vue'
import type { TicketPriority, TicketStatus } from '../types/tickets'
import { formatStatus } from '../utils/formatters'

const props = defineProps<{
  status?: TicketStatus
  priority?: TicketPriority
}>()

// State colors carry meaning, so they stay explicit per value (with their dark variants)
// instead of going through the neutral theme tokens.
const neutral = 'bg-surface-muted text-fg-muted ring-line dark:ring-line-strong'

const priorityClasses: Record<TicketPriority, string> = {
  Low: neutral,
  Medium: 'bg-amber-50 text-amber-700 ring-amber-200 dark:bg-amber-950 dark:text-amber-200 dark:ring-amber-700',
  High: 'bg-rose-50 text-rose-700 ring-rose-200 dark:bg-rose-950 dark:text-rose-300 dark:ring-rose-700',
}

const statusClasses: Record<TicketStatus, string> = {
  Open: 'bg-blue-50 text-blue-700 ring-blue-200 dark:bg-blue-950 dark:text-blue-200 dark:ring-blue-700',
  InProgress: 'bg-violet-50 text-violet-700 ring-violet-200 dark:bg-violet-950 dark:text-violet-200 dark:ring-violet-600',
  Resolved: 'bg-emerald-50 text-emerald-700 ring-emerald-200 dark:bg-emerald-900 dark:text-emerald-200 dark:ring-emerald-600',
  Closed: neutral,
}

const label = computed(() => (props.priority ? props.priority : formatStatus(props.status ?? 'Open')))
const badgeClass = computed(() => {
  return props.priority ? priorityClasses[props.priority] : statusClasses[props.status ?? 'Open']
})
</script>

<template>
  <span
    class="inline-flex items-center gap-1.5 whitespace-nowrap rounded-full px-2.5 py-1 text-xs font-semibold ring-1 ring-inset"
    :class="badgeClass"
  >
    <span v-if="priority" class="size-1.5 rounded-full bg-current" />
    {{ label }}
  </span>
</template>
