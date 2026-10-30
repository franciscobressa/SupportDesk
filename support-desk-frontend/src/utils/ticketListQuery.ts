import type { LocationQuery, LocationQueryValue } from 'vue-router'
import type {
  SortDirection,
  TicketListFilters,
  TicketPriority,
  TicketSortField,
  TicketStatus,
} from '../types/tickets'

export const defaultTicketListFilters = {
  search: '',
  status: '',
  priority: '',
  sortBy: 'createdAt',
  sortDirection: 'desc',
  page: 1,
  pageSize: 10,
} as const satisfies TicketListFilters

const statuses: readonly TicketStatus[] = ['Open', 'InProgress', 'Resolved', 'Closed']
const priorities: readonly TicketPriority[] = ['Low', 'Medium', 'High']
const sortFields: readonly TicketSortField[] = ['createdAt', 'priority', 'status']
const sortDirections: readonly SortDirection[] = ['asc', 'desc']

function readValue(value: LocationQueryValue | LocationQueryValue[] | undefined): string {
  return (Array.isArray(value) ? value[0] : value) ?? ''
}

function parseOption<T extends string>(value: string, options: readonly T[]): T | undefined {
  return options.find((option) => option === value)
}

// Unknown values from a hand-edited URL fall back to defaults instead of reaching the API as a 400.
export function filtersFromQuery(query: LocationQuery): TicketListFilters {
  const page = Number(readValue(query.page))

  return {
    ...defaultTicketListFilters,
    search: readValue(query.search),
    status: parseOption(readValue(query.status), statuses) ?? '',
    priority: parseOption(readValue(query.priority), priorities) ?? '',
    sortBy: parseOption(readValue(query.sortBy), sortFields) ?? defaultTicketListFilters.sortBy,
    sortDirection:
      parseOption(readValue(query.sortDirection), sortDirections) ?? defaultTicketListFilters.sortDirection,
    page: Number.isInteger(page) && page > 0 ? page : defaultTicketListFilters.page,
  }
}

// Only non-default values are written so the dashboard URL stays short and readable.
export function filtersToQuery(filters: TicketListFilters): Record<string, string> {
  const query: Record<string, string> = {}

  if (filters.search) query.search = filters.search
  if (filters.status) query.status = filters.status
  if (filters.priority) query.priority = filters.priority
  if (filters.sortBy && filters.sortBy !== defaultTicketListFilters.sortBy) query.sortBy = filters.sortBy
  if (filters.sortDirection && filters.sortDirection !== defaultTicketListFilters.sortDirection) {
    query.sortDirection = filters.sortDirection
  }
  if (filters.page && filters.page > 1) query.page = String(filters.page)

  return query
}
