<script setup lang="ts">
import { computed, ref, watch } from "vue";
import type { ScheduleNode } from "~/stores/schedule.ts";
import type { ConstructionSite } from "~/stores/sites.ts";

const props = defineProps<{
  root: ConstructionSite;
  /** the entire array */
  nodes: ScheduleNode[];
  descendants: number[];
  /** always true, reuse the same array */
  search: boolean[];
}>();
const emit = defineEmits<{
  close: [];
}>();

const scrollTop = ref(0);
const visible = ref(new Array<boolean>(props.nodes.length).fill(true));
watch(
  () => props.nodes,
  (nodes) => (visible.value = new Array<boolean>(nodes.length).fill(true)),
);
const modalNodes = computed(() =>
  props.nodes.slice(props.root.index, props.descendants[props.root.index]),
);
const modalDescendants = computed(() =>
  props.descendants.map((idx) => idx - props.root.index),
);
</script>

<template>
  <div class="sites-modal">
    <div style="height: 60px"></div>
    <Timeline
      v-model="scrollTop"
      :nodes="modalNodes"
      :visible="visible"
      :search="props.search"
    />
    <Sidebar
      v-model:scroll-top="scrollTop"
      v-model:visible="visible"
      :nodes="modalNodes"
      :descendants="modalDescendants"
      :search="props.search"
    />
  </div>

  <button class="modal-close" type="button" @click="emit('close')">
    <svg
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        d="M6.4 19L5 17.6L10.6 12L5 6.4L6.4 5L12 10.6L17.6 5L19 6.4L13.4 12L19 17.6L17.6 19L12 13.4L6.4 19Z"
        fill="#ddd"
      />
    </svg>
  </button>
</template>

<style lang="css" scoped>
.sites-modal {
  width: stretch;
  height: stretch;
  padding: 2rem;
  display: grid;
  grid-template-columns: minmax(0, 1fr) 2fr;
  grid-template-rows: 60px auto;
  column-gap: 1rem;
}
.modal-close {
  position: absolute;
  top: 0.5rem;
  right: 1.25rem;
  background-color: transparent;
  border: none;
  border-radius: 0.25rem;
  margin: 0;
  padding: 0;
  display: block;
  height: 24px;

  &:hover {
    background-color: var(--secondary-background);
  }
}
</style>
