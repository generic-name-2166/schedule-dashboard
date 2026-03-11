<script setup lang="ts">
import { computed } from "vue";
import { useScheduleStore, type ScheduleNode } from "../stores/schedule.ts";

const TIMELINE_START: Date = new Date("2025-01-01");
const TIMELINE_END: Date = new Date("2030-01-01");
const TOTAL_DURATION: number =
  TIMELINE_END.valueOf() - TIMELINE_START.valueOf();

const store = useScheduleStore();

const props = defineProps<{
  nodes: ScheduleNode[];
}>();

function calculateOffset(start?: Date): string {
  if (!start) {
    return "0";
  }
  const offset: number = start.valueOf() - TIMELINE_START.valueOf();
  return ((offset / TOTAL_DURATION) * 100).toFixed(2) + "%";
}

function calculateWidth(start?: Date, end?: Date): string {
  if (!start || !end) {
    return "0";
  }
  const duration: number = end.valueOf() - start.valueOf();
  return ((duration / TOTAL_DURATION) * 100).toFixed(2) + "%";
}

const nodes = computed<ScheduleNode[]>(() =>
  props.nodes.filter(
    (node): boolean =>
      !store.closed
        .keys()
        .some((wbs) => node.wbsCode.startsWith(wbs) && node.wbsCode !== wbs),
  ),
);
</script>

<template>
  <div class="timeline">
    <ul class="nodes">
      <li v-for="node of nodes" :key="node.id">
        <div
          style="position: absolute; outline: 2px solid red; height: stretch"
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
  grid-row: span 2;
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
    height: 40px;
    position: relative;
  }
}
</style>
