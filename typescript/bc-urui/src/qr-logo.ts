/** Shape of the cleared module area around the logo. */
export enum QRLogoClearShape {
  /** Rectangular (square) clearing -- the default. */
  Square = "square",
  /** Circular clearing -- modules are cleared only if their center falls within the circle. */
  Circle = "circle",
}

/** Pixel data for a logo image (same shape as the browser ImageData interface). */
export interface QRLogoImageData {
  readonly width: number;
  readonly height: number;
  readonly data: Uint8ClampedArray;
}

/** Raw SVG markup to be rasterized at the exact size needed. */
export interface QRLogoSVG {
  readonly svgSource: string;
}

/** A logo source: either pre-rasterized pixel data or vector SVG markup. */
export type QRLogoSource = QRLogoImageData | QRLogoSVG;

/**
 * A logo image to overlay on the center of a QR code.
 *
 * Accepts either pre-rasterized `QRLogoImageData` or a `QRLogoSVG` with raw
 * SVG markup. When SVG is provided, `renderToCanvas` rasterizes it at the
 * exact pixel size needed, producing crisp output at any QR size or DPR.
 */
export class QRLogo {
  readonly source: QRLogoSource;
  readonly requestedFraction: number;
  readonly clearBorder: number;
  readonly clearShape: QRLogoClearShape;

  constructor(
    source: QRLogoSource,
    fraction: number = 0.25,
    clearBorder: number = 1,
    clearShape: QRLogoClearShape = QRLogoClearShape.Square,
  ) {
    this.source = source;
    this.requestedFraction = Math.max(0.01, Math.min(0.99, fraction));
    this.clearBorder = Math.max(0, Math.min(5, Math.round(clearBorder)));
    this.clearShape = clearShape;
  }

  /** Type guard: is the source SVG markup? */
  static isSVG(source: QRLogoSource): source is QRLogoSVG {
    return "svgSource" in source;
  }
}
