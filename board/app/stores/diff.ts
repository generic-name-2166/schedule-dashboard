import { defineStore } from "pinia";
import { ref } from "vue";
import { getAvailableDates } from "./schedule.ts";

interface ScheduleDiffDTO {
  id: number;
  wbsCode: string;
  name: string;
  oldStart: string;
  newStart: string;
  oldEnd: string;
  newEnd: string;
}

export interface ScheduleDiff extends Pick<
  ScheduleDiffDTO,
  "id" | "wbsCode" | "name"
> {
  oldStart: Date;
  newStart: Date;
  oldEnd: Date;
  newEnd: Date;
}

export const useDiffStore = defineStore("diff-store", () => {
  const dates = ref<Date[]>([]);
  const oldDate = ref<Date>();
  const newDate = ref<Date>();

  const nodes = ref<ScheduleDiff[]>([]);

  const fetchDiff = async (): Promise<void> => {
    const query = `
      query DateDiff($oldDate: DateTime!, $newDate: DateTime!) {
        dateDiff(oldDate: $oldDate, newDate: $newDate) {
          id
          wbsCode
          name
          oldStart
          newStart
          oldEnd
          newEnd
        }
      }
    `;
    const variables = `{ "oldDate": "${oldDate.value!.toISOString()}", "newDate": "${newDate.value!.toISOString()}" }`;
    const params = new URLSearchParams({
      query,
      variables,
    });
    // TODO: move this to env var
    // eslint-disable-next-line @typescript-eslint/no-unnecessary-condition
    const url: string = window
      ? `/graphql?${params}`
      : `http://localhost:5095/graphql?${params}`;

    // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
    const response: { data: { dateDiff: ScheduleDiffDTO[] } } = await fetch(
      url,
    ).then((r) => r.json());

    nodes.value = response.data.dateDiff.map((diff) => ({
      ...diff,
      oldStart: new Date(diff.oldStart),
      newStart: new Date(diff.newStart),
      oldEnd: new Date(diff.oldEnd),
      newEnd: new Date(diff.newEnd),
    }));
  };

  const init = async (): Promise<void> => {
    dates.value = await getAvailableDates();
    oldDate.value = dates.value.at(-1);
    newDate.value = dates.value.at(-1);
  };

  return {
    dates,
    oldDate,
    newDate,
    nodes,
    init,
    fetchDiff,
  };
});
