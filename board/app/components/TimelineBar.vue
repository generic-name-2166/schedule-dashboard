<script setup lang="ts">
const props = defineProps<{
  visible: boolean;
  left: string;
  width: string;
}>();
</script>

<template>
  <div v-show="props.visible" class="bar-wrapper">
    <div
      class="bar"
      :style="{
        left: props.left,
        width: props.width,
      }"
    ></div>
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
  border-radius: 0.5rem;
}
</style>
