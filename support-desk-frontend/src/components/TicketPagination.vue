<script setup lang="ts">
import { computed } from 'vue'
import type { TicketPageMetadata } from '../types/tickets'
import AppIcon from './icons/AppIcon.vue'

const props = defineProps<{
  pagination: TicketPageMetadata
  currentPage: number
  disabled: boolean
}>()

const emit = defineEmits<{
  change: [page: number]
}>()

type PageNavigationItem = { kind: 'page'; page: number } | { kind: 'ellipsis'; id: string }

const firstTicketNumber = computed(() =>
  props.pagination.totalCount === 0 ? 0 : (props.pagination.page - 1) * props.pagination.pageSize + 1,
)
const lastTicketNumber = computed(() =>
  Math.min(props.pagination.page * props.pagination.pageSize, props.pagination.totalCount),
)
const isFirstPage = computed(() => props.currentPage <= 1)
const isLastPage = computed(() => props.currentPage >= props.pagination.totalPages)

// Always show the first, last, current and neighbouring pages; collapse longer gaps into "…".
// A gap of exactly one page shows that page instead, since an ellipsis would not save space.
const pageNavigationItems = computed<PageNavigationItem[]>(() => {
  const { totalPages } = props.pagination
  if (totalPages <= 5) {
    return Array.from({ length: totalPages }, (_, index) => ({ kind: 'page' as const, page: index + 1 }))
  }

  const pages = [...new Set([1, props.currentPage - 1, props.currentPage, props.currentPage + 1, totalPages])]
    .filter((page) => page >= 1 && page <= totalPages)
    .sort((first, second) => first - second)
  const items: PageNavigationItem[] = []

  pages.forEach((page, index) => {
    const previousPage = pages[index - 1]
    if (previousPage !== undefined) {
      const gap = page - previousPage
      if (gap === 2) {
        items.push({ kind: 'page', page: previousPage + 1 })
      } else if (gap > 2) {
        items.push({ kind: 'ellipsis', id: `${previousPage}-${page}` })
      }
    }

    items.push({ kind: 'page', page })
  })

  return items
})
</script>

<template>
  <div class="flex flex-col gap-3 border-t border-line-subtle px-5 py-4 text-xs text-fg-subtle sm:px-6">
    <div class="flex flex-col justify-between gap-2 sm:flex-row sm:items-center">
      <p>
        Showing
        <span class="font-semibold text-fg-soft">{{ firstTicketNumber }}–{{ lastTicketNumber }}</span>
        of
        <span class="font-semibold text-fg-soft">{{ pagination.totalCount }}</span>
        tickets
      </p>
      <slot />
    </div>

    <nav class="flex flex-wrap items-center justify-center gap-2" aria-label="Ticket pages">
      <div class="hidden items-center gap-2 sm:flex">
        <button
          type="button"
          class="button-secondary min-h-9 px-3 py-1.5"
          :disabled="isFirstPage || disabled"
          @click="emit('change', currentPage - 1)"
        >
          Previous
        </button>
        <template v-for="item in pageNavigationItems" :key="item.kind === 'page' ? item.page : item.id">
          <span v-if="item.kind === 'ellipsis'" class="px-1 text-fg-faint" aria-hidden="true">…</span>
          <button
            v-else
            type="button"
            class="button-secondary min-h-9 min-w-9 px-2 py-1.5"
            :class="item.page === currentPage ? 'border-accent! bg-accent! text-white!' : ''"
            :aria-label="`Go to page ${item.page}`"
            :aria-current="item.page === currentPage ? 'page' : undefined"
            :disabled="disabled"
            @click="emit('change', item.page)"
          >
            {{ item.page }}
          </button>
        </template>
        <button
          type="button"
          class="button-secondary min-h-9 px-3 py-1.5"
          :disabled="isLastPage || disabled"
          @click="emit('change', currentPage + 1)"
        >
          Next
        </button>
      </div>

      <div class="grid w-full max-w-48 grid-cols-4 gap-2 sm:hidden">
        <button type="button" class="button-icon size-10" aria-label="First page" :disabled="isFirstPage || disabled" @click="emit('change', 1)">
          <AppIcon name="chevrons-left" class="size-4.5" />
        </button>
        <button type="button" class="button-icon size-10" aria-label="Previous page" :disabled="isFirstPage || disabled" @click="emit('change', currentPage - 1)">
          <AppIcon name="chevron-left" class="size-4.5" />
        </button>
        <button type="button" class="button-icon size-10" aria-label="Next page" :disabled="isLastPage || disabled" @click="emit('change', currentPage + 1)">
          <AppIcon name="chevron-right" class="size-4.5" />
        </button>
        <button type="button" class="button-icon size-10" aria-label="Last page" :disabled="isLastPage || disabled" @click="emit('change', pagination.totalPages)">
          <AppIcon name="chevrons-right" class="size-4.5" />
        </button>
      </div>

      <p class="basis-full text-center" aria-live="polite">
        Page
        <span class="font-semibold text-fg-soft">{{ pagination.page }}</span>
        of
        <span class="font-semibold text-fg-soft">{{ pagination.totalPages }}</span>
      </p>
    </nav>
  </div>
</template>
