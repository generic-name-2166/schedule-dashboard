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
      <p :title="model.name">
        <span>
          {{ model.name }}
        </span>
      </p>

      <svg
        v-if="model.children.length !== 0"
        class="icon"
        width="24"
        height="24"
        viewBox="0 0 24 24"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <g id="SVGRepo_bgCarrier" stroke-width="0"></g>
        <g
          id="SVGRepo_tracerCarrier"
          stroke-linecap="round"
          stroke-linejoin="round"
        ></g>
        <g id="SVGRepo_iconCarrier">
          <path
            fill-rule="evenodd"
            clip-rule="evenodd"
            d="M16.5303 8.96967C16.8232 9.26256 16.8232 9.73744 16.5303 10.0303L12.5303 14.0303C12.2374 14.3232 11.7626 14.3232 11.4697 14.0303L7.46967 10.0303C7.17678 9.73744 7.17678 9.26256 7.46967 8.96967C7.76256 8.67678 8.23744 8.67678 8.53033 8.96967L12 12.4393L15.4697 8.96967C15.7626 8.67678 16.2374 8.67678 16.5303 8.96967Z"
            fill="#eee"
          ></path>
        </g>
      </svg>
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

  &[open] > .summary > .icon {
    rotate: 180deg;
  }
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
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: relative;
  border-bottom: 0.125rem solid var(--secondary-color);
  box-sizing: border-box;

  height: 40px;

  > p {
    white-space: nowrap;
    overflow-x: hidden;
    text-overflow: ellipsis;
    margin: 0;
  }

  > .icon {
    flex-shrink: 0;
    transition-duration: 200ms;
    transition-property: rotate;
  }

  &.leaf {
    list-style: none;
  }
}
</style>
