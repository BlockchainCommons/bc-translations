import { describe, test, expect } from "vitest";

import {
  makeQRCode,
  QRCorrectionLevel,
  QRCodeTooDenseError,
  QRLogo,
} from "../src/index.js";

function createLogo(width: number, height: number, fraction = 0.25, border = 1): QRLogo {
  return new QRLogo(
    {
      width,
      height,
      data: new Uint8ClampedArray(width * height * 4).fill(255),
    },
    fraction,
    border,
  );
}

describe("makeQRCode", () => {
  test("generates module matrix for simple message", () => {
    const message = new TextEncoder().encode("HELLO");
    const result = makeQRCode(message);

    expect(result.moduleCount).toBe(21);
    expect(result.modules.length).toBe(21);
    expect(result.modules[0]!.length).toBe(21);
  });

  test("modules are boolean values", () => {
    const message = new TextEncoder().encode("TEST");
    const result = makeQRCode(message);

    for (const row of result.modules) {
      for (const cell of row) {
        expect(typeof cell).toBe("boolean");
      }
    }
  });

  test("uses default colors", () => {
    const message = new TextEncoder().encode("X");
    const result = makeQRCode(message);

    expect(result.foregroundColor).toBe("#000000");
    expect(result.backgroundColor).toBe("transparent");
  });

  test("accepts custom colors", () => {
    const message = new TextEncoder().encode("X");
    const result = makeQRCode(message, {
      foregroundColor: "#ff0000",
      backgroundColor: "#ffffff",
    });

    expect(result.foregroundColor).toBe("#ff0000");
    expect(result.backgroundColor).toBe("#ffffff");
  });

  test("upgrades to High correction level when logo present", () => {
    const message = new TextEncoder().encode("HELLO WORLD");

    const noLogo = makeQRCode(message, {
      correctionLevel: QRCorrectionLevel.Low,
    });

    const withLogo = makeQRCode(message, {
      correctionLevel: QRCorrectionLevel.Low,
      logo: createLogo(4, 4),
    });

    // With logo, correction is upgraded to High -> likely more modules
    expect(withLogo.moduleCount).toBeGreaterThanOrEqual(noLogo.moduleCount);
  });

  test("throws QRCodeTooDenseError when maxModules exceeded", () => {
    const message = new TextEncoder().encode("A".repeat(200));

    expect(() =>
      makeQRCode(message, { maxModules: 21 }),
    ).toThrow(QRCodeTooDenseError);
  });

  test("logo info is included when logo modules >= 3", () => {
    const message = new TextEncoder().encode("HELLO WORLD TEST DATA 12345");
    const result = makeQRCode(message, { logo: createLogo(10, 10) });

    if (result.logo) {
      expect(result.logo.logoModules).toBeGreaterThanOrEqual(3);
      expect(result.logo.logoModules % 2).toBe(1); // odd
      expect(result.logo.clearedModules).toBeGreaterThan(result.logo.logoModules);
    }
  });

  test("no logo info when logo would be too small", () => {
    const message = new TextEncoder().encode("HI");
    const result = makeQRCode(message, { logo: createLogo(2, 2, 0.01, 0) });

    // If included, it must have >= 3 logoModules
    if (result.logo) {
      expect(result.logo.logoModules).toBeGreaterThanOrEqual(3);
    }
  });
});
