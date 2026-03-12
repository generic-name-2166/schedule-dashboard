<script setup lang="ts">
import { useScheduleStore, type ScheduleNode } from "../stores/schedule.ts";
import SidebarStep from "./SidebarStep.vue";

const store = useScheduleStore();

const model = defineModel<ScheduleNode>({
  required: true,
});

const props = defineProps<{
  array: ScheduleNode[];
}>();

const toggle = (event: Event): void => {
  const el = event.target as HTMLDetailsElement;
  if (el.open) {
    store.closed.delete(model.value.wbsCode);
  } else {
    store.closed.add(model.value.wbsCode);
  }
};
</script>

<template>
  <details :open="model.open.value" class="details" @toggle="toggle">
    <summary
      v-once
      :class="{ leaf: model.children.length === 0 }"
      class="summary"
    >
      <span>
        {{ model.name }}
      </span>
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
  width: stretch;
}

.children {
  padding-left: 1rem;
  border-left: 0.125rem solid var(--secondary-color);
}

.summary {
  list-style-type: none;
  cursor: pointer;
  transition: all 0.2s ease;
  font-weight: bold;
  display: block;
  white-space: nowrap;
  overflow-x: hidden;
  text-overflow: ellipsis;
  position: relative;
  border-bottom: 0.125rem solid var(--secondary-color);
  box-sizing: border-box;
  height: 40px;

  height: 40px;

  /* can't use flex because it breaks text-overflow, margin: auto and vertical-align aren't working */
  > span {
    position: absolute;
    top: 50%;
    left: 0;
    right: 0;
    transform: translateY(-50%);
    white-space: nowrap;
    overflow-x: hidden;
    text-overflow: ellipsis;
  }

  &.leaf {
    list-style: none;
    pointer-events: none;
  }
}
</style>
