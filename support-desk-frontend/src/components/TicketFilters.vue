<script setup lang="ts">
import { computed } from 'vue'
import type { TicketListFilters } from '../types/tickets'

const props = defineProps<{
  filters: TicketListFilters
}>()

const emit = defineEmits<{
  'update:filters': [filters: TicketListFilters]
}>()

const search = computed({
  get: () => props.filters.search ?? '',
  set: (value: string) => emit('update:filters', { ...props.filters, search: value }),
})

const status = computed({
  get: () => props.filters.status ?? '',
  set: (value: TicketListFilters['status']) => emit('update:filters', { ...props.filters, status: value }),
})

const priority = computed({
  get: () => props.filters.priority ?? '',
  set: (value: TicketListFilters['priority']) => emit('update:filters', { ...props.filters, priority: value }),
})

const sortBy = computed({
  get: () => props.filters.sortBy ?? 'createdAt',
  set: (value: TicketListFilters['sortBy']) => emit('update:filters', { ...props.filters, sortBy: value }),
})

const sortDirection = computed(() => props.filters.sortDirection ?? 'desc')

function toggleSortDirection(): void {
  emit('update:filters', {
    ...props.filters,
    sortDirection: sortDirection.value === 'asc' ? 'desc' : 'asc',
  })
}
</script>

<template>
  <div class="grid gap-3 md:grid-cols-[minmax(220px,1fr)_160px_160px_auto]">
    <label class="relative block">
      <span class="sr-only">Search tickets</span>
      <svg viewBox="0 0 24 24" fill="none" class="pointer-events-none absolute left-3 top-1/2 size-[18px] -translate-y-1/2 text-slate-400" aria-hidden="true">
        <circle cx="10.8" cy="10.8" r="6.8" stroke="currentColor" stroke-width="1.8" />
        <path d="m16 16 4 4" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" />
      </svg>
      <input v-model="search" class="form-input pl-10" placeholder="Search tickets..." />
    </label>

    <label>
      <span class="sr-only">Filter by status</span>
      <select v-model="status" class="form-input">
        <option value="">All statuses</option>
        <option value="Open">Open</option>
        <option value="InProgress">In progress</option>
        <option value="Resolved">Resolved</option>
        <option value="Closed">Closed</option>
      </select>
    </label>

    <label>
      <span class="sr-only">Filter by priority</span>
      <select v-model="priority" class="form-input">
        <option value="">All priorities</option>
        <option value="High">High priority</option>
        <option value="Medium">Medium priority</option>
        <option value="Low">Low priority</option>
      </select>
    </label>

    <div class="flex gap-2">
      <label class="min-w-0 flex-1">
        <span class="sr-only">Sort tickets by</span>
        <select v-model="sortBy" class="form-input">
          <option value="createdAt">Created date</option>
          <option value="priority">Priority</option>
          <option value="status">Status</option>
        </select>
      </label>
      <button
        type="button"
        class="button-icon"
        :aria-label="sortDirection === 'desc' ? 'Sort descending' : 'Sort ascending'"
        :title="sortDirection === 'desc' ? 'Sort descending' : 'Sort ascending'"
        @click="toggleSortDirection"
      >
        <svg viewBox="0 0 24 24" fill="none" class="size-[18px]" aria-hidden="true">
          <path v-if="sortDirection === 'desc'" d="M12 5v14m0 0-6-6m6 6 6-6" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" />
          <path v-else d="M12 19V5m0 0 6 6m-6-6-6 6" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" />
        </svg>
      </button>
    </div>
  </div>
</template>
