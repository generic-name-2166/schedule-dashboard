<script setup lang="ts">
import { useScheduleStore } from "~/stores/schedule.ts";
import DatePicker from "./DatePicker.vue";

const store = useScheduleStore();

async function change(date: Date): Promise<void> {
  await store.changeDate(date);
}
</script>

<template>
  <p v-if="store.currentDate" class="data-date">
    Данные предоставлены на
    <DatePicker
      :dates="store.dates"
      :current-date="store.currentDate"
      @change="change"
    />
  </p>
</template>

<style lang="css" scoped>
.data-date {
  margin: 0;
  font-weight: bold;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
</style>
