<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppShell from '../components/AppShell.vue'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import StatusBadge from '../components/StatusBadge.vue'
import TicketCommentsPanel from '../components/TicketCommentsPanel.vue'
import TicketForm from '../components/TicketForm.vue'
import TicketStatusHistoryPanel from '../components/TicketStatusHistoryPanel.vue'
import { useTicketsStore } from '../stores/tickets'
import type { TicketWriteRequest } from '../types/tickets'
import { formatDate, getInitials } from '../utils/formatters'

const route = useRoute()
const router = useRouter()
const ticketsStore = useTicketsStore()
const commentText = ref('')
const commentError = ref('')
const actionError = ref('')
const isEditing = ref(false)
const isDeleteOpen = ref(false)
const ticketId = computed(() => {
  const id = route.params.id
  return Array.isArray(id) ? id[0] ?? '' : id ?? ''
})

const ticket = computed(() => ticketsStore.selectedTicket)

// Return to the dashboard entry the user came from so its URL filters and page are kept.
const previousPath = router.options.history.state.back
const dashboardLocation =
  typeof previousPath === 'string' && router.resolve(previousPath).name === 'dashboard'
    ? previousPath
    : { name: 'dashboard' }

async function loadTicket(id: string): Promise<void> {
  isEditing.value = false
  actionError.value = ''

  try {
    await ticketsStore.fetchTicket(id)
  } catch {
    // The store exposes the request error to the page.
    return
  }

  if (route.query.edit === '1') {
    isEditing.value = true
    // Drop the flag so a reload or a later visit opens the read-only view.
    const { edit: _edit, ...query } = route.query
    await router.replace({ query })
  }
}

async function saveTicket(updatedTicket: TicketWriteRequest): Promise<void> {
  actionError.value = ''

  try {
    await ticketsStore.saveTicket(ticketId.value, updatedTicket)
    isEditing.value = false
  } catch (error: unknown) {
    actionError.value = getErrorMessage(error)
  }
}

async function submitComment(): Promise<void> {
  commentError.value = ''

  if (!commentText.value.trim()) {
    commentError.value = 'Write a comment before posting.'
    return
  }

  try {
    await ticketsStore.addComment(ticketId.value, commentText.value.trim())
    commentText.value = ''
  } catch (error: unknown) {
    commentError.value = getErrorMessage(error)
  }
}

async function deleteTicket(): Promise<void> {
  actionError.value = ''

  try {
    await ticketsStore.removeTicket(ticketId.value)
    isDeleteOpen.value = false
    await router.push(dashboardLocation)
  } catch (error: unknown) {
    actionError.value = getErrorMessage(error)
  }
}

watch(
  ticketId,
  (id) => {
    if (id) {
      void loadTicket(id)
    }
  },
  { immediate: true },
)

function getErrorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'An unexpected error occurred.'
}
</script>

