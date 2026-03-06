import { describe, test, expect } from 'vitest';
import { bytesToHex } from '@bc/dcbor';
import { sha256, Xoshiro256StarStar } from '../src/index.js';

describe('xoshiro256starstar', () => {
  test('rng', () => {
    const data = new TextEncoder().encode('Hello World');
    const digest = sha256(data);
    const rng = Xoshiro256StarStar.fromData(digest);
    const key = rng.nextBytes(32);
    expect(bytesToHex(key)).toBe(
      'b18b446df414ec00714f19cb0f03e45cd3c3d5d071d2e7483ba8627c65b9926a',
    );
  });

  test('save_rng_state', () => {
    const state: [bigint, bigint, bigint, bigint] = [
      17295166580085024720n,
      422929670265678780n,
      5577237070365765850n,
      7953171132032326923n,
    ];
    const data = Xoshiro256StarStar.fromState(state).toData();
    expect(bytesToHex(data)).toBe(
      'd0e72cf15ec604f0bcab28594b8cde05dab04ae79053664d0b9dadc201575f6e',
    );
    const state2 = Xoshiro256StarStar.fromData(data).toState();
    const data2 = Xoshiro256StarStar.fromState(state2).toData();
    expect(bytesToHex(data2)).toBe(bytesToHex(data));
  });
});
