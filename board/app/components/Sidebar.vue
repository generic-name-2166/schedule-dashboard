<script setup lang="ts">
import type { ScheduleNode } from "~/stores/schedule.ts";
import SidebarStep from "./SidebarStep.vue";

const props = defineProps<{
  nodes: ScheduleNode[];
  roots: number[];
  childrenMap: Map<string, number[]>;
}>();

const top: ScheduleNode[] = props.roots.map((idx) => props.nodes[idx]!);
</script>

<template>
  <div>
    <SidebarStep
      v-for="node of top"
      :node="node"
      :array="props.nodes"
      :childrenMap="props.childrenMap"
    />
  </div>
</template>