<template>
  <AppShell page-title="Ticket details" :breadcrumbs="['Workspace', 'Tickets', 'Details']">
    <main class="mx-auto max-w-[1320px] px-5 py-7 sm:px-8 lg:px-10 lg:py-9">
      <RouterLink :to="dashboardLocation" class="inline-flex items-center gap-2 text-sm font-semibold text-slate-500 transition hover:text-indigo-700">
        <svg viewBox="0 0 24 24" fill="none" class="size-4" aria-hidden="true">
          <path d="m14.5 5-7 7 7 7" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" />
        </svg>
        Back to tickets
      </RouterLink>

      <div v-if="ticketsStore.detailLoading" class="mt-7 space-y-4" aria-label="Loading ticket">
        <div class="h-40 animate-pulse rounded-2xl bg-white" />
        <div class="grid gap-5 lg:grid-cols-[minmax(0,1.7fr)_minmax(280px,0.8fr)]">
          <div class="h-80 animate-pulse rounded-2xl bg-white" />
          <div class="h-64 animate-pulse rounded-2xl bg-white" />
        </div>
      </div>

      <section v-else-if="ticketsStore.detailError" class="mt-8 rounded-2xl border border-slate-200 bg-white px-6 py-14 text-center shadow-sm">
        <span class="mx-auto grid size-12 place-items-center rounded-full bg-rose-50 text-rose-600">
          <svg viewBox="0 0 24 24" fill="none" class="size-6" aria-hidden="true">
            <path d="M12 8v4m0 4h.01M10.3 3.8 2.8 17a2 2 0 0 0 1.7 3h15a2 2 0 0 0 1.7-3l-7.5-13.2a2 2 0 0 0-3.4 0Z" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
        </span>
        <h1 class="mt-4 text-xl font-bold text-slate-900">Ticket unavailable</h1>
        <p class="mt-2 text-sm text-slate-500">{{ ticketsStore.detailError }}</p>
        <RouterLink :to="dashboardLocation" class="button-primary mx-auto mt-5">Return to dashboard</RouterLink>
      </section>

      <template v-else-if="ticket">
        <section class="mt-6 rounded-2xl border border-slate-200/80 bg-white p-5 shadow-sm shadow-slate-200/30 sm:p-7">
          <div class="flex flex-col justify-between gap-5 xl:flex-row xl:items-start">
            <div class="min-w-0">
              <div class="flex flex-wrap items-center gap-2">
                <span class="rounded-md bg-slate-100 px-2 py-1 font-mono text-[11px] font-semibold text-slate-500">#{{ ticket.id.slice(0, 8).toUpperCase() }}</span>
                <StatusBadge :status="ticket.status" />
                <StatusBadge :priority="ticket.priority" />
              </div>
              <h1 class="mt-4 break-words text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">{{ ticket.title }}</h1>
              <p class="mt-2 flex flex-wrap items-center gap-x-3 gap-y-1 text-sm text-slate-500">
                <span>Created {{ formatDate(ticket.createdAt, { dateStyle: 'medium', timeStyle: 'short' }) }}</span>
                <span class="hidden size-1 rounded-full bg-slate-300 sm:block" />
                <span>Last updated {{ formatDate(ticket.updatedAt, { dateStyle: 'medium', timeStyle: 'short' }) }}</span>
              </p>
            </div>
            <div class="flex flex-wrap gap-2">
              <button type="button" class="button-secondary" :disabled="isEditing" @click="isEditing = true; actionError = ''">
                <svg viewBox="0 0 24 24" fill="none" class="size-4" aria-hidden="true">
                  <path d="m14 5 5 5M4 20l4.3-.9L19 8.4 15.6 5 4.9 15.7 4 20Z" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
                Edit ticket
              </button>
              <button type="button" class="button-danger-outline" @click="isDeleteOpen = true; actionError = ''">
                <svg viewBox="0 0 24 24" fill="none" class="size-4" aria-hidden="true">
                  <path d="M4 7h16M10 11v6m4-6v6M5.5 7l1 13h11l1-13M9 7V4h6v3" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
                Delete
              </button>
            </div>
          </div>
        </section>

        <p v-if="actionError" class="mt-5 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700" role="alert">{{ actionError }}</p>

        <div class="mt-5 grid items-start gap-5 lg:grid-cols-[minmax(0,1.65fr)_minmax(280px,0.8fr)]">
          <div class="space-y-5">
            <section v-if="isEditing" class="rounded-2xl border border-slate-200/80 bg-white p-5 shadow-sm sm:p-6">
              <div class="mb-5">
                <p class="text-xs font-bold uppercase tracking-[0.14em] text-indigo-600">Ticket information</p>
                <h2 class="mt-1 text-lg font-bold text-slate-900">Edit ticket</h2>
              </div>
              <TicketForm :ticket="ticket" :saving="ticketsStore.saving" submit-label="Save changes" show-cancel @submit="saveTicket" @cancel="isEditing = false" />
            </section>

            <section v-else class="rounded-2xl border border-slate-200/80 bg-white p-5 shadow-sm sm:p-6">
              <div>
                <p class="text-xs font-bold uppercase tracking-[0.14em] text-indigo-600">Ticket information</p>
                <h2 class="mt-1 text-lg font-bold text-slate-900">Request details</h2>
              </div>
              <div class="mt-5 rounded-xl bg-slate-50/80 p-4 sm:p-5">
                <p class="whitespace-pre-wrap text-sm leading-7 text-slate-600">{{ ticket.description }}</p>
              </div>
              <div class="mt-6 grid gap-5 border-t border-slate-100 pt-5 sm:grid-cols-2">
                <div>
                  <p class="text-xs font-semibold uppercase tracking-wide text-slate-400">Customer</p>
                  <div class="mt-2 flex items-center gap-3">
                    <span class="grid size-10 place-items-center rounded-full bg-indigo-50 text-xs font-bold text-indigo-700">{{ getInitials(ticket.customerName) }}</span>
                    <div class="min-w-0">
                      <p class="truncate text-sm font-semibold text-slate-800">{{ ticket.customerName || 'Customer' }}</p>
                      <a v-if="ticket.customerEmail" :href="`mailto:${ticket.customerEmail}`" class="mt-0.5 block truncate text-xs text-indigo-600 hover:underline">{{ ticket.customerEmail }}</a>
                    </div>
                  </div>
                </div>
                <div>
                  <p class="text-xs font-semibold uppercase tracking-wide text-slate-400">Ticket ID</p>
                  <p class="mt-3 break-all font-mono text-xs text-slate-600">{{ ticket.id }}</p>
                </div>
              </div>
            </section>

            <TicketCommentsPanel
              v-model="commentText"
              :comments="ticket.comments"
              :submitting="ticketsStore.commentSubmitting"
              :error="commentError"
              @clear-error="commentError = ''"
              @submit="submitComment"
            />
          </div>

          <aside class="space-y-5">
            <TicketStatusHistoryPanel :status-changes="ticket.statusChanges" />
            <section class="rounded-2xl border border-indigo-100 bg-indigo-50/70 p-5">
              <span class="grid size-9 place-items-center rounded-xl bg-white text-indigo-600 shadow-sm">
                <svg viewBox="0 0 24 24" fill="none" class="size-[18px]" aria-hidden="true">
                  <path d="M12 3.5 14.6 9l5.9.9-4.3 4.2 1 5.9-5.2-2.8-5.2 2.8 1-5.9-4.3-4.2 5.9-.9L12 3.5Z" stroke="currentColor" stroke-width="1.6" stroke-linejoin="round" />
                </svg>
              </span>
              <h3 class="mt-3 text-sm font-bold text-slate-800">Keep the conversation moving</h3>
              <p class="mt-1.5 text-xs leading-5 text-slate-600">Leave a clear update so the next teammate knows exactly what happened.</p>
            </section>
          </aside>
        </div>
      </template>
    </main>

    <ConfirmDialog
      :open="isDeleteOpen"
      title="Delete this ticket?"
      :description="ticket ? `${ticket.title} and its comments and history will be permanently deleted.` : ''"
      :error="actionError"
      :loading="ticketsStore.saving"
      @cancel="isDeleteOpen = false"
      @confirm="deleteTicket"
    />
  </AppShell>
</template>
