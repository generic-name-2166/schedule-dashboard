import { defineStore } from "pinia";
import { ref, shallowRef } from "vue";
import {
  collectTree,
  type ScheduleDTO,
  type ScheduleNode,
} from "./schedule.ts";

const MOCK_SITES: ScheduleDTO["code"][] = ["2121474769192", "214747750911213"];

export interface ConstructionSite {
  index: number;
  wbsCode: string;
  name: string;
  start?: Date;
  end?: Date;
}

export const useSitesStore = defineStore("sites-store", () => {
  const sites = ref<ConstructionSite[]>([]);
  const subtrees = shallowRef<ScheduleNode[][]>([]);

  const init = async (date: Date): Promise<void> => {
    const query = `
      query ScheduleSubtrees($date: DateTime!, $codes: [String!]!) {
        scheduleSubtrees(date: $date, codes: $codes) {
          id
          level
          wbsCode
          code
          name
          start
          end
          index
          descendantEndIdx
        }
      }
    `;
    const variables = `{ "date": "${date.toISOString()}", "codes": ${JSON.stringify(MOCK_SITES)} }`;
    const params = new URLSearchParams({
      query,
      variables,
    });
    // eslint-disable-next-line @typescript-eslint/no-unnecessary-condition
    const url: string = window
      ? `/graphql?${params}`
      : `http://localhost:5095/graphql?${params}`;

    // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
    const response: {
      data?: { scheduleSubtrees: ScheduleDTO[][] };
      errors?: { message: string }[];
    } = await fetch(url).then((r) => r.json());

    const raw = response.data?.scheduleSubtrees ?? [];

    const sitesList: ConstructionSite[] = [];
    const trees: ScheduleNode[][] = [];

    for (const subtree of raw) {
      if (subtree.length === 0) {
        trees.push([]);
        continue;
      }

      const root: ScheduleDTO = subtree[0]!;
      sitesList.push({
        index: root.index,
        wbsCode: root.wbsCode,
        name: root.name,
        start: root.start ? new Date(root.start) : undefined,
        end: root.end ? new Date(root.end) : undefined,
      } satisfies ConstructionSite);

      // Remap indices to be local to this subtree
      const rootIdx = root.index;
      const remapped: ScheduleDTO[] = subtree.map((dto) => ({
        ...dto,
        index: dto.index - rootIdx,
        descendantEndIdx: dto.descendantEndIdx - rootIdx,
      }));

      trees.push(collectTree(remapped).nodes);
    }

    sites.value = sitesList;
    subtrees.value = trees;
  };

  return {
    sites,
    subtrees,
    init,
  };
});
