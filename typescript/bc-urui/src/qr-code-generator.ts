import QRCode from "qrcode";

import { QRCorrectionLevel } from "./qr-correction-level.js";
import { QRLogo, QRLogoClearShape, type QRLogoImageData, type QRLogoSource } from "./qr-logo.js";
import { checkQRDensity } from "./qr-code-validator.js";

/** A single row of QR modules where `true` means dark. */
export type QRModuleRow = boolean[];

/** Result of QR code generation — a module matrix plus rendering metadata. */
export interface QRCodeResult {
  /** 2D boolean matrix of QR modules (`true` = dark, `false` = light). */
  readonly modules: QRModuleRow[];
  /** Number of modules per side (the QR "size"). */
  readonly moduleCount: number;
  /** Foreground (dark module) CSS color. */
  readonly foregroundColor: string;
  /** Background (light module) CSS color. */
  readonly backgroundColor: string;
  /** Logo overlay info, if a logo was provided and large enough to render. */
  readonly logo?: QRCodeLogoInfo;
  /** Number of background-colored quiet zone modules on each side. */
  readonly quietZone: number;
}

/** Metadata for rendering a logo overlay on a QR code. */
export interface QRCodeLogoInfo {
  /** The logo source (ImageData or SVG markup). */
  readonly source: QRLogoSource;
  /** Number of QR modules covered by the logo (odd, centered). */
  readonly logoModules: number;
  /** Number of QR modules in the cleared area around the logo (includes border). */
  readonly clearedModules: number;
  /** Shape of the cleared area. */
  readonly clearShape: QRLogoClearShape;
}

/** Options for `makeQRCode()`. */
export interface MakeQRCodeOptions {
  correctionLevel?: QRCorrectionLevel;
  foregroundColor?: string;
  backgroundColor?: string;
  logo?: QRLogo;
  maxModules?: number;
  /** Number of background-colored modules around the QR data area (default 1). */
  quietZone?: number;
}

/**
 * Calculates logo and clearing dimensions in QR modules.
 *
 * Matches the Kotlin/Swift `LogoLayout` logic: odd module count for symmetry,
 * clear border on each side, 40% cap on cleared area width.
 */
export class LogoLayout {
  readonly logoModules: number;
  readonly clearedModules: number;

  constructor(moduleCount: number, requestedFraction: number, clearBorder: number) {
    // Calculate logo size in modules
    let logo = Math.round(moduleCount * requestedFraction);
    // Make odd for symmetry
    if (logo % 2 === 0) logo++;
    // Add clearBorder modules on each side
    let cleared = logo + 2 * clearBorder;
    // Cap: cleared area must not exceed 40% of QR width
    const maxCleared = Math.floor(moduleCount * 0.4);
    if (cleared > maxCleared) {
      cleared = maxCleared;
      logo = cleared - 2 * clearBorder;
    }
    // Ensure logo has odd module count
    if (logo % 2 === 0) logo--;

    this.logoModules = Math.max(0, logo);
    this.clearedModules = Math.max(0, cleared);
  }
}

/**
 * Generate a QR code from data bytes.
 *
 * Returns a framework-agnostic `QRCodeResult` containing the module matrix and
 * rendering metadata. Use `renderToCanvas()` to draw onto a canvas, or consume
 * the matrix directly for SVG/React/etc. rendering.
 *
 * When a `logo` is provided, the error correction level is automatically
 * upgraded to High.
 */
export function makeQRCode(
  message: Uint8Array,
  options: MakeQRCodeOptions = {},
): QRCodeResult {
  const {
    correctionLevel = QRCorrectionLevel.Medium,
    foregroundColor = "#000000",
    backgroundColor = "transparent",
    logo,
    maxModules,
    quietZone = 1,
  } = options;

  const effectiveCorrection = logo ? QRCorrectionLevel.High : correctionLevel;
  const text = new TextDecoder().decode(message);

  const qr = QRCode.create(text, {
    errorCorrectionLevel: effectiveCorrection,
  });

  const moduleCount = qr.modules.size;

  if (maxModules !== undefined) {
    checkQRDensity(moduleCount, maxModules);
  }

  // Build 2D boolean matrix from flat data
  const modules: QRModuleRow[] = [];
  for (let row = 0; row < moduleCount; row++) {
    const rowData: boolean[] = [];
    for (let col = 0; col < moduleCount; col++) {
      rowData.push((qr.modules.data[row * moduleCount + col] ?? 0) !== 0);
    }
    modules.push(rowData);
  }

  // Calculate logo info if present
  let logoInfo: QRCodeLogoInfo | undefined;
  if (logo) {
    const layout = new LogoLayout(moduleCount, logo.requestedFraction, logo.clearBorder);
    if (layout.logoModules >= 3) {
      logoInfo = {
        source: logo.source,
        logoModules: layout.logoModules,
        clearedModules: layout.clearedModules,
        clearShape: logo.clearShape,
      };
    }
  }

  return {
    modules,
    moduleCount,
    foregroundColor,
    backgroundColor,
    logo: logoInfo,
    quietZone,
  };
}

