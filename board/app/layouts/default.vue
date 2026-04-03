<script setup lang="ts">
import Notifier from "~/components/Notifier.vue";
</script>

<template>
  <div class="wrapper">
    <div class="content">
      <slot />
    </div>
  </div>
  <Notifier />
</template>

<style lang="css" scoped>
.wrapper {
  display: flex;
  justify-content: center;
  width: stretch;
  height: 100vh;
}

.content {
  padding-inline: 0.5rem;
  width: max(80%, 1200px);

  @media screen and (max-width: 1200px) {
    width: stretch;
  }
}
</style>
