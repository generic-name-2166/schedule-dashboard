<script setup lang="ts">
import {
  collectTree,
  useScheduleStore,
  type ScheduleNode,
} from "~/stores/schedule.ts";
import Sidebar from "./Sidebar.vue";

const store = useScheduleStore();

const nodes: ScheduleNode[] = await store.init();
const { roots, childrenMap } = collectTree(nodes);
</script>

<template>
  <div class="gantt-container">
    <Sidebar :nodes="nodes" :roots="roots" :childrenMap="childrenMap" />

    <!-- Timeline -->
    <!-- <svg
      class="timeline"
      :width="chartWidth"
      :height="visibleRows.length * rowHeight"
    >
      <g v-for="(node, index) of visibleRows" :key="'bar-' + node.id">
        <rect
          :x="getX(node.start)"
          :y="index * rowHeight + 5"
          :width="getWidth(node.start, node.end)"
          :height="rowHeight - 10"
          rx="4"
          class="bar"
        />
      </g>
    </svg> -->
  </div>
</template>

<style scoped>
</style>
