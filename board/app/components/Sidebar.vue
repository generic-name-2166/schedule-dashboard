<script setup lang="ts">
import { useId, useTemplateRef, watch } from "vue";
import { useVirtualizer } from "@tanstack/vue-virtual";
import type { ScheduleNode } from "../stores/schedule.ts";
import SidebarStep from "./SidebarStep.vue";

const props = defineProps<{
  nodes: ScheduleNode[];
  descendants: number[];
  search: boolean[];
}>();

const scrollTop = defineModel<number>("scrollTop");
const visible = defineModel<boolean[]>("visible", { required: true });

const sidebarId = useId();
const sidebar = useTemplateRef<HTMLDivElement>("sidebar");

const virtualizer = useVirtualizer({
  count: props.nodes.length,
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
  [visible, () => props.search],
  ([visible, search]): void => {
    for (let idx = 0; idx < visible.length; idx++) {
      const size = visible[idx] && search[idx] ? 40 : 0;
      virtualizer.value.resizeItem(idx, size);
    }
  },
  { deep: true, immediate: true },
);
</script>

<template>
  <div ref="sidebar" class="sidebar" @scroll.passive="scroll">
    <SidebarStep
      v-for="{ key, index, start } of virtualizer.getVirtualItems()"
      :key="key.toString()"
      v-model="props.nodes[index]!.open.value"
      v-model:visible="visible"
      :sidebar-id="sidebarId"
      :array="props.nodes"
      :index="index"
      :name="props.nodes[index]!.name"
      :children="props.nodes[index]!.children"
      :depth="props.nodes[index]!.depth"
      :descendants="props.descendants"
      :start="start"
      :show="visible[index]! && props.search[index]!"
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
