import { cbor } from "@bc/dcbor";

import { BYTEMOJIS, BYTEWORDS, Style, bytemojiIdentifier, decode, encode, identifier } from "../src/index.js";

describe("bytewords", () => {
  test("test_bytemoji_uniqueness", () => {
    const counts = new Map<string, number>();
    for (const bytemoji of BYTEMOJIS) {
      counts.set(bytemoji, (counts.get(bytemoji) ?? 0) + 1);
    }

    const duplicates = Array.from(counts.entries()).filter(([, count]) => count > 1);
    expect(duplicates).toEqual([]);
  });

  test("test_bytemoji_lengths", () => {
    const overLength: Array<[string, number]> = [];
    for (const bytemoji of BYTEMOJIS) {
      const length = new TextEncoder().encode(bytemoji).length;
      if (length > 4) {
        overLength.push([bytemoji, length]);
      }
    }

    expect(overLength).toEqual([]);
  });

  test("encode and decode", () => {
    const data = cbor([1, 2, 3]).toData();
    const encoded = encode(data, Style.MINIMAL);

    expect(encoded).toBe("lsadaoaxjygonesw");
    expect(decode(encoded, Style.MINIMAL)).toEqual(data);
  });

  test("identifier helpers", () => {
    const data = new Uint8Array([0, 1, 2, 3]);

    expect(identifier(data)).toBe("able acid also apex");
    expect(bytemojiIdentifier(data)).toBe("\u{1f600} \u{1f602} \u{1f606} \u{1f609}");
    expect(BYTEWORDS).toHaveLength(256);
    expect(BYTEMOJIS).toHaveLength(256);
  });
});
