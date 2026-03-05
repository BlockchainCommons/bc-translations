import {
    ARID,
    Compressed,
    Digest,
    EncryptedMessage,
    EncryptedKey,
    Nonce,
    PrivateKeyBase,
    PrivateKeys,
    PublicKeys,
    Reference,
    SSKRShare,
    Salt,
    SealedMessage,
    Signature,
    SymmetricKey,
    URI,
    UUID,
    XID,
} from '@bc/components';
import {
    CborDate,
    CborMap,
    cbor as toCbor,
    createTaggedCbor,
    decodeCbor,
    extractTaggedContent,
    tagsForValues,
    toTaggedValue,
    validateTag,
    type Cbor,
    type Tag,
} from '@bc/dcbor';
import {
    fromUr as decodeFromUr,
    fromUrString as decodeFromUrString,
    urString,
    UR,
} from '@bc/ur';
import {
    KNOWN_VALUES,
    KnownValue,
    OK_VALUE,
    POSITION,
    SALT as SALT_KV,
    UNIT,
    UNKNOWN_VALUE,
} from '@bc/known-values';
import {
    TAG_COMPRESSED,
    TAG_ENCODED_CBOR,
    TAG_ENCRYPTED,
    TAG_ENVELOPE,
    TAG_LEAF,
} from '@bc/tags';
import { RandomNumberGenerator, SecureRandomNumberGenerator } from '@bc/rand';

import { Assertion } from './assertion.js';
import type { EnvelopeCase } from './envelope-case.js';
import { asEnvelope } from './envelope-encodable.js';
import { EnvelopeError } from './error.js';
import { EdgeType } from './edge-type.js';
import { ObscureActions, type ObscureAction } from './obscure-action.js';
import { ObscureType } from './obscure-type.js';
import { FormatContextOpts, type FormatContextOpt } from './format-context.js';
import { formatEnvelope } from './envelope-notation.js';
import { treeFormat, treeFormatOpt } from './envelope-tree.js';
import { envelopeSummary } from './envelope-summary.js';
import { diagnostic, diagnosticAnnotated } from './envelope-diagnostic.js';
import { envelopeHex } from './envelope-hex.js';
import { mermaidFormat, mermaidFormatOpt } from './envelope-mermaid.js';

function envelopeTags(): Tag[] {
    return tagsForValues([TAG_ENVELOPE]);
}

function digestHex(digest: Digest): string {
    return digest.hex();
}

function digestEquals(a: Digest, b: Digest): boolean {
    return a.equals(b);
}

function digestIn(target: Iterable<Digest>, digest: Digest): boolean {
    for (const item of target) {
        if (item.equals(digest)) {
            return true;
        }
    }
    return false;
}

function toDigestSet(values: Iterable<Digest>): Map<string, Digest> {
    const map = new Map<string, Digest>();
    for (const value of values) {
        map.set(digestHex(value), value);
    }
    return map;
}

export type EnvelopeVisitor<State> = (
    envelope: Envelope,
    level: number,
    incomingEdge: EdgeType,
    state: State,
) => [State, boolean];

/** Core Gordian Envelope object. */
export class Envelope {
    readonly #envelopeCase: EnvelopeCase;

    private constructor(envelopeCase: EnvelopeCase) {
        this.#envelopeCase = envelopeCase;
    }

    case(): EnvelopeCase {
        return this.#envelopeCase;
    }

    toEnvelope(): Envelope {
        return this;
    }

    digest(): Digest {
        const c = this.#envelopeCase;
        switch (c.kind) {
            case 'node':
            case 'leaf':
            case 'wrapped':
            case 'elided':
            case 'known-value':
                return c.digest;
            case 'assertion':
                return c.assertion.digest();
            case 'encrypted':
                return c.encryptedMessage.digest();
            case 'compressed':
                return c.compressed.digest();
        }
    }

    static from(subject: unknown): Envelope {
        return asEnvelope(subject);
    }

    static fromOrNull(subject?: unknown | null): Envelope {
        return subject == null ? Envelope.null_() : Envelope.from(subject);
    }

    static fromOrNone(subject?: unknown | null): Envelope | undefined {
        return subject == null ? undefined : Envelope.from(subject);
    }

    static newAssertion(predicate: unknown, objectValue: unknown): Envelope {
        return Envelope.newWithAssertion(Assertion.create(predicate, objectValue));
    }

    static newLeaf(cbor: Cbor): Envelope {
        const digest = Digest.fromImage(cbor.toData());
        return new Envelope({ kind: 'leaf', cbor, digest });
    }

    static newWrapped(envelope: Envelope): Envelope {
        const digest = Digest.fromDigests([envelope.digest()]);
        return new Envelope({ kind: 'wrapped', envelope, digest });
    }

    static newElided(digest: Digest): Envelope {
        return new Envelope({ kind: 'elided', digest });
    }

    static newWithAssertion(assertion: Assertion): Envelope {
        return new Envelope({ kind: 'assertion', assertion });
    }

    static newWithKnownValue(value: KnownValue): Envelope {
        const digest = value.digest();
        return new Envelope({ kind: 'known-value', value, digest });
    }

    static newWithEncrypted(encryptedMessage: EncryptedMessage): Envelope {
        if (!encryptedMessage.hasDigest()) {
            throw EnvelopeError.missingDigest();
        }
        return new Envelope({ kind: 'encrypted', encryptedMessage });
    }

    static newWithCompressed(compressed: Compressed): Envelope {
        if (!compressed.hasDigest()) {
            throw EnvelopeError.missingDigest();
        }
        return new Envelope({ kind: 'compressed', compressed });
    }

    static newWithUncheckedAssertions(subject: Envelope, uncheckedAssertions: Envelope[]): Envelope {
        if (uncheckedAssertions.length === 0) {
            throw EnvelopeError.invalidFormat('unchecked assertions cannot be empty');
        }
        const sortedAssertions = [...uncheckedAssertions].sort((a, b) => {
            const aa = digestHex(a.digest());
            const bb = digestHex(b.digest());
            if (aa < bb) return -1;
            if (aa > bb) return 1;
            return 0;
        });
        const digests = [subject.digest(), ...sortedAssertions.map((item) => item.digest())];
        const digest = Digest.fromDigests(digests);
        return new Envelope({ kind: 'node', subject, assertions: sortedAssertions, digest });
    }

