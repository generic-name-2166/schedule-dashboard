import { defineStore } from "pinia";

export interface ScheduleNode {
  id: number;
  level: number;
  wbsCode: string;
  code: string;
  name: string;
  /** Date */
  start?: string;
  /** Date */
  end?: string;
}

export interface ScheduleTreeLike {
  /** indicies of root nodes */
  roots: number[];
  /** map of WBS codes of nodes to indicies of their children */
  childrenMap: Map<string, number[]>;
}

/**
 * @param array MUST be sorted by WBS code
 */
export function collectTree(array: ScheduleNode[]): ScheduleTreeLike {
  const roots: number[] = [];
  const childrenMap: Map<string, number[]> = new Map();

  /**
   * a stack of the latest "open" nodes' wbs codes (split into arrays)
   * utilizing the guarantee that the `array` is given in a WBS sorted order
   */
  const open: string[][] = [];

  for (let index = 0; index < array.length; index++) {
    const node: ScheduleNode = array[index]!;
    const wbs: string[] = node.wbsCode.split(".");
    // 1. Clean up stack to find actual parent
    while (open.length > 0 && open.at(-1)!.length >= wbs.length) {
      open.pop();
    }

    if (open.length === 0) {
      roots.push(index);
    } else {
      const parent: string = open.at(-1)!.join(".");
      if (!childrenMap.has(parent)) {
        childrenMap.set(parent, []);
      }
      childrenMap.get(parent)!.push(index);
    }

    open.push(wbs);
  }

  return {
    roots,
    childrenMap,
  };
}

export const useScheduleStore = defineStore("schedule-store", () => {
  async function init(): Promise<ScheduleNode[]> {
    const query = `
      query {
        scheduleObjects {
          id
          level
          wbsCode
          code
          name
          start
          end
        }
      }
    `;
    const params = new URLSearchParams({
      query,
    });
    const url: string = window
      ? `/graphql?${params}`
      : `http://localhost:5095/graphql?${params}`;

    const response: { data: { scheduleObjects: ScheduleNode[] } } = await fetch(
      url,
    ).then((r) => r.json());
    return response.data.scheduleObjects;
  }

  return {
    init,
  };
});
