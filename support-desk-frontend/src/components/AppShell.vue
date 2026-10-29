<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'

withDefaults(defineProps<{
  pageTitle: string
  breadcrumbs?: string[]
}>(), {
  breadcrumbs: () => ['Workspace', 'Tickets'],
})

const isDarkMode = ref(localStorage.getItem('support-desk-theme') === 'dark')

function toggleTheme(): void {
  isDarkMode.value = !isDarkMode.value
}

function applyTheme(isDark: boolean): void {
  document.documentElement.classList.toggle('dark', isDark)
  localStorage.setItem('support-desk-theme', isDark ? 'dark' : 'light')
}

watch(isDarkMode, applyTheme, { flush: 'sync' })

onMounted(() => {
  document.documentElement.classList.toggle('dark', isDarkMode.value)
})
</script>

<template>
  <div class="min-h-screen bg-[#f7f8fc] text-slate-900">
    <aside class="fixed inset-y-0 left-0 z-20 hidden w-64 flex-col border-r border-slate-200/80 bg-white lg:flex">
      <RouterLink to="/" class="flex h-[76px] items-center gap-3 border-b border-slate-100 px-7">
        <span class="grid size-9 place-items-center rounded-xl bg-indigo-600 text-white shadow-sm shadow-indigo-200">
          <svg viewBox="0 0 24 24" fill="none" class="size-5" aria-hidden="true">
            <path d="M4 5.75A1.75 1.75 0 0 1 5.75 4h12.5A1.75 1.75 0 0 1 20 5.75v8.5A1.75 1.75 0 0 1 18.25 16H13l-4.5 4v-4h-2.75A1.75 1.75 0 0 1 4 14.25v-8.5Z" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round" />
            <path d="M8 9h8M8 12h5" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" />
          </svg>
        </span>
        <span class="text-[15px] font-bold tracking-tight text-slate-900">Support<span class="text-indigo-600">Desk</span></span>
      </RouterLink>

      <div class="px-4 pt-7">
        <p class="px-3 text-[10px] font-bold uppercase tracking-[0.16em] text-slate-400">Workspace</p>
        <RouterLink
          to="/"
          class="mt-3 flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium text-slate-600 transition hover:bg-slate-50 hover:text-slate-900"
          active-class="!bg-indigo-50 !font-semibold !text-indigo-700"
        >
          <svg viewBox="0 0 24 24" fill="none" class="size-[18px]" aria-hidden="true">
            <rect x="4" y="4" width="6.5" height="6.5" rx="1.5" stroke="currentColor" stroke-width="1.7" />
            <rect x="13.5" y="4" width="6.5" height="4" rx="1.5" stroke="currentColor" stroke-width="1.7" />
            <rect x="13.5" y="10.5" width="6.5" height="9.5" rx="1.5" stroke="currentColor" stroke-width="1.7" />
            <rect x="4" y="13.5" width="6.5" height="6.5" rx="1.5" stroke="currentColor" stroke-width="1.7" />
          </svg>
          Ticket dashboard
        </RouterLink>
      </div>

      <div class="mt-auto border-t border-slate-100 p-4">
        <div class="flex items-center gap-3 rounded-xl p-2">
          <div class="grid size-9 place-items-center rounded-full bg-amber-100 text-xs font-bold text-amber-800">SM</div>
          <div class="min-w-0">
            <p class="truncate text-sm font-semibold text-slate-800">Support Manager</p>
            <p class="text-xs text-slate-500">Demo workspace</p>
          </div>
          <span class="ml-auto size-2 rounded-full bg-emerald-500" aria-label="Online" />
        </div>
      </div>
    </aside>

    <div class="lg:pl-64">
      <header class="sticky top-0 z-10 flex h-[76px] items-center justify-between border-b border-slate-200/80 bg-white/90 px-5 backdrop-blur-md sm:px-8">
        <div class="flex items-center gap-3">
          <span class="grid size-9 place-items-center rounded-xl bg-indigo-600 text-white lg:hidden">
            <svg viewBox="0 0 24 24" fill="none" class="size-5" aria-hidden="true">
              <path d="M4 5.75A1.75 1.75 0 0 1 5.75 4h12.5A1.75 1.75 0 0 1 20 5.75v8.5A1.75 1.75 0 0 1 18.25 16H13l-4.5 4v-4h-2.75A1.75 1.75 0 0 1 4 14.25v-8.5Z" stroke="currentColor" stroke-width="1.7" stroke-linejoin="round" />
            </svg>
          </span>
          <div>
            <p class="text-sm font-bold text-slate-800">{{ pageTitle }}</p>
            <p class="hidden text-xs text-slate-500 sm:block">
              <span v-for="(breadcrumb, index) in breadcrumbs" :key="breadcrumb">
                <span v-if="index" class="mx-1 text-slate-300">/</span>{{ breadcrumb }}
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
            <svg v-if="isDarkMode" viewBox="0 0 24 24" fill="none" class="size-[18px]" aria-hidden="true">
              <circle cx="12" cy="12" r="4" stroke="currentColor" stroke-width="1.7" />
              <path d="M12 2v2m0 16v2M4.93 4.93l1.42 1.42m11.3 11.3 1.42 1.42M2 12h2m16 0h2M4.93 19.07l1.42-1.42m11.3-11.3 1.42-1.42" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" />
            </svg>
            <svg v-else viewBox="0 0 24 24" fill="none" class="size-[18px]" aria-hidden="true">
              <path d="M20.2 15.4A8.5 8.5 0 0 1 8.6 3.8 8.5 8.5 0 1 0 20.2 15.4Z" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" />
            </svg>
          </button>
          <div class="grid size-9 place-items-center rounded-full bg-amber-100 text-xs font-bold text-amber-800">SM</div>
        </div>
      </header>

      <slot />
    </div>
  </div>
</template>
