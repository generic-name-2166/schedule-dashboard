import { defineStore } from "pinia";
import { computed, ref, shallowRef, type Ref } from "vue";

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
  /** index in the sorted array for a specific date */
  index: number;
}

export interface ScheduleNode extends Omit<ScheduleDTO, "start" | "end"> {
  start?: Date;
  end?: Date;
  children: number[];
  /**
   * иметь мутабельное поле `open` производительнее чем отдельный мутабельный `Array<boolean>`
   * как `visible` и `search` из-за спагетти реактивности Vue
   */
  open: Ref<boolean | null>;
  /** whether the branch should be drawn on the depth level */
  depth: boolean[];
}

export interface ScheduleTreeLike {
  roots: Set<number>;
  /** parse the object and add properties for rendering, MUST be the same size as input array */
  nodes: ScheduleNode[];
  /** index of the last descendant for each node + 1, for leaves it will be current index + 1 */
  descendants: number[];
}

function maybeParse(maybeDate?: string): Date | undefined {
  return maybeDate ? new Date(maybeDate) : undefined;
}

/**
 * @param array MUST be sorted by WBS code
 */
export function collectTree(array: ScheduleDTO[]): ScheduleTreeLike {
  const roots = new Set<number>();
  const descendants: number[] = new Array<number>(array.length).fill(0);

  interface OpenNode {
    index: number;
    depth: number;
  }

  /**
   * a stack of the latest "open" nodes' indicies and their WBS depth in the input array
   * utilizing the guarantee that the `array` is given in a WBS sorted order
   */
  const open: OpenNode[] = [];

  const rootDepth = array[0]?.wbsCode.split(".").length ?? 0;

  const nodes: ScheduleNode[] = new Array<ScheduleNode>(array.length);
  for (let index = 0; index < array.length; index++) {
    const value = array[index]!;
    const wbs: string[] = value.wbsCode.split(".");
    // current depth
    const depth: number = wbs.length;
    // Clean up stack to find actual parent
    while (open.length > 0 && open.at(-1)!.depth >= depth) {
      const closed: OpenNode = open.pop()!;
      descendants[closed.index] = index;
    }

    if (open.length == 0) {
      roots.add(index);
    } else {
      // utilizing the fact that input array is WBS sorted to backtrack to parent by index
      const parent: ScheduleNode = nodes[open.at(-1)!.index]!;
      parent.children.push(index);
      parent.open.value = true;
    }

    open.push({
      index,
      depth,
    } satisfies OpenNode);

    nodes[index] = {
      ...value,
      start: maybeParse(value.start),
      end: maybeParse(value.end),
      depth: new Array<boolean>(depth - rootDepth + 1).fill(true),
      open: ref(null),
      children: [],
    } satisfies ScheduleNode;
  }

  // fill out the descendants array values for the last elements of each level
  for (const closed of open) {
    descendants[closed.index] = array.length;
  }

  for (let index = 0; index < nodes.length; ++index) {
    const node = nodes[index]!;
    const lastChild = node.children.at(-1);
    if (!lastChild) {
      continue;
    }
    const childDepthIdx: number = node.depth.length;
    for (let idx = lastChild + 1; idx < descendants[index]!; ++idx) {
      const descendantNode = nodes[idx]!;
      descendantNode.depth[childDepthIdx] = false;
    }
  }
  const lastRoot = Math.max(...roots, 0);
  for (let idx = lastRoot + 1; idx < nodes.length; ++idx) {
    const descendantNode = nodes[idx]!;
    descendantNode.depth[0] = false;
  }

  return {
    roots,
    nodes,
    descendants,
  };
}

export function searchFilter(
  nodes: ScheduleDTO[],
  searchString: string,
): boolean[] {
  if (!searchString) {
    return new Array<boolean>(nodes.length).fill(true);
  }

  const query = searchString.toUpperCase();
  const mask = new Array<boolean>(nodes.length).fill(false);
  let latestKeptWbs: string | null = null;

  for (let idx = nodes.length - 1; idx >= 0; idx--) {
    const node = nodes[idx]!;
    const isMatch = node.name.toUpperCase().includes(query);
    const isParentOfMatch = latestKeptWbs?.startsWith(node.wbsCode + ".");

    if (isMatch || isParentOfMatch) {
      mask[idx] = true;
      latestKeptWbs = node.wbsCode;
    }
  }
  return mask;
}

