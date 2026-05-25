<script setup lang="ts">
import { computed, useTemplateRef, onMounted, watch, ref } from "vue";
import type { ScheduleNode } from "../stores/schedule.ts";
import TimelineBar from "./TimelineBar.vue";
import { useVirtualizer } from "@tanstack/vue-virtual";

const TIMELINE_START: Date = new Date("2021-01-01");
const TIMELINE_END: Date = new Date("2029-01-01");
const TOTAL_DURATION: number =
  TIMELINE_END.valueOf() - TIMELINE_START.valueOf();
/** percentage of 2000px (width of the inner timeline) that fits days text */
const WIDTH_CUTOFF = 5;

const cursorPos = ref<{ x: number; y: number }>();

const onTimelineMouseMove = (e: MouseEvent): void => {
  const el = timeline.value!;
  const rect = el.getBoundingClientRect();
  const style = getComputedStyle(el);
  const paddingLeft = parseFloat(style.paddingLeft);
  const paddingTop = parseFloat(style.paddingTop);
  const pixelX = Math.min(
    Math.max(e.clientX - rect.left - paddingLeft + el.scrollLeft, 0),
    2000,
  );
  const pixelY = e.clientY - rect.top - paddingTop;
  const ratio = pixelX / 2000;
  const date = TIMELINE_START.valueOf() + ratio * TOTAL_DURATION;
  cursorPos.value = { x: date, y: pixelY };
};

const onTimelineMouseLeave = (): void => {
  cursorPos.value = undefined;
};

const timeline = useTemplateRef<HTMLDivElement>("timeline");

const props = defineProps<{
  filtered: ScheduleNode[];
}>();

const scrollTop = defineModel<number>();

const virtualizer = useVirtualizer({
  count: props.filtered.length,
  getScrollElement: () => timeline.value!,
  estimateSize: () => 40,
  overscan: 50,
});

const scroll = () => (scrollTop.value = timeline.value!.scrollTop);
watch(
  scrollTop,
  (top: number | undefined) => (timeline.value!.scrollTop = top ?? 0),
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
  if (!start || !end || start > end) {
    return { percentage: "0", type: "small" };
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

watch(
  () => props.filtered,
  (filtered): void => {
    virtualizer.value.setOptions({
      ...virtualizer.value.options,
      count: filtered.length,
    });
  },
);

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
  <div
    ref="timeline"
    class="timeline"
    @scroll.passive="scroll"
    @mousemove.passive="onTimelineMouseMove"
    @mouseleave.passive="onTimelineMouseLeave"
  >
    <div>
      <div
        class="today-line"
        :style="{
          left: calculateOffset(new Date()),
          height: `${virtualizer.getTotalSize() + 60}px`,
        }"
      ></div>
      <div
        v-if="cursorPos"
        :key="cursorPos.x"
        class="cursor-line"
        :style="{
          left: calculateOffset(new Date(cursorPos.x)),
          height: `${virtualizer.getTotalSize() + 60}px`,
        }"
      ></div>
      <div
        v-if="cursorPos"
        :key="cursorPos.x"
        class="cursor-label"
        :style="{
          left: calculateOffset(new Date(cursorPos.x)),
          top: cursorPos.y + 'px',
        }"
      >
        {{ new Date(cursorPos.x).toLocaleDateString("ru-RU") }}
      </div>

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
        <div class="today-label" :style="{ left: calculateOffset(new Date()) }">
          {{ new Date().toLocaleDateString("ru-RU") }}
        </div>
      </div>

      <ul class="nodes">
        <li
          v-for="{ key, index, start } of virtualizer.getVirtualItems()"
          :key="key.toString()"
        >
          <TimelineBar
            :days="
              calculateDays(
                props.filtered[index]!.start,
                props.filtered[index]!.end,
              )
            "
            :left="calculateOffset(props.filtered[index]!.start)"
            :width="
              calculateWidth(
                props.filtered[index]!.start,
                props.filtered[index]!.end,
              )
            "
            :start="start"
            :level="props.filtered[index]!.depth.length"
          />
        </li>
      </ul>
    </div>
  </div>
</template>

<style lang="css" scoped>
@keyframes cursor-fade {
  0% {
    opacity: 1;
  }
  60% {
    opacity: 0.8;
  }
  100% {
    opacity: 0;
  }
}

.cursor-line {
  width: 2px;
  position: absolute;
  top: 0;
  background-color: var(--primary-color);
  opacity: 0.7;
  pointer-events: none;
  animation: cursor-fade 10s ease forwards;
}

.cursor-label {
  position: absolute;
  transform: translate(-50%, -100%);
  background-color: var(--primary-color);
  color: var(--secondary-background);
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
  white-space: nowrap;
  line-height: normal;
  pointer-events: none;
  animation: cursor-fade 4s ease forwards;
}

.today-label {
  position: absolute;
  top: 0;
  transform: translateX(-50%);
  background-color: var(--secondary-color);
  color: var(--secondary-background);
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
  white-space: nowrap;
  line-height: normal;
  margin-block: 10px;
  z-index: 1;
}

.timeline {
  overflow-x: scroll;
  scrollbar-color: var(--primary-background) var(--secondary-background);
  scrollbar-gutter: stable;
  grid-row: span 2;
  background-color: var(--secondary-background);
  border-radius: 1rem;
  padding-inline: 0.5rem;
  height: 100%;

  > div {
    width: 2000px;
    position: relative;

    > .today-line {
      width: 2px;
      position: absolute;
      top: 0;
      height: 100%;
      background-color: var(--secondary-color);
    }
  }
}

.timeline-header {
  height: 60px;
  box-sizing: border-box;
  width: 100%;
  border-bottom: 0.125rem solid var(--primary-color);
  position: relative;
}

.marker-year {
  position: absolute;
  top: 0;
  height: 100%;
  display: flex;
  flex-direction: column;

  > span {
    transform: translateX(-50%);
    margin-block: 10px;
  }

  > .marker-line {
    width: 2px;
    height: 100%;
    background-color: var(--secondary-color);
  }
}

.marker-month {
  position: absolute;
  top: 0;
  height: 100%;
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
  width: 100%;
  list-style-type: none;
  display: grid;
  grid-template-columns: 1fr;

  > li {
    display: block;
    margin: 0;
    padding: 0;
    border: 0;
  }
}
</style>
