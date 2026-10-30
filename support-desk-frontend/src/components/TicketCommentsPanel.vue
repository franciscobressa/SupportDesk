<script setup lang="ts">
import AppIcon from './icons/AppIcon.vue'
import type { TicketComment } from '../types/tickets'
import { formatDate, getInitials } from '../utils/formatters'

defineProps<{
  comments: TicketComment[]
  modelValue: string
  submitting: boolean
  error: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
  'clear-error': []
  submit: []
}>()
</script>

<template>
  <section class="rounded-2xl border border-line/80 bg-surface p-5 shadow-sm sm:p-6">
    <div class="flex items-center justify-between">
      <div>
        <p class="text-xs font-bold uppercase tracking-[0.14em] text-accent-fg">Conversation</p>
        <h2 class="mt-1 text-lg font-bold text-fg-strong">
          Comments <span class="ml-1 rounded-full bg-surface-muted px-2 py-0.5 align-middle text-xs font-semibold text-fg-muted">{{ comments.length }}</span>
        </h2>
      </div>
      <span class="grid size-9 place-items-center rounded-xl bg-accent-soft text-accent-fg">
        <AppIcon name="chat" class="size-4.5" />
      </span>
    </div>

    <div v-if="comments.length" class="mt-5 space-y-5">
      <article v-for="comment in comments" :key="comment.id" class="flex gap-3">
        <span class="grid size-9 shrink-0 place-items-center rounded-full bg-amber-100 dark:bg-amber-900 text-[10px] font-bold text-amber-800 dark:text-amber-200">{{ getInitials(comment.author) }}</span>
        <div class="min-w-0 flex-1">
          <div class="flex flex-wrap items-baseline justify-between gap-x-3 gap-y-1">
            <p class="text-sm font-semibold text-fg">{{ comment.author }}</p>
            <time class="text-xs text-fg-faint">{{ formatDate(comment.createdAt, { dateStyle: 'medium', timeStyle: 'short' }) }}</time>
          </div>
          <p class="mt-2 whitespace-pre-wrap rounded-r-xl rounded-bl-xl bg-surface-subtle px-4 py-3 text-sm leading-6 text-fg-muted">{{ comment.text }}</p>
        </div>
      </article>
    </div>
    <div v-else class="mt-5 rounded-xl border border-dashed border-line px-4 py-8 text-center">
      <p class="text-sm font-medium text-fg-muted">No comments yet</p>
      <p class="mt-1 text-xs text-fg-faint">Start the conversation with your team.</p>
    </div>

    <form class="mt-6 border-t border-line-subtle pt-5" @submit.prevent="emit('submit')">
      <label for="new-comment" class="form-label">Add a comment</label>
      <textarea
        id="new-comment"
        :value="modelValue"
        class="form-input min-h-24 resize-y"
        maxlength="2000"
        placeholder="Share an update or next step..."
        @input="emit('update:modelValue', ($event.target as HTMLTextAreaElement).value); emit('clear-error')"
      />
      <p v-if="error" class="mt-2 text-sm text-danger-fg" role="alert">{{ error }}</p>
      <div class="mt-3 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <p class="text-xs text-fg-faint">Posted as Support Manager (mock)</p>
        <button type="submit" class="button-primary self-start sm:self-auto" :disabled="submitting">
          <AppIcon v-if="!submitting" name="send" class="size-4" />
          <span v-else class="size-4 animate-spin rounded-full border-2 border-white/40 border-t-white" />
          {{ submitting ? 'Posting...' : 'Post comment' }}
        </button>
      </div>
    </form>
  </section>
</template>
