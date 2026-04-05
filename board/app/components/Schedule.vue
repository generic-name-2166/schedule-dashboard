<script setup lang="ts">
import { useScheduleStore } from "../stores/schedule.ts";
import Search from "./Search.vue";
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
    <Search />
    <Timeline :nodes="store.treelike.nodes" />
    <Sidebar
      :nodes="store.treelike.nodes"
      :roots="store.treelike.roots"
      :descendants="store.treelike.descendants"
    />
  </div>
  <p v-else class="missing-data">Данные не предоставлены</p>
</template>

<style scoped>
.gantt-chart {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 2fr;
  grid-template-rows: 60px auto;
  gap: 1rem;
  height: calc(100% - 4rem);
}

.missing-data {
  display: flex;
  justify-content: center;
}
</style>
