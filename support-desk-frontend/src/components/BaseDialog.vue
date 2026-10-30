<script setup lang="ts">
import { nextTick, ref, useId, watch } from 'vue'

const props = withDefaults(defineProps<{
  open: boolean
  title: string
  eyebrow?: string
  description?: string
  error?: string
  role?: 'dialog' | 'alertdialog'
  compact?: boolean
}>(), {
  eyebrow: '',
  description: '',
  error: '',
  role: 'dialog',
  compact: false,
})

const emit = defineEmits<{
  close: []
}>()

const titleId = useId()
const panel = ref<HTMLElement | null>(null)
const focusableSelector =
  'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'
let previouslyFocused: HTMLElement | null = null

function getFocusableElements(): HTMLElement[] {
  return panel.value ? [...panel.value.querySelectorAll<HTMLElement>(focusableSelector)] : []
}

// Move focus into the dialog (first form field, else first control) and give it back on close,
// so keyboard and screen reader users stay in context.
watch(
  () => props.open,
  async (open) => {
    if (!open) {
      previouslyFocused?.focus()
      previouslyFocused = null
      return
    }

    previouslyFocused = document.activeElement instanceof HTMLElement ? document.activeElement : null
    await nextTick()
    const firstField = panel.value?.querySelector<HTMLElement>('input, select, textarea')
    ;(firstField ?? getFocusableElements()[0] ?? panel.value)?.focus()
  },
  { immediate: true },
)

function handleKeydown(event: KeyboardEvent): void {
  if (event.key === 'Escape') {
    event.stopPropagation()
    emit('close')
    return
  }

  if (event.key !== 'Tab') {
    return
  }

  // Keep Tab navigation inside the modal while it is open.
  const elements = getFocusableElements()
  const first = elements[0]
  const last = elements[elements.length - 1]
  if (!first || !last) {
    event.preventDefault()
  } else if (event.shiftKey && document.activeElement === first) {
    event.preventDefault()
    last.focus()
  } else if (!event.shiftKey && document.activeElement === last) {
    event.preventDefault()
    first.focus()
  }
}
</script>

<template>
  <div v-if="open" class="modal-backdrop" @click.self="emit('close')" @keydown="handleKeydown">
    <section
      ref="panel"
      tabindex="-1"
      class="modal-panel focus:outline-none"
      :class="compact ? 'max-w-md p-6' : ''"
      :role="role"
      aria-modal="true"
      :aria-labelledby="titleId"
    >
      <template v-if="compact">
        <slot name="icon" />
        <h2 :id="titleId" class="mt-4 text-lg font-bold text-slate-900">{{ title }}</h2>
        <p v-if="description" class="mt-2 text-sm leading-6 text-slate-500">{{ description }}</p>
        <p v-if="error" class="mt-4 rounded-lg bg-rose-50 px-3 py-2 text-sm text-rose-700" role="alert">{{ error }}</p>
        <div class="mt-6 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
          <slot name="actions" />
        </div>
      </template>
      <template v-else>
        <div class="flex items-start justify-between gap-4 border-b border-slate-100 px-6 py-5">
          <div>
            <p v-if="eyebrow" class="text-xs font-bold uppercase tracking-[0.14em] text-indigo-600">{{ eyebrow }}</p>
            <h2 :id="titleId" class="mt-1 text-xl font-bold text-slate-900">{{ title }}</h2>
            <p v-if="description" class="mt-1 text-sm text-slate-500">{{ description }}</p>
          </div>
          <button type="button" class="button-icon -mr-2 -mt-1" aria-label="Close dialog" @click="emit('close')">
            <svg viewBox="0 0 24 24" fill="none" class="size-5" aria-hidden="true">
              <path d="m6 6 12 12M18 6 6 18" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" />
            </svg>
          </button>
        </div>
        <p v-if="error" class="mx-6 mt-5 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700" role="alert">{{ error }}</p>
        <div class="px-6 py-5">
          <slot />
        </div>
      </template>
    </section>
  </div>
</template>
