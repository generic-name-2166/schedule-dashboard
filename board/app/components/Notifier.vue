<script setup lang="ts">
import { useNotifier } from "~/stores/notifier";

const notifier = useNotifier();
</script>

<template>
  <ul class="notifier">
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
  </ul>
</template>

<style lang="css" scoped>
.notifier {
  position: fixed;
}
</style>
