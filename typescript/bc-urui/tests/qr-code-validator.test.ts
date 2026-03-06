import { describe, test, expect } from "vitest";

import {
  DEFAULT_MAX_QR_MODULES,
  qrModuleCount,
  checkQRDensity,
  QRCorrectionLevel,
  QRCodeTooDenseError,
} from "../src/index.js";

describe("qrModuleCount", () => {
  test("returns module count for short message", () => {
    const message = new TextEncoder().encode("HELLO");
    const count = qrModuleCount(message, QRCorrectionLevel.Medium);
    expect(count).toBe(21); // QR version 1
  });

  test("higher correction level may increase module count", () => {
    // A message that fits version 1 at Low but might need more at High
    const message = new TextEncoder().encode("HELLO WORLD 1234567890");
    const countLow = qrModuleCount(message, QRCorrectionLevel.Low);
    const countHigh = qrModuleCount(message, QRCorrectionLevel.High);
    expect(countHigh).toBeGreaterThanOrEqual(countLow);
  });
});

describe("checkQRDensity", () => {
  test("does not throw for modules within limit", () => {
    expect(() => checkQRDensity(100, 117)).not.toThrow();
  });

  test("does not throw for modules exactly at limit", () => {
    expect(() => checkQRDensity(117, 117)).not.toThrow();
  });

  test("throws QRCodeTooDenseError when modules exceed limit", () => {
    expect(() => checkQRDensity(120, 117)).toThrow(QRCodeTooDenseError);
  });

  test("error contains module count and max modules", () => {
    try {
      checkQRDensity(150, 117);
      expect.fail("should have thrown");
    } catch (e) {
      expect(e).toBeInstanceOf(QRCodeTooDenseError);
      const error = e as QRCodeTooDenseError;
      expect(error.moduleCount).toBe(150);
      expect(error.maxModules).toBe(117);
    }
  });

  test("uses DEFAULT_MAX_QR_MODULES by default", () => {
    expect(DEFAULT_MAX_QR_MODULES).toBe(117);
    // 117 modules should pass
    expect(() => checkQRDensity(117)).not.toThrow();
    // 118 should fail
    expect(() => checkQRDensity(118)).toThrow(QRCodeTooDenseError);
  });
});
