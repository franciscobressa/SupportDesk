<script setup lang="ts">
import { ref } from 'vue'
import StatusBadge from './StatusBadge.vue'
import TicketRowActions from './TicketRowActions.vue'
import type { Ticket } from '../types/tickets'
import { formatDate, getInitials } from '../utils/formatters'

defineProps<{
  tickets: Ticket[]
}>()

const emit = defineEmits<{
  delete: [ticket: Ticket]
}>()

const expandedDescriptions = ref(new Set<string>())

function toggleDescription(ticketId: string): void {
  const expanded = new Set(expandedDescriptions.value)
  if (expanded.has(ticketId)) {
    expanded.delete(ticketId)
  } else {
    expanded.add(ticketId)
  }
  expandedDescriptions.value = expanded
}
</script>

<template>
  <!-- Cards below xl: with the sidebar open, a 6-column table only fits from 1280px without squeezing the title. -->
  <div class="grid gap-3 p-3 md:grid-cols-2 xl:hidden">
    <article
      v-for="ticket in tickets"
      :key="ticket.id"
      class="min-w-0 rounded-xl border border-line/80 bg-surface p-4 shadow-sm"
    >
      <RouterLink
        :to="{ name: 'ticket-details', params: { id: ticket.id } }"
        class="block rounded-sm focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-accent"
      >
        <h3 class="wrap-break-word text-base font-semibold leading-6 text-fg-strong hover:text-accent-fg-strong">{{ ticket.title }}</h3>
      </RouterLink>
      <p
        :id="`ticket-description-${ticket.id}`"
        class="mt-2 whitespace-pre-wrap wrap-break-word text-sm leading-5 text-fg-muted"
        :class="{ 'line-clamp-3': !expandedDescriptions.has(ticket.id) }"
      >{{ ticket.description }}</p>
      <button
        v-if="ticket.description.length > 120"
        type="button"
        class="mt-1 text-sm font-semibold text-accent-fg hover:text-accent-fg-strong focus-visible:rounded-sm focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-accent"
        :aria-expanded="expandedDescriptions.has(ticket.id)"
        :aria-controls="`ticket-description-${ticket.id}`"
        @click="toggleDescription(ticket.id)"
      >
        {{ expandedDescriptions.has(ticket.id) ? 'Show less' : 'Read more' }}
      </button>

      <dl class="mt-4 grid grid-cols-2 gap-x-3 gap-y-4 border-t border-line-subtle pt-4">
        <div class="col-span-2 min-w-0">
          <dt class="text-[11px] font-semibold uppercase tracking-wide text-fg-faint">Customer</dt>
          <dd class="mt-1 wrap-break-word text-sm font-medium text-fg-soft">{{ ticket.customerName || 'Customer' }}</dd>
          <dd class="mt-0.5 break-all text-xs text-fg-subtle">{{ ticket.customerEmail || '—' }}</dd>
        </div>
        <div class="min-w-0">
          <dt class="text-[11px] font-semibold uppercase tracking-wide text-fg-faint">Status</dt>
          <dd class="mt-1"><StatusBadge :status="ticket.status" /></dd>
        </div>
        <div class="min-w-0">
          <dt class="text-[11px] font-semibold uppercase tracking-wide text-fg-faint">Priority</dt>
          <dd class="mt-1"><StatusBadge :priority="ticket.priority" /></dd>
        </div>
        <div class="col-span-2 min-w-0">
          <dt class="text-[11px] font-semibold uppercase tracking-wide text-fg-faint">Created</dt>
          <dd class="mt-1 wrap-break-word text-sm text-fg-muted">{{ formatDate(ticket.createdAt) }}</dd>
        </div>
      </dl>

      <TicketRowActions :ticket="ticket" layout="card" @delete="emit('delete', $event)" />
    </article>
  </div>

  <div class="hidden overflow-x-auto xl:block">
    <table class="w-full min-w-197.5 border-collapse text-left">
      <thead>
        <tr class="bg-surface-subtle/80 text-[11px] font-bold uppercase tracking-[0.11em] text-fg-faint">
          <th scope="col" class="px-6 py-3.5">Ticket</th>
          <th scope="col" class="px-5 py-3.5">Customer</th>
          <th scope="col" class="px-5 py-3.5">Status</th>
          <th scope="col" class="px-5 py-3.5">Priority</th>
          <th scope="col" class="px-5 py-3.5">Created</th>
          <th scope="col" class="px-5 py-3.5"><span class="sr-only">Actions</span></th>
        </tr>
      </thead>
      <tbody class="divide-y divide-line-subtle">
        <tr v-for="ticket in tickets" :key="ticket.id" class="group transition-colors hover:bg-surface-hover/70">
          <!-- w-full + max-w-0: the title column takes the leftover width and truncates, so the other columns never get pushed off-screen. -->
          <td class="w-full max-w-0 px-6 py-4">
            <RouterLink :to="{ name: 'ticket-details', params: { id: ticket.id } }" class="block">
              <p class="truncate text-sm font-semibold text-fg group-hover:text-accent-fg-strong">{{ ticket.title }}</p>
              <p class="mt-1 truncate text-xs text-fg-subtle">{{ ticket.description }}</p>
            </RouterLink>
          </td>
          <td class="px-5 py-4">
            <div class="flex items-center gap-2.5">
              <span class="grid size-8 shrink-0 place-items-center rounded-full bg-accent-soft text-[10px] font-bold text-accent-fg-strong">{{ getInitials(ticket.customerName) }}</span>
              <div class="min-w-0">
                <p class="max-w-40 truncate text-xs font-semibold text-fg-soft">{{ ticket.customerName || 'Customer' }}</p>
                <p class="max-w-40 truncate text-[11px] text-fg-faint">{{ ticket.customerEmail || '—' }}</p>
              </div>
            </div>
          </td>
          <td class="px-5 py-4"><StatusBadge :status="ticket.status" /></td>
          <td class="px-5 py-4"><StatusBadge :priority="ticket.priority" /></td>
          <td class="whitespace-nowrap px-5 py-4 text-xs text-fg-subtle">{{ formatDate(ticket.createdAt) }}</td>
          <td class="px-5 py-4 text-right">
            <TicketRowActions :ticket="ticket" layout="table" @delete="emit('delete', $event)" />
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
