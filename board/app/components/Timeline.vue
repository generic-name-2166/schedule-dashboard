<script setup lang="ts">
import { computed, useTemplateRef, onMounted, watch } from "vue";
import { useScheduleStore, type ScheduleNode } from "../stores/schedule.ts";
import TimelineBar from "./TimelineBar.vue";
import { useVirtualizer } from "@tanstack/vue-virtual";

const TIMELINE_START: Date = new Date("2021-01-01");
const TIMELINE_END: Date = new Date("2029-01-01");
const TOTAL_DURATION: number =
  TIMELINE_END.valueOf() - TIMELINE_START.valueOf();
/** percentage of 2000px (width of the inner timeline) that fits days text */
const WIDTH_CUTOFF = 5;

const timeline = useTemplateRef<HTMLDivElement>("timeline");

const props = defineProps<{
  nodes: ScheduleNode[];
}>();

const store = useScheduleStore();

const rowVirtualizer = useVirtualizer({
  count: props.nodes.length,
  getScrollElement: () => timeline.value!,
  estimateSize: () => 40,
  overscan: 50,
});

const scroll = () => (store.scrollTop = timeline.value!.scrollTop);
watch(
  () => store.scrollTop,
  (top: number) => (timeline.value!.scrollTop = top),
);

function calculateDays(start?: Date, end?: Date): number {
  if (!start || !end) {
    return 0;
  }
  const msDiff: number = end.valueOf() - start.valueOf();
  return msDiff / (1000 * 60 * 60 * 24);
}

function calculateOffset(start?: Date): string {
  if (!start) {
    return "0";
  }
  const offset: number = start.valueOf() - TIMELINE_START.valueOf();
  return ((offset / TOTAL_DURATION) * 100).toFixed(2) + "%";
}

function calculateWidth(
  start?: Date,
  end?: Date,
): { percentage: string; type: "big" | "small" } {
  if (!start || !end) {
    return { percentage: "0", type: "big" };
  }
  const duration: number = end.valueOf() - start.valueOf();
  const percentage = (duration / TOTAL_DURATION) * 100;
  return {
    percentage: percentage.toFixed(2) + "%",
    type: percentage > WIDTH_CUTOFF ? "big" : "small",
  };
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
  <div ref="timeline" class="timeline" @scroll.passive="scroll">
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
          v-for="{ key, index, start } of rowVirtualizer.getVirtualItems()"
          :key="key.toString()"
        >
          <TimelineBar
            :visible="props.nodes[index]!.visible.value"
            :days="calculateDays(props.nodes[index]!.start, props.nodes[index]!.end)"
            :left="calculateOffset(props.nodes[index]!.start)"
            :width="calculateWidth(props.nodes[index]!.start, props.nodes[index]!.end)"
            :start="start"
            :virtualizer="rowVirtualizer"
            :index="index"
          />
        </li>
      </ul>
    </div>
  </div>
</template>

<style lang="css" scoped>
.timeline {
  overflow-x: scroll;
  scrollbar-color: var(--primary-background) var(--secondary-background);
  grid-row: span 2;
  background-color: var(--secondary-background);
  border-radius: 1rem;
  padding-inline: 0.5rem;
  height: 300px;

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
  box-sizing: border-box;
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
