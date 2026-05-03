<script setup lang="ts">
const props = defineProps<{
  dates: Date[];
  currentDate: Date;
}>();
const emit = defineEmits<{
  change: [date: Date];
}>();

function change(event: Event): void {
  const element = event.target as HTMLSelectElement;
  const value: string = element.value;
  emit("change", new Date(parseInt(value)));
}
</script>

<template>
  <select class="date-picker" @change="change">
    <option
      v-for="date of props.dates"
      :key="date.valueOf()"
      :selected="date.valueOf() === props.currentDate.valueOf()"
      :value="date.valueOf()"
    >
      <!-- <time :datetime="date.toISOString()">
          {{ date.toLocaleDateString("ru-RU") }}
        </time> -->
      {{ date.toLocaleDateString("ru-RU") }}
    </option>
  </select>
</template>

<style lang="css" scoped>
.date-picker {
  font-size: 1.25rem;
}
</style>
