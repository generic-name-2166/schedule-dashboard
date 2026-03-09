<script setup lang="ts">
import {
  collectTree,
  useScheduleStore,
  type ScheduleDTO,
} from "../stores/schedule.ts";
import Sidebar from "./Sidebar.vue";
import Timeline from "./Timeline.vue";

const store = useScheduleStore();

const data: ScheduleDTO[] = await store.init();
const { roots, nodes } = collectTree(data);
</script>

<template>
  <div class="gantt-chart">
    <Sidebar :nodes="nodes" :roots="roots" />

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
