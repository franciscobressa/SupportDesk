<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import AppShell from '../components/AppShell.vue'
import BaseDialog from '../components/BaseDialog.vue'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import TicketFilters from '../components/TicketFilters.vue'
import TicketForm from '../components/TicketForm.vue'
import TicketTable from '../components/TicketTable.vue'
import { useTicketsStore } from '../stores/tickets'
import type { Ticket, TicketListFilters, TicketWriteRequest } from '../types/tickets'

const router = useRouter()
const ticketsStore = useTicketsStore()
const filters = reactive<TicketListFilters>({
  search: '',
  status: '',
  priority: '',
  sortBy: 'createdAt',
  sortDirection: 'desc',
})
const isCreateOpen = ref(false)
const ticketToDelete = ref<Ticket | null>(null)
const actionError = ref('')
const currentHour = ref(new Date().getHours())
const greeting = computed(() => {
  if (currentHour.value < 12) return 'Good morning'
  if (currentHour.value < 18) return 'Good afternoon'
  return 'Good evening'
})
let filterTimer: number | undefined
let greetingTimer: number | undefined

const tickets = computed<Ticket[]>(() => ticketsStore.tickets)
const hasActiveFilters = computed(() => Boolean(filters.search || filters.status || filters.priority))
const sortLabel = computed(() => filters.sortBy === 'createdAt' ? 'created date' : filters.sortBy)

async function fetchTickets(): Promise<void> {
  try {
    await ticketsStore.fetchTickets(filters)
  } catch {
    // The store exposes the request error in the list state.
  }
}

function clearFilters(): void {
  Object.assign(filters, {
    search: '',
    status: '',
    priority: '',
    sortBy: 'createdAt',
    sortDirection: 'desc',
  })
}

function updateFilters(nextFilters: TicketListFilters): void {
  Object.assign(filters, nextFilters)
}

async function createTicket(ticket: TicketWriteRequest): Promise<void> {
  actionError.value = ''

  try {
    const created = await ticketsStore.addTicket(ticket)
    isCreateOpen.value = false
    await router.push({ name: 'ticket-details', params: { id: created.id } })
  } catch (error: unknown) {
    actionError.value = getErrorMessage(error)
  }
}

async function deleteTicket(): Promise<void> {
  if (!ticketToDelete.value) {
    return
  }

  actionError.value = ''
  try {
    await ticketsStore.removeTicket(ticketToDelete.value.id)
    ticketToDelete.value = null
  } catch (error: unknown) {
    actionError.value = getErrorMessage(error)
  }
}

watch(
  filters,
  () => {
    window.clearTimeout(filterTimer)
    filterTimer = window.setTimeout(() => {
      void fetchTickets()
    }, 250)
  },
  { deep: true },
)

function getErrorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'An unexpected error occurred.'
}

onMounted(() => {
  void fetchTickets()
  greetingTimer = window.setInterval(() => {
    currentHour.value = new Date().getHours()
  }, 60_000)
})

onBeforeUnmount(() => {
  window.clearTimeout(filterTimer)
  window.clearInterval(greetingTimer)
})
</script>

