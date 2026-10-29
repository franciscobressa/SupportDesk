<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import type { TicketDetails, TicketPriority, TicketStatus, TicketWriteRequest } from '../types/tickets'

const props = withDefaults(defineProps<{
  ticket?: TicketDetails | null
  saving?: boolean
  submitLabel?: string
  showCancel?: boolean
}>(), {
  ticket: null,
  saving: false,
  submitLabel: 'Save ticket',
  showCancel: false,
})

const emit = defineEmits<{
  submit: [ticket: TicketWriteRequest]
  cancel: []
}>()
const customerError = ref('')
const form = reactive<{
  title: string
  description: string
  status: TicketStatus
  priority: TicketPriority
  customerName: string
  customerEmail: string
}>({
  title: '',
  description: '',
  status: 'Open',
  priority: 'Medium',
  customerName: '',
  customerEmail: '',
})

watch(
  () => props.ticket,
  (ticket) => {
    Object.assign(form, {
      title: ticket?.title ?? '',
      description: ticket?.description ?? '',
      status: ticket?.status ?? 'Open',
      priority: ticket?.priority ?? 'Medium',
      customerName: ticket?.customerName ?? '',
      customerEmail: ticket?.customerEmail ?? '',
    })
    customerError.value = ''
  },
  { immediate: true },
)

function submitForm(): void {
  customerError.value = ''

  if (!form.customerName.trim() && !form.customerEmail.trim()) {
    customerError.value = 'Add a customer name or email address.'
    return
  }

  emit('submit', {
    title: form.title.trim(),
    description: form.description.trim(),
    status: form.status,
    priority: form.priority,
    customerName: form.customerName.trim() || null,
    customerEmail: form.customerEmail.trim() || null,
  })
}
</script>

<template>
  <form class="space-y-5" @submit.prevent="submitForm">
    <div>
      <label for="ticket-title" class="form-label">Subject</label>
      <input
        id="ticket-title"
        v-model="form.title"
        class="form-input"
        maxlength="120"
        placeholder="What does the customer need help with?"
        required
      />
    </div>

    <div>
      <label for="ticket-description" class="form-label">Description</label>
      <textarea
        id="ticket-description"
        v-model="form.description"
        class="form-input min-h-32 resize-y"
        maxlength="5000"
        placeholder="Add the details support needs to resolve this request..."
        required
      />
    </div>

    <div class="grid gap-4 sm:grid-cols-2">
      <div>
        <label for="ticket-status" class="form-label">Status</label>
        <select id="ticket-status" v-model="form.status" class="form-input">
          <option value="Open">Open</option>
          <option value="InProgress">In progress</option>
          <option value="Resolved">Resolved</option>
          <option value="Closed">Closed</option>
        </select>
      </div>
      <div>
        <label for="ticket-priority" class="form-label">Priority</label>
        <select id="ticket-priority" v-model="form.priority" class="form-input">
          <option value="Low">Low</option>
          <option value="Medium">Medium</option>
          <option value="High">High</option>
        </select>
      </div>
    </div>

    <div class="grid gap-4 sm:grid-cols-2">
      <div>
        <label for="customer-name" class="form-label">Customer name</label>
        <input
          id="customer-name"
          v-model="form.customerName"
          class="form-input"
          maxlength="120"
          autocomplete="name"
          placeholder="e.g. Alex Morgan"
          @input="customerError = ''"
        />
      </div>
      <div>
        <label for="customer-email" class="form-label">Customer email</label>
        <input
          id="customer-email"
          v-model="form.customerEmail"
          class="form-input"
          maxlength="254"
          type="email"
          autocomplete="email"
          placeholder="alex@example.com"
          @input="customerError = ''"
        />
      </div>
    </div>
    <p v-if="customerError" class="-mt-3 text-sm text-rose-600" role="alert">
      {{ customerError }}
    </p>

    <div class="flex flex-col-reverse gap-3 border-t border-slate-100 pt-5 sm:flex-row sm:justify-end">
      <button
        v-if="showCancel"
        type="button"
        class="button-secondary"
        :disabled="saving"
        @click="emit('cancel')"
      >
        Cancel
      </button>
      <button type="submit" class="button-primary" :disabled="saving">
        <span
          v-if="saving"
          class="size-4 animate-spin rounded-full border-2 border-white/40 border-t-white"
        />
        {{ saving ? 'Saving...' : submitLabel }}
      </button>
    </div>
  </form>
</template>
