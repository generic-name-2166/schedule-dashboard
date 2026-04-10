<script setup lang="ts">
import { provide, useId } from "vue";

const selectName = useId();
provide("tab-select", selectName);
</script>

<template>
  <div class="tab-select">
    <slot />
  </div>
</template>

<style lang="css" scoped>
.tab-select {
  display: flex;
  gap: 1rem;
  justify-content: space-between;
  box-sizing: border-box;
  height: 3rem;
  padding-block: 0.5rem;
}
</style>
