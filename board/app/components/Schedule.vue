<script setup lang="ts">
import {
  collectTree,
  useScheduleStore,
  type ScheduleNode,
} from "../stores/schedule.ts";
import Sidebar from "./Sidebar.vue";

const store = useScheduleStore();

const nodes: ScheduleNode[] = await store.init();
const { roots, childrenMap } = collectTree(nodes);
</script>

<template>
  <div class="gantt-chart">
    <Sidebar :nodes="nodes" :roots="roots" :childrenMap="childrenMap" />

    <Timeline :nodes="nodes" />
  </div>
</template>

<style scoped>
.gantt-chart {
  display: grid;
  grid-template-columns: 1fr 2fr;
  gap: 1rem;
}
</style>
