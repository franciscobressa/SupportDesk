<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppShell from '../components/AppShell.vue'
import BaseDialog from '../components/BaseDialog.vue'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import AppIcon from '../components/icons/AppIcon.vue'
import SummaryCards from '../components/SummaryCards.vue'
import TicketFilters from '../components/TicketFilters.vue'
import TicketForm from '../components/TicketForm.vue'
import TicketListEmptyState from '../components/TicketListEmptyState.vue'
import TicketPagination from '../components/TicketPagination.vue'
import TicketTable from '../components/TicketTable.vue'
import { useGreeting } from '../composables/useGreeting'
import { useTicketsStore } from '../stores/tickets'
import type { Ticket, TicketListFilters, TicketStatus, TicketWriteRequest } from '../types/tickets'
import { getErrorMessage } from '../utils/errors'
import { defaultTicketListFilters, filtersFromQuery, filtersToQuery } from '../utils/ticketListQuery'

const route = useRoute()
const router = useRouter()
const ticketsStore = useTicketsStore()
const greeting = useGreeting()

// The URL is the persisted source of the list criteria: it survives reloads, can be shared,
// and restores the same view when the user comes back from ticket details.
const filters = reactive<TicketListFilters>(filtersFromQuery(route.query))
const isCreateOpen = ref(false)
const ticketToDelete = ref<Ticket | null>(null)
const actionError = ref('')
let filterTimer: number | undefined

const currentPage = computed(() => filters.page ?? 1)
const hasActiveFilters = computed(() => Boolean(filters.search || filters.status || filters.priority))
const sortLabel = computed(() => (filters.sortBy === 'createdAt' ? 'created date' : filters.sortBy))

async function fetchTickets(): Promise<void> {
  try {
    await ticketsStore.fetchTickets(filters)
  } catch {
    // The store exposes the request error in the list state.
    return
  }

  // A restored or shared URL can point past the last page; move to the last page that has results.
  const { totalPages } = ticketsStore.pagination
  if (!ticketsStore.listLoading && totalPages > 0 && currentPage.value > totalPages) {
    filters.page = totalPages
  }
}

function updateFilters(nextFilters: TicketListFilters): void {
  const criteriaChanged =
    nextFilters.search !== filters.search ||
    nextFilters.status !== filters.status ||
    nextFilters.priority !== filters.priority ||
    nextFilters.sortBy !== filters.sortBy ||
    nextFilters.sortDirection !== filters.sortDirection

  Object.assign(filters, nextFilters, {
    page: criteriaChanged ? 1 : (nextFilters.page ?? filters.page),
    pageSize: defaultTicketListFilters.pageSize,
  })
}

function clearFilters(): void {
  Object.assign(filters, defaultTicketListFilters)
}

function toggleStatusFilter(status: TicketStatus): void {
  filters.status = filters.status === status ? '' : status
  filters.page = 1
}

function toggleHighPriorityFilter(): void {
  filters.priority = filters.priority === 'High' ? '' : 'High'
  filters.page = 1
}

function openCreateDialog(): void {
  actionError.value = ''
  isCreateOpen.value = true
}

function requestDelete(ticket: Ticket): void {
  actionError.value = ''
  ticketToDelete.value = ticket
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
    // Deleting the last ticket of the last page would leave an empty page; step back instead.
    const lastPage = Math.max(1, ticketsStore.pagination.totalPages)
    if (currentPage.value > lastPage) {
      filters.page = lastPage
    } else {
      await fetchTickets()
    }
  } catch (error: unknown) {
    actionError.value = getErrorMessage(error)
  }
}

watch(
  filters,
  () => {
    window.clearTimeout(filterTimer)
    filterTimer = window.setTimeout(() => {
      // replace (not push) so typing or paging does not flood the browser history.
      void router.replace({ query: filtersToQuery(filters) })
      void fetchTickets()
    }, 250)
  },
  { deep: true },
)

onMounted(() => {
  void fetchTickets()
})

onBeforeUnmount(() => {
  window.clearTimeout(filterTimer)
})
</script>

