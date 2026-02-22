import { cbor } from "@bc/dcbor";

import { UR, URType, URError } from "../src/index.js";

describe("UR", () => {
  test("test_ur", () => {
    const cborValue = cbor([1, 2, 3]);
    const ur = new UR("test", cborValue);
    const urString = ur.toString();

    expect(urString).toBe("ur:test/lsadaoaxjygonesw");

    const decoded = UR.fromUrString(urString);
    expect(decoded.type).toBe("test");
    expect(decoded.cbor().toData()).toEqual(cborValue.toData());

    const caps = UR.fromUrString("UR:TEST/LSADAOAXJYGONESW");
    expect(caps.type).toBe("test");
    expect(caps.cbor().toData()).toEqual(cborValue.toData());
  });

  test("type checks and qr helpers", () => {
    const ur = new UR("bytes", cbor(new Uint8Array([0xde, 0xad, 0xbe, 0xef])));

    expect(ur.qrString()).toBe(ur.toString().toUpperCase());
    expect(new TextDecoder().decode(ur.qrData())).toBe(ur.qrString());

    expect(() => ur.checkType("test")).toThrow(URError);
    expect(() => ur.checkType(new URType("bytes"))).not.toThrow();
  });
});
