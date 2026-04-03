<script setup lang="ts">
import type { Virtualizer } from '@tanstack/vue-virtual';
import { watch } from 'vue';

const props = defineProps<{
  visible: boolean;
  days: number;
  left: string;
  width: {
    percentage: string;
    type: "small" | "big";
  };
  start: number;
  virtualizer: Virtualizer<HTMLDivElement, Element>;
  index: number;
}>();

const formatter = new Intl.NumberFormat("ru-RU", {
  style: "unit",
  unit: "day",
  unitDisplay: "long",
});

watch(() => props.visible, (visible) => {
  props.virtualizer.resizeItem(props.index, visible ? 40 : 0);
});
</script>

<template>
  <div class="bar-offset" :style="{ top: `${props.start}px` }">
    <div v-show="props.visible" class="bar-wrapper">
      <p
        v-if="props.days"
        class="bar"
        :style="{
          left: props.left,
          width: props.width.percentage,
        }"
      >
        <span>
          <span :class="{ 'bar-outside': props.width.type === 'small' }">
            {{ formatter.format(props.days) }}
          </span>
        </span>
      </p>
    </div>
  </div>
</template>

<style lang="css" scoped>
.bar-offset {
  position: absolute;
  left: 0;
  transform: translateY(60px);
  width: stretch;
}

.bar-wrapper {
  display: block;
  position: relative;
  height: 40px;
  box-sizing: border-box;
  border-bottom: 0.125rem solid var(--secondary-color);
}

.bar {
  height: 60%;
  position: absolute;
  top: 20%;
  background-color: var(--highlight-color);
  color: var(--primary-background);
  border-radius: 0.5rem;
  margin: 0;
  font-weight: bold;
  white-space: nowrap;

  > span {
    position: relative;
    width: stretch;
    height: stretch;
    display: flex;
    align-items: center;
    justify-content: center;

    > .bar-outside {
      position: absolute;
      top: 50%;
      left: calc(100% + 0.5rem);
      transform: translateY(-50%);
      color: var(--primary-color);
    }
  }
}
</style>
