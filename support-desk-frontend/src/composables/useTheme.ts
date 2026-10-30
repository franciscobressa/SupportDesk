import { ref, watch } from 'vue'

const storageKey = 'support-desk-theme'

// Storage can be unavailable (private mode, blocked site data); the theme then just isn't remembered.
function readStoredTheme(): boolean {
  try {
    return localStorage.getItem(storageKey) === 'dark'
  } catch {
    return false
  }
}

function storeTheme(isDark: boolean): void {
  try {
    localStorage.setItem(storageKey, isDark ? 'dark' : 'light')
  } catch {
    // Not persisting the preference is acceptable; the toggle still works for this visit.
  }
}

// Module-level state: every component that calls useTheme shares the same preference.
const isDarkMode = ref(readStoredTheme())

watch(
  isDarkMode,
  (isDark) => {
    // The `dark` class on <html> switches the CSS theme tokens defined in style.css.
    document.documentElement.classList.toggle('dark', isDark)
    storeTheme(isDark)
  },
  { immediate: true },
)

export function useTheme() {
  function toggleTheme(): void {
    isDarkMode.value = !isDarkMode.value
  }

  return { isDarkMode, toggleTheme }
}
