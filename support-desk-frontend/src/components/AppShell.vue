<script setup lang="ts">
import { useTheme } from '../composables/useTheme'
import AppIcon from './icons/AppIcon.vue'

withDefaults(defineProps<{
  pageTitle: string
  breadcrumbs?: string[]
}>(), {
  breadcrumbs: () => ['Workspace', 'Tickets'],
})

const { isDarkMode, toggleTheme } = useTheme()
</script>

<template>
  <div class="min-h-screen bg-canvas text-fg-strong">
    <aside class="fixed inset-y-0 left-0 z-20 hidden w-64 flex-col border-r border-line/80 bg-surface lg:flex">
      <RouterLink to="/" class="flex h-19 items-center gap-3 border-b border-line-subtle px-7">
        <span class="grid size-9 place-items-center rounded-xl bg-accent text-white shadow-sm shadow-accent-soft">
          <AppIcon name="logo" class="size-5" />
        </span>
        <span class="text-[15px] font-bold tracking-tight text-fg-strong">Support<span class="text-accent-fg">Desk</span></span>
      </RouterLink>

      <div class="px-4 pt-7">
        <p class="px-3 text-[10px] font-bold uppercase tracking-[0.16em] text-fg-faint">Workspace</p>
        <RouterLink
          to="/"
          class="mt-3 flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium text-fg-muted transition hover:bg-surface-hover hover:text-fg-strong"
          active-class="bg-accent-soft! font-semibold! text-accent-fg-strong!"
        >
          <AppIcon name="dashboard" class="size-4.5" />
          Ticket dashboard
        </RouterLink>
      </div>

      <div class="mt-auto border-t border-line-subtle p-4">
        <div class="flex items-center gap-3 rounded-xl p-2">
          <div class="grid size-9 place-items-center rounded-full bg-amber-100 text-xs font-bold text-amber-800 dark:bg-amber-900 dark:text-amber-200">SM</div>
          <div class="min-w-0">
            <p class="truncate text-sm font-semibold text-fg">Support Manager</p>
            <p class="text-xs text-fg-subtle">Demo workspace</p>
          </div>
          <span class="ml-auto size-2 rounded-full bg-emerald-500" aria-label="Online" />
        </div>
      </div>
    </aside>

    <div class="lg:pl-64">
      <header class="sticky top-0 z-10 flex h-19 items-center justify-between border-b border-line/80 bg-surface/90 px-5 backdrop-blur-md sm:px-8 dark:bg-canvas/90">
        <div class="flex items-center gap-3">
          <span class="grid size-9 place-items-center rounded-xl bg-accent text-white lg:hidden">
            <AppIcon name="chat" class="size-5" />
          </span>
          <div>
            <p class="text-sm font-bold text-fg">{{ pageTitle }}</p>
            <p class="hidden text-xs text-fg-subtle sm:block">
              <span v-for="(breadcrumb, index) in breadcrumbs" :key="breadcrumb">
                <span v-if="index" class="mx-1 text-line-strong">/</span>{{ breadcrumb }}
              </span>
            </p>
          </div>
        </div>
        <div class="flex items-center gap-3">
          <button
            type="button"
            class="button-icon"
            :aria-label="isDarkMode ? 'Switch to light mode' : 'Switch to dark mode'"
            :aria-pressed="isDarkMode"
            :title="isDarkMode ? 'Switch to light mode' : 'Switch to dark mode'"
            @click="toggleTheme"
          >
            <AppIcon :name="isDarkMode ? 'sun' : 'moon'" class="size-4.5" />
          </button>
          <div class="grid size-9 place-items-center rounded-full bg-amber-100 text-xs font-bold text-amber-800 dark:bg-amber-900 dark:text-amber-200">SM</div>
        </div>
      </header>

      <slot />
    </div>
  </div>
</template>
