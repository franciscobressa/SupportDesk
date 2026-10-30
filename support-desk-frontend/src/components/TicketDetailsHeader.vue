<script setup lang="ts">
import type { TicketDetails } from '../types/tickets'
import { formatDate } from '../utils/formatters'
import AppIcon from './icons/AppIcon.vue'
import StatusBadge from './StatusBadge.vue'

defineProps<{
  ticket: TicketDetails
  isEditing: boolean
}>()

const emit = defineEmits<{
  edit: []
  delete: []
}>()

const dateTime: Intl.DateTimeFormatOptions = { dateStyle: 'medium', timeStyle: 'short' }
</script>

<template>
  <section class="rounded-2xl border border-line/80 bg-surface p-5 shadow-sm shadow-slate-200/30 sm:p-7 dark:shadow-none">
    <div class="flex flex-col justify-between gap-5 xl:flex-row xl:items-start">
      <div class="min-w-0">
        <div class="flex flex-wrap items-center gap-2">
          <span class="rounded-md bg-surface-muted px-2 py-1 font-mono text-[11px] font-semibold text-fg-subtle">#{{ ticket.id.slice(0, 8).toUpperCase() }}</span>
          <StatusBadge :status="ticket.status" />
          <StatusBadge :priority="ticket.priority" />
        </div>
        <h1 class="mt-4 wrap-break-word text-2xl font-bold tracking-tight text-fg-strong sm:text-3xl">{{ ticket.title }}</h1>
        <p class="mt-2 flex flex-wrap items-center gap-x-3 gap-y-1 text-sm text-fg-subtle">
          <span>Created {{ formatDate(ticket.createdAt, dateTime) }}</span>
          <span class="hidden size-1 rounded-full bg-line-strong sm:block" />
          <span>Last updated {{ formatDate(ticket.updatedAt, dateTime) }}</span>
        </p>
      </div>
      <div class="flex flex-wrap gap-2">
        <button type="button" class="button-secondary" :disabled="isEditing" @click="emit('edit')">
          <AppIcon name="pencil" class="size-4" />
          Edit ticket
        </button>
        <button type="button" class="button-danger-outline" @click="emit('delete')">
          <AppIcon name="trash" class="size-4" />
          Delete
        </button>
      </div>
    </div>
  </section>
</template>
