<script setup lang="ts">
import { useId } from "vue";
import type { ScheduleNode } from "../stores/schedule.ts";
import SidebarStep from "./SidebarStep.vue";

const model = defineModel<ScheduleNode>({
  required: true,
});

const props = defineProps<{
  array: ScheduleNode[];
  descendants: number[];
  /** index in the global immutable array given above  */
  index: number;
  /** anchor to put on the last child for the parent to reference */
  lastChildAnchor?: string;
}>();

const toggle = (event: Event): void => {
  const el = event.target as HTMLDetailsElement;
  const open: boolean = el.open;
  const descendantEndIdx: number = props.descendants[props.index]!;

  for (let idx = props.index + 1; idx < descendantEndIdx; idx++) {
    props.array[idx]!.visible.value = open;
  }
};

const parentAnchor = `--${useId()}`;
</script>

<template>
  <div v-if="model.children.length === 0" v-once class="summary">
    <p class="leaf" :title="model.name">
      <span>
        {{ model.name }}
      </span>
    </p>
  </div>

  <details v-else :open="model.open.value" class="details" @toggle="toggle">
    <summary v-once class="summary">
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
    </summary>
    <div
      v-memo="[model.children.length]"
      class="children"
      :style="{ 'anchor-name': props.lastChildAnchor }"
    >
      <div class="branch" :style="{ 'position-anchor': parentAnchor }"></div>

      <SidebarStep
        v-for="idx of model.children.slice(0, model.children.length - 1)"
        :key="props.array[idx]!.id"
        v-model="props.array[idx]!"
        :array="props.array"
        :descendants="props.descendants"
        :index="idx"
      />
      <SidebarStep
        v-model="props.array[model.children.at(-1)!]!"
        :array="props.array"
        :descendants="props.descendants"
        :index="model.children.at(-1)!"
        :last-child-anchor="parentAnchor"
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
  position: relative;

  > .branch {
    display: block;
    position: absolute;
    top: -21px;
    left: -0.5rem;
    box-sizing: border-box;
    border-left: 2px solid var(--secondary-color);
    height: calc(100% - anchor-size(height, 0px));
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
  position: relative;
  border-bottom: 0.125rem solid var(--secondary-color);
  box-sizing: border-box;

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
    margin: 0;
  }
}
</style>
