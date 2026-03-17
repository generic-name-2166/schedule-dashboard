<script setup lang="ts">
import { useScheduleStore } from "~/stores/schedule";

const store = useScheduleStore();
</script>

<template>
  <p v-if="store.currentDate" class="data-date">
    Данные предоставлены на
    <time :datetime="store.currentDate.toISOString()">
      {{ store.currentDate?.toLocaleDateString("ru-RU") }}
    </time>
  </p>
</template>

<style lang="css" scoped>
.data-date {
  margin: auto;
  font-weight: bold;
}
</style>