    static newWithAssertions(subject: Envelope, assertions: Envelope[]): Envelope {
        if (!assertions.every((item) => item.isSubjectAssertion() || item.isSubjectObscured())) {
            throw EnvelopeError.invalidFormat();
        }
        return Envelope.newWithUncheckedAssertions(subject, assertions);
    }

    static null_(): Envelope {
        return Envelope.newLeaf(toCbor(null));
    }

    static true_(): Envelope {
        return Envelope.newLeaf(toCbor(true));
    }

    static false_(): Envelope {
        return Envelope.newLeaf(toCbor(false));
    }

    static unit(): Envelope {
        return Envelope.from(UNIT);
    }

    static unknown(): Envelope {
        return Envelope.from(UNKNOWN_VALUE);
    }

    static ok(): Envelope {
        return Envelope.from(OK_VALUE);
    }

    static fromUntaggedCbor(cbor: Cbor): Envelope {
        if (cbor.isTagged()) {
            const [tag, item] = cbor.toTagged();
            const tagValue = Number(tag.value);
            if (tagValue === TAG_LEAF || tagValue === TAG_ENCODED_CBOR) {
                return Envelope.newLeaf(item);
            }
            if (tagValue === TAG_ENVELOPE) {
                const envelope = Envelope.fromTaggedCbor(cbor);
                return Envelope.newWrapped(envelope);
            }
            if (tagValue === TAG_ENCRYPTED) {
                return Envelope.newWithEncrypted(EncryptedMessage.fromUntaggedCbor(item));
            }
            if (tagValue === TAG_COMPRESSED) {
                return Envelope.newWithCompressed(Compressed.fromUntaggedCbor(item));
            }
            throw EnvelopeError.cbor(`unknown envelope tag: ${tag.value}`);
        }

        if (cbor.isByteString()) {
            const bytes = cbor.toByteString();
            return Envelope.newElided(Digest.fromData(bytes));
        }

        if (cbor.isArray()) {
            const elements = cbor.toArray();
            if (elements.length < 2) {
                throw EnvelopeError.cbor('node must have at least two elements');
            }
            const subject = Envelope.fromUntaggedCbor(elements[0]!);
            const assertions = elements.slice(1).map((item) => Envelope.fromUntaggedCbor(item));
            return Envelope.newWithAssertions(subject, assertions);
        }

        if (cbor.isMap()) {
            return Envelope.newWithAssertion(Assertion.fromCbor(cbor));
        }

        if (cbor.isUnsigned()) {
            return Envelope.newWithKnownValue(new KnownValue(BigInt(cbor.toInteger() as bigint)));
        }

        throw EnvelopeError.cbor('invalid envelope');
    }

    static fromTaggedCbor(cbor: Cbor): Envelope {
        validateTag(cbor, envelopeTags());
        return Envelope.fromUntaggedCbor(extractTaggedContent(cbor));
    }

    static fromTaggedCborData(data: Uint8Array): Envelope {
        return Envelope.fromTaggedCbor(decodeCbor(data));
    }

    static fromUr(ur: UR): Envelope {
        return decodeFromUr(Envelope, ur);
    }

    static fromUrString(value: string): Envelope {
        return decodeFromUrString(Envelope, value);
    }

    cborTags(): Tag[] {
        return envelopeTags();
    }

    static cborTags(): Tag[] {
        return envelopeTags();
    }

    untaggedCbor(): Cbor {
        const c = this.#envelopeCase;
        switch (c.kind) {
            case 'node': {
                const result: Cbor[] = [c.subject.untaggedCbor(), ...c.assertions.map((item) => item.untaggedCbor())];
                return toCbor(result);
            }
            case 'leaf':
                return toTaggedValue(TAG_LEAF, c.cbor);
            case 'wrapped':
                return c.envelope.taggedCbor();
            case 'assertion':
                return c.assertion.toCbor();
            case 'elided':
                return c.digest.untaggedCbor();
            case 'known-value':
                return c.value.untaggedCbor();
            case 'encrypted':
                return c.encryptedMessage.taggedCbor();
            case 'compressed':
                return c.compressed.taggedCbor();
        }
    }

    taggedCbor(): Cbor {
        return createTaggedCbor(this);
    }

    taggedCborData(): Uint8Array {
        return this.taggedCbor().toData();
    }

    urString(): string {
        return urString(this);
    }

    subject(): Envelope {
        const c = this.#envelopeCase;
        if (c.kind === 'node') {
            return c.subject;
        }
        return this;
    }

    assertions(): Envelope[] {
        const c = this.#envelopeCase;
        return c.kind === 'node' ? [...c.assertions] : [];
    }

    hasAssertions(): boolean {
        const c = this.#envelopeCase;
        return c.kind === 'node' && c.assertions.length > 0;
    }

    isAssertion(): boolean {
        return this.#envelopeCase.kind === 'assertion';
    }

    isEncrypted(): boolean {
        return this.#envelopeCase.kind === 'encrypted';
    }

    isCompressed(): boolean {
        return this.#envelopeCase.kind === 'compressed';
    }

    isElided(): boolean {
        return this.#envelopeCase.kind === 'elided';
    }

    isLeaf(): boolean {
        return this.#envelopeCase.kind === 'leaf';
    }

    isNode(): boolean {
        return this.#envelopeCase.kind === 'node';
    }

    isWrapped(): boolean {
        return this.#envelopeCase.kind === 'wrapped';
    }

    isKnownValue(): boolean {
        return this.#envelopeCase.kind === 'known-value';
    }

    isSubjectAssertion(): boolean {
        const c = this.#envelopeCase;
        if (c.kind === 'assertion') {
            return true;
        }
        if (c.kind === 'node') {
            return c.subject.isSubjectAssertion();
        }
        return false;
    }

