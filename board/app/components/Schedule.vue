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
    <Timeline
      v-model="store.scrollTop"
      :nodes="store.treelike.nodes"
      :visible="store.visible"
      :search="store.searchFiltered"
    />
    <Sidebar
      v-model:scroll-top="store.scrollTop"
      v-model:visible="store.visible"
      :nodes="store.treelike.nodes"
      :descendants="store.treelike.descendants"
      :search="store.searchFiltered"
    />
  </div>
  <p v-else class="missing-data">Данные не предоставлены</p>
</template>

<style scoped>
.gantt-chart {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 2fr;
  grid-template-rows: 60px auto;
  column-gap: 1rem;
  height: calc(100% - 4rem);
}

.missing-data {
  display: flex;
  justify-content: center;
}
</style>
