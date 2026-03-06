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
/* Tree structure styling */
details {
  position: relative;
  margin-left: 20px;
  border-left: 2px solid #e0e0e0;
  padding-left: 12px;
  margin-bottom: 8px;
}

/* Connecting line from parent to children */
details::before {
  content: '';
  position: absolute;
  left: -2px;
  top: 22px; /* Position at summary height */
  width: 10px;
  height: 2px;
  background-color: #e0e0e0;
}

/* Remove bottom line for last child */
details:last-child::before {
  height: 50%;
}

/* Summary styling */
summary {
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 4px;
  background: #f8f9fa;
  border: 1px solid #dee2e6;
  transition: all 0.2s ease;
}

summary:hover {
  background: #e9ecef;
  border-color: #adb5bd;
}

/* Leaf nodes (no children) styling */
summary.leaf {
  list-style: none;
  border-left: 3px solid #28a745;
  background: #f1f8f1;
  border-color: #c3e6cb;
}

/* Children container */
.children {
  margin-top: 4px;
}

/* Remove left border for leaf nodes */
details:not(:has(details)) {
  border-left: none;
}

details:not(:has(details))::before {
  display: none;
}

/* Responsive design */
@media (max-width: 768px) {
  details {
    margin-left: 16px;
    padding-left: 8px;
  }
}
</style>
