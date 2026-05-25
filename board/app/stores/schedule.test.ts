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
      descendantEndIdx: 4,
    },
    {
      id: 2,
      wbsCode: "1.1",
      name: "Phase A",
      level: 0,
      code: "",
      index: 1,
      descendantEndIdx: 4,
    },
    {
      id: 3,
      wbsCode: "1.1.1",
      name: "Task Alpha",
      level: 0,
      code: "",
      index: 1,
      descendantEndIdx: 4,
    }, // Match here
    {
      id: 4,
      wbsCode: "1.2",
      name: "Phase B",
      level: 0,
      code: "",
      index: 1,
      descendantEndIdx: 4,
    }, // Should be filtered
  ];
  const result = searchFilter(mockData, "Alpha");
  expect(result).not.toBe(null);
  expect(result.length).toBe(mockData.length);
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
      descendantEndIdx: 2,
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
      descendantEndIdx: 2,
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
      descendantEndIdx: 3,
    },
  ];
  const { roots, nodes } = collectTree(data);
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

  // descendantEndIdx is set from the DTO (simulating database-provided value)
  expect(nodes[0]!.descendantEndIdx).toBe(2);
  expect(nodes[1]!.descendantEndIdx).toBe(2);
  expect(nodes[2]!.descendantEndIdx).toBe(3);
});
