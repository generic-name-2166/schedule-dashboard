<script setup lang="ts">
import { useScheduleStore } from "../stores/schedule.ts";
import Sidebar from "./Sidebar.vue";
import Timeline from "./Timeline.vue";

const store = useScheduleStore();

await store.init();
</script>

<template>
  <div
    v-if="store.currentDate"
    :key="store.currentDate.valueOf()"
    class="gantt-chart"
  >
    <Sidebar
      :nodes="store.treelike.nodes"
      :roots="store.treelike.roots"
      :descendants="store.treelike.descendants"
    />
    <Timeline :nodes="store.treelike.nodes" />
  </div>
  <p v-else class="missing-data">Данные не предоставлены</p>
</template>

<style scoped>
.gantt-chart {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 2fr;
  gap: 1rem;
  height: calc(100% - 4rem);
}

.missing-data {
  display: flex;
  justify-content: center;
}
</style>
