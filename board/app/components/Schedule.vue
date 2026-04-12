<script setup lang="ts">
import { useScheduleStore } from "../stores/schedule.ts";
import Search from "./Search.vue";
import Sidebar from "./Sidebar.vue";
import TabButton from "./tabs/TabButton.vue";
import TabSelect from "./tabs/TabSelect.vue";
import Timeline from "./Timeline.vue";

const store = useScheduleStore();

await store.init();
</script>

<template>
  <TabSelect>
    <TabButton name="Этап 1" />
    <TabButton name="Этап 2" />
    <TabButton name="Этап 3" />
    <TabButton name="Этап 4" />
    <TabButton name="Этап 5" />
    <TabButton name="Этап 6" />
    <TabButton name="Этап 7" />
  </TabSelect>
  <div
    v-if="store.currentDate"
    :key="store.currentDate.valueOf()"
    class="gantt-chart"
  >
    <Search />
    <Timeline v-model="store.scrollTop" :filtered="store.filtered" />
    <Sidebar
      v-model:scroll-top="store.scrollTop"
      v-model:visible="store.visible"
      :descendants="store.treelike.descendants"
      :filtered="store.filtered"
      :search="store.searchString"
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
  height: calc(100% - 4rem - 3rem);
}

.missing-data {
  display: flex;
  justify-content: center;
}
</style>