/**
 * Render a `QRCodeResult` onto a `CanvasRenderingContext2D`.
 *
 * Works with both browser `<canvas>` and Node.js `node-canvas`. The caller
 * controls the canvas size; this function fills the entire context area.
 *
 * Returns a promise that resolves when rendering is complete. The promise
 * resolves immediately for ImageData logos; for SVG logos it waits for the
 * browser to rasterize the SVG via an Image element.
 */
export async function renderToCanvas(
  result: QRCodeResult,
  ctx: CanvasRenderingContext2D,
  size: number,
): Promise<void> {
  const { modules, moduleCount, foregroundColor, backgroundColor, logo, quietZone } = result;
  const totalModules = moduleCount + 2 * quietZone;
  const pixelsPerModule = size / totalModules;
  const qzPx = quietZone * pixelsPerModule;

  // Fill background (covers quiet zone and QR area)
  if (backgroundColor !== "transparent") {
    ctx.fillStyle = backgroundColor;
    ctx.fillRect(0, 0, size, size);
  } else {
    ctx.clearRect(0, 0, size, size);
  }

  // Draw QR modules — offset by quiet zone, snap to integer pixel boundaries
  ctx.fillStyle = foregroundColor;
  for (let row = 0; row < moduleCount; row++) {
    for (let col = 0; col < moduleCount; col++) {
      if (modules[row]?.[col]) {
        const x = Math.floor(qzPx + col * pixelsPerModule);
        const y = Math.floor(qzPx + row * pixelsPerModule);
        const w = Math.floor(qzPx + (col + 1) * pixelsPerModule) - x;
        const h = Math.floor(qzPx + (row + 1) * pixelsPerModule) - y;
        ctx.fillRect(x, y, w, h);
      }
    }
  }

  // Draw logo overlay if present (centered within the QR data area)
  if (logo) {
    const clearColor = backgroundColor === "transparent" ? "#ffffff" : backgroundColor;
    const centerModule = moduleCount / 2;
    const qrPixels = moduleCount * pixelsPerModule;

    // Clear center area — snap to integer pixels
    ctx.fillStyle = clearColor;
    if (logo.clearShape === QRLogoClearShape.Square) {
      const clearPixels = logo.clearedModules * pixelsPerModule;
      const clearOrigin = Math.floor(qzPx + (qrPixels - clearPixels) / 2);
      const clearSize = Math.ceil(clearPixels);
      ctx.fillRect(clearOrigin, clearOrigin, clearSize, clearSize);
    } else {
      // Circle: clear individual modules whose centers fall within the circle
      const radius = logo.clearedModules / 2;
      const startModule = (moduleCount - logo.clearedModules) / 2;
      for (let row = 0; row < logo.clearedModules; row++) {
        for (let col = 0; col < logo.clearedModules; col++) {
          const mx = startModule + col + 0.5;
          const my = startModule + row + 0.5;
          const dx = mx - centerModule;
          const dy = my - centerModule;
          if (dx * dx + dy * dy <= radius * radius) {
            const x = Math.floor(qzPx + (startModule + col) * pixelsPerModule);
            const y = Math.floor(qzPx + (startModule + row) * pixelsPerModule);
            const w = Math.floor(qzPx + (startModule + col + 1) * pixelsPerModule) - x;
            const h = Math.floor(qzPx + (startModule + row + 1) * pixelsPerModule) - y;
            ctx.fillRect(x, y, w, h);
          }
        }
      }
    }

    // Draw logo image centered within the QR data area
    const logoPixels = logo.logoModules * pixelsPerModule;
    const logoOrigin = qzPx + (qrPixels - logoPixels) / 2;

    if (QRLogo.isSVG(logo.source)) {
      await drawLogoSVG(ctx, logo.source.svgSource, logoOrigin, logoOrigin, logoPixels, logoPixels);
    } else {
      drawLogoImageData(ctx, logo.source, logoOrigin, logoOrigin, logoPixels, logoPixels);
    }
  }
}

