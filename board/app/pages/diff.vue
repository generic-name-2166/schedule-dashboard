<script setup lang="ts">
import DatePicker from "~/components/DatePicker.vue";
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
    <div class="diff">
      <div v-for="diff in store.nodes" :key="diff.id" class="diff-row">
        <p>{{ diff.name }}</p>
        <div>
          <span>{{ diff.oldStart.toLocaleDateString("ru-RU") }}</span>
          <span>{{ diff.oldEnd.toLocaleDateString("ru-RU") }}</span>
          <span
            :class="{
              earlier: diff.newStart < diff.oldStart,
              later: diff.newStart > diff.oldStart,
            }"
            >{{ diff.newStart.toLocaleDateString("ru-RU") }}</span
          >
          <span
            :class="{
              earlier: diff.newEnd < diff.oldEnd,
              later: diff.newEnd > diff.oldEnd,
            }"
            >{{ diff.newEnd.toLocaleDateString("ru-RU") }}</span
          >
        </div>
      </div>
    </div>
  </template>
</template>

<style lang="css" scoped>
.diff-form {
  display: flex;
  justify-content: space-evenly;

  > button {
    background-color: var(--secondary-background);
    color: var(--primary-color);
    border: 0;
    border-radius: 0.5rem;
    padding: 0.5rem;
  }
}

.diff {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.diff-row {
  display: flex;
  justify-content: space-between;
  gap: 1rem;

  > div {
    flex-grow: 1;
    display: grid;
    grid-template-columns: auto auto;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
  }
}

.earlier {
  color: var(--highlight-color);
}
.later {
  color: var(--error-color);
}
</style>
