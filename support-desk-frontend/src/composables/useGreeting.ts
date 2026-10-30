import { computed, onBeforeUnmount, onMounted, ref } from 'vue'

/** Time-of-day greeting that stays correct when the dashboard is left open across hours. */
export function useGreeting() {
  const currentHour = ref(new Date().getHours())
  let timer: number | undefined

  onMounted(() => {
    timer = window.setInterval(() => {
      currentHour.value = new Date().getHours()
    }, 60_000)
  })

  onBeforeUnmount(() => {
    window.clearInterval(timer)
  })

  return computed(() => {
    if (currentHour.value < 12) return 'Good morning'
    if (currentHour.value < 18) return 'Good afternoon'
    return 'Good evening'
  })
}
