<script setup lang="ts">
import {
  computed,
  onBeforeUnmount,
  onMounted,
  reactive,
  ref,
  watch,
} from "vue";
import { useRoute, useRouter } from "vue-router";
import AppShell from "../components/AppShell.vue";
import BaseDialog from "../components/BaseDialog.vue";
import ConfirmDialog from "../components/ConfirmDialog.vue";
import TicketFilters from "../components/TicketFilters.vue";
import TicketForm from "../components/TicketForm.vue";
import TicketTable from "../components/TicketTable.vue";
import { useTicketsStore } from "../stores/tickets";
import type {
  Ticket,
  TicketListFilters,
  TicketStatus,
  TicketWriteRequest,
} from "../types/tickets";
import {
  defaultTicketListFilters,
  filtersFromQuery,
  filtersToQuery,
} from "../utils/ticketListQuery";

const route = useRoute();
const router = useRouter();
const ticketsStore = useTicketsStore();
// The URL is the persisted source of the list criteria: it survives reloads, can be shared,
// and restores the same view when the user comes back from ticket details.
const filters = reactive<TicketListFilters>(filtersFromQuery(route.query));
const isCreateOpen = ref(false);
const ticketToDelete = ref<Ticket | null>(null);
const actionError = ref("");
const currentHour = ref(new Date().getHours());
const greeting = computed(() => {
  if (currentHour.value < 12) return "Good morning";
  if (currentHour.value < 18) return "Good afternoon";
  return "Good evening";
});
let filterTimer: number | undefined;
let greetingTimer: number | undefined;

const tickets = computed<Ticket[]>(() => ticketsStore.tickets);
const pagination = computed(() => ticketsStore.pagination);
const currentPage = computed(() => filters.page ?? 1);
const firstTicketNumber = computed(() =>
  pagination.value.totalCount === 0
    ? 0
    : (pagination.value.page - 1) * pagination.value.pageSize + 1,
);
type PageNavigationItem =
  | { kind: "page"; page: number }
  | { kind: "ellipsis"; id: string };

const pageNavigationItems = computed<PageNavigationItem[]>(() => {
  const totalPages = pagination.value.totalPages;
  if (totalPages <= 5) {
    return Array.from({ length: totalPages }, (_, index) => ({
      kind: "page" as const,
      page: index + 1,
    }));
  }

  const visiblePages = new Set(
    [1, currentPage.value - 1, currentPage.value, currentPage.value + 1, totalPages]
      .filter((page) => page >= 1 && page <= totalPages),
  );
  const pages = [...visiblePages].sort((first, second) => first - second);
  const items: PageNavigationItem[] = [];

  pages.forEach((page, index) => {
    const previousPage = pages[index - 1];
    if (previousPage !== undefined) {
      const gap = page - previousPage;
      if (gap === 2) {
        items.push({ kind: "page", page: previousPage + 1 });
      } else if (gap > 2) {
        items.push({ kind: "ellipsis", id: `${previousPage}-${page}` });
      }
    }

    items.push({ kind: "page", page });
  });

  return items;
});
const lastTicketNumber = computed(() =>
  Math.min(
    pagination.value.page * pagination.value.pageSize,
    pagination.value.totalCount,
  ),
);
const hasActiveFilters = computed(() =>
  Boolean(filters.search || filters.status || filters.priority),
);
const sortLabel = computed(() =>
  filters.sortBy === "createdAt" ? "created date" : filters.sortBy,
);

async function fetchTickets(): Promise<void> {
  try {
    await ticketsStore.fetchTickets(filters);
  } catch {
    // The store exposes the request error in the list state.
    return;
  }

  // A restored or shared URL can point past the last page; move to the last page that has results.
  const { totalPages } = ticketsStore.pagination;
  if (!ticketsStore.listLoading && totalPages > 0 && currentPage.value > totalPages) {
    filters.page = totalPages;
  }
}

function clearFilters(): void {
  Object.assign(filters, defaultTicketListFilters);
}

