<script setup lang="ts">
import type { ScheduleNode } from "../stores/schedule.ts";
import SidebarStep from "./SidebarStep.vue";

const props = defineProps<{
  nodes: ScheduleNode[];
  roots: number[];
  descendants: number[];
}>();
</script>

<template>
  <div class="sidebar">
    <div style="height: 60px"></div>
    <SidebarStep
      v-for="idx of props.roots"
      :key="props.nodes[idx]!.id"
      v-model="props.nodes[idx]!"
      :array="props.nodes"
      :index="idx"
      :descendants="props.descendants"
    />
  </div>
</template>