export async function getAvailableDates(): Promise<Date[]> {
  const query = `
    query {
      availableDates
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
  const response: {
    data?: { availableDates: string[] };
    errors?: { message: string }[];
  } = await fetch(url).then((r) => r.json());
  return response.data?.availableDates.map((date) => new Date(date)) ?? [];
}

async function sendGraphQL(
  operations: { query: string; variables: { date: Date; file: null } },
  file: File,
): Promise<string | undefined> {
  const map: string = JSON.stringify({
    "0": ["variables.file"],
  });

  const formData = new FormData();
  formData.append("operations", JSON.stringify(operations));
  formData.append("map", map);
  formData.append("0", file);

  const res = await fetch("/graphql", {
    method: "POST",
    // Do NOT set Content-Type; let the browser set multipart
    body: formData,
    // https://stackoverflow.com/a/76686111
    headers: {
      "GraphQL-preflight": "1",
    },
  });

  // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
  const result: { errors?: { message: string }[] } = await res.json();
  return result.errors?.[0]?.message;
}

export const useScheduleStore = defineStore("schedule-store", () => {
  const dates = ref<Date[]>([]);
  const currentDate = ref<Date>();

  const nodes = ref<ScheduleDTO[]>([]);
  const treelike = computed<ScheduleTreeLike>(() => collectTree(nodes.value));
  // необходимо shallow потому что от этого зависит filtered от которого зависит filtered[index].index
  // хоть индекс не изменяется, это приводит к круговой зависимости и фризу
  const visible = shallowRef<boolean[]>([]);
  const searchString = ref("");
  const searchFiltered = computed<boolean[]>(() =>
    searchFilter(nodes.value, searchString.value),
  );
  const filtered = computed<ScheduleNode[]>(() =>
    treelike.value.nodes.filter(
      (_, idx) => visible.value[idx] && searchFiltered.value[idx],
    ),
  );

  const scrollTop = ref(0);

  const fetchCurrent = async (): Promise<void> => {
    const query = `
      query ScheduleObjects($date: DateTime!) {
        scheduleObjects(date: $date) {
          id
          level
          wbsCode
          code
          name
          start
          end
          index
        }
      }
    `;
    const variables = `{ "date": "${currentDate.value!.toISOString()}" }`;
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
    const response: { data: { scheduleObjects: ScheduleDTO[] } } = await fetch(
      url,
    ).then((r) => r.json());

    nodes.value = response.data.scheduleObjects;
    visible.value = new Array<boolean>(nodes.value.length).fill(true);
  };

  const init = async (): Promise<void> => {
    dates.value = await getAvailableDates();
    currentDate.value = dates.value.at(-1);
    if (!currentDate.value) {
      return;
    }
    return fetchCurrent();
  };

  const changeDate = (date: Date): Promise<void> => {
    currentDate.value = date;
    return fetchCurrent();
  };

  async function create(file: File, date: Date): Promise<string | undefined> {
    const operations = {
      query: `
      mutation CreateScheduleObjects($date: DateTime!, $file: Upload!) {
        createScheduleObjects(date: $date, file: $file)
      }
    `,
      variables: { date, file: null },
    };

    return sendGraphQL(operations, file);
  }

  async function edit(file: File, date: Date): Promise<string | undefined> {
    const operations = {
      query: `
      mutation EditScheduleObjects($date: DateTime!, $file: Upload!) {
        editScheduleObjects(date: $date, file: $file)
      }
    `,
      variables: { date, file: null },
    };

    return sendGraphQL(operations, file);
  }

  const remove = async (): Promise<string | undefined> => {
    const date = currentDate.value!;
    const query = `
      mutation DeleteScheduleObjects($date: DateTime!) {
        deleteScheduleObjects(date: $date)
      }
    `;
    const operations = {
      query,
      variables: { date },
    };
    const res = await fetch("/graphql", {
      method: "POST",
      body: JSON.stringify(operations),
      // https://stackoverflow.com/a/76686111
      headers: {
        "GraphQL-preflight": "1",
        "Content-Type": "application/json",
      },
    });
    // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
    const result: { errors?: { message: string }[] } = await res.json();
    const error = result.errors?.[0]?.message;
    if (error) {
      return error;
    } else {
      await init();
      return undefined;
    }
  };

  return {
    dates,
    currentDate,
    treelike,
    visible,
    searchString,
    filtered,
    scrollTop,
    init,
    create,
    edit,
    remove,
    changeDate,
  };
});
