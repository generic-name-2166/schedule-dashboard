<script setup lang="ts">
import { useId } from "vue";

const menuId = useId();
</script>

<template>
  <button
    type="button"
    class="kebab"
    :popovertarget="menuId"
    :style="{
      anchorName: `--${menuId}`,
    }"
  >
    <svg
      width="60px"
      height="60px"
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <circle cx="12" cy="7" r="1.5" fill="#FFF" />
      <circle cx="12" cy="12" r="1.5" fill="#FFF" />
      <circle cx="12" cy="17" r="1.5" fill="#FFF" />
    </svg>
  </button>
  <div
    :id="menuId"
    popover
    class="popover"
    :style="{
      positionAnchor: `--${menuId}`,
    }"
  >
    <menu class="menu">
      <slot />
    </menu>
  </div>
</template>

<style lang="css" scoped>
.kebab {
  border: 0;
  border-radius: 50%;
  margin: 0;
  padding: 0;
  background-color: transparent;
  cursor: pointer;
  aspect-ratio: 1 / 1;

  &:hover {
    background-color: var(--primary-background);
    filter: brightness(150%);
  }
}

.popover {
  inset: auto;
  position: absolute;
  position-area: bottom left;
  transform: translate(1rem, -2rem);
  padding: 0;
  margin: 0;
  border: 0;
  border-radius: 1rem;
  background: transparent;
}

.menu {
  list-style-type: none;
  font-weight: bold;
  color: var(--primary-color);
  background-color: var(--tertiary-background);
  margin: 0;
  padding: 0;
  border: 0;
  display: flex;
  flex-direction: column;
}
</style>
