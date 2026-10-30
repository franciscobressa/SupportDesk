<script setup lang="ts">
import { computed } from 'vue'
import type { TicketPriority, TicketStatus, TicketSummary } from '../types/tickets'

const props = defineProps<{
  summary: TicketSummary
  activeStatus: TicketStatus | ''
  activePriority: TicketPriority | ''
}>()

const emit = defineEmits<{
  'toggle-status': [status: TicketStatus]
  'toggle-high-priority': []
}>()

interface SummaryCard {
  key: string
  label: string
  hint: string
  count: number
  active: boolean
  highlight: 'accent' | 'amber'
  toggle: () => void
}

const statusCards: { status: TicketStatus; label: string; hint: string; count: keyof TicketSummary }[] = [
  { status: 'Open', label: 'Open', hint: 'View open tickets', count: 'open' },
  { status: 'InProgress', label: 'In progress', hint: 'View active work', count: 'inProgress' },
  { status: 'Resolved', label: 'Resolved', hint: 'View resolved tickets', count: 'resolved' },
]

// Counts are queue-wide; each card is also a shortcut for the matching list filter.
const cards = computed<SummaryCard[]>(() => [
  ...statusCards.map((card) => ({
    key: card.status,
    label: card.label,
    hint: card.hint,
    count: props.summary[card.count],
    active: props.activeStatus === card.status,
    highlight: 'accent' as const,
    toggle: () => emit('toggle-status', card.status),
  })),
  {
    key: 'high-priority',
    label: 'High priority',
    hint: 'View urgent tickets',
    count: props.summary.highPriority,
    active: props.activePriority === 'High',
    highlight: 'amber',
    toggle: () => emit('toggle-high-priority'),
  },
])

const highlightClasses = {
  accent: {
    active: 'border-accent-line bg-accent-soft/70 ring-1 ring-accent-line',
    hover: 'hover:border-accent-line',
  },
  amber: {
    active: 'border-amber-300 bg-amber-50/70 ring-1 ring-amber-200 dark:border-amber-800 dark:bg-amber-950 dark:ring-amber-700',
    hover: 'hover:border-amber-200 dark:hover:border-amber-800',
  },
}
</script>

<template>
  <section class="grid grid-cols-2 gap-3 lg:grid-cols-4" aria-label="Ticket queue summary">
    <button
      v-for="card in cards"
      :key="card.key"
      type="button"
      class="rounded-2xl border p-4 text-left shadow-sm transition hover:shadow-md focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-accent sm:p-5"
      :class="[
        highlightClasses[card.highlight].hover,
        card.active ? highlightClasses[card.highlight].active : 'border-line/80 bg-surface',
      ]"
      :aria-pressed="card.active"
      @click="card.toggle"
    >
      <span class="text-sm font-medium text-fg-subtle">{{ card.label }}</span>
      <span class="mt-2 block text-2xl font-bold tracking-tight text-fg-strong">{{ card.count }}</span>
      <span class="mt-1 block text-xs text-fg-faint">{{ card.hint }}</span>
    </button>
  </section>
</template>
