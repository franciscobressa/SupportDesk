<script setup lang="ts">
import type { TicketStatusChange } from '../types/tickets'
import { formatDate, formatStatus } from '../utils/formatters'

defineProps<{
  statusChanges: TicketStatusChange[]
}>()
</script>

<template>
  <section class="rounded-2xl border border-slate-200/80 bg-white p-5 shadow-sm sm:p-6">
    <div>
      <p class="text-xs font-bold uppercase tracking-[0.14em] text-indigo-600">Activity</p>
      <h2 class="mt-1 text-lg font-bold text-slate-900">Status history</h2>
      <p class="mt-1 text-xs text-slate-500">Changes to this ticket’s status.</p>
    </div>
    <div v-if="statusChanges.length" class="relative mt-6 space-y-6 before:absolute before:bottom-3 before:left-[7px] before:top-2 before:w-px before:bg-slate-200">
      <article v-for="change in [...statusChanges].reverse()" :key="change.id" class="relative flex gap-3">
        <span class="relative z-[1] mt-1 size-[15px] shrink-0 rounded-full border-[3px] border-white bg-indigo-500 ring-1 ring-indigo-200" />
        <div class="min-w-0">
          <p class="text-sm leading-6 text-slate-700">
            Changed status from <span class="font-semibold">{{ formatStatus(change.previousStatus) }}</span> to
            <span class="font-semibold">{{ formatStatus(change.newStatus) }}</span>
          </p>
          <p class="mt-1 text-xs text-slate-400">{{ change.author }}</p>
          <time class="mt-1 block text-[11px] text-slate-400">{{ formatDate(change.createdAt, { dateStyle: 'medium', timeStyle: 'short' }) }}</time>
        </div>
      </article>
    </div>
    <div v-else class="mt-5 rounded-xl border border-dashed border-slate-200 px-4 py-7 text-center">
      <span class="mx-auto grid size-9 place-items-center rounded-xl bg-slate-50 text-slate-400">
        <svg viewBox="0 0 24 24" fill="none" class="size-[18px]" aria-hidden="true">
          <circle cx="12" cy="12" r="8" stroke="currentColor" stroke-width="1.7" />
          <path d="M12 8v4l2.5 1.5" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" />
        </svg>
      </span>
      <p class="mt-3 text-sm font-medium text-slate-600">No status changes yet</p>
      <p class="mt-1 text-xs text-slate-400">Updates will appear here.</p>
    </div>
  </section>
</template>
