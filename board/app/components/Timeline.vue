<script setup lang="ts">
import { computed, useTemplateRef, onMounted, watch, ref } from "vue";
import type { ScheduleNode } from "../stores/schedule.ts";
import TimelineBar from "./TimelineBar.vue";
import { useVirtualizer } from "@tanstack/vue-virtual";

const TIMELINE_START: Date = new Date("2021-01-01");
const TIMELINE_END: Date = new Date("2029-01-01");
const TOTAL_DURATION: number =
  TIMELINE_END.valueOf() - TIMELINE_START.valueOf();
const BASE_WIDTH = 2000;

const MIN_ZOOM = 0.3;
const MAX_ZOOM = 12;

const zoom = ref(1);
const timelineWidth = computed(() => BASE_WIDTH * zoom.value);

const cursorPos = ref<{ x: number; y: number }>();

const onTimelineMouseMove = (e: MouseEvent): void => {
  const el = timeline.value!;
  const rect = el.getBoundingClientRect();
  const style = getComputedStyle(el);
  const paddingLeft = parseFloat(style.paddingLeft);
  const paddingTop = parseFloat(style.paddingTop);
  const pixelX = Math.min(
    Math.max(e.clientX - rect.left - paddingLeft + el.scrollLeft, 0),
    timelineWidth.value,
  );
  const pixelY = e.clientY - rect.top - paddingTop;
  const ratio = pixelX / timelineWidth.value;
  const date = TIMELINE_START.valueOf() + ratio * TOTAL_DURATION;
  cursorPos.value = { x: date, y: pixelY };
};

const onTimelineMouseLeave = (): void => {
  cursorPos.value = undefined;
};

