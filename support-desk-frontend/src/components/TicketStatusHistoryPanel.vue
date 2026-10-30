<script setup lang="ts">
import AppIcon from './icons/AppIcon.vue'
import type { TicketStatusChange } from '../types/tickets'
import { formatDate, formatStatus } from '../utils/formatters'

defineProps<{
  statusChanges: TicketStatusChange[]
}>()
</script>

<template>
  <section class="rounded-2xl border border-line/80 bg-surface p-5 shadow-sm sm:p-6">
    <div>
      <p class="text-xs font-bold uppercase tracking-[0.14em] text-accent-fg">Activity</p>
      <h2 class="mt-1 text-lg font-bold text-fg-strong">Status history</h2>
      <p class="mt-1 text-xs text-fg-subtle">Changes to this ticket’s status.</p>
    </div>
    <div v-if="statusChanges.length" class="relative mt-6 space-y-6 before:absolute before:bottom-3 before:left-[7px] before:top-2 before:w-px before:bg-line">
      <article v-for="change in [...statusChanges].reverse()" :key="change.id" class="relative flex gap-3">
        <span class="relative z-1 mt-1 size-3.75 shrink-0 rounded-full border-[3px] border-surface bg-accent ring-1 ring-accent-line" />
        <div class="min-w-0">
          <p class="text-sm leading-6 text-fg-soft">
            Changed status from <span class="font-semibold">{{ formatStatus(change.previousStatus) }}</span> to
            <span class="font-semibold">{{ formatStatus(change.newStatus) }}</span>
          </p>
          <p class="mt-1 text-xs text-fg-faint">{{ change.author }}</p>
          <time class="mt-1 block text-[11px] text-fg-faint">{{ formatDate(change.createdAt, { dateStyle: 'medium', timeStyle: 'short' }) }}</time>
        </div>
      </article>
    </div>
    <div v-else class="mt-5 rounded-xl border border-dashed border-line px-4 py-7 text-center">
      <span class="mx-auto grid size-9 place-items-center rounded-xl bg-surface-subtle text-fg-faint">
        <AppIcon name="clock" class="size-4.5" />
      </span>
      <p class="mt-3 text-sm font-medium text-fg-muted">No status changes yet</p>
      <p class="mt-1 text-xs text-fg-faint">Updates will appear here.</p>
    </div>
  </section>
</template>
