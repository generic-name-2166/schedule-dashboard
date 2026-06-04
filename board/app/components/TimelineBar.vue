<script setup lang="ts">
import { useTemplateRef, onMounted, onUnmounted, ref, computed } from "vue";

const props = defineProps<{
  days: number;
  left: string;
  width: string;
  start: number;
  level: number;
  canvasCtx: CanvasRenderingContext2D | null;
}>();

const formatter = new Intl.NumberFormat("ru-RU", {
  style: "unit",
  unit: "day",
  unitDisplay: "long",
});

const displayMode = ref<"full" | "number" | "none">("full");

const barRef = useTemplateRef<HTMLParagraphElement>("bar");

const updateDisplayMode = (): void => {
  const bar = barRef.value;
  if (!bar || !props.canvasCtx) return;

  props.canvasCtx.font = getComputedStyle(bar).font;

  const fullText: string = formatter.format(props.days);
  const numText: string = props.days.toString();

  const barWidth: number = bar.clientWidth;

  // небольшой padding
  const fullWidth: number = props.canvasCtx.measureText(fullText).width + 10;
  const numWidth: number = props.canvasCtx.measureText(numText).width + 10;

  if (fullWidth <= barWidth) {
    displayMode.value = "full";
  } else if (numWidth <= barWidth) {
    displayMode.value = "number";
  } else {
    displayMode.value = "none";
  }
}

const displayText = computed<string>(() => {
  if (!props.days) return "";
  if (displayMode.value === "full") return formatter.format(props.days);
  if (displayMode.value === "number") return props.days.toString();
  return "";
});

const observer = ref<ResizeObserver>(new ResizeObserver(updateDisplayMode));

onMounted(() => {
  if (!barRef.value) return;
  updateDisplayMode();
  observer.value.observe(barRef.value);
});

onUnmounted(() => {
  observer.value.disconnect();
});
</script>

<template>
  <div class="bar-offset" :style="{ top: `${props.start}px` }">
    <div class="bar-wrapper" :style="{ '--level': `${props.level * 7.5}%` }">
      <p
        v-if="props.days && displayMode !== 'none'"
        ref="bar"
        class="bar"
        :style="{
          left: props.left,
          width: props.width,
        }"
      >
        <span>
          <span>{{ displayText }}</span>
        </span>
      </p>
    </div>
  </div>
</template>

<style lang="css" scoped>
.bar-offset {
  position: absolute;
  left: 0;
  transform: translateY(60px);
  width: stretch;
  overflow-y: hidden;
}

.bar-wrapper {
  display: block;
  position: relative;
  height: 40px;
  box-sizing: border-box;
  border-bottom: 0.125rem solid var(--secondary-color);
  filter: brightness(calc(120% - var(--level, 0%)));
}

.bar {
  height: 60%;
  position: absolute;
  top: 20%;
  background-color: var(--highlight-color);
  color: var(--primary-background);
  border-radius: 0.5rem;
  margin: 0;
  font-weight: bold;
  white-space: nowrap;

  > span {
    position: relative;
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
  }
}
</style>