<template>
  <AppShell page-title="Ticket dashboard">
    <main class="mx-auto max-w-[1440px] px-5 py-8 sm:px-8 lg:px-10 lg:py-10">
      <section class="flex flex-col justify-between gap-5 sm:flex-row sm:items-end">
        <div>
          <p class="text-xs font-bold uppercase tracking-[0.16em] text-indigo-600">Overview</p>
          <h1 class="mt-2 text-3xl font-bold tracking-tight text-slate-900">{{ greeting }}, Manager <span aria-hidden="true">✦</span></h1>
          <p class="mt-2 text-sm text-slate-500">Here’s what’s happening with your support queue today.</p>
        </div>
        <button type="button" class="button-primary self-start sm:self-auto" @click="isCreateOpen = true; actionError = ''">
          <svg viewBox="0 0 24 24" fill="none" class="size-[18px]" aria-hidden="true">
            <path d="M12 5v14M5 12h14" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
          </svg>
          Create ticket
        </button>
      </section>

      <section class="mt-8 overflow-hidden rounded-2xl border border-slate-200/80 bg-white shadow-sm shadow-slate-200/30">
        <div class="flex flex-col gap-4 border-b border-slate-100 p-5 sm:p-6">
          <div class="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
            <div>
              <h2 class="text-lg font-bold text-slate-900">All tickets <span class="ml-1 rounded-full bg-slate-100 px-2 py-0.5 text-xs font-semibold text-slate-600">{{ tickets.length }}</span></h2>
              <p class="mt-1 text-sm text-slate-500">Manage and keep track of your customer requests.</p>
            </div>
            <button type="button" class="button-secondary self-start sm:self-auto" :disabled="ticketsStore.listLoading" @click="fetchTickets">
              <svg viewBox="0 0 24 24" fill="none" class="size-4" :class="{ 'animate-spin': ticketsStore.listLoading }" aria-hidden="true">
                <path d="M20 11a8 8 0 0 0-14.9-3M4 5v3h3m-3 5a8 8 0 0 0 14.9 3M20 19v-3h-3" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" />
              </svg>
              Refresh
            </button>
          </div>
          <TicketFilters :filters="filters" @update:filters="updateFilters" />
        </div>

        <div v-if="actionError || ticketsStore.listError" class="mx-5 mt-5 flex items-start justify-between gap-3 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700 sm:mx-6" role="alert">
          <p>{{ actionError || ticketsStore.listError }}</p>
          <button type="button" class="font-semibold underline underline-offset-2" @click="fetchTickets">Try again</button>
        </div>

        <div v-if="ticketsStore.listLoading && !tickets.length" class="space-y-3 p-6" aria-label="Loading tickets">
          <div v-for="item in 5" :key="item" class="h-16 animate-pulse rounded-xl bg-slate-100" />
        </div>

        <div v-else-if="!tickets.length && !ticketsStore.listError" class="px-6 py-16 text-center">
          <span class="mx-auto grid size-12 place-items-center rounded-2xl bg-indigo-50 text-indigo-600">
            <svg viewBox="0 0 24 24" fill="none" class="size-6" aria-hidden="true">
              <path d="M4 5.75A1.75 1.75 0 0 1 5.75 4h12.5A1.75 1.75 0 0 1 20 5.75v8.5A1.75 1.75 0 0 1 18.25 16H13l-4.5 4v-4h-2.75A1.75 1.75 0 0 1 4 14.25v-8.5Z" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round" />
            </svg>
          </span>
          <h3 class="mt-4 text-base font-bold text-slate-900">{{ hasActiveFilters ? 'No matching tickets' : 'Your queue is clear' }}</h3>
          <p class="mx-auto mt-1 max-w-sm text-sm text-slate-500">
            {{ hasActiveFilters ? 'Try adjusting your filters or search terms.' : 'Create a ticket to start tracking a customer request.' }}
          </p>
          <button v-if="hasActiveFilters" type="button" class="mt-4 text-sm font-semibold text-indigo-600 hover:text-indigo-700" @click="clearFilters">
            Clear filters
          </button>
          <button v-else type="button" class="button-primary mx-auto mt-5" @click="isCreateOpen = true">
            <span aria-hidden="true">+</span>
            Create your first ticket
          </button>
        </div>

        <TicketTable v-else-if="tickets.length" :tickets="tickets" @delete="ticketToDelete = $event; actionError = ''" />

        <div v-if="tickets.length" class="flex flex-col justify-between gap-2 border-t border-slate-100 px-5 py-4 text-xs text-slate-500 sm:flex-row sm:items-center sm:px-6">
          <p>Showing <span class="font-semibold text-slate-700">{{ tickets.length }}</span> tickets</p>
          <p>Sorted by <span class="font-semibold text-slate-700">{{ sortLabel }}</span>, {{ filters.sortDirection === 'desc' ? 'descending' : 'ascending' }}</p>
        </div>
      </section>

      <footer class="py-7 text-center text-xs text-slate-400">Support Desk <span class="mx-1">·</span> Built for better customer conversations</footer>
    </main>

    <BaseDialog
      :open="isCreateOpen"
      eyebrow="New request"
      title="Create a ticket"
      description="Add the customer details and we'll get it in the queue."
      :error="actionError"
      @close="isCreateOpen = false"
    >
      <TicketForm :saving="ticketsStore.saving" submit-label="Create ticket" show-cancel @submit="createTicket" @cancel="isCreateOpen = false" />
    </BaseDialog>

    <ConfirmDialog
      :open="Boolean(ticketToDelete)"
      title="Delete this ticket?"
      :description="ticketToDelete ? `${ticketToDelete.title} and its comments and history will be permanently deleted.` : ''"
      :error="actionError"
      :loading="ticketsStore.saving"
      @cancel="ticketToDelete = null"
      @confirm="deleteTicket"
    />
  </AppShell>
</template>
