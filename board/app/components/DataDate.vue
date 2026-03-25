<script setup lang="ts">
import { useScheduleStore } from "~/stores/schedule.ts";

const store = useScheduleStore();

async function change(event: Event): Promise<void> {
  const element = event.target as HTMLSelectElement;
  const value: string = element.value;
  await store.changeDate(new Date(parseInt(value)));
}
</script>

<template>
  <p v-if="store.currentDate" class="data-date">
    Данные предоставлены на
    <select @change="change">
      <option v-for="date of store.dates" :key="date.valueOf()" :selected="date.valueOf() === store.currentDate.valueOf()" :value="date.valueOf()">
        <!-- <time :datetime="date.toISOString()">
          {{ date.toLocaleDateString("ru-RU") }}
        </time> -->
        {{ date.toLocaleDateString("ru-RU") }}
      </option>
    </select>
  </p>
</template>

<style lang="css" scoped>
.data-date {
  margin: 0;
  font-weight: bold;
  display: flex;
  align-items: center;
  gap: 0.5rem;

  > select {
    font-size: 1.25rem;
  }
}
</style>
