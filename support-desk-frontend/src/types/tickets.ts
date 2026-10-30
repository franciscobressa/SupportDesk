export type TicketStatus = 'Open' | 'InProgress' | 'Resolved' | 'Closed'

export type TicketPriority = 'Low' | 'Medium' | 'High'

export type TicketSortField = 'createdAt' | 'priority' | 'status'

export type SortDirection = 'asc' | 'desc'

export interface TicketListFilters {
  search?: string
  status?: TicketStatus | ''
  priority?: TicketPriority | ''
  sortBy?: TicketSortField
  sortDirection?: SortDirection
  page?: number
  pageSize?: number
}

export interface TicketPageMetadata {
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface TicketListResponse extends TicketPageMetadata {
  items: Ticket[]
  summary: TicketSummary
}

export interface TicketSummary {
  open: number
  inProgress: number
  resolved: number
  highPriority: number
}

export interface Ticket {
  id: string
  title: string
  description: string
  status: TicketStatus
  priority: TicketPriority
  customerName: string | null
  customerEmail: string | null
  createdAt: string
  updatedAt: string
}

export interface TicketComment {
  id: string
  text: string
  author: string
  createdAt: string
}

export interface TicketStatusChange {
  id: string
  previousStatus: TicketStatus
  newStatus: TicketStatus
  author: string
  createdAt: string
}

export interface TicketDetails extends Ticket {
  comments: TicketComment[]
  statusChanges: TicketStatusChange[]
}

export interface TicketWriteRequest {
  title: string
  description: string
  status: TicketStatus
  priority: TicketPriority
  customerName: string | null
  customerEmail: string | null
}

export type CreateTicketRequest = TicketWriteRequest

export interface UpdateTicketRequest extends TicketWriteRequest {
  // The updatedAt value the form was loaded from, sent back unchanged so the API can detect edits made meanwhile.
  expectedUpdatedAt: string
}
