<script setup lang="ts">
const props = defineProps<{
  visible: boolean;
  days: number;
  left: string;
  width: string;
}>();

const formatter = new Intl.NumberFormat("ru-RU", {
  style: "unit",
  unit: "day",
  unitDisplay: "long",
});
</script>

<template>
  <div v-show="props.visible" class="bar-wrapper">
    <p
      v-if="props.days"
      class="bar"
      :style="{
        left: props.left,
        width: props.width,
      }"
    >
      {{ formatter.format(props.days) }}
    </p>
  </div>
</template>

<style lang="css" scoped>
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
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  white-space: nowrap;
}
</style>
