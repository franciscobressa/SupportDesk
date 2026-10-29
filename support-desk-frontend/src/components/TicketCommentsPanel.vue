<script setup lang="ts">
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
  <section class="rounded-2xl border border-slate-200/80 bg-white p-5 shadow-sm sm:p-6">
    <div class="flex items-center justify-between">
      <div>
        <p class="text-xs font-bold uppercase tracking-[0.14em] text-indigo-600">Conversation</p>
        <h2 class="mt-1 text-lg font-bold text-slate-900">
          Comments <span class="ml-1 rounded-full bg-slate-100 px-2 py-0.5 align-middle text-xs font-semibold text-slate-600">{{ comments.length }}</span>
        </h2>
      </div>
      <span class="grid size-9 place-items-center rounded-xl bg-indigo-50 text-indigo-600">
        <svg viewBox="0 0 24 24" fill="none" class="size-[18px]" aria-hidden="true">
          <path d="M4 5.75A1.75 1.75 0 0 1 5.75 4h12.5A1.75 1.75 0 0 1 20 5.75v8.5A1.75 1.75 0 0 1 18.25 16H13l-4.5 4v-4h-2.75A1.75 1.75 0 0 1 4 14.25v-8.5Z" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round" />
        </svg>
      </span>
    </div>

    <div v-if="comments.length" class="mt-5 space-y-5">
      <article v-for="comment in comments" :key="comment.id" class="flex gap-3">
        <span class="grid size-9 shrink-0 place-items-center rounded-full bg-amber-100 text-[10px] font-bold text-amber-800">{{ getInitials(comment.author) }}</span>
        <div class="min-w-0 flex-1">
          <div class="flex flex-wrap items-baseline justify-between gap-x-3 gap-y-1">
            <p class="text-sm font-semibold text-slate-800">{{ comment.author }}</p>
            <time class="text-xs text-slate-400">{{ formatDate(comment.createdAt, { dateStyle: 'medium', timeStyle: 'short' }) }}</time>
          </div>
          <p class="mt-2 whitespace-pre-wrap rounded-r-xl rounded-bl-xl bg-slate-50 px-4 py-3 text-sm leading-6 text-slate-600">{{ comment.text }}</p>
        </div>
      </article>
    </div>
    <div v-else class="mt-5 rounded-xl border border-dashed border-slate-200 px-4 py-8 text-center">
      <p class="text-sm font-medium text-slate-600">No comments yet</p>
      <p class="mt-1 text-xs text-slate-400">Start the conversation with your team.</p>
    </div>

    <form class="mt-6 border-t border-slate-100 pt-5" @submit.prevent="emit('submit')">
      <label for="new-comment" class="form-label">Add a comment</label>
      <textarea
        id="new-comment"
        :value="modelValue"
        class="form-input min-h-24 resize-y"
        maxlength="2000"
        placeholder="Share an update or next step..."
        @input="emit('update:modelValue', ($event.target as HTMLTextAreaElement).value); emit('clear-error')"
      />
      <p v-if="error" class="mt-2 text-sm text-rose-600" role="alert">{{ error }}</p>
      <div class="mt-3 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <p class="text-xs text-slate-400">Posted as Support Manager (mock)</p>
        <button type="submit" class="button-primary self-start sm:self-auto" :disabled="submitting">
          <svg v-if="!submitting" viewBox="0 0 24 24" fill="none" class="size-4" aria-hidden="true">
            <path d="m21 3-7.2 18-3.7-7.1L3 10.2 21 3Zm0 0L10.1 13.9" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
          <span v-else class="size-4 animate-spin rounded-full border-2 border-white/40 border-t-white" />
          {{ submitting ? 'Posting...' : 'Post comment' }}
        </button>
      </div>
    </form>
  </section>
</template>
