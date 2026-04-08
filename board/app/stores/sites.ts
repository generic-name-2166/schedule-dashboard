import { defineStore } from "pinia";
import { ref } from "vue";
import type { ScheduleNode } from "./schedule.ts";

interface ConstructionDTO {
  name: string;
  index: number;
}

const MOCK_SITES: ConstructionDTO[] = [
  {
    name: "Мост на ПК 1234",
    index: 4563,
  },
  {
    name: "Путепровод на ПК 5678",
    index: 3564,
  },
];

export interface ConstructionSite extends ConstructionDTO {
  wbsCode: string;
  start?: Date;
  end?: Date;
}

export const useSitesStore = defineStore("sites-store", () => {
  const sites = ref<ConstructionSite[]>([]);

  const init = (nodes: ScheduleNode[]): void => {
    sites.value = MOCK_SITES.map((site) => {
      // TODO: hardcoded that the ksg ID is just the index in the array minus 1
      const { wbsCode, start, end } = nodes[site.index]!;
      return {
        ...site,
        wbsCode,
        start,
        end,
      };
    });
  };

  return {
    sites,
    init,
  };
});
