import { describe, test, expect } from "vitest";

import { LogoLayout } from "../src/index.js";

describe("LogoLayout", () => {
  test("basic layout with default fraction and border", () => {
    // 25 modules, fraction 0.25, border 1
    const layout = new LogoLayout(25, 0.25, 1);
    // 25 * 0.25 = 6.25, round = 6, make odd = 7
    // cleared = 7 + 2*1 = 9
    // maxCleared = floor(25 * 0.4) = 10
    // 9 <= 10, so no cap
    expect(layout.logoModules).toBe(7);
    expect(layout.clearedModules).toBe(9);
  });

  test("logo modules are always odd for symmetry", () => {
    // 20 modules, fraction 0.3, border 1
    // 20 * 0.3 = 6, already even, +1 = 7
    const layout = new LogoLayout(20, 0.3, 1);
    expect(layout.logoModules % 2).toBe(1);
  });

  test("cleared area capped at 40% of module count", () => {
    // 21 modules, fraction 0.5, border 2
    // 21 * 0.5 = 10.5, round = 11 (odd)
    // cleared = 11 + 4 = 15
    // maxCleared = floor(21 * 0.4) = 8
    // 15 > 8 → cleared = 8, logo = 8 - 4 = 4, make odd → 3
    const layout = new LogoLayout(21, 0.5, 2);
    expect(layout.clearedModules).toBeLessThanOrEqual(Math.floor(21 * 0.4));
    expect(layout.logoModules % 2).toBe(1);
  });

  test("zero border", () => {
    const layout = new LogoLayout(29, 0.25, 0);
    // 29 * 0.25 = 7.25, round = 7 (odd)
    // cleared = 7 + 0 = 7
    // maxCleared = floor(29 * 0.4) = 11
    expect(layout.logoModules).toBe(7);
    expect(layout.clearedModules).toBe(7);
  });

  test("very small module count yields zero logo modules", () => {
    // 5 modules, fraction 0.1, border 2
    // 5 * 0.1 = 0.5, round = 1 (odd)
    // cleared = 1 + 4 = 5
    // maxCleared = floor(5 * 0.4) = 2
    // 5 > 2 → cleared = 2, logo = 2 - 4 = -2, max(0) = 0
    const layout = new LogoLayout(5, 0.1, 2);
    expect(layout.logoModules).toBe(0);
  });

  test("large module count", () => {
    const layout = new LogoLayout(117, 0.25, 1);
    // 117 * 0.25 = 29.25, round = 29 (odd)
    // cleared = 29 + 2 = 31
    // maxCleared = floor(117 * 0.4) = 46
    expect(layout.logoModules).toBe(29);
    expect(layout.clearedModules).toBe(31);
  });
});
