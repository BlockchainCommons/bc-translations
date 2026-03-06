import { describe, test, expect, vi, afterEach } from "vitest";

import { UR } from "@bc/ur";

import { URDisplayState, FragmentState } from "../src/index.js";

function createTestUR(size: number): UR {
  const data = new Uint8Array(size);
  for (let i = 0; i < size; i++) {
    data[i] = i & 0xff;
  }
  return new UR("bytes", data);
}

describe("URDisplayState", () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  test("single-part UR", () => {
    const ur = createTestUR(10);
    const state = new URDisplayState(ur, 100);

    expect(state.isSinglePart).toBe(true);
    expect(state.seqLen).toBe(1);
    expect(state.part.length).toBeGreaterThan(0);

    // Part should be uppercase UR string
    const partStr = new TextDecoder().decode(state.part);
    expect(partStr).toMatch(/^UR:BYTES\//i);
  });

  test("multi-part UR has correct fragment count", () => {
    const ur = createTestUR(200);
    const state = new URDisplayState(ur, 50);

    expect(state.isSinglePart).toBe(false);
    expect(state.seqLen).toBeGreaterThan(1);
  });

  test("emitNextPart advances sequence", () => {
    const ur = createTestUR(200);
    const state = new URDisplayState(ur, 50);

    const firstSeq = state.seqNum;
    state.emitNextPart();
    expect(state.seqNum).toBe(firstSeq + 1);
  });

  test("fragment states match fragment count", () => {
    const ur = createTestUR(200);
    const state = new URDisplayState(ur, 50);

    expect(state.fragmentStates.length).toBe(state.seqLen);
  });

  test("fragment states contain highlighted entries", () => {
    const ur = createTestUR(200);
    const state = new URDisplayState(ur, 50);

    const highlighted = state.fragmentStates.filter(
      (s) => s === FragmentState.Highlighted,
    );
    expect(highlighted.length).toBeGreaterThan(0);
  });

  test("fragment states are Off or Highlighted", () => {
    const ur = createTestUR(200);
    const state = new URDisplayState(ur, 50);

    for (const fs of state.fragmentStates) {
      expect([FragmentState.Off, FragmentState.Highlighted]).toContain(fs);
    }
  });

  test("restart resets state", () => {
    const ur = createTestUR(200);
    const state = new URDisplayState(ur, 50);

    // Advance several times
    state.emitNextPart();
    state.emitNextPart();
    state.emitNextPart();

    state.restart();
    // After restart, seqNum should be 1 (just emitted first part)
    expect(state.seqNum).toBe(1);
  });

  test("onUpdate callback is called", () => {
    const updates: number[] = [];
    const ur = createTestUR(200);
    const state = new URDisplayState(ur, 50, (s) => {
      updates.push(s.seqNum);
    });

    // Constructor emits first part
    expect(updates.length).toBe(1);

    state.emitNextPart();
    expect(updates.length).toBe(2);
  });

  test("run starts timer for multi-part", () => {
    vi.useFakeTimers();

    const ur = createTestUR(200);
    const state = new URDisplayState(ur, 50);
    const initialSeq = state.seqNum;

    state.run();
    vi.advanceTimersByTime(500); // 8fps = ~4 frames in 500ms
    state.stop();

    expect(state.seqNum).toBeGreaterThan(initialSeq);
  });

  test("run does nothing for single-part", () => {
    vi.useFakeTimers();

    const ur = createTestUR(10);
    const state = new URDisplayState(ur, 100);
    const initialSeq = state.seqNum;

    state.run();
    vi.advanceTimersByTime(500);
    state.stop();

    expect(state.seqNum).toBe(initialSeq);
  });

  test("stop halts timer", () => {
    vi.useFakeTimers();

    const ur = createTestUR(200);
    const state = new URDisplayState(ur, 50);

    state.run();
    vi.advanceTimersByTime(250);
    state.stop();
    const seqAfterStop = state.seqNum;

    vi.advanceTimersByTime(500);
    expect(state.seqNum).toBe(seqAfterStop);
  });

  test("part is uppercase UR string as bytes", () => {
    const ur = createTestUR(200);
    const state = new URDisplayState(ur, 50);

    const partStr = new TextDecoder().decode(state.part);
    // Multi-part format: UR:TYPE/SEQ-COUNT/BODY
    expect(partStr).toMatch(/^UR:BYTES\/\d+-\d+\//i);
    // Should be uppercase
    expect(partStr).toBe(partStr.toUpperCase());
  });
});
