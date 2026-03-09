<script setup lang="ts">
import type { ScheduleNode } from "../stores/schedule.ts";
import SidebarStep from "./SidebarStep.vue";

const model = defineModel<ScheduleNode>({
  required: true,
});

const props = defineProps<{
  array: ScheduleNode[];
}>();

const toggle = (event: Event): void => {
  const el = event.target as HTMLDetailsElement;
  model.value.open.value = el.open;
};
</script>

<template>
  <details :open="model.open.value" class="details" @toggle="toggle">
    <summary
      v-once
      :class="{ leaf: model.children.length === 0 }"
      class="summary"
    >
      {{ model.name }}
    </summary>
    <div class="children">
      <SidebarStep
        v-for="idx of model.children"
        :key="props.array[idx]!.id"
        v-model="props.array[idx]!"
        :array="props.array"
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
