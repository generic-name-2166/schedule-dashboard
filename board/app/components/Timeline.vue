<script setup lang="ts">
import type { ScheduleNode } from "../stores/schedule.ts";

const TIMELINE_START: Date = new Date("2025-01-01");
const TIMELINE_END: Date = new Date("2030-01-01");
const TOTAL_DURATION: number = TIMELINE_END.valueOf() - TIMELINE_START.valueOf();

const props = defineProps<{
  nodes: ScheduleNode[];
}>();

function calculateOffset(startDate?: string): string {
  if (!startDate) {
    return "0";
  }
  const start = new Date(startDate);
  const offset: number = start.valueOf() - TIMELINE_START.valueOf();
  return ((offset / TOTAL_DURATION) * 100).toFixed(2) + "%";
}

function calculateWidth(startDate?: string, endDate?: string): string {
  if (!startDate || !endDate) {
    return "0";
  }
  const start = new Date(startDate);
  const end = new Date(endDate);
  const duration: number = end.valueOf() - start.valueOf();
  return ((duration / TOTAL_DURATION) * 100).toFixed(2) + "%";
}
</script>

<template>
  <div class="timeline">
    <ul class="nodes">
      <li v-for="node of props.nodes">
        <div
          style="position: absolute; outline: 2px solid red; height: stretch;"
          :style="{
            left: calculateOffset(node.start),
            width: calculateWidth(node.start, node.end),
          }"
        ></div>
      </li>
    </ul>
  </div>
</template>

<style lang="css" scoped>
.timeline {
  overflow-x: scroll;
}

.nodes {
  padding: 0;
  margin: 0;
  width: 2000px;
  list-style-type: none;
  display: grid;
  grid-template-columns: 1fr;

  > li {
    display: block;
    height: 60px;
    position: relative;
  }
}
</style>
