import { defineStore } from 'pinia'
import type {
  Ticket,
  TicketDetails,
  TicketListFilters,
  TicketPageMetadata,
  TicketSummary,
  TicketWriteRequest,
  UpdateTicketRequest,
} from '../types/tickets'
import {
  createComment,
  createTicket,
  deleteTicket,
  getTicket,
  getTickets,
  updateTicket,
} from '../services/ticketsApi'

const minimumListLoadingDurationMs = 400

export const useTicketsStore = defineStore('tickets', {
  state: () => ({
    tickets: [] as Ticket[],
    pagination: {
      page: 1,
      pageSize: 10,
      totalCount: 0,
      totalPages: 0,
    } as TicketPageMetadata,
    summary: {
      open: 0,
      inProgress: 0,
      resolved: 0,
      highPriority: 0,
    } as TicketSummary,
    selectedTicket: null as TicketDetails | null,
    listLoading: false,
    detailLoading: false,
    saving: false,
    commentSubmitting: false,
    listError: '',
    detailError: '',
    mutationError: '',
    listRequestId: 0,
    detailRequestId: 0,
  }),

  actions: {
    async fetchTickets(filters: TicketListFilters = {}) {
      const requestId = ++this.listRequestId
      const loadingStartedAt = Date.now()
      this.listLoading = true
      this.listError = ''

      try {
        const result = await getTickets(filters)
        if (requestId === this.listRequestId) {
          this.tickets = result.items
          this.pagination = {
            page: result.page,
            pageSize: result.pageSize,
            totalCount: result.totalCount,
            totalPages: result.totalPages,
          }
          this.summary = result.summary
        }
      } catch (error: unknown) {
        if (requestId === this.listRequestId) {
          this.listError = getErrorMessage(error)
        }
        throw error
      } finally {
        const remainingLoadingTime = minimumListLoadingDurationMs - (Date.now() - loadingStartedAt)
        if (remainingLoadingTime > 0) {
          await new Promise<void>((resolve) => {
            setTimeout(resolve, remainingLoadingTime)
          })
        }

        if (requestId === this.listRequestId) {
          this.listLoading = false
        }
      }
    },

    async fetchTicket(id: string) {
      const requestId = ++this.detailRequestId
      this.detailLoading = true
      this.detailError = ''
      this.selectedTicket = null

      try {
        const ticket = await getTicket(id)
        if (requestId === this.detailRequestId) {
          this.selectedTicket = ticket
        }
        return ticket
      } catch (error: unknown) {
        if (requestId === this.detailRequestId) {
          this.detailError = getErrorMessage(error)
        }
        throw error
      } finally {
        if (requestId === this.detailRequestId) {
          this.detailLoading = false
        }
      }
    },

    async addTicket(ticket: TicketWriteRequest) {
      this.saving = true
      this.mutationError = ''

      try {
        const created = await createTicket(ticket)
        this.selectedTicket = created
        return created
      } catch (error: unknown) {
        this.mutationError = getErrorMessage(error)
        throw error
      } finally {
        this.saving = false
      }
    },

    async saveTicket(id: string, ticket: UpdateTicketRequest) {
      this.saving = true
      this.mutationError = ''

      try {
        const updated = await updateTicket(id, ticket)
        this.replaceTicket(updated)
        this.selectedTicket = updated
        return updated
      } catch (error: unknown) {
        this.mutationError = getErrorMessage(error)
        throw error
      } finally {
        this.saving = false
      }
    },

    async removeTicket(id: string) {
      this.saving = true
      this.mutationError = ''

      try {
        await deleteTicket(id)
        this.tickets = this.tickets.filter((ticket) => ticket.id !== id)
        this.pagination.totalCount = Math.max(0, this.pagination.totalCount - 1)
        this.pagination.totalPages = Math.ceil(this.pagination.totalCount / this.pagination.pageSize)
        if (this.selectedTicket?.id === id) {
          this.selectedTicket = null
        }
      } catch (error: unknown) {
        this.mutationError = getErrorMessage(error)
        throw error
      } finally {
        this.saving = false
      }
    },

    async addComment(ticketId: string, text: string) {
      this.commentSubmitting = true
      this.mutationError = ''

      try {
        const comment = await createComment(ticketId, text)

        if (this.selectedTicket?.id === ticketId) {
          this.selectedTicket.comments = [...this.selectedTicket.comments, comment]
          this.selectedTicket.updatedAt = comment.createdAt
        }

        const listTicket = this.tickets.find((ticket) => ticket.id === ticketId)
        if (listTicket) {
          listTicket.updatedAt = comment.createdAt
        }

        return comment
      } catch (error: unknown) {
        this.mutationError = getErrorMessage(error)
        throw error
      } finally {
        this.commentSubmitting = false
      }
    },

    replaceTicket(updated: TicketDetails) {
      const index = this.tickets.findIndex((ticket) => ticket.id === updated.id)
      if (index >= 0) {
        this.tickets.splice(index, 1, updated)
      }
    },
  },
})

function getErrorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'An unexpected error occurred.'
}
