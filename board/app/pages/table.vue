<script setup lang="ts">
import { useTemplateRef } from "vue";
import Sidebar from "~/components/Sidebar.vue";
import Timeline from "~/components/Timeline.vue";
import { useScheduleStore } from "~/stores/schedule";
import { useSitesStore } from "~/stores/sites.ts";

const scheduleStore = useScheduleStore();
await scheduleStore.init();
const store = useSitesStore();
store.init(scheduleStore.treelike.nodes);

const dialogs = useTemplateRef("dialog");
</script>

<template>
  <div>
    <h1>Стройобъекты</h1>
  </div>
  <div class="table">
    <p class="cell cell-header">WBS код</p>
    <p class="cell cell-header">Название</p>
    <p class="cell cell-header">Начало</p>
    <p class="cell cell-header">Окончание</p>

    <template v-for="(node, idx) of store.sites" :key="node.ksgId">
      <p class="cell">
        <button type="button" @click="() => dialogs?.[idx]?.showModal()">
          {{ node.wbsCode }}

          <svg
            width="36"
            height="36"
            viewBox="0 0 48 48"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M14 34L34 14M34 14H14M34 14V34"
              stroke="#ddd"
              stroke-width="3"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
          </svg>
        </button>
      </p>
      <p class="cell cell-name">{{ node.name }}</p>
      <p class="cell">{{ node.start?.toLocaleDateString("ru-RU") }}</p>
      <p class="cell">{{ node.end?.toLocaleDateString("ru-RU") }}</p>

      <dialog ref="dialog" class="dialog">
        <div>
          <Sidebar
            :nodes="scheduleStore.treelike.nodes"
            :descendants="scheduleStore.treelike.descendants"
            :roots="[node.ksgId - 1]"
          />
          <Timeline
            :nodes="
              scheduleStore.treelike.nodes.slice(
                node.ksgId - 1,
                scheduleStore.treelike.descendants[node.ksgId - 1],
              )
            "
          />
        </div>

        <button type="button" @click="() => dialogs?.[idx]?.close()">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M6.4 19L5 17.6L10.6 12L5 6.4L6.4 5L12 10.6L17.6 5L19 6.4L13.4 12L19 17.6L17.6 19L12 13.4L6.4 19Z" fill="#ddd"/>
          </svg>
        </button>
      </dialog>
    </template>
  </div>
</template>

<style lang="css" scoped>
.table {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 4fr) repeat(2, minmax(0, 1fr));
  gap: 1rem;
}

.cell {
  background-color: var(--secondary-background);
  border-radius: 0.5rem;
  padding: 1rem;
  margin: 0;
  display: flex;
  align-items: center;
  justify-content: center;

  &.cell-name {
    justify-content: start;
  }

  > button {
    display: flex;
    align-items: center;
    justify-content: space-around;
    margin: 0;
    padding: 0;
    width: stretch;
    height: stretch;
    border: 0;
    background-color: transparent;
    color: inherit;
    font-weight: inherit;
    font-size: inherit;
    cursor: pointer;
  }

  &.cell-header {
    font-weight: bold;
    display: flex;
    justify-content: center;
  }
}

.dialog {
  width: max(90%, 1200px);
  height: max(90%, 800px);
  padding: 0;
  background-color: var(--primary-background);
  color: var(--primary-color);
  border-radius: 2rem;
  border: 0.25rem solid var(--secondary-background);
  overflow-y: hidden;
  position: relative;

  > div {
    width: stretch;
    height: stretch;
    overflow-y: scroll;
    scrollbar-color: var(--primary-background) var(--secondary-background);
    padding: 2rem;
    display: grid;
    grid-template-columns: minmax(0, 1fr) 2fr;
    gap: 1rem;
  }

  > button {
    position: absolute;
    top: 0.5rem;
    right: 1.25rem;
    background-color: transparent;
    border: none;
    border-radius: 0.25rem;
    margin: 0;
    padding: 0;
    display: block;
    height: 24px;

    &:hover {
      background-color: var(--secondary-background);
    }
  }
}
</style>
