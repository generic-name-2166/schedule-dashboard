<script setup lang="ts">
import { computed, useTemplateRef, onMounted } from "vue";
import { useScheduleStore, type ScheduleNode } from "../stores/schedule.ts";
import TimelineBar from "./TimelineBar.vue";

const TIMELINE_START: Date = new Date("2021-01-01");
const TIMELINE_END: Date = new Date("2029-01-01");
const TOTAL_DURATION: number =
  TIMELINE_END.valueOf() - TIMELINE_START.valueOf();

const store = useScheduleStore();
const timeline = useTemplateRef<HTMLDivElement>("timeline");

const props = defineProps<{
  nodes: ScheduleNode[];
}>();

function calculateOffset(start?: Date): string {
  if (!start) {
    return "0";
  }
  const offset: number = start.valueOf() - TIMELINE_START.valueOf();
  return ((offset / TOTAL_DURATION) * 100).toPrecision(2) + "%";
}

function calculateWidth(start?: Date, end?: Date): string {
  if (!start || !end) {
    return "0";
  }
  const duration: number = end.valueOf() - start.valueOf();
  return ((duration / TOTAL_DURATION) * 100).toPrecision(2) + "%";
}

interface YearTick {
  year: number;
  offset: string;
}

const markers = computed<{
  major: YearTick[];
  /** offsets */
  minor: string[];
}>(() => {
  const startYear: number = TIMELINE_START.getFullYear();
  const endYear: number = TIMELINE_END.getFullYear();

  const major = new Array<YearTick>(endYear - startYear);
  // 11 потому что тик за январь ставиться большим как год
  const minor = new Array<string>((endYear - startYear) * 11);

  let idx = 0;
  for (let year = startYear; year < endYear; year++) {
    const yearDate = new Date(year, 0, 1);
    major[idx] = {
      year,
      offset: calculateOffset(yearDate),
    } satisfies YearTick;

    for (let month = 1; month < 12; month++) {
      const monthDate = new Date(year, month, 1);
      minor[idx * 11 + month] = calculateOffset(monthDate);
    }

    ++idx;
  }

  return {
    major,
    minor,
  };
});

onMounted(() => {
  if (!timeline.value) return;

  const todayOffset: number =
    (new Date().valueOf() - TIMELINE_START.valueOf()) / TOTAL_DURATION;
  const timelineWidth = 2000;
  const margin = 200;

  const scrollPosition = todayOffset * timelineWidth - margin;
  timeline.value.scrollLeft = Math.max(0, scrollPosition);
});
</script>

<template>
  <div ref="timeline" class="timeline">
    <div>
      <div
        v-once
        class="today-line"
        :style="{ left: calculateOffset(new Date()) }"
      ></div>

      <div v-once class="timeline-header">
        <div
          v-for="year of markers.major"
          :key="year.offset"
          class="marker-year"
          :style="{ left: year.offset }"
        >
          <span>{{ year.year }}</span>
          <div class="marker-line"></div>
        </div>
        <div
          v-for="month of markers.minor"
          :key="month"
          class="marker-month"
          :style="{ left: month }"
        >
          <div class="marker-line"></div>
        </div>
      </div>

      <ul class="nodes">
        <li
          v-for="node of props.nodes"
          :key="node.id"
          v-memo="[node.visible.value]"
        >
          <TimelineBar
            :visible="node.visible.value"
            :left="calculateOffset(node.start)"
            :width="calculateWidth(node.start, node.end)"
          />
        </li>
      </ul>
    </div>
  </div>
</template>

<style lang="css" scoped>
.timeline {
  overflow-x: scroll;
  grid-row: span 2;
  background-color: var(--secondary-background);
  border-radius: 1rem;
  padding-inline: 0.5rem;

  > div {
    width: 2000px;
    position: relative;

    > .today-line {
      width: 2px;
      position: absolute;
      top: 0;
      bottom: 0;
      height: auto;
      background-color: var(--secondary-color);
    }
  }
}

.timeline-header {
  height: 60px;
  width: stretch;
  border-bottom: 0.125rem solid var(--primary-color);
  position: relative;
}

.marker-year {
  position: absolute;
  top: 0;
  height: stretch;
  display: flex;
  flex-direction: column;

  > span {
    transform: translateX(-50%);
    margin-block: 10px;
  }

  > .marker-line {
    width: 2px;
    height: stretch;
    background-color: var(--secondary-color);
  }
}

.marker-month {
  position: absolute;
  top: 0;
  height: stretch;
  display: flex;
  align-items: end;

  > .marker-line {
    width: 1px;
    height: 30%;
    background-color: var(--secondary-color);
  }
}

.nodes {
  padding: 0;
  margin: 0;
  width: stretch;
  list-style-type: none;
  display: grid;
  grid-template-columns: 1fr;

  > li {
    display: contents;
  }
}
</style>
