<script setup lang="ts">
import { computed } from "vue";
import type { ScheduleNode } from "../stores/schedule.ts";

const props = defineProps<{
  node: ScheduleNode;
  array: ScheduleNode[];
  childrenMap: Map<string, number[]>;
}>();

const children = computed<ScheduleNode[]>(
  () =>
    props.childrenMap
      .get(props.node.wbsCode)
      ?.map((idx) => props.array[idx]!) ?? [],
);
</script>

<template>
  <details open class="details">
    <summary :class="{ leaf: children.length === 0 }" class="summary">
      {{ props.node.name }}
    </summary>
    <div class="children">
      <SidebarStep
        v-for="child of children"
        :node="child"
        :array="props.array"
        :childrenMap="props.childrenMap"
      />
    </div>
  </details>
</template>

<style scoped>
.details {
  border-left: 2px solid #e0e0e0;
  padding-left: 12px;

  /* Remove left border for leaf nodes */
  &:not(:has(.details)) {
    border-left: none;

    &::before {
      display: none;
    }
  }
}

.summary {
  list-style-type: none;
  position: relative;
  cursor: pointer;
  border-radius: 4px;
  transition: all 0.2s ease;

  height: 60px;

  &::before {
    content: "";
    position: absolute;
    left: -10px;
    top: 50%;
    width: 10px;
    height: 2px;
    background-color: #e0e0e0;
  }

  &:hover {
    background: #e9ecef;
    border-color: #adb5bd;
  }

  &.leaf {
    list-style: none;
    border-left: 3px solid #28a745;
    background: #f1f8f1;
    border-color: #c3e6cb;
  }
}
</style>
