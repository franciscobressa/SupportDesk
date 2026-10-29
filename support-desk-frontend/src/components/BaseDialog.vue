<script setup lang="ts">
withDefaults(defineProps<{
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
</script>

<template>
  <div v-if="open" class="modal-backdrop" @click.self="emit('close')">
    <section
      class="modal-panel"
      :class="compact ? 'max-w-md p-6' : ''"
      :role="role"
      aria-modal="true"
      aria-labelledby="dialog-title"
    >
      <template v-if="compact">
        <slot name="icon" />
        <h2 id="dialog-title" class="mt-4 text-lg font-bold text-slate-900">{{ title }}</h2>
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
            <h2 id="dialog-title" class="mt-1 text-xl font-bold text-slate-900">{{ title }}</h2>
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
