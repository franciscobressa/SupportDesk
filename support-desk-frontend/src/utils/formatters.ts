import type { TicketStatus } from '../types/tickets'

export function formatStatus(value: TicketStatus): string {
  return value === 'InProgress' ? 'In progress' : value
}

export function formatDate(
  value: string | null | undefined,
  options: Intl.DateTimeFormatOptions = { dateStyle: 'medium' },
): string {
  if (!value) {
    return '—'
  }

  return new Intl.DateTimeFormat('en-US', options).format(new Date(value))
}

export function getInitials(name: string | null | undefined = ''): string {
  const initials = name
    ?.trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part[0])
    .join('')
    .toUpperCase()

  return initials || 'C'
}
