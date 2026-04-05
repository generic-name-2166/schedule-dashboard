<script setup lang="ts">
import { useId, useTemplateRef, watch } from "vue";
import { useVirtualizer } from "@tanstack/vue-virtual";
import { useScheduleStore, type ScheduleNode } from "../stores/schedule.ts";
import SidebarStep from "./SidebarStep.vue";

const props = defineProps<{
  nodes: ScheduleNode[];
  descendants: number[];
}>();

const store = useScheduleStore();
const sidebarId = useId();
const sidebar = useTemplateRef<HTMLDivElement>("sidebar");

const rowVirtualizer = useVirtualizer({
  count: props.nodes.length,
  getScrollElement: () => sidebar.value!,
  estimateSize: () => 40,
  overscan: 50,
});

const scroll = () => (store.scrollTop = sidebar.value!.scrollTop);
watch(
  () => store.scrollTop,
  (top: number) => (sidebar.value!.scrollTop = top),
);
</script>

<template>
  <div ref="sidebar" class="sidebar" @scroll.passive="scroll">
    <!-- <div style="height: 60px"></div> -->
    <SidebarStep
      v-for="{ key, index, start } of rowVirtualizer.getVirtualItems()"
      :key="key.toString()"
      v-model="props.nodes[index]!"
      :sidebar-id="sidebarId"
      :array="props.nodes"
      :index="index"
      :virtualizer="rowVirtualizer"
      :open="props.nodes[index]!.open.value"
      :visible="props.nodes[index]!.visible.value"
      :descendants="props.descendants"
      :start="start"
    />
  </div>
</template>

<style lang="css" scoped>
.sidebar {
  height: 100%;
  position: relative;
  overflow-y: scroll;
  scrollbar-width: none;
  scrollbar-gutter: stable;
}
</style>
