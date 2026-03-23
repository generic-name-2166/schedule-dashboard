<script setup lang="ts">
import { useNotifier } from "~/stores/notifier";

const notifier = useNotifier();
</script>

<template>
  <TransitionGroup tag="ul" name="notifier" class="notifier">
    <li
      v-for="[timestamp, { text, type }] of notifier.messages"
      :key="timestamp"
      :class="{
        'notifier-info': type === 'info',
        'notifier-error': type === 'error',
      }"
    >
      {{ text }}
    </li>
  </TransitionGroup>
</template>

<style lang="css" scoped>
.notifier {
  position: fixed;
  top: 20vh;
  right: 10vw;
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 1rem;

  > li {
    display: flex;
    align-items: center;
    background-color: var(--tertiary-background);
    margin: 0;
    padding: 1rem;
    width: 15vw;
    border-radius: 0.25rem;

    &.notifier-info {
      border-left: 0.25rem solid var(--secondary-color);
    }
    &.notifier-error {
      border-left: 0.25rem solid var(--error-color);
    }
  }
}

/* https://vuejs.org/guide/built-ins/transition-group.html */
.notifier-move, 
.notifier-enter-active,
.notifier-leave-active {
  transition: all 0.5s ease;
}

.notifier-enter-from,
.notifier-leave-to {
  opacity: 0;
  transform: translateX(30px);
}

.notifier-leave-active {
  position: absolute;
}
</style>
