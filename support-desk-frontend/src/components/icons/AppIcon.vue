<script setup lang="ts">
import { computed } from 'vue'
import { icons, type IconName } from './icons'

const props = defineProps<{
  name: IconName
}>()

const icon = computed(() => icons[props.name])
</script>

<template>
  <!-- Decorative: the surrounding control provides the accessible name. Size comes from the class. -->
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" :stroke-width="icon.strokeWidth" aria-hidden="true">
    <component :is="element.tag" v-for="(element, index) in icon.elements" :key="index" v-bind="element.attrs" />
  </svg>
</template>
