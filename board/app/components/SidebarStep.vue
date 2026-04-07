<script setup lang="ts">
import type { ScheduleNode } from "../stores/schedule.ts";

const model = defineModel<ScheduleNode>({
  required: true,
});
const visible = defineModel<boolean[]>("visible", { required: true });

const props = defineProps<{
  sidebarId: string;
  array: ScheduleNode[];
  descendants: number[];
  /** index in the global immutable array given above  */
  index: number;
  open: boolean;
  start: number;
  /** simply using visible[index] doesn't do deep reactive updates */
  show: boolean;
}>();

const toggle = (): void => {
  const open = !props.open;
  const descendantEndIdx: number = props.descendants[props.index]!;

  visible.value = visible.value.fill(open, props.index + 1, descendantEndIdx);
  model.value.open.value = open;
};
</script>

<template>
  <div
    v-if="model.children.length === 0"
    v-show="props.show"
    class="details"
    :style="{ top: `${props.start}px`  }"
  >
    <div
      v-for="(branch, idx) of model.depth"
      :key="`${branch}-${idx}`"
      class="branch"
      :class="{ 'branch-parent': branch }"
    ></div>

    <div class="summary">
      <p class="leaf" :title="model.name">
        <span>
          {{ model.name }}
        </span>
      </p>
    </div>
  </div>

  <div
    v-else
    v-show="props.show"
    :class="{ open: props.open }"
    class="details"
    :style="{ top: `${props.start}px` }"
    @click="toggle"
  >
    <div
      v-for="(branch, idx) of model.depth"
      :key="`${branch}-${idx}`"
      class="branch"
      :class="{ 'branch-parent': branch }"
    ></div>

    <div class="summary">
      <p :title="model.name">
        <span>
          {{ model.name }}
        </span>
      </p>

      <svg
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
    </div>
  </div>
</template>

<style scoped>
.details {
  width: stretch;
  position: absolute;
  left: 0;

  &.open > .summary > .icon {
    rotate: 180deg;
  }
}

.branch {
  width: 1rem;
  position: relative;
  height: 40px;
  float: left;

  &.branch-parent::before {
    content: "";
    position: absolute;
    top: -21px;
    left: -0.5rem;
    display: block;
    height: 40px;
    border-left: 2px solid var(--secondary-color);
  }
}

.summary {
  list-style-type: none;
  cursor: pointer;
  transition: all 0.2s ease;
  font-weight: bold;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 0.125rem solid var(--secondary-color);
  box-sizing: border-box;
  position: relative;

  height: 40px;

  &::before {
    content: "";
    display: block;
    position: absolute;
    top: 50%;
    left: -1.5rem;
    height: 0;
    border-top: 2px solid var(--secondary-color);
    width: 1rem;
  }

  > p {
    white-space: nowrap;
    text-overflow: ellipsis;
    overflow-x: hidden;
    margin: 0;
    text-align: start;
    flex-grow: 1;
  }

  > .icon {
    flex-shrink: 0;
    transition-duration: 200ms;
    transition-property: rotate;
  }

  &.leaf {
    list-style: none;
    margin: 0;
  }
}
</style>
