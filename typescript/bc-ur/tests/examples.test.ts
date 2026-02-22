import { toByteString } from "@bc/dcbor";

import {
  MultipartDecoder,
  MultipartEncoder,
  UR,
} from "../src/index.js";

const runFountainTest = (startPart: number): number => {
  const message = "The only thing we have to fear is fear itself.";
  const cborValue = toByteString(new TextEncoder().encode(message));
  const ur = new UR("bytes", cborValue);

  const encoder = new MultipartEncoder(ur, 10);
  const decoder = new MultipartDecoder();

  for (let i = 0; i < 1000; i++) {
    const part = encoder.nextPart();
    if (encoder.currentIndex >= startPart) {
      decoder.receive(part);
    }
    if (decoder.isComplete) {
      break;
    }
  }

  const received = decoder.message;
  expect(received).toBeDefined();
  expect(received?.equals(ur)).toBe(true);

  return encoder.currentIndex;
};

describe("examples", () => {
  test("encode", () => {
    const cborValue = [1, 2, 3];
    const ur = new UR("test", cborValue);
    const urString = ur.toString();

    expect(urString).toBe("ur:test/lsadaoaxjygonesw");
  });

  test("decode", () => {
    const ur = UR.fromUrString("ur:test/lsadaoaxjygonesw");

    expect(ur.type).toBe("test");
    expect(ur.cbor().toData()).toEqual(new UR("test", [1, 2, 3]).cbor().toData());
  });

  test("test_fountain", () => {
    expect(runFountainTest(1)).toBe(5);
    expect(runFountainTest(51)).toBe(61);
    expect(runFountainTest(101)).toBe(110);
    expect(runFountainTest(501)).toBe(507);
  });
});
