import { CborError } from "@bc/dcbor";

import { messageFromUnknown } from "./utils.js";

/** Discriminated union describing the kind of UR error. */
export type URErrorKind =
  | { readonly type: "UR"; readonly message: string }
  | { readonly type: "Bytewords"; readonly message: string }
  | { readonly type: "Cbor"; readonly message: string }
  | { readonly type: "InvalidScheme" }
  | { readonly type: "TypeUnspecified" }
  | { readonly type: "InvalidType" }
  | { readonly type: "NotSinglePart" }
  | { readonly type: "UnexpectedType"; readonly expected: string; readonly actual: string };

const describeKind = (kind: URErrorKind): string => {
  switch (kind.type) {
    case "UR":
      return `UR decoder error (${kind.message})`;
    case "Bytewords":
      return `Bytewords error (${kind.message})`;
    case "Cbor":
      return `CBOR error (${kind.message})`;
    case "InvalidScheme":
      return "invalid UR scheme";
    case "TypeUnspecified":
      return "no UR type specified";
    case "InvalidType":
      return "invalid UR type";
    case "NotSinglePart":
      return "UR is not a single-part";
    case "UnexpectedType":
      return `expected UR type ${kind.expected}, but found ${kind.actual}`;
  }
};

/** Error class for all UR-related failures. */
export class URError extends Error {
  readonly kind: URErrorKind;

  constructor(kind: URErrorKind) {
    super(describeKind(kind));
    this.name = "URError";
    this.kind = kind;
  }

  static ur(message: string): URError {
    return new URError({ type: "UR", message });
  }

  static bytewords(message: string): URError {
    return new URError({ type: "Bytewords", message });
  }

  static cbor(message: string): URError {
    return new URError({ type: "Cbor", message });
  }

  static invalidScheme(): URError {
    return new URError({ type: "InvalidScheme" });
  }

  static typeUnspecified(): URError {
    return new URError({ type: "TypeUnspecified" });
  }

  static invalidType(): URError {
    return new URError({ type: "InvalidType" });
  }

  static notSinglePart(): URError {
    return new URError({ type: "NotSinglePart" });
  }

  static unexpectedType(expected: string, actual: string): URError {
    return new URError({ type: "UnexpectedType", expected, actual });
  }

  /** Wraps an unknown thrown value into a `URError`. */
  static fromUnknown(error: unknown): URError {
    if (error instanceof URError) {
      return error;
    }
    if (CborError.isCborError(error)) {
      return URError.cbor(error.message);
    }
    return URError.ur(messageFromUnknown(error));
  }
}
