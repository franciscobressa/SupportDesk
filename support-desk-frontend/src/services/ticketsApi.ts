import type {
  CreateTicketRequest,
  TicketComment,
  TicketDetails,
  TicketListFilters,
  TicketListResponse,
  UpdateTicketRequest,
} from '../types/tickets'

interface ApiProblem {
  detail?: string
  title?: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

const configuredApiBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim() || '/api'
const apiBaseUrl = configuredApiBaseUrl.replace(/\/+$/, '')

async function request(path: string, options: RequestInit = {}): Promise<Response> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...options,
    headers: {
      ...(options.body ? { 'Content-Type': 'application/json' } : {}),
      ...options.headers,
    },
  })

  if (!response.ok) {
    let message = `Request failed with status ${response.status}.`

    try {
      const problem = (await response.json()) as ApiProblem
      const validationMessages = Object.values(problem.errors ?? {}).flat()
      message = validationMessages[0] ?? problem.detail ?? problem.title ?? message
    } catch (error) {
      if (!(error instanceof SyntaxError)) {
        throw error
      }
    }

    throw new ApiError(message, response.status)
  }

  return response
}

async function requestJson<TResponse>(path: string, options: RequestInit = {}): Promise<TResponse> {
  const response = await request(path, options)
  return response.json() as Promise<TResponse>
}

export function getTickets(filters: TicketListFilters = {}): Promise<TicketListResponse> {
  const query = new URLSearchParams()

  for (const [key, value] of Object.entries(filters)) {
    if (value !== '' && value !== null && value !== undefined) {
      query.set(key, value)
    }
  }

  const queryString = query.toString()
  return requestJson<TicketListResponse>(`/tickets${queryString ? `?${queryString}` : ''}`)
}

export function getTicket(id: string): Promise<TicketDetails> {
  return requestJson<TicketDetails>(`/tickets/${encodeURIComponent(id)}`)
}

export function createTicket(ticket: CreateTicketRequest): Promise<TicketDetails> {
  return requestJson<TicketDetails>('/tickets', {
    method: 'POST',
    body: JSON.stringify(ticket),
  })
}

export function updateTicket(id: string, ticket: UpdateTicketRequest): Promise<TicketDetails> {
  return requestJson<TicketDetails>(`/tickets/${encodeURIComponent(id)}`, {
    method: 'PUT',
    body: JSON.stringify(ticket),
  })
}

export function deleteTicket(id: string): Promise<void> {
  return request(`/tickets/${encodeURIComponent(id)}`, {
    method: 'DELETE',
  }).then(() => undefined)
}

export function createComment(ticketId: string, text: string): Promise<TicketComment> {
  return requestJson<TicketComment>(`/tickets/${encodeURIComponent(ticketId)}/comments`, {
    method: 'POST',
    body: JSON.stringify({ text }),
  })
}
