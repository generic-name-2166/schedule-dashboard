<script setup lang="ts">
import { ref } from 'vue';

const searchString = ref("");
</script>

<template>
  <div class="search">
    <input v-model="searchString" type="text" />
    
    <svg width="24" height="24" viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path d="M42 42L33.3 33.3M38 22C38 30.8366 30.8366 38 22 38C13.1634 38 6 30.8366 6 22C6 13.1634 13.1634 6 22 6C30.8366 6 38 13.1634 38 22Z" stroke="#eee" stroke-width="4" stroke-linecap="round" stroke-linejoin="round"/>
    </svg>
  </div>
</template>

<style lang="css" scoped>
.search {
  height: 60px;
  display: flex;
  align-items: center;
  position: relative;

  > input {
    width: stretch;
    height: 2rem;
    padding-inline: 0.5rem;
    font-size: 1.25rem;
    border: 0.125rem solid var(--tertiary-background);
    border-radius: 0.5rem;
    background-color: var(--secondary-background);
    color: var(--primary-color);
  }

  > svg {
    position: absolute;
    top: 50%;
    transform: translateY(-50%);
    right: 0.5rem;
  }
}
</style>
