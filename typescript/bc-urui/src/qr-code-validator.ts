import QRCode from "qrcode";

import { QRCorrectionLevel } from "./qr-correction-level.js";
import { QRCodeTooDenseError } from "./qr-generation-error.js";

/** Default maximum QR module count for reliable phone scanning (QR version 25). */
export const DEFAULT_MAX_QR_MODULES = 117;

/** Get the QR module count for a message at a given correction level. */
export function qrModuleCount(
  message: Uint8Array,
  correctionLevel: QRCorrectionLevel = QRCorrectionLevel.Medium,
): number {
  const text = new TextDecoder().decode(message);
  const qr = QRCode.create(text, {
    errorCorrectionLevel: correctionLevel,
  });
  return qr.modules.size;
}

/**
 * Validate that a QR module count is within a density limit.
 *
 * @throws {QRCodeTooDenseError} if `moduleCount` exceeds `maxModules`.
 */
export function checkQRDensity(
  moduleCount: number,
  maxModules: number = DEFAULT_MAX_QR_MODULES,
): void {
  if (moduleCount > maxModules) {
    throw new QRCodeTooDenseError(moduleCount, maxModules);
  }
}
