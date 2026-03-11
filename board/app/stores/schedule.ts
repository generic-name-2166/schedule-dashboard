import { defineStore } from "pinia";
import { reactive, ref, type Ref } from "vue";

export interface ScheduleDTO {
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

export interface ScheduleNode extends Omit<ScheduleDTO, "start" | "end"> {
  start?: Date;
  end?: Date;
  children: number[];
  open: Ref<boolean>;
}

export interface ScheduleTreeLike {
  /** indicies of root nodes */
  roots: number[];
  /** parse the object and add properties for rendering, MUST be the same size as input array */
  nodes: ScheduleNode[];
}

function maybeParse(maybeDate?: string): Date | undefined {
  return maybeDate ? new Date(maybeDate) : undefined;
}

/**
 * @param array MUST be sorted by WBS code
 */
export function collectTree(array: ScheduleDTO[]): ScheduleTreeLike {
  const roots: number[] = [];

  interface OpenNode {
    index: number;
    depth: number;
  }

  /**
   * a stack of the latest "open" nodes' indicies and their WBS depth in the input array
   * utilizing the guarantee that the `array` is given in a WBS sorted order
   */
  const open: OpenNode[] = [];

  const nodes: ScheduleNode[] = new Array<ScheduleNode>(array.length);
  for (let index = 0; index < array.length; index++) {
    const value = array[index]!;
    const wbs: string[] = value.wbsCode.split(".");
    // current depth
    const depth: number = wbs.length;
    // Clean up stack to find actual parent
    while (open.length > 0 && open.at(-1)!.depth >= depth) {
      void open.pop();
    }

    if (open.length === 0) {
      roots.push(index);
    } else {
      // utilizing the fact that input array is WBS sorted to backtrack to parent by index
      const parent: ScheduleNode = nodes[open.at(-1)!.index]!;
      parent.children.push(index);
    }

    open.push({
      index,
      depth,
    } satisfies OpenNode);

    nodes[index] = {
      ...value,
      start: maybeParse(value.start),
      end: maybeParse(value.end),
      open: ref(true),
      children: [],
    } satisfies ScheduleNode;
  }

  return {
    roots,
    nodes,
  };
}

export const useScheduleStore = defineStore("schedule-store", () => {
  /** WBS codes of closed nodes */
  const closed = reactive<string[]>([]);

  async function init(): Promise<ScheduleDTO[]> {
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
    // TODO: move this to env var
    // eslint-disable-next-line @typescript-eslint/no-unnecessary-condition
    const url: string = window
      ? `/graphql?${params}`
      : `http://localhost:5095/graphql?${params}`;

    // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
    const response: { data: { scheduleObjects: ScheduleDTO[] } } = await fetch(
      url,
    ).then((r) => r.json());
    return response.data.scheduleObjects;
  }

  return {
    closed,
    init,
  };
});
