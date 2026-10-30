<script setup lang="ts">
import type { TicketDetails } from '../types/tickets'
import { getInitials } from '../utils/formatters'

defineProps<{
  ticket: TicketDetails
}>()
</script>

<template>
  <section class="rounded-2xl border border-line/80 bg-surface p-5 shadow-sm sm:p-6">
    <div>
      <p class="text-xs font-bold uppercase tracking-[0.14em] text-accent-fg">Ticket information</p>
      <h2 class="mt-1 text-lg font-bold text-fg-strong">Request details</h2>
    </div>
    <div class="mt-5 rounded-xl bg-surface-subtle/80 p-4 sm:p-5">
      <p class="whitespace-pre-wrap text-sm leading-7 text-fg-muted">{{ ticket.description }}</p>
    </div>
    <div class="mt-6 grid gap-5 border-t border-line-subtle pt-5 sm:grid-cols-2">
      <div>
        <p class="text-xs font-semibold uppercase tracking-wide text-fg-faint">Customer</p>
        <div class="mt-2 flex items-center gap-3">
          <span class="grid size-10 place-items-center rounded-full bg-accent-soft text-xs font-bold text-accent-fg-strong">{{ getInitials(ticket.customerName) }}</span>
          <div class="min-w-0">
            <p class="truncate text-sm font-semibold text-fg">{{ ticket.customerName || 'Customer' }}</p>
            <a
              v-if="ticket.customerEmail"
              :href="`mailto:${ticket.customerEmail}`"
              class="mt-0.5 block truncate text-xs text-accent-fg hover:underline"
            >{{ ticket.customerEmail }}</a>
          </div>
        </div>
      </div>
      <div>
        <p class="text-xs font-semibold uppercase tracking-wide text-fg-faint">Ticket ID</p>
        <p class="mt-3 break-all font-mono text-xs text-fg-muted">{{ ticket.id }}</p>
      </div>
    </div>
  </section>
</template>
