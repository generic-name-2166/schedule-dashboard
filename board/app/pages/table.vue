<script setup lang="ts">
import { useScheduleStore } from '~/stores/schedule';

const store = useScheduleStore();
await store.init();
</script>

<template>
  <div>
    <h1>Стройобъекты</h1>
  </div>
  <div class="table">
    <template v-for="node of store.treelike.nodes" :key="node.id">
      <p>
        <button type="button">
          {{ node.wbsCode }}
        </button>
      </p>
      <p>{{ node.name }}</p>
      <p>{{ node.start }}</p>
      <p>{{ node.end }}</p>
    </template>
  </div>
</template>

<style lang="css" scoped>
.table {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 4fr) repeat(2, minmax(0, 1fr));
}
</style>
