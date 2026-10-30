<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import AppShell from '../components/AppShell.vue'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import AppIcon from '../components/icons/AppIcon.vue'
import TicketCommentsPanel from '../components/TicketCommentsPanel.vue'
import TicketDetailsHeader from '../components/TicketDetailsHeader.vue'
import TicketForm from '../components/TicketForm.vue'
import TicketInfoCard from '../components/TicketInfoCard.vue'
import TicketStatusHistoryPanel from '../components/TicketStatusHistoryPanel.vue'
import { ApiError } from '../services/ticketsApi'
import { useTicketsStore } from '../stores/tickets'
import type { TicketWriteRequest } from '../types/tickets'
import { getErrorMessage } from '../utils/errors'

const route = useRoute()
const router = useRouter()
const ticketsStore = useTicketsStore()
const commentText = ref('')
const commentError = ref('')
const actionError = ref('')
const hasConflict = ref(false)
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
  hasConflict.value = false

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
  if (!ticket.value) {
    return
  }

  actionError.value = ''
  hasConflict.value = false

  try {
    await ticketsStore.saveTicket(ticketId.value, {
      ...updatedTicket,
      expectedUpdatedAt: ticket.value.updatedAt,
    })
    isEditing.value = false
  } catch (error: unknown) {
    actionError.value = getErrorMessage(error)
    // Keep the form open so the manager can copy their edits before loading the newer version.
    hasConflict.value = error instanceof ApiError && error.status === 409
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

function startEditing(): void {
  actionError.value = ''
  isEditing.value = true
}

function openDeleteDialog(): void {
  actionError.value = ''
  isDeleteOpen.value = true
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
</script>

<template>
  <AppShell page-title="Ticket details" :breadcrumbs="['Workspace', 'Tickets', 'Details']">
    <main class="mx-auto max-w-330 px-5 py-7 sm:px-8 lg:px-10 lg:py-9">
      <RouterLink :to="dashboardLocation" class="inline-flex items-center gap-2 text-sm font-semibold text-fg-subtle transition hover:text-accent-fg-strong">
        <AppIcon name="arrow-left" class="size-4" />
        Back to tickets
      </RouterLink>

      <div v-if="ticketsStore.detailLoading" class="mt-7 space-y-4" aria-label="Loading ticket">
        <div class="h-40 animate-pulse rounded-2xl bg-surface" />
        <div class="grid gap-5 lg:grid-cols-[minmax(0,1.7fr)_minmax(280px,0.8fr)]">
          <div class="h-80 animate-pulse rounded-2xl bg-surface" />
          <div class="h-64 animate-pulse rounded-2xl bg-surface" />
        </div>
      </div>

      <section v-else-if="ticketsStore.detailError" class="mt-8 rounded-2xl border border-line bg-surface px-6 py-14 text-center shadow-sm">
        <span class="mx-auto grid size-12 place-items-center rounded-full bg-danger-soft text-danger-fg">
          <AppIcon name="alert" class="size-6" />
        </span>
        <h1 class="mt-4 text-xl font-bold text-fg-strong">Ticket unavailable</h1>
        <p class="mt-2 text-sm text-fg-subtle">{{ ticketsStore.detailError }}</p>
        <RouterLink :to="dashboardLocation" class="button-primary mx-auto mt-5">Return to dashboard</RouterLink>
      </section>

      <template v-else-if="ticket">
        <TicketDetailsHeader class="mt-6" :ticket="ticket" :is-editing="isEditing" @edit="startEditing" @delete="openDeleteDialog" />

        <div v-if="actionError" class="mt-5 flex flex-col gap-2 rounded-xl border border-danger-line bg-danger-soft px-4 py-3 text-sm text-danger-fg sm:flex-row sm:items-center sm:justify-between" role="alert">
          <p>{{ actionError }}</p>
          <button v-if="hasConflict" type="button" class="shrink-0 self-start font-semibold underline underline-offset-2 sm:self-auto" @click="loadTicket(ticketId)">
            Load latest version
          </button>
        </div>

        <div class="mt-5 grid items-start gap-5 lg:grid-cols-[minmax(0,1.65fr)_minmax(280px,0.8fr)]">
          <div class="space-y-5">
            <section v-if="isEditing" class="rounded-2xl border border-line/80 bg-surface p-5 shadow-sm sm:p-6">
              <div class="mb-5">
                <p class="text-xs font-bold uppercase tracking-[0.14em] text-accent-fg">Ticket information</p>
                <h2 class="mt-1 text-lg font-bold text-fg-strong">Edit ticket</h2>
              </div>
              <TicketForm :ticket="ticket" :saving="ticketsStore.saving" submit-label="Save changes" show-cancel @submit="saveTicket" @cancel="isEditing = false" />
            </section>
            <TicketInfoCard v-else :ticket="ticket" />

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
            <section class="rounded-2xl border border-accent-line bg-accent-soft/70 p-5">
              <span class="grid size-9 place-items-center rounded-xl bg-surface text-accent-fg shadow-sm">
                <AppIcon name="star" class="size-4.5" />
              </span>
              <h3 class="mt-3 text-sm font-bold text-fg">Keep the conversation moving</h3>
              <p class="mt-1.5 text-xs leading-5 text-fg-muted">Leave a clear update so the next teammate knows exactly what happened.</p>
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
