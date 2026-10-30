// Icon geometry only: AppIcon sets stroke color and width on the <svg>, and children inherit them.

interface IconElement {
  tag: 'path' | 'circle' | 'rect'
  attrs: Record<string, string | number>
}

interface IconDefinition {
  strokeWidth: number
  elements: IconElement[]
}

const roundCaps = { 'stroke-linecap': 'round' }
const roundJoins = { 'stroke-linejoin': 'round' }
const round = { ...roundCaps, ...roundJoins }

function path(d: string, attrs: Record<string, string> = {}): IconElement {
  return { tag: 'path', attrs: { d, ...attrs } }
}

const chatBubble =
  'M4 5.75A1.75 1.75 0 0 1 5.75 4h12.5A1.75 1.75 0 0 1 20 5.75v8.5A1.75 1.75 0 0 1 18.25 16H13l-4.5 4v-4h-2.75A1.75 1.75 0 0 1 4 14.25v-8.5Z'

export const icons = {
  alert: { strokeWidth: 1.7, elements: [path('M12 8v4m0 4h.01M10.3 3.8 2.8 17a2 2 0 0 0 1.7 3h15a2 2 0 0 0 1.7-3l-7.5-13.2a2 2 0 0 0-3.4 0Z', round)] },
  'arrow-left': { strokeWidth: 1.8, elements: [path('m14.5 5-7 7 7 7', round)] },
  chat: { strokeWidth: 1.7, elements: [path(chatBubble, roundJoins)] },
  'chevron-left': { strokeWidth: 1.8, elements: [path('m15 18-6-6 6-6', round)] },
  'chevron-right': { strokeWidth: 1.8, elements: [path('m9 18 6-6-6-6', round)] },
  'chevrons-left': { strokeWidth: 1.8, elements: [path('m11 18-6-6 6-6M19 18l-6-6 6-6', round)] },
  'chevrons-right': { strokeWidth: 1.8, elements: [path('m13 6 6 6-6 6M5 6l6 6-6 6', round)] },
  clock: {
    strokeWidth: 1.7,
    elements: [{ tag: 'circle', attrs: { cx: 12, cy: 12, r: 8 } }, path('M12 8v4l2.5 1.5', roundCaps)],
  },
  close: { strokeWidth: 1.8, elements: [path('m6 6 12 12M18 6 6 18', roundCaps)] },
  dashboard: {
    strokeWidth: 1.7,
    elements: [
      { tag: 'rect', attrs: { x: 4, y: 4, width: 6.5, height: 6.5, rx: 1.5 } },
      { tag: 'rect', attrs: { x: 13.5, y: 4, width: 6.5, height: 4, rx: 1.5 } },
      { tag: 'rect', attrs: { x: 13.5, y: 10.5, width: 6.5, height: 9.5, rx: 1.5 } },
      { tag: 'rect', attrs: { x: 4, y: 13.5, width: 6.5, height: 6.5, rx: 1.5 } },
    ],
  },
  eye: {
    strokeWidth: 1.7,
    elements: [
      path('M2.5 12s3.2-6 9.5-6 9.5 6 9.5 6-3.2 6-9.5 6-9.5-6-9.5-6Z'),
      { tag: 'circle', attrs: { cx: 12, cy: 12, r: 2.5 } },
    ],
  },
  logo: { strokeWidth: 1.7, elements: [path(chatBubble, roundJoins), path('M8 9h8M8 12h5', roundCaps)] },
  moon: { strokeWidth: 1.7, elements: [path('M20.2 15.4A8.5 8.5 0 0 1 8.6 3.8 8.5 8.5 0 1 0 20.2 15.4Z', round)] },
  pencil: { strokeWidth: 1.7, elements: [path('m14 5 5 5M4 20l4.3-.9L19 8.4 15.6 5 4.9 15.7 4 20Z', round)] },
  plus: { strokeWidth: 2, elements: [path('M12 5v14M5 12h14', roundCaps)] },
  refresh: { strokeWidth: 1.8, elements: [path('M20 11a8 8 0 0 0-14.9-3M4 5v3h3m-3 5a8 8 0 0 0 14.9 3M20 19v-3h-3', round)] },
  search: {
    strokeWidth: 1.8,
    elements: [{ tag: 'circle', attrs: { cx: 10.8, cy: 10.8, r: 6.8 } }, path('m16 16 4 4', roundCaps)],
  },
  send: { strokeWidth: 1.7, elements: [path('m21 3-7.2 18-3.7-7.1L3 10.2 21 3Zm0 0L10.1 13.9', round)] },
  'sort-asc': { strokeWidth: 1.8, elements: [path('M12 19V5m0 0 6 6m-6-6-6 6', round)] },
  'sort-desc': { strokeWidth: 1.8, elements: [path('M12 5v14m0 0-6-6m6 6 6-6', round)] },
  star: { strokeWidth: 1.6, elements: [path('M12 3.5 14.6 9l5.9.9-4.3 4.2 1 5.9-5.2-2.8-5.2 2.8 1-5.9-4.3-4.2 5.9-.9L12 3.5Z', roundJoins)] },
  sun: {
    strokeWidth: 1.7,
    elements: [
      { tag: 'circle', attrs: { cx: 12, cy: 12, r: 4 } },
      path('M12 2v2m0 16v2M4.93 4.93l1.42 1.42m11.3 11.3 1.42 1.42M2 12h2m16 0h2M4.93 19.07l1.42-1.42m11.3-11.3 1.42-1.42', roundCaps),
    ],
  },
  trash: { strokeWidth: 1.7, elements: [path('M4 7h16M10 11v6m4-6v6M5.5 7l1 13h11l1-13M9 7V4h6v3', round)] },
} satisfies Record<string, IconDefinition>

export type IconName = keyof typeof icons