function updateFilters(nextFilters: TicketListFilters): void {
  const criteriaChanged =
    nextFilters.search !== filters.search ||
    nextFilters.status !== filters.status ||
    nextFilters.priority !== filters.priority ||
    nextFilters.sortBy !== filters.sortBy ||
    nextFilters.sortDirection !== filters.sortDirection;

  Object.assign(filters, nextFilters, {
    page: criteriaChanged ? 1 : (nextFilters.page ?? filters.page),
    pageSize: 10,
  });
}

function changePage(page: number): void {
  filters.page = page;
}

function toggleStatusCard(status: TicketStatus): void {
  filters.status = filters.status === status ? "" : status;
  filters.page = 1;
}

function toggleHighPriorityCard(): void {
  filters.priority = filters.priority === "High" ? "" : "High";
  filters.page = 1;
}

async function createTicket(ticket: TicketWriteRequest): Promise<void> {
  actionError.value = "";

  try {
    const created = await ticketsStore.addTicket(ticket);
    isCreateOpen.value = false;
    await router.push({ name: "ticket-details", params: { id: created.id } });
  } catch (error: unknown) {
    actionError.value = getErrorMessage(error);
  }
}

async function deleteTicket(): Promise<void> {
  if (!ticketToDelete.value) {
    return;
  }

  actionError.value = "";
  try {
    await ticketsStore.removeTicket(ticketToDelete.value.id);
    ticketToDelete.value = null;
    const lastPage = Math.max(1, pagination.value.totalPages);
    if (currentPage.value > lastPage) {
      filters.page = lastPage;
    } else {
      await fetchTickets();
    }
  } catch (error: unknown) {
    actionError.value = getErrorMessage(error);
  }
}

watch(
  filters,
  () => {
    window.clearTimeout(filterTimer);
    filterTimer = window.setTimeout(() => {
      // replace (not push) so typing or paging does not flood the browser history.
      void router.replace({ query: filtersToQuery(filters) });
      void fetchTickets();
    }, 250);
  },
  { deep: true },
);

function getErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : "An unexpected error occurred.";
}

onMounted(() => {
  void fetchTickets();
  greetingTimer = window.setInterval(() => {
    currentHour.value = new Date().getHours();
  }, 60_000);
});

onBeforeUnmount(() => {
  window.clearTimeout(filterTimer);
  window.clearInterval(greetingTimer);
});
</script>

