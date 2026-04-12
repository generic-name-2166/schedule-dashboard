<script setup lang="ts">
import { inject, useId } from "vue";

const selected = defineModel<boolean>();

const props = defineProps<{
  name: string;
}>();

const selectName: string | undefined = inject("tab-select");
const id = useId();
</script>

<template>
  <input
    :id="id"
    v-model="selected"
    class="tab-input"
    type="radio"
    :name="selectName"
  />
  <label :for="id" class="tab-option">
    {{ props.name }}
  </label>
</template>

<style lang="css" scoped>
.tab-input {
  display: none;

  &:checked + .tab-option {
    background-color: rgba(255, 255, 255, 0.75);
    color: var(--primary-background);
  }
}

.tab-option {
  flex-grow: 1;
  display: flex;
  justify-content: center;
  align-items: center;
  margin: 0;
  padding: 0.5rem;
  border-radius: 0.5rem;
  background-color: var(--secondary-background);
}
</style>
