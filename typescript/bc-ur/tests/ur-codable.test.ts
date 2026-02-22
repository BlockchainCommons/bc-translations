import {
  cbor,
  createTag,
  createTaggedCbor,
  extractTaggedContent,
  validateTag,
  type Cbor,
} from "@bc/dcbor";

import {
  fromUrString,
  ur,
  urString,
  type URCodable,
} from "../src/index.js";

class TestValue implements URCodable<TestValue> {
  readonly s: string;

  constructor(s: string) {
    this.s = s;
  }

  cborTags() {
    return [createTag(24, "leaf")];
  }

  untaggedCbor(): Cbor {
    return cbor(this.s);
  }

  taggedCbor(): Cbor {
    return createTaggedCbor(this);
  }

  fromUntaggedCbor(value: Cbor): TestValue {
    return new TestValue(value.toText());
  }

  fromTaggedCbor(value: Cbor): TestValue {
    validateTag(value, this.cborTags());
    return this.fromUntaggedCbor(extractTaggedContent(value));
  }
}

describe("URCodable", () => {
  test("test_ur_codable", () => {
    const value = new TestValue("test");

    const urValue = ur(value);
    expect(urString(value)).toBe("ur:leaf/iejyihjkjygupyltla");
    expect(urValue.toString()).toBe("ur:leaf/iejyihjkjygupyltla");

    const decoded = fromUrString(value, urValue.toString());
    expect(decoded.s).toBe(value.s);
  });
});
