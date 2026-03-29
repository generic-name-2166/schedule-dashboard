<script setup lang="ts">
import { useId } from "vue";
import type { ScheduleNode } from "../stores/schedule.ts";
import SidebarStep from "./SidebarStep.vue";

const props = defineProps<{
  nodes: ScheduleNode[];
  descendants: number[];
}>();

const sidebarId = useId();
</script>

<template>
  <div class="sidebar">
    <div style="height: 60px"></div>
    <SidebarStep
      v-for="(node, idx) of props.nodes"
      :key="node.id"
      v-model="props.nodes[idx]!"
      :sidebar-id="sidebarId"
      :array="props.nodes"
      :index="idx"
      :open="node.open.value"
      :visible="node.visible.value"
      :descendants="props.descendants"
    />
  </div>
</template>
