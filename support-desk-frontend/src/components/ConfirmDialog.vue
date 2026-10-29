<script setup lang="ts">
import BaseDialog from './BaseDialog.vue'

withDefaults(defineProps<{
  open: boolean
  title: string
  description: string
  confirmLabel?: string
  loading?: boolean
  error?: string
}>(), {
  confirmLabel: 'Delete ticket',
  loading: false,
  error: '',
})

const emit = defineEmits<{
  cancel: []
  confirm: []
}>()
</script>

<template>
  <BaseDialog
    :open="open"
    :title="title"
    :description="description"
    :error="error"
    role="alertdialog"
    compact
    @close="emit('cancel')"
  >
    <template #icon>
      <span class="grid size-11 place-items-center rounded-full bg-rose-50 text-rose-600">
        <svg viewBox="0 0 24 24" fill="none" class="size-5" aria-hidden="true">
          <path d="M4 7h16M10 11v6m4-6v6M5.5 7l1 13h11l1-13M9 7V4h6v3" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" />
        </svg>
      </span>
    </template>
    <template #actions>
      <button type="button" class="button-secondary" :disabled="loading" @click="emit('cancel')">Cancel</button>
      <button type="button" class="button-danger" :disabled="loading" @click="emit('confirm')">
        {{ loading ? 'Deleting...' : confirmLabel }}
      </button>
    </template>
  </BaseDialog>
</template>
