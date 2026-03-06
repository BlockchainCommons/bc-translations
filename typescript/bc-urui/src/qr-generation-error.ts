/** Base class for QR code generation errors. */
export class QRGenerationError extends Error {
  constructor(message: string) {
    super(message);
    this.name = "QRGenerationError";
  }
}

/** The QR code's module count exceeds the recommended scanning limit. */
export class QRCodeTooDenseError extends QRGenerationError {
  readonly moduleCount: number;
  readonly maxModules: number;

  constructor(moduleCount: number, maxModules: number) {
    super(
      `QR code too dense: ${moduleCount} modules exceeds limit of ${maxModules}`,
    );
    this.name = "QRCodeTooDenseError";
    this.moduleCount = moduleCount;
    this.maxModules = maxModules;
  }
}

/** Fewer frames were requested than the message has fountain-coded fragments. */
export class InsufficientFramesError extends QRGenerationError {
  readonly requested: number;
  readonly fragments: number;

  constructor(requested: number, fragments: number) {
    super(
      `Insufficient frames: ${requested} requested but message requires at least ${fragments} fragments`,
    );
    this.name = "InsufficientFramesError";
    this.requested = requested;
    this.fragments = fragments;
  }
}
