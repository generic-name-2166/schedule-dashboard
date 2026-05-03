<script setup lang="ts">
import DatePicker from "~/components/DatePicker.vue";
import DiffSidebar from "~/components/diff/DiffSidebar.vue";
import DiffTimeline from "~/components/diff/DiffTimeline.vue";
import { useDiffStore } from "~/stores/diff.ts";

const store = useDiffStore();
await store.init();
</script>

<template>
  <div>
    <h1>График строительства</h1>
  </div>
  <p v-if="!store.oldDate || !store.newDate" class="missing-data">
    Данные не предоставлены
  </p>
  <template v-else>
    <form class="diff-form" @submit.prevent="store.fetchDiff">
      <DatePicker
        :dates="store.dates"
        :current-date="store.oldDate"
        @change="(date) => (store.oldDate = date)"
      />
      <DatePicker
        :dates="store.dates"
        :current-date="store.newDate"
        @change="(date) => (store.newDate = date)"
      />
      <button type="submit">Сравнить</button>
    </form>
    <div v-if="store.nodes.length > 0" class="gantt-chart">
      <DiffSidebar v-model:scroll-top="store.scrollTop" :diffs="store.nodes" />
      <DiffTimeline v-model="store.scrollTop" :diffs="store.nodes" />
    </div>
    <p v-else class="missing-data">Нет данных для сравнения</p>
  </template>
</template>

<style lang="css" scoped>
.diff-form {
  display: flex;
  justify-content: space-evenly;
  margin-bottom: 1rem;

  > button {
    background-color: var(--secondary-background);
    color: var(--primary-color);
    border: 0;
    border-radius: 0.5rem;
    padding: 0.5rem;
  }
}

.gantt-chart {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 2fr;
  grid-template-rows: auto;
  column-gap: 1rem;
  align-items: start;
}

.missing-data {
  display: flex;
  justify-content: center;
}
</style>
