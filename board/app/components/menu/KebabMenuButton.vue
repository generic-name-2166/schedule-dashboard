<script setup lang="ts">
import type { ButtonHTMLAttributes } from "vue";

const props = withDefaults(
  defineProps<{
    type?: ButtonHTMLAttributes["type"];
  }>(),
  { type: "button" },
);

const emit = defineEmits<{
  click: [event: MouseEvent];
}>();
</script>

<template>
  <li class="container">
    <button
      :type="props.type"
      class="button"
      @click="(event) => emit('click', event)"
    >
      <slot />
    </button>
  </li>
</template>

<style lang="css" scoped>
.container {
  display: block;
  margin: 0;
  padding: 0;
  width: stretch;
  height: stretch;

  &:not(:last-child) {
    border-bottom: 1px solid var(--secondary-color);
  }
}

.button {
  display: block;
  text-decoration: none;
  color: inherit;
  background-color: inherit;
  border: 0;
  margin: 0;
  padding: 0.5rem 1rem;
  text-align: start;
  width: stretch;
  height: stretch;
  font-size: inherit;
  font-weight: inherit;

  &:hover {
    background-color: var(--primary-background);
  }
}
</style>
