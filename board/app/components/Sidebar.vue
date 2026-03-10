<script setup lang="ts">
import type { ScheduleNode } from "../stores/schedule.ts";
import SidebarStep from "./SidebarStep.vue";

const props = defineProps<{
  nodes: ScheduleNode[];
  roots: number[];
}>();

const top: ScheduleNode[] = props.roots.map((idx) => props.nodes[idx]!);
</script>

<template>
  <div class="sidebar">
    <SidebarStep
      v-for="(node, idx) of top"
      :key="node.id"
      v-model="props.nodes[idx]!"
      :array="props.nodes"
    />
  </div>
</template>