    isSubjectEncrypted(): boolean {
        const c = this.#envelopeCase;
        if (c.kind === 'encrypted') {
            return true;
        }
        if (c.kind === 'node') {
            return c.subject.isSubjectEncrypted();
        }
        return false;
    }

    isSubjectCompressed(): boolean {
        const c = this.#envelopeCase;
        if (c.kind === 'compressed') {
            return true;
        }
        if (c.kind === 'node') {
            return c.subject.isSubjectCompressed();
        }
        return false;
    }

    isSubjectElided(): boolean {
        const c = this.#envelopeCase;
        if (c.kind === 'elided') {
            return true;
        }
        if (c.kind === 'node') {
            return c.subject.isSubjectElided();
        }
        return false;
    }

    isSubjectObscured(): boolean {
        return this.isSubjectElided() || this.isSubjectEncrypted() || this.isSubjectCompressed();
    }

    isInternal(): boolean {
        const kind = this.#envelopeCase.kind;
        return kind === 'node' || kind === 'wrapped' || kind === 'assertion';
    }

    isObscured(): boolean {
        return this.isElided() || this.isEncrypted() || this.isCompressed();
    }

    asAssertion(): Envelope | undefined {
        return this.#envelopeCase.kind === 'assertion' ? this : undefined;
    }

    tryAssertion(): Envelope {
        const result = this.asAssertion();
        if (result === undefined) throw EnvelopeError.notAssertion();
        return result;
    }

    asPredicate(): Envelope | undefined {
        const sub = this.subject().case();
        if (sub.kind === 'assertion') {
            return sub.assertion.predicate();
        }
        return undefined;
    }

    tryPredicate(): Envelope {
        const result = this.asPredicate();
        if (result === undefined) throw EnvelopeError.notAssertion();
        return result;
    }

    asObject(): Envelope | undefined {
        const sub = this.subject().case();
        if (sub.kind === 'assertion') {
            return sub.assertion.objectEnvelope();
        }
        return undefined;
    }

    tryObject(): Envelope {
        const result = this.asObject();
        if (result === undefined) throw EnvelopeError.notAssertion();
        return result;
    }

    asLeaf(): Cbor | undefined {
        const c = this.#envelopeCase;
        return c.kind === 'leaf' ? c.cbor : undefined;
    }

    tryLeaf(): Cbor {
        const result = this.asLeaf();
        if (result === undefined) throw EnvelopeError.notLeaf();
        return result;
    }

    asKnownValue(): KnownValue | undefined {
        const c = this.#envelopeCase;
        return c.kind === 'known-value' ? c.value : undefined;
    }

    tryKnownValue(): KnownValue {
        const result = this.asKnownValue();
        if (result === undefined) throw EnvelopeError.notKnownValue();
        return result;
    }

    isNull(): boolean {
        return this.asLeaf()?.isNull() ?? false;
    }

    isTrue(): boolean {
        return this.asLeaf()?.isTrue() ?? false;
    }

    isFalse(): boolean {
        return this.asLeaf()?.isFalse() ?? false;
    }

    isBool(): boolean {
        return this.isTrue() || this.isFalse();
    }

    isNumber(): boolean {
        return this.asLeaf()?.isNumber() ?? false;
    }

    isSubjectNumber(): boolean {
        return this.subject().isNumber();
    }

    isNaN(): boolean {
        return this.asLeaf()?.isNaN() ?? false;
    }

    isSubjectNaN(): boolean {
        return this.subject().isNaN();
    }

    tryByteString(): Uint8Array {
        return this.tryLeaf().toByteString();
    }

    asByteString(): Uint8Array | undefined {
        try {
            return this.asLeaf()?.toByteString();
        } catch {
            return undefined;
        }
    }

    asText(): string | undefined {
        try {
            return this.asLeaf()?.toText();
        } catch {
            return undefined;
        }
    }

    asArray(): readonly Cbor[] | undefined {
        try {
            return this.asLeaf()?.toArray();
        } catch {
            return undefined;
        }
    }

    asMap(): CborMap | undefined {
        try {
            return this.asLeaf()?.toMap();
        } catch {
            return undefined;
        }
    }

    isSubjectUnit(): boolean {
        const known = this.subject().asKnownValue();
        return known != null && known.equals(UNIT);
    }

    checkSubjectUnit(): Envelope {
        if (!this.isSubjectUnit()) {
            throw EnvelopeError.subjectNotUnit();
        }
        return this;
    }

    #decodeLeafAs<T>(cbor: Cbor, decoder?: (envelope: Envelope) => T): T {
        if (decoder != null) {
            return decoder(this);
        }

        try {
            if (cbor.isText()) return cbor.toText() as T;
            if (cbor.isBool()) return cbor.toBool() as T;
            if (cbor.isNull()) return null as T;
            if (cbor.isNumber()) return cbor.toNumber() as T;
            if (cbor.isByteString()) return cbor.toByteString() as T;
            if (cbor.isArray()) return cbor.toArray() as T;
            if (cbor.isMap()) return cbor.toMap() as T;
        } catch {
            // fall through to tagged decoding
        }

        const taggedDecoders: Array<() => unknown> = [
            () => Digest.fromCbor(cbor),
            () => Salt.fromCbor(cbor),
            () => Nonce.fromCbor(cbor),
            () => ARID.fromCbor(cbor),
            () => URI.fromCbor(cbor),
            () => UUID.fromCbor(cbor),
            () => XID.fromCbor(cbor),
            () => Reference.fromCbor(cbor),
            () => PublicKeys.fromCbor(cbor),
            () => PrivateKeys.fromCbor(cbor),
            () => PrivateKeyBase.fromCbor(cbor),
            () => SealedMessage.fromCbor(cbor),
            () => EncryptedKey.fromCbor(cbor),
            () => Signature.fromCbor(cbor),
            () => SSKRShare.fromCbor(cbor),
            () => SymmetricKey.fromCbor(cbor),
            () => CborDate.fromTaggedCbor(cbor),
            () => KnownValue.fromCbor(cbor),
        ];

        for (const tryDecode of taggedDecoders) {
            try {
                return tryDecode() as T;
            } catch {
                // continue
            }
        }