<template>
  <AppShell page-title="Ticket dashboard">
    <main class="mx-auto max-w-[1440px] px-5 py-8 sm:px-8 lg:px-10 lg:py-10">
      <section
        class="flex flex-col justify-between gap-5 sm:flex-row sm:items-end"
      >
        <div>
          <p
            class="text-xs font-bold uppercase tracking-[0.16em] text-indigo-600"
          >
            Overview
          </p>
          <h1 class="mt-2 text-3xl font-bold tracking-tight text-slate-900">
            {{ greeting }}, Manager!
          </h1>
          <p class="mt-2 text-sm text-slate-500">
            Here’s what’s happening with your support queue today.
          </p>
        </div>
        <button
          type="button"
          class="button-primary self-start sm:self-auto"
          @click="
            isCreateOpen = true;
            actionError = '';
          "
        >
          <svg
            viewBox="0 0 24 24"
            fill="none"
            class="size-[18px]"
            aria-hidden="true"
          >
            <path
              d="M12 5v14M5 12h14"
              stroke="currentColor"
              stroke-width="2"
              stroke-linecap="round"
            />
          </svg>
          Create ticket
        </button>
      </section>

      <section
        class="mt-8 grid grid-cols-2 gap-3 lg:grid-cols-4"
        aria-label="Ticket queue summary"
      >
        <button
          type="button"
          class="rounded-2xl border p-4 text-left shadow-sm transition hover:border-indigo-200 hover:shadow-md focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 sm:p-5"
          :class="
            filters.status === 'Open'
              ? 'border-indigo-300 bg-indigo-50/70 ring-1 ring-indigo-200'
              : 'border-slate-200/80 bg-white'
          "
          :aria-pressed="filters.status === 'Open'"
          @click="toggleStatusCard('Open')"
        >
          <span class="text-sm font-medium text-slate-500">Open</span>
          <span
            class="mt-2 block text-2xl font-bold tracking-tight text-slate-900"
            >{{ ticketsStore.summary.open }}</span
          >
          <span class="mt-1 block text-xs text-slate-400"
            >View open tickets</span
          >
        </button>
        <button
          type="button"
          class="rounded-2xl border p-4 text-left shadow-sm transition hover:border-indigo-200 hover:shadow-md focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 sm:p-5"
          :class="
            filters.status === 'InProgress'
              ? 'border-indigo-300 bg-indigo-50/70 ring-1 ring-indigo-200'
              : 'border-slate-200/80 bg-white'
          "
          :aria-pressed="filters.status === 'InProgress'"
          @click="toggleStatusCard('InProgress')"
        >
          <span class="text-sm font-medium text-slate-500">In progress</span>
          <span
            class="mt-2 block text-2xl font-bold tracking-tight text-slate-900"
            >{{ ticketsStore.summary.inProgress }}</span
          >
          <span class="mt-1 block text-xs text-slate-400"
            >View active work</span
          >
        </button>
        <button
          type="button"
          class="rounded-2xl border p-4 text-left shadow-sm transition hover:border-indigo-200 hover:shadow-md focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 sm:p-5"
          :class="
            filters.status === 'Resolved'
              ? 'border-indigo-300 bg-indigo-50/70 ring-1 ring-indigo-200'
              : 'border-slate-200/80 bg-white'
          "
          :aria-pressed="filters.status === 'Resolved'"
          @click="toggleStatusCard('Resolved')"
        >
          <span class="text-sm font-medium text-slate-500">Resolved</span>
          <span
            class="mt-2 block text-2xl font-bold tracking-tight text-slate-900"
            >{{ ticketsStore.summary.resolved }}</span
          >
          <span class="mt-1 block text-xs text-slate-400"
            >View resolved tickets</span
          >
        </button>
        <button
          type="button"
          class="rounded-2xl border p-4 text-left shadow-sm transition hover:border-amber-200 hover:shadow-md focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 sm:p-5"
          :class="
            filters.priority === 'High'
              ? 'border-amber-300 bg-amber-50/70 ring-1 ring-amber-200'
              : 'border-slate-200/80 bg-white'
          "
          :aria-pressed="filters.priority === 'High'"
          @click="toggleHighPriorityCard"
        >
          <span class="text-sm font-medium text-slate-500">High priority</span>
          <span
            class="mt-2 block text-2xl font-bold tracking-tight text-slate-900"
            >{{ ticketsStore.summary.highPriority }}</span
          >
          <span class="mt-1 block text-xs text-slate-400"
            >View urgent tickets</span
          >
        </button>
      </section>

      <section
        class="mt-6 overflow-hidden rounded-2xl border border-slate-200/80 bg-white shadow-sm shadow-slate-200/30"
      >
        <div class="flex flex-col gap-4 border-b border-slate-100 p-5 sm:p-6">
          <div
            class="flex flex-col justify-between gap-3 sm:flex-row sm:items-center"
          >
            <div>
              <h2 class="text-lg font-bold text-slate-900">
                All tickets
                <span
                  class="ml-1 rounded-full bg-slate-100 px-2 py-0.5 text-xs font-semibold text-slate-600"
                  >{{ pagination.totalCount }}</span
                >
              </h2>
              <p class="mt-1 text-sm text-slate-500">
                Manage and keep track of your customer requests.
              </p>
            </div>
            <button
              type="button"
              class="button-secondary self-start sm:self-auto"
              :disabled="ticketsStore.listLoading"
              @click="fetchTickets"
            >
              <svg
                viewBox="0 0 24 24"
                fill="none"
                class="size-4"
                :class="{ 'animate-spin': ticketsStore.listLoading }"
                aria-hidden="true"
              >
                <path
                  d="M20 11a8 8 0 0 0-14.9-3M4 5v3h3m-3 5a8 8 0 0 0 14.9 3M20 19v-3h-3"
                  stroke="currentColor"
                  stroke-width="1.8"
                  stroke-linecap="round"
                  stroke-linejoin="round"
                />
              </svg>
              Refresh
            </button>
          </div>
          <TicketFilters :filters="filters" @update:filters="updateFilters" />
        </div>

        <div
          v-if="actionError || ticketsStore.listError"
          class="mx-5 mt-5 flex items-start justify-between gap-3 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700 sm:mx-6"
          role="alert"
        >
          <p>{{ actionError || ticketsStore.listError }}</p>
          <button
            type="button"
            class="font-semibold underline underline-offset-2"
            @click="fetchTickets"
          >
            Try again
          </button>
        </div>

        <div
          v-if="ticketsStore.listLoading"
          class="space-y-3 p-6"
          aria-label="Loading tickets"
        >
          <div
            v-for="item in 5"
            :key="item"
            class="h-56 animate-pulse rounded-xl bg-slate-100 sm:h-16"
          />
        </div>

        <div
          v-else-if="!tickets.length && !ticketsStore.listError"
          class="px-6 py-16 text-center"
        >
          <span
            class="mx-auto grid size-12 place-items-center rounded-2xl bg-indigo-50 text-indigo-600"
          >
            <svg
              viewBox="0 0 24 24"
              fill="none"
              class="size-6"
              aria-hidden="true"
            >
              <path
                d="M4 5.75A1.75 1.75 0 0 1 5.75 4h12.5A1.75 1.75 0 0 1 20 5.75v8.5A1.75 1.75 0 0 1 18.25 16H13l-4.5 4v-4h-2.75A1.75 1.75 0 0 1 4 14.25v-8.5Z"
                stroke="currentColor"
                stroke-width="1.7"
                stroke-linejoin="round"
              />
            </svg>
          </span>
          <h3 class="mt-4 text-base font-bold text-slate-900">
            {{
              hasActiveFilters ? "No matching tickets" : "Your queue is clear"
            }}
          </h3>
          <p class="mx-auto mt-1 max-w-sm text-sm text-slate-500">
            {{
              hasActiveFilters
                ? "Try adjusting your filters or search terms."
                : "Create a ticket to start tracking a customer request."
            }}
          </p>
          <button
            v-if="hasActiveFilters"
            type="button"
            class="mt-4 text-sm font-semibold text-indigo-600 hover:text-indigo-700"
            @click="clearFilters"
          >
            Clear filters
          </button>
          <button
            v-else
            type="button"
            class="button-primary mx-auto mt-5"
            @click="isCreateOpen = true"
          >
            <span aria-hidden="true">+</span>
            Create your first ticket
          </button>
        </div>

        <TicketTable
          v-else-if="tickets.length"
          :tickets="tickets"
          @delete="
            ticketToDelete = $event;
            actionError = '';
          "
        />

        <div
          v-if="pagination.totalCount"
          class="flex flex-col gap-3 border-t border-slate-100 px-5 py-4 text-xs text-slate-500 sm:px-6"
        >
          <div
            class="flex flex-col justify-between gap-2 sm:flex-row sm:items-center"
          >
            <p>
              Showing
              <span class="font-semibold text-slate-700"
                >{{ firstTicketNumber }}–{{ lastTicketNumber }}</span
              >
              of
              <span class="font-semibold text-slate-700">{{
                pagination.totalCount
              }}</span>
              tickets
            </p>
            <p>
              Sorted by
              <span class="font-semibold text-slate-700">{{ sortLabel }}</span
              >,
              {{
                filters.sortDirection === "desc" ? "descending" : "ascending"
              }}
            </p>
          </div>
          <nav
            class="flex flex-wrap items-center justify-center gap-2"
            aria-label="Ticket pages"
          >
            <div class="hidden items-center gap-2 sm:flex">
              <button
                type="button"
                class="button-secondary min-h-9 px-3 py-1.5"
                :disabled="currentPage <= 1 || ticketsStore.listLoading"
                @click="changePage(currentPage - 1)"
              >
                Previous
              </button>
              <template
                v-for="item in pageNavigationItems"
                :key="item.kind === 'page' ? item.page : item.id"
              >
                <span
                  v-if="item.kind === 'ellipsis'"
                  class="px-1 text-slate-400"
                  aria-hidden="true"
                >…</span>
                <button
                  v-else
                  type="button"
                  class="button-secondary min-h-9 min-w-9 px-2 py-1.5"
                  :class="
                    item.page === currentPage
                      ? '!border-indigo-600 !bg-indigo-600 !text-white'
                      : ''
                  "
                  :aria-label="`Go to page ${item.page}`"
                  :aria-current="item.page === currentPage ? 'page' : undefined"
                  :disabled="ticketsStore.listLoading"
                  @click="changePage(item.page)"
                >
                  {{ item.page }}
                </button>
              </template>
              <button
                type="button"
                class="button-secondary min-h-9 px-3 py-1.5"
                :disabled="
                  currentPage >= pagination.totalPages || ticketsStore.listLoading
                "
                @click="changePage(currentPage + 1)"
              >
                Next
              </button>
            </div>
            <div class="grid w-full max-w-[12rem] grid-cols-4 gap-2 sm:hidden">
              <button
                type="button"
                class="button-icon size-10"
                aria-label="First page"
                :disabled="currentPage <= 1 || ticketsStore.listLoading"
                @click="changePage(1)"
              >
                <svg viewBox="0 0 24 24" fill="none" class="size-[18px]" aria-hidden="true">
                  <path d="m11 18-6-6 6-6M19 18l-6-6 6-6" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
              </button>
              <button
                type="button"
                class="button-icon size-10"
                aria-label="Previous page"
                :disabled="currentPage <= 1 || ticketsStore.listLoading"
                @click="changePage(currentPage - 1)"
              >
                <svg viewBox="0 0 24 24" fill="none" class="size-[18px]" aria-hidden="true">
                  <path d="m15 18-6-6 6-6" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
              </button>
              <button
                type="button"
                class="button-icon size-10"
                aria-label="Next page"
                :disabled="currentPage >= pagination.totalPages || ticketsStore.listLoading"
                @click="changePage(currentPage + 1)"
              >
                <svg viewBox="0 0 24 24" fill="none" class="size-[18px]" aria-hidden="true">
                  <path d="m9 18 6-6-6-6" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
              </button>
              <button
                type="button"
                class="button-icon size-10"
                aria-label="Last page"
                :disabled="currentPage >= pagination.totalPages || ticketsStore.listLoading"
                @click="changePage(pagination.totalPages)"
              >
                <svg viewBox="0 0 24 24" fill="none" class="size-[18px]" aria-hidden="true">
                  <path d="m13 6 6 6-6 6M5 6l6 6-6 6" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
              </button>
            </div>
            <p class="basis-full text-center sm:hidden" aria-live="polite">
              Page
              <span class="font-semibold text-slate-700">{{ pagination.page }}</span>
              of
              <span class="font-semibold text-slate-700">{{ pagination.totalPages }}</span>
            </p>
            <p class="hidden basis-full text-center sm:block" aria-live="polite">
              Page
              <span class="font-semibold text-slate-700">{{ pagination.page }}</span>
              of
              <span class="font-semibold text-slate-700">{{ pagination.totalPages }}</span>
            </p>
          </nav>
        </div>
      </section>

      <footer class="py-7 text-center text-xs text-slate-400">
        Support Desk <span class="mx-1">·</span> Built for better customer
        conversations
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
      :description="
        ticketToDelete
          ? `${ticketToDelete.title} and its comments and history will be permanently deleted.`
          : ''
      "
      :error="actionError"
      :loading="ticketsStore.saving"
      @cancel="ticketToDelete = null"
      @confirm="deleteTicket"
    />
  </AppShell>
</template>
