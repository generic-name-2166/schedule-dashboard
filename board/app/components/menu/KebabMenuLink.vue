<script setup lang="ts">
import { RouterLink, type RouterLinkProps } from "vue-router";

const props = defineProps<{
  to: RouterLinkProps["to"];
}>();
</script>

<template>
  <li class="container">
    <RouterLink :to="props.to" class="link">
      <slot />
    </RouterLink>
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

.link {
  display: block;
  text-decoration: none;
  color: inherit;
  background-color: inherit;
  border: 0;
  margin: 0;
  padding: 0.5rem 1rem;
  font-size: inherit;
  font-weight: inherit;

  &:visited {
    color: inherit;
  }
}
</style>