/**
 * Draw logo ImageData onto a canvas context, scaled to the given rectangle.
 *
 * Creates a temporary canvas from the source ImageData, then uses the main
 * context's `drawImage` for high-quality bilinear scaling (instead of
 * pixel-by-pixel nearest-neighbor which looks blocky at small source sizes).
 */
function drawLogoImageData(
  ctx: CanvasRenderingContext2D,
  image: QRLogoImageData,
  dx: number,
  dy: number,
  dw: number,
  dh: number,
): void {
  // Check for OffscreenCanvas (works in browsers and modern Node)
  if (typeof OffscreenCanvas !== "undefined") {
    const offscreen = new OffscreenCanvas(image.width, image.height);
    const offCtx = offscreen.getContext("2d")!;
    const imgData = new ImageData(
      new Uint8ClampedArray(image.data),
      image.width,
      image.height,
    );
    offCtx.putImageData(imgData, 0, 0);
    ctx.imageSmoothingEnabled = true;
    ctx.imageSmoothingQuality = "high";
    ctx.drawImage(offscreen, dx, dy, dw, dh);
    return;
  }

  // Fallback: pixel-by-pixel nearest-neighbor for environments without OffscreenCanvas
  const scaleX = image.width / dw;
  const scaleY = image.height / dh;

  for (let py = 0; py < Math.ceil(dh); py++) {
    for (let px = 0; px < Math.ceil(dw); px++) {
      const sx = Math.floor(px * scaleX);
      const sy = Math.floor(py * scaleY);
      const offset = (sy * image.width + sx) * 4;
      const r = image.data[offset] ?? 0;
      const g = image.data[offset + 1] ?? 0;
      const b = image.data[offset + 2] ?? 0;
      const a = image.data[offset + 3] ?? 0;
      if (a > 0) {
        ctx.fillStyle = `rgba(${r},${g},${b},${a / 255})`;
        ctx.fillRect(dx + px, dy + py, 1, 1);
      }
    }
  }
}

/**
 * Cache of loaded SVG Image elements, keyed by SVG source string.
 *
 * Once an SVG logo has been loaded into an Image (via Blob URL), we cache it
 * so subsequent frames can draw it synchronously — eliminating the flicker
 * caused by re-loading the SVG asynchronously every frame.
 */
const svgImageCache = new Map<string, HTMLImageElement>();

/**
 * Draw an SVG logo onto a canvas context at the exact pixel size needed.
 *
 * On the first call for a given SVG source, creates a Blob URL, loads it into
 * an Image element, caches the Image, and draws it. On subsequent calls, the
 * cached Image is drawn synchronously — no flicker between frames.
 */
function drawLogoSVG(
  ctx: CanvasRenderingContext2D,
  svgSource: string,
  dx: number,
  dy: number,
  dw: number,
  dh: number,
): Promise<void> {
  const cached = svgImageCache.get(svgSource);
  if (cached) {
    ctx.imageSmoothingEnabled = true;
    ctx.imageSmoothingQuality = "high";
    ctx.drawImage(cached, dx, dy, dw, dh);
    return Promise.resolve();
  }

  return new Promise((resolve, reject) => {
    const blob = new Blob([svgSource], { type: "image/svg+xml" });
    const url = URL.createObjectURL(blob);
    const img = new Image();
    img.onload = () => {
      URL.revokeObjectURL(url);
      svgImageCache.set(svgSource, img);
      ctx.imageSmoothingEnabled = true;
      ctx.imageSmoothingQuality = "high";
      ctx.drawImage(img, dx, dy, dw, dh);
      resolve();
    };
    img.onerror = () => {
      URL.revokeObjectURL(url);
      reject(new Error("Failed to load SVG logo"));
    };
    img.src = url;
  });
}