const onTimelineWheel = (e: WheelEvent): void => {
  if (!e.ctrlKey && !e.metaKey) return;
  e.preventDefault();

  const el = timeline.value!;
  const rect = el.getBoundingClientRect();
  const style = getComputedStyle(el);
  const paddingLeft = parseFloat(style.paddingLeft);

  const oldZoom = zoom.value;
  const oldWidth = timelineWidth.value;
  const zoomDelta = e.deltaY < 0 ? 1.12 : 1 / 1.12;
  const newZoom = Math.min(MAX_ZOOM, Math.max(MIN_ZOOM, oldZoom * zoomDelta));
  if (newZoom === oldZoom) return;

  // Zoom towards cursor: keep the point under the cursor stationary
  const viewportCursorX = e.clientX - rect.left - paddingLeft;
  const cursorWorldX = el.scrollLeft + viewportCursorX;
  const ratio = cursorWorldX / oldWidth;
  zoom.value = newZoom;

  // After zoom, the point at ratio * newWidth should be at the same viewport position
  requestAnimationFrame(() => {
    el.scrollLeft = ratio * timelineWidth.value - viewportCursorX;
  });
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

function calculateWidth(start?: Date, end?: Date): string {
  if (!start || !end || start > end) {
    return "0";
  }
  const duration: number = end.valueOf() - start.valueOf();
  const percentage = (duration / TOTAL_DURATION) * 100;
  return percentage.toFixed(2) + "%";
}

// canvas для мерки "X дней" в TimelineBar
const sharedCanvasCtx = (() => {
  // eslint-disable-next-line @typescript-eslint/no-unnecessary-condition
  const canvas = document?.createElement("canvas");
  // eslint-disable-next-line @typescript-eslint/no-unnecessary-condition
  if (!canvas) {
    // SSR
    return null;
  }
  const ctx = canvas.getContext("2d")!;
  ctx.font = "bold 1rem serif";
  return ctx;
})();

interface YearTick {
  year: number;
  offset: string;
}

interface MonthLabelTick {
  label: string;
  offset: string;
}

interface WeekTick {
  offset: string;
}

const markers = computed<{
  yearTicks: YearTick[];
  monthLineTicks: string[];
  monthLabelTicks: MonthLabelTick[];
  weekTicks: WeekTick[];
}>(() => {
  const startYear: number = TIMELINE_START.getFullYear();
  const endYear: number = TIMELINE_END.getFullYear();
  const yearCount = endYear - startYear;

  const yearTicks = new Array<YearTick>(yearCount);
  const monthLabelTicks = new Array<MonthLabelTick>(yearCount * 12);
  // 11 month-line ticks per year (jan is covered by year tick)
  const monthLineTicks = new Array<string>(yearCount * 11);
  const weekTicks: WeekTick[] = [];

  const MONTH_NAMES = [
    "Янв",
    "Фев",
    "Мар",
    "Апр",
    "Май",
    "Июн",
    "Июл",
    "Авг",
    "Сен",
    "Окт",
    "Ноя",
    "Дек",
  ];

  let idx = 0;
  for (let year = startYear; year < endYear; year++) {
    const yearDate = new Date(year, 0, 1);
    yearTicks[idx] = {
      year,
      offset: calculateOffset(yearDate),
    } satisfies YearTick;

    // Month label ticks (all 12 months)
    for (let month = 0; month < 12; month++) {
      const monthDate = new Date(year, month, 1);
      const offset = calculateOffset(monthDate);
      monthLabelTicks[idx * 12 + month] = {
        label: month === 0 ? "" : MONTH_NAMES[month]!,
        offset,
      } satisfies MonthLabelTick;
    }

    // Month line ticks (feb-dec, jan covered by year)
    for (let month = 1; month < 12; month++) {
      const monthDate = new Date(year, month, 1);
      monthLineTicks[idx * 11 + month - 1] = calculateOffset(monthDate);
    }

    // Week ticks (start each Monday)
    const d = new Date(year, 0, 1);
    // Find first Monday on or after Jan 1
    const dayOfWeek = d.getDay();
    const daysUntilMonday = dayOfWeek === 0 ? 1 : (8 - dayOfWeek) % 7;
    d.setDate(d.getDate() + (daysUntilMonday || 7));

    while (d.getFullYear() < year + 1) {
      weekTicks.push({ offset: calculateOffset(new Date(d)) });
      d.setDate(d.getDate() + 7);
    }

    ++idx;
  }

  return {
    yearTicks,
    monthLineTicks,
    monthLabelTicks,
    weekTicks,
  };
});

/** Show month name labels when zoomed in enough */
const showMonthLabels = computed(() => zoom.value >= 1.4);
/** Show week tick lines */
const showWeekTicks = computed(() => zoom.value >= 3.5);
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
  const margin = 200;

  const scrollPosition = todayOffset * timelineWidth.value - margin;
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
    @wheel="onTimelineWheel"
  >
    <div :style="{ width: timelineWidth + 'px' }">
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

      <div class="timeline-header">
        <div
          v-for="year of markers.yearTicks"
          :key="year.offset"
          class="marker-year"
          :style="{ left: year.offset }"
        >
          <span>{{ year.year }}</span>
          <div class="marker-line"></div>
        </div>

        <div
          v-for="month of markers.monthLineTicks"
          :key="month"
          class="marker-month"
          :style="{ left: month }"
        >
          <div class="marker-line"></div>
        </div>

        <div
          v-for="month of markers.monthLabelTicks"
          v-show="showMonthLabels && month.label"
          :key="month.offset"
          class="marker-month-label"
          :style="{ left: month.offset }"
        >
          <span>{{ month.label }}</span>
        </div>

        <div
          v-for="week of markers.weekTicks"
          v-show="showWeekTicks"
          :key="week.offset"
          class="marker-week"
          :style="{ left: week.offset }"
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
            :canvas-ctx="sharedCanvasCtx"
            :start-date="props.filtered[index]!.start"
            :end-date="props.filtered[index]!.end"
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

.marker-month-label {
  position: absolute;
  top: 50%;
  transform: translate(-50%, -50%);
  font-size: 0.7rem;
  color: var(--primary-color);
  white-space: nowrap;
  pointer-events: none;
}

.marker-week {
  position: absolute;
  top: 0;
  height: 100%;
  display: flex;
  align-items: end;

  > .marker-line {
    width: 1px;
    height: 15%;
    background-color: var(--secondary-color);
    opacity: 0.5;
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
