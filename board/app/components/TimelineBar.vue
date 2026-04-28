<script setup lang="ts">
const props = defineProps<{
  days: number;
  left: string;
  width: {
    percentage: string;
    type: "small" | "big";
  };
  start: number;
  level: number;
}>();

const formatter = new Intl.NumberFormat("ru-RU", {
  style: "unit",
  unit: "day",
  unitDisplay: "long",
});
</script>

<template>
  <div class="bar-offset" :style="{ top: `${props.start}px` }">
    <div class="bar-wrapper" :style="{ '--level': `${props.level * 5}%` }">
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
  overflow-y: hidden;
}

.bar-wrapper {
  display: block;
  position: relative;
  height: 40px;
  box-sizing: border-box;
  border-bottom: 0.125rem solid var(--secondary-color);
  filter: brightness(calc(120% - var(--level, 0%)));
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
