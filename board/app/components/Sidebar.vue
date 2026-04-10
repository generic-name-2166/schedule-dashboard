<script setup lang="ts">
import { computed, useId, useTemplateRef, watch } from "vue";
import { useVirtualizer } from "@tanstack/vue-virtual";
import type { ScheduleNode } from "../stores/schedule.ts";
import SidebarStep from "./SidebarStep.vue";

const props = defineProps<{
  descendants: number[];
  filtered: ScheduleNode[];
  search?: string;
}>();

const scrollTop = defineModel<number>("scrollTop");
const visible = defineModel<boolean[]>("visible", { required: true });

const sidebarId = useId();
const sidebar = useTemplateRef<HTMLDivElement>("sidebar");

const virtualizer = useVirtualizer({
  count: props.filtered.length,
  getScrollElement: () => sidebar.value!,
  estimateSize: () => 40,
  overscan: 50,
});

const scroll = () => (scrollTop.value = sidebar.value!.scrollTop);
watch(
  scrollTop,
  (top: number | undefined) => (sidebar.value!.scrollTop = top ?? 0),
);

watch(
  () => props.filtered,
  (filtered): void => {
    virtualizer.value.setOptions({
      ...virtualizer.value.options,
      count: filtered.length,
    });
  },
);
</script>

<template>
  <div ref="sidebar" class="sidebar" @scroll.passive="scroll">
    <SidebarStep
      v-for="{ index, start } of virtualizer.getVirtualItems()"
      :key="filtered[index]!.index"
      v-model="filtered[index]!.open.value"
      v-model:visible="visible"
      :sidebar-id="sidebarId"
      :index="filtered[index]!.index"
      :name="filtered[index]!.name"
      :children="filtered[index]!.children"
      :depth="filtered[index]!.depth"
      :descendants="props.descendants"
      :start="start"
      :search="props.search"
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
