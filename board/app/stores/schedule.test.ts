import { expect, test } from "bun:test";
import { collectTree, type ScheduleDTO } from "./schedule.ts";

/* function filterNodes(flatNodes, searchStr) {
  if (!searchStr) return flatNodes.map((_, i) => i);

  const query = searchStr.toLowerCase();
  const keepIndices = [];
  let latestKeptWbs = null;

  for (let i = flatNodes.length - 1; i >= 0; i--) {
    const node = flatNodes[i];
    const isMatch = node.name.toLowerCase().includes(query);
    const isParentOfMatch =
      latestKeptWbs && latestKeptWbs.startsWith(node.wbsCode + ".");

    if (isMatch || isParentOfMatch) {
      keepIndices.unshift(i); // Keep original order
      latestKeptWbs = node.wbsCode;
    }
  }
  return keepIndices;
} */

// --- UNIT TEST ---
/* const mockData = [
  { id: 1, wbsCode: "1", name: "Root" },
  { id: 2, wbsCode: "1.1", name: "Phase A" },
  { id: 3, wbsCode: "1.1.1", name: "Task Alpha" }, // Match here
  { id: 4, wbsCode: "1.2", name: "Phase B" }, // Should be filtered
];

const result = filterNodes(mockData, "Alpha");
console.assert(result.length === 3, "Should keep match and 2 parents");
console.assert(
  result.includes(0) && result.includes(1) && result.includes(2),
  "Indices 0,1,2 should be present",
);
console.assert(!result.includes(3), "Phase B should be excluded"); */

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
    },
    {
      level: 5,
      wbsCode: "6.4.1.1",
      name: "Child",
      id: 1,
      code: "",
      start: "",
      end: "",
    },
  ];
  const { roots, nodes } = collectTree(data);
  // Root should be index 0
  expect(roots.length).toBe(1);
  expect(roots[0]).toBe(0);

  expect(nodes.length).toEqual(data.length);

  // WBS code "6.4.1" should have 1 child which has index 1 in the input array
  expect(nodes[0]?.wbsCode).toEqual("6.4.1");
  expect(nodes[0]?.children).toContain(1);
  expect(nodes[1]?.children).toHaveLength(0);
});
