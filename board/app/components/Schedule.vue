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
const { roots, nodes, descendants } = collectTree(data);
</script>

<template>
  <div v-if="store.currentDate" class="gantt-chart">
    <Sidebar :nodes="nodes" :roots="roots" :descendants="descendants" />
    <Timeline :nodes="nodes" />
  </div>
  <p v-else>Данные не предоставлены</p>
</template>

<style scoped>
.gantt-chart {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 3fr;
  gap: 1rem;
}
</style>
