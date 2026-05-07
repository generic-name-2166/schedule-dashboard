<script setup lang="ts">
import type { ScheduleDiff } from "../stores/diff.ts";

const props = defineProps<{
  diffs: ScheduleDiff[];
}>();
</script>

<template>
  <div class="diff-sidebar">
    <div class="sidebar-header"></div>
    <div v-for="diff in props.diffs" :key="diff.id" class="diff-name">
      <span>{{ diff.name }}</span>
    </div>
  </div>
</template>

<style lang="css" scoped>
.diff-sidebar {
  height: 100%;
}

.sidebar-header {
  height: 60px;
  box-sizing: border-box;
  border-bottom: 0.125rem solid var(--primary-color);
}

.diff-name {
  height: 80px;
  display: flex;
  align-items: center;
  box-sizing: border-box;
  border-bottom: 0.125rem solid var(--secondary-color);
  padding: 0 0.5rem;

  > span {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
}
</style>
