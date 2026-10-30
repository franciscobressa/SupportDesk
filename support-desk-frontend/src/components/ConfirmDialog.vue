<script setup lang="ts">
import AppIcon from './icons/AppIcon.vue'
import BaseDialog from './BaseDialog.vue'

const props = withDefaults(defineProps<{
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

// Escape and backdrop clicks are ignored while deleting, so the dialog cannot vanish mid-request.
function requestClose(): void {
  if (!props.loading) {
    emit('cancel')
  }
}
</script>

<template>
  <BaseDialog
    :open="open"
    :title="title"
    :description="description"
    :error="error"
    role="alertdialog"
    compact
    @close="requestClose"
  >
    <template #icon>
      <span class="grid size-11 place-items-center rounded-full bg-danger-soft text-danger-fg">
        <AppIcon name="trash" class="size-5" />
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