        throw EnvelopeError.invalidFormat('unable to decode subject type');
    }

    extractSubject<T>(decoder?: (envelope: Envelope) => T): T {
        let current: Envelope = this;
        while (true) {
            const currentCase = current.case();
            if (currentCase.kind !== 'node') {
                break;
            }
            current = currentCase.subject;
        }
        const c = current.case();
        switch (c.kind) {
            case 'leaf':
                return current.#decodeLeafAs<T>(c.cbor, decoder);
            case 'wrapped':
                return (decoder ? decoder(c.envelope) : c.envelope) as T;
            case 'assertion':
                return (decoder ? decoder(current) : c.assertion) as T;
            case 'elided':
                return (decoder ? decoder(current) : c.digest) as T;
            case 'known-value':
                return (decoder ? decoder(current) : c.value) as T;
            case 'encrypted':
                return (decoder ? decoder(current) : c.encryptedMessage) as T;
            case 'compressed':
                return (decoder ? decoder(current) : c.compressed) as T;
            case 'node':
                throw EnvelopeError.invalidFormat();
        }
    }

    extractObject<T>(decoder?: (envelope: Envelope) => T): T {
        return this.tryObject().extractSubject(decoder);
    }

    extractPredicate<T>(decoder?: (envelope: Envelope) => T): T {
        return this.tryPredicate().extractSubject(decoder);
    }

    assertionsWithPredicate(predicate: unknown): Envelope[] {
        const predicateEnvelope = Envelope.from(predicate);
        return this.assertions().filter((assertion) => {
            const pred = assertion.subject().asPredicate();
            return pred != null && digestEquals(pred.digest(), predicateEnvelope.digest());
        });
    }

    assertionWithPredicate(predicate: unknown): Envelope {
        const assertions = this.assertionsWithPredicate(predicate);
        if (assertions.length === 0) {
            throw EnvelopeError.nonexistentPredicate();
        }
        if (assertions.length > 1) {
            throw EnvelopeError.ambiguousPredicate();
        }
        return assertions[0]!;
    }

    optionalAssertionWithPredicate(predicate: unknown): Envelope | undefined {
        const assertions = this.assertionsWithPredicate(predicate);
        if (assertions.length === 0) {
            return undefined;
        }
        if (assertions.length > 1) {
            throw EnvelopeError.ambiguousPredicate();
        }
        return assertions[0]!;
    }

    objectForPredicate(predicate: unknown): Envelope {
        return this.assertionWithPredicate(predicate).asObject()!;
    }

    optionalObjectForPredicate(predicate: unknown): Envelope | undefined {
        const assertion = this.optionalAssertionWithPredicate(predicate);
        return assertion?.subject().asObject();
    }

    objectsForPredicate(predicate: unknown): Envelope[] {
        return this.assertionsWithPredicate(predicate).map((item) => item.asObject()!);
    }

    extractObjectForPredicate<T>(predicate: unknown, decoder?: (envelope: Envelope) => T): T {
        return this.assertionWithPredicate(predicate).extractObject(decoder);
    }

    extractOptionalObjectForPredicate<T>(predicate: unknown, decoder?: (envelope: Envelope) => T): T | undefined {
        const objectEnvelope = this.optionalObjectForPredicate(predicate);
        if (objectEnvelope == null) {
            return undefined;
        }
        return objectEnvelope.extractSubject(decoder);
    }

    extractObjectForPredicateWithDefault<T>(predicate: unknown, defaultValue: T, decoder?: (envelope: Envelope) => T): T {
        return this.extractOptionalObjectForPredicate(predicate, decoder) ?? defaultValue;
    }

    extractObjectsForPredicate<T>(predicate: unknown, decoder?: (envelope: Envelope) => T): T[] {
        return this.objectsForPredicate(predicate).map((item) => item.extractSubject(decoder));
    }

    elementsCount(): number {
        let result = 1;
        const c = this.#envelopeCase;
        if (c.kind === 'node') {
            result += c.subject.elementsCount();
            for (const assertion of c.assertions) {
                result += assertion.elementsCount();
            }
        } else if (c.kind === 'assertion') {
            result += c.assertion.predicate().elementsCount();
            result += c.assertion.objectEnvelope().elementsCount();
        } else if (c.kind === 'wrapped') {
            result += c.envelope.elementsCount();
        }
        return result;
    }

    addAssertion(predicate: unknown, objectValue: unknown): Envelope {
        return this.addOptionalAssertionEnvelope(Envelope.newAssertion(predicate, objectValue));
    }

    addAssertionEnvelope(assertionEnvelope: Envelope): Envelope {
        return this.addOptionalAssertionEnvelope(assertionEnvelope);
    }

    addAssertionEnvelopes(assertions: Envelope[]): Envelope {
        return assertions.reduce((current, assertion) => current.addAssertionEnvelope(assertion), this);
    }

    addOptionalAssertionEnvelope(assertion?: Envelope): Envelope {
        if (assertion == null) {
            return this;
        }
        if (!assertion.isSubjectAssertion() && !assertion.isSubjectObscured()) {
            throw EnvelopeError.invalidFormat();
        }

        const c = this.#envelopeCase;
        if (c.kind === 'node') {
            if (c.assertions.some((item) => digestEquals(item.digest(), assertion.digest()))) {
                return this;
            }
            return Envelope.newWithUncheckedAssertions(c.subject, [...c.assertions, assertion]);
        }

        return Envelope.newWithUncheckedAssertions(this.subject(), [assertion]);
    }

    addOptionalAssertion(predicate: unknown, objectValue?: unknown): Envelope {
        if (objectValue == null) {
            return this;
        }
        return this.addAssertionEnvelope(Envelope.newAssertion(predicate, objectValue));
    }

    addNonemptyStringAssertion(predicate: unknown, text: string): Envelope {
        if (text.length === 0) {
            return this;
        }
        return this.addAssertion(predicate, text);
    }

    addAssertions(envelopes: Envelope[]): Envelope {
        return envelopes.reduce((current, envelope) => current.addAssertionEnvelope(envelope), this);
    }

    addAssertionIf(condition: boolean, predicate: unknown, objectValue: unknown): Envelope {
        return condition ? this.addAssertion(predicate, objectValue) : this;
    }

    addAssertionEnvelopeIf(condition: boolean, assertionEnvelope: Envelope): Envelope {
        return condition ? this.addAssertionEnvelope(assertionEnvelope) : this;
    }

    removeAssertion(target: Envelope): Envelope {
        const currentAssertions = this.assertions();
        const targetDigest = target.digest();
        const index = currentAssertions.findIndex((item) => digestEquals(item.digest(), targetDigest));
        if (index < 0) {
            return this;
        }
        const newAssertions = [...currentAssertions];
        newAssertions.splice(index, 1);
        if (newAssertions.length === 0) {
            return this.subject();
        }
        return Envelope.newWithUncheckedAssertions(this.subject(), newAssertions);
    }

    replaceAssertion(oldAssertion: Envelope, newAssertion: Envelope): Envelope {
        return this.removeAssertion(oldAssertion).addAssertionEnvelope(newAssertion);
    }

    replaceSubject(newSubject: Envelope): Envelope {
        return this.assertions().reduce((current, assertion) => current.addAssertionEnvelope(assertion), newSubject);
    }

    addAssertionSalted(predicate: unknown, objectValue: unknown, salted: boolean): Envelope {
        return this.addOptionalAssertionEnvelopeSalted(Envelope.newAssertion(predicate, objectValue), salted);
    }

    addAssertionEnvelopeSalted(assertionEnvelope: Envelope, salted: boolean): Envelope {
        return this.addOptionalAssertionEnvelopeSalted(assertionEnvelope, salted);
    }

    addOptionalAssertionEnvelopeSalted(assertion: Envelope | undefined, salted: boolean): Envelope {
        if (assertion == null) {
            return this;
        }
        if (!assertion.isSubjectAssertion() && !assertion.isSubjectObscured()) {
            throw EnvelopeError.invalidFormat();
        }
        const envelope2 = salted ? assertion.addSalt() : assertion;
        const c = this.#envelopeCase;
        if (c.kind === 'node') {
            if (c.assertions.some((item) => digestEquals(item.digest(), envelope2.digest()))) {
                return this;
            }
            return Envelope.newWithUncheckedAssertions(c.subject, [...c.assertions, envelope2]);
        }
        return Envelope.newWithUncheckedAssertions(this.subject(), [envelope2]);
    }

    addAssertionsSalted(assertions: Envelope[], salted: boolean): Envelope {
        return assertions.reduce((current, assertion) => current.addAssertionEnvelopeSalted(assertion, salted), this);
    }

    wrap(): Envelope {
        return Envelope.newWrapped(this);
    }

    unwrap(): Envelope {
        const sub = this.subject().case();
        if (sub.kind === 'wrapped') {
            return sub.envelope;
        }
        throw EnvelopeError.notWrapped();
    }

    elide(): Envelope {
        return this.#envelopeCase.kind === 'elided' ? this : Envelope.newElided(this.digest());
    }

    elideRemovingSet(target: Set<Digest>): Envelope {
        return this.elideSetWithAction(target, false, ObscureActions.elide());
    }

    elideRevealingSet(target: Set<Digest>): Envelope {
        return this.elideSetWithAction(target, true, ObscureActions.elide());
    }

    elideRemovingTarget(target: { digest(): Digest }): Envelope {
        return this.elideRemovingSet(new Set([target.digest()]));
    }

    elideRevealingTarget(target: { digest(): Digest }): Envelope {
        return this.elideRevealingSet(new Set([target.digest()]));
    }

    elideRemovingSetWithAction(target: Set<Digest>, action: ObscureAction): Envelope {
        return this.elideSetWithAction(target, false, action);
    }

    elideRevealingSetWithAction(target: Set<Digest>, action: ObscureAction): Envelope {
        return this.elideSetWithAction(target, true, action);
    }

    elideRemovingTargetWithAction(target: { digest(): Digest }, action: ObscureAction): Envelope {
        return this.elideRemovingSetWithAction(new Set([target.digest()]), action);
    }

    elideRevealingTargetWithAction(target: { digest(): Digest }, action: ObscureAction): Envelope {
        return this.elideRevealingSetWithAction(new Set([target.digest()]), action);
    }

    elideRemovingArray(target: Array<{ digest(): Digest }>): Envelope {
        return this.elideRemovingSet(new Set(target.map((item) => item.digest())));
    }

    elideRevealingArray(target: Array<{ digest(): Digest }>): Envelope {
        return this.elideRevealingSet(new Set(target.map((item) => item.digest())));
    }

    elideRemovingArrayWithAction(target: Array<{ digest(): Digest }>, action: ObscureAction): Envelope {
        return this.elideRemovingSetWithAction(new Set(target.map((item) => item.digest())), action);
    }

    elideRevealingArrayWithAction(target: Array<{ digest(): Digest }>, action: ObscureAction): Envelope {
        return this.elideRevealingSetWithAction(new Set(target.map((item) => item.digest())), action);
    }

    elideSetWithAction(target: Set<Digest>, isRevealing: boolean, action: ObscureAction): Envelope {
        const selfDigest = this.digest();
        if (digestIn(target, selfDigest) !== isRevealing) {
            if (action.type === 'elide') {
                return this.elide();
            }
            if (action.type === 'encrypt') {
                const message = action.key.encryptWithDigest(this.taggedCborData(), selfDigest);
                return Envelope.newWithEncrypted(message);
            }
            return this.compress();
        }

        const c = this.#envelopeCase;
        if (c.kind === 'assertion') {
            const predicate = c.assertion.predicate().elideSetWithAction(target, isRevealing, action);
            const objectEnvelope = c.assertion.objectEnvelope().elideSetWithAction(target, isRevealing, action);
            return Envelope.newWithAssertion(Assertion.create(predicate, objectEnvelope));
        }

        if (c.kind === 'node') {
            const subject = c.subject.elideSetWithAction(target, isRevealing, action);
            const assertions = c.assertions.map((item) => item.elideSetWithAction(target, isRevealing, action));
            return Envelope.newWithUncheckedAssertions(subject, assertions);
        }

        if (c.kind === 'wrapped') {
            return Envelope.newWrapped(c.envelope.elideSetWithAction(target, isRevealing, action));
        }

        return this;
    }

    unelide(original: Envelope): Envelope {
        if (digestEquals(this.digest(), original.digest())) {
            return original;
        }
        throw EnvelopeError.invalidDigest();
    }

    walkUnelide(envelopes: Envelope[]): Envelope {
        const map = new Map<string, Envelope>();
        for (const envelope of envelopes) {
            map.set(digestHex(envelope.digest()), envelope);
        }
        return this.#walkUnelideWithMap(map);
    }

    #walkUnelideWithMap(map: Map<string, Envelope>): Envelope {
        const c = this.#envelopeCase;
        if (c.kind === 'elided') {
            return map.get(digestHex(this.digest())) ?? this;
        }

        if (c.kind === 'node') {
            const subject = c.subject.#walkUnelideWithMap(map);
            const assertions = c.assertions.map((item) => item.#walkUnelideWithMap(map));
            if (
                subject.isIdenticalTo(c.subject)
                && assertions.every((item, index) => item.isIdenticalTo(c.assertions[index]!))
            ) {
                return this;
            }
            return Envelope.newWithUncheckedAssertions(subject, assertions);
        }

        if (c.kind === 'wrapped') {
            const envelope = c.envelope.#walkUnelideWithMap(map);
            return envelope.isIdenticalTo(c.envelope) ? this : envelope.wrap();
        }

        if (c.kind === 'assertion') {
            const predicate = c.assertion.predicate().#walkUnelideWithMap(map);
            const objectEnvelope = c.assertion.objectEnvelope().#walkUnelideWithMap(map);
            if (predicate.isIdenticalTo(c.assertion.predicate()) && objectEnvelope.isIdenticalTo(c.assertion.objectEnvelope())) {
                return this;
            }
            return Envelope.newAssertion(predicate, objectEnvelope);
        }

        return this;
    }

    walkReplace(target: Set<Digest>, replacement: Envelope): Envelope {
        if (digestIn(target, this.digest())) {
            return replacement;
        }

        const c = this.#envelopeCase;
        if (c.kind === 'node') {
            const subject = c.subject.walkReplace(target, replacement);
            const assertions = c.assertions.map((item) => item.walkReplace(target, replacement));
            if (
                subject.isIdenticalTo(c.subject)
                && assertions.every((item, index) => item.isIdenticalTo(c.assertions[index]!))
            ) {
                return this;
            }
            return Envelope.newWithAssertions(subject, assertions);
        }

        if (c.kind === 'wrapped') {
            const envelope = c.envelope.walkReplace(target, replacement);
            return envelope.isIdenticalTo(c.envelope) ? this : envelope.wrap();
        }

        if (c.kind === 'assertion') {
            const predicate = c.assertion.predicate().walkReplace(target, replacement);
            const objectEnvelope = c.assertion.objectEnvelope().walkReplace(target, replacement);
            if (predicate.isIdenticalTo(c.assertion.predicate()) && objectEnvelope.isIdenticalTo(c.assertion.objectEnvelope())) {
                return this;
            }
            return Envelope.newAssertion(predicate, objectEnvelope);
        }

        return this;
    }

    walkDecrypt(keys: SymmetricKey[]): Envelope {
        const c = this.#envelopeCase;
        if (c.kind === 'encrypted') {
            for (const key of keys) {
                try {
                    return this.decryptSubject(key).walkDecrypt(keys);
                } catch {
                    // try next key
                }
            }
            return this;
        }

        if (c.kind === 'node') {
            const subject = c.subject.walkDecrypt(keys);
            const assertions = c.assertions.map((item) => item.walkDecrypt(keys));
            if (
                subject.isIdenticalTo(c.subject)
                && assertions.every((item, index) => item.isIdenticalTo(c.assertions[index]!))
            ) {
                return this;
            }
            return Envelope.newWithUncheckedAssertions(subject, assertions);
        }

        if (c.kind === 'wrapped') {
            const envelope = c.envelope.walkDecrypt(keys);
            return envelope.isIdenticalTo(c.envelope) ? this : envelope.wrap();
        }

        if (c.kind === 'assertion') {
            const predicate = c.assertion.predicate().walkDecrypt(keys);
            const objectEnvelope = c.assertion.objectEnvelope().walkDecrypt(keys);
            if (predicate.isIdenticalTo(c.assertion.predicate()) && objectEnvelope.isIdenticalTo(c.assertion.objectEnvelope())) {
                return this;
            }
            return Envelope.newAssertion(predicate, objectEnvelope);
        }

        return this;
    }

    walkDecompress(targetDigests?: Set<Digest>): Envelope {
        const c = this.#envelopeCase;
        if (c.kind === 'compressed') {
            const matches = targetDigests == null || digestIn(targetDigests, this.digest());
            if (matches) {
                try {
                    return this.decompress().walkDecompress(targetDigests);
                } catch {
                    return this;
                }
            }
            return this;
        }

        if (c.kind === 'node') {
            const subject = c.subject.walkDecompress(targetDigests);
            const assertions = c.assertions.map((item) => item.walkDecompress(targetDigests));
            if (
                subject.isIdenticalTo(c.subject)
                && assertions.every((item, index) => item.isIdenticalTo(c.assertions[index]!))
            ) {
                return this;
            }
            return Envelope.newWithUncheckedAssertions(subject, assertions);
        }

        if (c.kind === 'wrapped') {
            const envelope = c.envelope.walkDecompress(targetDigests);
            return envelope.isIdenticalTo(c.envelope) ? this : envelope.wrap();
        }

        if (c.kind === 'assertion') {
            const predicate = c.assertion.predicate().walkDecompress(targetDigests);
            const objectEnvelope = c.assertion.objectEnvelope().walkDecompress(targetDigests);
            if (predicate.isIdenticalTo(c.assertion.predicate()) && objectEnvelope.isIdenticalTo(c.assertion.objectEnvelope())) {
                return this;
            }
            return Envelope.newAssertion(predicate, objectEnvelope);
        }

        return this;
    }

    nodesMatching(targetDigests?: Set<Digest>, obscureTypes: ObscureType[] = []): Set<Digest> {
        const map = new Map<string, Digest>();
        this.walk(false, undefined, (envelope, _level, _edge, state) => {
            const digestMatches = targetDigests == null || digestIn(targetDigests, envelope.digest());
            if (digestMatches) {
                if (obscureTypes.length === 0) {
                    map.set(digestHex(envelope.digest()), envelope.digest());
                } else {
                    const typeMatches = obscureTypes.some((item) => {
                        if (item === ObscureType.Elided) return envelope.isElided();
                        if (item === ObscureType.Encrypted) return envelope.isEncrypted();
                        if (item === ObscureType.Compressed) return envelope.isCompressed();
                        return false;
                    });
                    if (typeMatches) {
                        map.set(digestHex(envelope.digest()), envelope.digest());
                    }
                }
            }
            return [state, false];
        });
        return new Set(map.values());
    }

    digests(levelLimit: number): Set<Digest> {
        const map = new Map<string, Digest>();
        this.walk(false, undefined, (envelope, level, _edge, state) => {
            if (level < levelLimit) {
                map.set(digestHex(envelope.digest()), envelope.digest());
                map.set(digestHex(envelope.subject().digest()), envelope.subject().digest());
            }
            return [state, false];
        });
        return new Set(map.values());
    }

    deepDigests(): Set<Digest> {
        return this.digests(Number.MAX_SAFE_INTEGER);
    }

    shallowDigests(): Set<Digest> {
        return this.digests(2);
    }

    structuralDigest(): Digest {
        const bytes: number[] = [];
        this.walk(false, undefined, (envelope, _level, _edge, state) => {
            const c = envelope.case();
            if (c.kind === 'elided') {
                bytes.push(1);
            } else if (c.kind === 'encrypted') {
                bytes.push(0);
            } else if (c.kind === 'compressed') {
                bytes.push(2);
            }
            bytes.push(...envelope.digest().data);
            return [state, false];
        });
        return Digest.fromImage(new Uint8Array(bytes));
    }

    isEquivalentTo(other: Envelope): boolean {
        return digestEquals(this.digest(), other.digest());
    }

    isIdenticalTo(other: Envelope): boolean {
        if (!this.isEquivalentTo(other)) {
            return false;
        }
        return digestEquals(this.structuralDigest(), other.structuralDigest());
    }

    walk<State>(hideNodes: boolean, state: State, visit: EnvelopeVisitor<State>): void {
        if (hideNodes) {
            this.#walkTree(0, EdgeType.None, state, visit);
        } else {
            this.#walkStructure(0, EdgeType.None, state, visit);
        }
    }

    #walkStructure<State>(
        level: number,
        incomingEdge: EdgeType,
        state: State,
        visit: EnvelopeVisitor<State>,
    ): void {
        const [nextState, stop] = visit(this, level, incomingEdge, state);
        if (stop) {
            return;
        }

        const nextLevel = level + 1;
        const c = this.#envelopeCase;
        if (c.kind === 'node') {
            c.subject.#walkStructure(nextLevel, EdgeType.Subject, nextState, visit);
            for (const assertion of c.assertions) {
                assertion.#walkStructure(nextLevel, EdgeType.Assertion, nextState, visit);
            }
        } else if (c.kind === 'wrapped') {
            c.envelope.#walkStructure(nextLevel, EdgeType.Content, nextState, visit);
        } else if (c.kind === 'assertion') {
            c.assertion.predicate().#walkStructure(nextLevel, EdgeType.Predicate, nextState, visit);
            c.assertion.objectEnvelope().#walkStructure(nextLevel, EdgeType.Object, nextState, visit);
        }
    }

    #walkTree<State>(
        level: number,
        incomingEdge: EdgeType,
        state: State,
        visit: EnvelopeVisitor<State>,
    ): State {
        let currentState = state;
        let subjectLevel = level;

        if (!this.isNode()) {
            const [nextState, stop] = visit(this, level, incomingEdge, currentState);
            if (stop) {
                return nextState;
            }
            currentState = nextState;
            subjectLevel = level + 1;
        }

        const c = this.#envelopeCase;
        if (c.kind === 'node') {
            let assertionState = c.subject.#walkTree(subjectLevel, EdgeType.Subject, currentState, visit);
            const assertionLevel = subjectLevel + 1;
            for (const assertion of c.assertions) {
                assertionState = assertion.#walkTree(assertionLevel, EdgeType.Assertion, assertionState, visit);
            }
            return assertionState;
        }

        if (c.kind === 'wrapped') {
            return c.envelope.#walkTree(subjectLevel, EdgeType.Content, currentState, visit);
        }

        if (c.kind === 'assertion') {
            currentState = c.assertion.predicate().#walkTree(subjectLevel, EdgeType.Predicate, currentState, visit);
            currentState = c.assertion.objectEnvelope().#walkTree(subjectLevel, EdgeType.Object, currentState, visit);
        }

        return currentState;
    }

    encryptSubject(key: SymmetricKey, nonce?: Nonce): Envelope {
        const c = this.#envelopeCase;
        if (c.kind === 'node') {
            if (c.subject.isEncrypted()) {
                throw EnvelopeError.alreadyEncrypted();
            }
            const encoded = c.subject.taggedCborData();
            const digest = c.subject.digest();
            const message = key.encryptWithDigest(encoded, digest, nonce);
            const encryptedSubject = Envelope.newWithEncrypted(message);
            return Envelope.newWithUncheckedAssertions(encryptedSubject, c.assertions);
        }

        if (c.kind === 'encrypted') {
            throw EnvelopeError.alreadyEncrypted();
        }
        if (c.kind === 'elided') {
            throw EnvelopeError.alreadyElided();
        }

        const encoded = this.taggedCborData();
        const digest = this.digest();
        const message = key.encryptWithDigest(encoded, digest, nonce);
        return Envelope.newWithEncrypted(message);
    }

    decryptSubject(key: SymmetricKey): Envelope {
        const sub = this.subject();
        const subCase = sub.case();
        if (subCase.kind !== 'encrypted') {
            throw EnvelopeError.notEncrypted();
        }
        const encryptedMessage = subCase.encryptedMessage;
        const decrypted = key.decrypt(encryptedMessage);
        const envelope = Envelope.fromTaggedCborData(decrypted);
        if (!digestEquals(envelope.digest(), sub.digest())) {
            throw EnvelopeError.invalidDigest();
        }
        const assertions = this.assertions();
        if (assertions.length === 0) {
            return envelope;
        }
        return Envelope.newWithUncheckedAssertions(envelope, assertions);
    }

    encrypt(key: SymmetricKey, nonce?: Nonce): Envelope {
        return this.wrap().encryptSubject(key, nonce);
    }

    decrypt(key: SymmetricKey): Envelope {
        return this.decryptSubject(key).unwrap();
    }

    compress(): Envelope {
        const c = this.#envelopeCase;
        if (c.kind === 'compressed') {
            return this;
        }
        if (c.kind === 'encrypted') {
            throw EnvelopeError.alreadyEncrypted();
        }
        if (c.kind === 'elided') {
            throw EnvelopeError.alreadyElided();
        }
        const data = this.taggedCborData();
        const compressed = Compressed.fromDecompressedData(data, this.digest());
        return Envelope.newWithCompressed(compressed);
    }

    decompress(): Envelope {
        const c = this.#envelopeCase;
        if (c.kind !== 'compressed') {
            throw EnvelopeError.notCompressed();
        }
        const data = c.compressed.decompress();
        return Envelope.fromTaggedCborData(data);
    }

    compressSubject(): Envelope {
        const sub = this.subject();
        if (sub.isElided()) {
            throw EnvelopeError.alreadyElided();
        }
        if (sub.isCompressed()) {
            throw EnvelopeError.alreadyCompressed();
        }
        const data = sub.taggedCborData();
        const compressed = Compressed.fromDecompressedData(data, sub.digest());
        const compressedEnvelope = Envelope.newWithCompressed(compressed);
        const assertions = this.assertions();
        if (assertions.length === 0) {
            return compressedEnvelope;
        }
        return Envelope.newWithUncheckedAssertions(compressedEnvelope, assertions);
    }

    decompressSubject(): Envelope {
        const sub = this.subject();
        const subCase = sub.case();
        if (subCase.kind !== 'compressed') {
            throw EnvelopeError.notCompressed();
        }
        const data = subCase.compressed.decompress();
        const decompressed = Envelope.fromTaggedCborData(data);
        if (!digestEquals(decompressed.digest(), sub.digest())) {
            throw EnvelopeError.invalidDigest();
        }
        const assertions = this.assertions();
        if (assertions.length === 0) {
            return decompressed;
        }
        return Envelope.newWithUncheckedAssertions(decompressed, assertions);
    }

    addSalt(): Envelope {
        const size = this.taggedCborData().length;
        const salt = Salt.newForSize(size);
        return this.addAssertion(SALT_KV, salt);
    }

    addSaltInstance(salt: Salt): Envelope {
        return this.addAssertion(SALT_KV, salt);
    }

    addSaltUsing(rng: RandomNumberGenerator): Envelope {
        const size = this.taggedCborData().length;
        const minLen = Math.max(16, Math.ceil(Math.log2(Math.max(1, size))) + 4);
        const salt = Salt.newWithLenUsing(minLen, rng);
        return this.addAssertion(SALT_KV, salt);
    }

    addSaltWithLength(count: number): Envelope {
        return this.addAssertion(SALT_KV, Salt.newWithLen(count));
    }

    addSaltInRange(range: [number, number]): Envelope {
        const [min, max] = range;
        const rng = new SecureRandomNumberGenerator();
        const span = Math.max(1, max - min + 1);
        const pick = min + (rng.nextU32() % span);
        return this.addAssertion(SALT_KV, Salt.newWithLen(pick));
    }

    setPosition(position: number): Envelope {
        const posAssertions = this.assertionsWithPredicate(POSITION);
        if (posAssertions.length > 1) {
            throw EnvelopeError.invalidFormat();
        }
        const base = posAssertions.length > 0 ? this.removeAssertion(posAssertions[0]!) : this;
        return base.addAssertion(POSITION, position);
    }

    position(): number {
        return this.extractObjectForPredicate(POSITION, (envelope) => Number(envelope.extractSubject()));
    }

    removePosition(): Envelope {
        const posAssertions = this.assertionsWithPredicate(POSITION);
        if (posAssertions.length > 1) {
            throw EnvelopeError.invalidFormat();
        }
        return posAssertions.length > 0 ? this.removeAssertion(posAssertions[0]!) : this;
    }

    formatOpt(flat = false, context: FormatContextOpt = FormatContextOpts.global()): string {
        return formatEnvelope(this, { flat, context });
    }

    format(): string {
        return this.formatOpt(false);
    }

    formatFlat(): string {
        return this.formatOpt(true);
    }

    treeFormat(): string {
        return treeFormat(this);
    }

    treeFormatOpt(opts: import('./envelope-tree.js').TreeFormatOpts): string {
        return treeFormatOpt(this, opts);
    }

    summary(maxLength: number, context: import('./format-context.js').FormatContext): string {
        return envelopeSummary(this, maxLength, context);
    }

    diagnostic(): string {
        return diagnostic(this);
    }

    diagnosticAnnotated(): string {
        return diagnosticAnnotated(this);
    }

    hex(): string {
        return envelopeHex(this);
    }

    mermaidFormat(): string {
        return mermaidFormat(this);
    }

    mermaidFormatOpt(opts: import('./envelope-mermaid.js').MermaidFormatOpts): string {
        return mermaidFormatOpt(this, opts);
    }

    equals(other: unknown): boolean {
        return other instanceof Envelope && this.isIdenticalTo(other);
    }

    toString(): string {
        return this.format();
    }
}