<template>
  <AppShell page-title="Ticket dashboard">
    <main class="mx-auto max-w-360 px-5 py-8 sm:px-8 lg:px-10 lg:py-10">
      <section class="flex flex-col justify-between gap-5 sm:flex-row sm:items-end">
        <div>
          <p class="text-xs font-bold uppercase tracking-[0.16em] text-accent-fg">Overview</p>
          <h1 class="mt-2 text-3xl font-bold tracking-tight text-fg-strong">{{ greeting }}, Manager!</h1>
          <p class="mt-2 text-sm text-fg-subtle">Here’s what’s happening with your support queue today.</p>
        </div>
        <button type="button" class="button-primary w-full sm:w-auto" @click="openCreateDialog">
          <AppIcon name="plus" class="size-4.5" />
          Create ticket
        </button>
      </section>

      <SummaryCards
        class="mt-8"
        :summary="ticketsStore.summary"
        :active-status="filters.status ?? ''"
        :active-priority="filters.priority ?? ''"
        @toggle-status="toggleStatusFilter"
        @toggle-high-priority="toggleHighPriorityFilter"
      />

      <section class="mt-6 overflow-hidden rounded-2xl border border-line/80 bg-surface shadow-sm shadow-slate-200/30 dark:shadow-none">
        <div class="flex flex-col gap-4 border-b border-line-subtle p-5 sm:p-6">
          <div class="flex flex-col justify-between gap-3 sm:flex-row sm:items-center">
            <div>
              <h2 class="text-lg font-bold text-fg-strong">
                All tickets
                <span class="ml-1 rounded-full bg-surface-muted px-2 py-0.5 text-xs font-semibold text-fg-muted">{{ ticketsStore.pagination.totalCount }}</span>
              </h2>
              <p class="mt-1 text-sm text-fg-subtle">Manage and keep track of your customer requests.</p>
            </div>
            <button
              type="button"
              class="button-secondary w-full sm:w-auto sm:self-auto"
              :disabled="ticketsStore.listLoading"
              @click="fetchTickets"
            >
              <AppIcon name="refresh" class="size-4" :class="{ 'animate-spin': ticketsStore.listLoading }" />
              Refresh
            </button>
          </div>
          <TicketFilters :filters="filters" @update:filters="updateFilters" />
        </div>

        <div
          v-if="actionError || ticketsStore.listError"
          class="mx-5 mt-5 flex items-start justify-between gap-3 rounded-xl border border-danger-line bg-danger-soft px-4 py-3 text-sm text-danger-fg sm:mx-6"
          role="alert"
        >
          <p>{{ actionError || ticketsStore.listError }}</p>
          <button type="button" class="font-semibold underline underline-offset-2" @click="fetchTickets">Try again</button>
        </div>

        <!-- Mirrors TicketTable: card placeholders below xl, row placeholders from xl. -->
        <div v-if="ticketsStore.listLoading" class="grid gap-3 p-6 md:grid-cols-2 xl:grid-cols-1" aria-label="Loading tickets">
          <div v-for="item in 4" :key="item" class="h-56 animate-pulse rounded-xl bg-surface-muted xl:h-16" />
        </div>

        <TicketListEmptyState
          v-else-if="!ticketsStore.tickets.length && !ticketsStore.listError"
          :has-active-filters="hasActiveFilters"
          @clear-filters="clearFilters"
          @create="openCreateDialog"
        />

        <TicketTable v-else-if="ticketsStore.tickets.length" :tickets="ticketsStore.tickets" @delete="requestDelete" />

        <TicketPagination
          v-if="ticketsStore.pagination.totalCount"
          :pagination="ticketsStore.pagination"
          :current-page="currentPage"
          :disabled="ticketsStore.listLoading"
          @change="filters.page = $event"
        >
          <p>
            Sorted by <span class="font-semibold text-fg-soft">{{ sortLabel }}</span>,
            {{ filters.sortDirection === 'desc' ? 'descending' : 'ascending' }}
          </p>
        </TicketPagination>
      </section>

      <footer class="py-7 text-center text-xs text-fg-faint">
        Support Desk <span class="mx-1">·</span> Built for better customer conversations
      </footer>
    </main>

    <BaseDialog
      :open="isCreateOpen"
      eyebrow="New request"
      title="Create a ticket"
      description="Add the customer details and we'll get it in the queue."
      :error="actionError"
      @close="isCreateOpen = false"
    >
      <TicketForm
        :saving="ticketsStore.saving"
        submit-label="Create ticket"
        show-cancel
        @submit="createTicket"
        @cancel="isCreateOpen = false"
      />
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
