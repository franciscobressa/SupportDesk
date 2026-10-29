<script setup lang="ts">
import { ref } from 'vue'
import StatusBadge from './StatusBadge.vue'
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
  <div class="space-y-3 p-3 sm:hidden">
    <article
      v-for="ticket in tickets"
      :key="ticket.id"
      class="min-w-0 rounded-xl border border-slate-200/80 bg-white p-4 shadow-sm"
    >
      <RouterLink
        :to="{ name: 'ticket-details', params: { id: ticket.id } }"
        class="block rounded-sm focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
      >
        <h3 class="break-words text-base font-semibold leading-6 text-slate-900 hover:text-indigo-700">{{ ticket.title }}</h3>
      </RouterLink>
      <p
        :id="`ticket-description-${ticket.id}`"
        class="mt-2 break-words text-sm leading-5 text-slate-600"
        :class="expandedDescriptions.has(ticket.id) ? 'whitespace-pre-wrap' : 'line-clamp-3 whitespace-pre-wrap'"
      >{{ ticket.description }}</p>
      <button
        v-if="ticket.description.length > 120"
        type="button"
        class="mt-1 text-sm font-semibold text-indigo-600 hover:text-indigo-700 focus-visible:rounded-sm focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
        :aria-expanded="expandedDescriptions.has(ticket.id)"
        :aria-controls="`ticket-description-${ticket.id}`"
        @click="toggleDescription(ticket.id)"
      >
        {{ expandedDescriptions.has(ticket.id) ? 'Show less' : 'Read more' }}
      </button>

      <dl class="mt-4 grid grid-cols-2 gap-x-3 gap-y-4 border-t border-slate-100 pt-4">
        <div class="col-span-2 min-w-0">
          <dt class="text-[11px] font-semibold uppercase tracking-wide text-slate-400">Customer</dt>
          <dd class="mt-1 break-words text-sm font-medium text-slate-700">{{ ticket.customerName || 'Customer' }}</dd>
          <dd class="mt-0.5 break-all text-xs text-slate-500">{{ ticket.customerEmail || '—' }}</dd>
        </div>
        <div class="min-w-0">
          <dt class="text-[11px] font-semibold uppercase tracking-wide text-slate-400">Status</dt>
          <dd class="mt-1"><StatusBadge :status="ticket.status" /></dd>
        </div>
        <div class="min-w-0">
          <dt class="text-[11px] font-semibold uppercase tracking-wide text-slate-400">Priority</dt>
          <dd class="mt-1"><StatusBadge :priority="ticket.priority" /></dd>
        </div>
        <div class="col-span-2 min-w-0">
          <dt class="text-[11px] font-semibold uppercase tracking-wide text-slate-400">Created</dt>
          <dd class="mt-1 break-words text-sm text-slate-600">{{ formatDate(ticket.createdAt) }}</dd>
        </div>
      </dl>

      <div class="mt-4 flex items-center justify-between gap-3 border-t border-slate-100 pt-3">
        <RouterLink
          :to="{ name: 'ticket-details', params: { id: ticket.id } }"
          class="button-secondary min-h-10 flex-1 justify-center"
        >
          View ticket
        </RouterLink>
        <button
          type="button"
          class="button-icon size-10 shrink-0 border-transparent text-slate-500 hover:border-rose-100 hover:bg-rose-50 hover:text-rose-600"
          :aria-label="`Delete ${ticket.title}`"
          @click="emit('delete', ticket)"
        >
          <svg viewBox="0 0 24 24" fill="none" class="size-[17px]" aria-hidden="true">
            <path d="M4 7h16M10 11v6m4-6v6M5.5 7l1 13h11l1-13M9 7V4h6v3" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
        </button>
      </div>
    </article>
  </div>

  <div class="hidden overflow-x-auto sm:block">
    <table class="w-full min-w-[790px] border-collapse text-left">
      <thead>
        <tr class="bg-slate-50/80 text-[11px] font-bold uppercase tracking-[0.11em] text-slate-400">
          <th scope="col" class="px-6 py-3.5">Ticket</th>
          <th scope="col" class="px-5 py-3.5">Customer</th>
          <th scope="col" class="px-5 py-3.5">Status</th>
          <th scope="col" class="px-5 py-3.5">Priority</th>
          <th scope="col" class="px-5 py-3.5">Created</th>
          <th scope="col" class="px-5 py-3.5"><span class="sr-only">Actions</span></th>
        </tr>
      </thead>
      <tbody class="divide-y divide-slate-100">
        <tr v-for="ticket in tickets" :key="ticket.id" class="group transition-colors hover:bg-slate-50/70">
          <td class="px-6 py-4">
            <RouterLink :to="{ name: 'ticket-details', params: { id: ticket.id } }" class="block max-w-[380px]">
              <p class="truncate text-sm font-semibold text-slate-800 group-hover:text-indigo-700">{{ ticket.title }}</p>
              <p class="mt-1 truncate text-xs text-slate-500">{{ ticket.description }}</p>
            </RouterLink>
          </td>
          <td class="px-5 py-4">
            <div class="flex items-center gap-2.5">
              <span class="grid size-8 shrink-0 place-items-center rounded-full bg-indigo-50 text-[10px] font-bold text-indigo-700">{{ getInitials(ticket.customerName) }}</span>
              <div class="min-w-0">
                <p class="max-w-40 truncate text-xs font-semibold text-slate-700">{{ ticket.customerName || 'Customer' }}</p>
                <p class="max-w-40 truncate text-[11px] text-slate-400">{{ ticket.customerEmail || '—' }}</p>
              </div>
            </div>
          </td>
          <td class="px-5 py-4"><StatusBadge :status="ticket.status" /></td>
          <td class="px-5 py-4"><StatusBadge :priority="ticket.priority" /></td>
          <td class="whitespace-nowrap px-5 py-4 text-xs text-slate-500">{{ formatDate(ticket.createdAt) }}</td>
          <td class="px-5 py-4 text-right">
            <div class="inline-flex items-center gap-1">
              <RouterLink
                :to="{ name: 'ticket-details', params: { id: ticket.id } }"
                class="button-icon size-8 border-transparent opacity-70 hover:border-slate-200 hover:bg-white hover:opacity-100"
                :aria-label="`View ${ticket.title}`"
              >
                <svg viewBox="0 0 24 24" fill="none" class="size-[17px]" aria-hidden="true">
                  <path d="M2.5 12s3.2-6 9.5-6 9.5 6 9.5 6-3.2 6-9.5 6-9.5-6-9.5-6Z" stroke="currentColor" stroke-width="1.7" />
                  <circle cx="12" cy="12" r="2.5" stroke="currentColor" stroke-width="1.7" />
                </svg>
              </RouterLink>
              <button
                type="button"
                class="button-icon size-8 border-transparent text-slate-400 opacity-70 hover:border-rose-100 hover:bg-rose-50 hover:text-rose-600 hover:opacity-100"
                :aria-label="`Delete ${ticket.title}`"
                @click="emit('delete', ticket)"
              >
                <svg viewBox="0 0 24 24" fill="none" class="size-[17px]" aria-hidden="true">
                  <path d="M4 7h16M10 11v6m4-6v6M5.5 7l1 13h11l1-13M9 7V4h6v3" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
              </button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
