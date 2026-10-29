<script setup lang="ts">
import { computed } from 'vue'
import type { TicketPriority, TicketStatus } from '../types/tickets'
import { formatStatus } from '../utils/formatters'

const props = defineProps<{
  status?: TicketStatus
  priority?: TicketPriority
}>()

const priorityClasses: Record<TicketPriority, string> = {
  Low: 'bg-slate-100 text-slate-600 ring-slate-200',
  Medium: 'bg-amber-50 text-amber-700 ring-amber-200',
  High: 'bg-rose-50 text-rose-700 ring-rose-200',
}

const statusClasses: Record<TicketStatus, string> = {
  Open: 'bg-blue-50 text-blue-700 ring-blue-200',
  InProgress: 'bg-violet-50 text-violet-700 ring-violet-200',
  Resolved: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  Closed: 'bg-slate-100 text-slate-600 ring-slate-200',
}

const label = computed(() => (props.priority ? props.priority : formatStatus(props.status ?? 'Open')))
const badgeClass = computed(() => {
  return props.priority ? priorityClasses[props.priority] : statusClasses[props.status ?? 'Open']
})
</script>

<template>
  <span
    class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold ring-1 ring-inset"
    :class="badgeClass"
  >
    <span v-if="priority" class="size-1.5 rounded-full bg-current" />
    {{ label }}
  </span>
</template>
