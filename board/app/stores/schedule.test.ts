import { expect, test } from "bun:test";
import { collectTree, searchFilter, type ScheduleDTO } from "./schedule.ts";

test("filtering by search string", () => {
  const mockData: ScheduleDTO[] = [
    {
      id: 1,
      wbsCode: "1",
      name: "Root",
      level: 0,
      code: "",
      index: 1,
    },
    {
      id: 2,
      wbsCode: "1.1",
      name: "Phase A",
      level: 0,
      code: "",
      index: 1,
    },
    {
      id: 3,
      wbsCode: "1.1.1",
      name: "Task Alpha",
      level: 0,
      code: "",
      index: 1,
    }, // Match here
    {
      id: 4,
      wbsCode: "1.2",
      name: "Phase B",
      level: 0,
      code: "",
      index: 1,
    }, // Should be filtered
  ];
  const result = searchFilter(mockData, "Alpha");
  expect(result).not.toBe(null);
  expect(result.length).toBe(3);
  expect(result[0]).toBe(true);
  expect(result[1]).toBe(true);
  expect(result[2]).toBe(true);
  expect(result[3]).toBe(false);
});

test("collecting schedule nodes into a tree", () => {
  const data: ScheduleDTO[] = [
    {
      level: 4,
      wbsCode: "6.4.1",
      name: "Root",
      id: 0,
      code: "",
      start: "",
      end: "",
      index: 0,
    },
    {
      level: 5,
      wbsCode: "6.4.1.1",
      name: "Child",
      id: 1,
      code: "",
      start: "",
      end: "",
      index: 1,
    },
    {
      level: 4,
      wbsCode: "6.4.2",
      name: "Root 2",
      id: 0,
      code: "",
      start: "",
      end: "",
      index: 2,
    },
  ];
  const { roots, nodes, descendants } = collectTree(data);
  // Root should be index 0
  expect(roots.size).toBe(2);
  expect(roots).toContain(0);
  expect(roots).toContain(2);

  expect(nodes.length).toEqual(data.length);

  // WBS code "6.4.1" should have 1 child which has index 1 in the input array
  expect(nodes[0]?.wbsCode).toEqual("6.4.1");
  expect(nodes[0]?.children).toContain(1);
  expect(nodes[1]?.children).toHaveLength(0);
  expect(nodes[2]?.wbsCode).toEqual("6.4.2");
  expect(nodes[2]?.children).toHaveLength(0);

  expect(descendants[0]).toBe(2);
  expect(descendants[1]).toBe(2);
  expect(descendants[2]).toBe(3);
});
