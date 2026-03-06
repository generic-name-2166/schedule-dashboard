<script setup lang="ts">
import { computed } from "vue";

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
  <details open>
    <summary :class="{ leaf: children.length === 0 }">
      {{ props.node.name }}
    </summary>
    <div>
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
.leaf {
  list-style: none;
}
</style>
