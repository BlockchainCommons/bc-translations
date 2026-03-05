import {
    ARID,
    Digest,
    Nonce,
    PrivateKeyBase,
    type SigningOptions,
    SymmetricKey,
} from '@bc/components';
import {
    cbor,
    CborDate,
    hexToBytes,
} from '@bc/dcbor';
import {
    CONTROLLER,
    IS_A,
    ISSUER,
    NOTE,
} from '@bc/known-values';
import { createFakeRandomNumberGenerator } from '@bc/rand';

import { Envelope } from '../src/index.js';

export const PLAINTEXT_HELLO = 'Hello.';

export function checkEncoding(envelope: Envelope): Envelope {
    const restored = Envelope.fromTaggedCbor(envelope.taggedCbor());
    if (!restored.digest().equals(envelope.digest())) {
        throw new Error(`Digest mismatch after roundtrip: ${envelope.digest()} != ${restored.digest()}`);
    }
    return envelope;
}

export function helloEnvelope(): Envelope {
    return Envelope.from(PLAINTEXT_HELLO);
}

export function knownValueEnvelope(): Envelope {
    return Envelope.from(NOTE);
}

export function assertionEnvelope(): Envelope {
    return Envelope.newAssertion('knows', 'Bob');
}

export function singleAssertionEnvelope(): Envelope {
    return Envelope.from('Alice').addAssertion('knows', 'Bob');
}

export function doubleAssertionEnvelope(): Envelope {
    return singleAssertionEnvelope().addAssertion('knows', 'Carol');
}

export function wrappedEnvelope(): Envelope {
    return helloEnvelope().wrap();
}

export function doubleWrappedEnvelope(): Envelope {
    return wrappedEnvelope().wrap();
}

export function aliceSeed(): Uint8Array {
    return hexToBytes('82f32c855d3d542256180810797e0073');
}

export function alicePrivateKey(): PrivateKeyBase {
    return PrivateKeyBase.fromData(aliceSeed());
}

export function alicePublicKey() {
    return alicePrivateKey().publicKeys();
}

export function bobSeed(): Uint8Array {
    return hexToBytes('187a5973c64d359c836eba466a44db7b');
}

export function bobPrivateKey(): PrivateKeyBase {
    return PrivateKeyBase.fromData(bobSeed());
}

export function bobPublicKey() {
    return bobPrivateKey().publicKeys();
}

export function carolSeed(): Uint8Array {
    return hexToBytes('8574afab18e229651c1be8f76ffee523');
}

export function carolPrivateKey(): PrivateKeyBase {
    return PrivateKeyBase.fromData(carolSeed());
}

export function carolPublicKey() {
    return carolPrivateKey().publicKeys();
}

export function fakeContentKey(): SymmetricKey {
    return SymmetricKey.fromData(
        hexToBytes('526afd95b2229c5381baec4a1788507a3c4a566ca5cce64543b46ad12aff0035'),
    );
}

export function fakeNonce(): Nonce {
    return Nonce.fromData(hexToBytes('4d785658f36c22fb5aed3ac0'));
}

function addDigests(target: Set<Digest>, values: Iterable<Digest>): void {
    for (const digest of values) {
        target.add(digest);
    }
}

export function credential(): Envelope {
    const options: SigningOptions = {
        kind: 'schnorr',
        rng: createFakeRandomNumberGenerator(),
    };

    return Envelope
        .from(
            ARID.fromData(
                hexToBytes('4676635a6e6068c2ef3ffd8ff726dd401fd341036e920f136a1d8af5e829496d'),
            ),
        )
        .addAssertion(IS_A, 'Certificate of Completion')
        .addAssertion(ISSUER, 'Example Electrical Engineering Board')
        .addAssertion(CONTROLLER, 'Example Electrical Engineering Board')
        .addAssertion('firstName', 'James')
        .addAssertion('lastName', 'Maxwell')
        .addAssertion('issueDate', CborDate.fromString('2020-01-01'))
        .addAssertion('expirationDate', CborDate.fromString('2028-01-01'))
        .addAssertion('photo', "This is James Maxwell's photo.")
        .addAssertion('certificateNumber', '123-456-789')
        .addAssertion('subject', 'RF and Microwave Engineering')
        .addAssertion('continuingEducationUnits', 1)
        .addAssertion('professionalDevelopmentHours', 15)
        .addAssertion('topics', cbor(['Subject 1', 'Subject 2']))
        .wrap()
        .addSignatureOpt(alicePrivateKey(), options)
        .addAssertion(NOTE, 'Signed by Example Electrical Engineering Board');
}

export function redactedCredential(): Envelope {
    const cred = credential();
    const target = new Set<Digest>([cred.digest()]);

    for (const assertion of cred.assertions()) {
        addDigests(target, assertion.deepDigests());
    }

    target.add(cred.subject().digest());

    const content = cred.subject().unwrap();
    target.add(content.digest());
    target.add(content.subject().digest());

    addDigests(target, content.assertionWithPredicate('firstName').shallowDigests());
    addDigests(target, content.assertionWithPredicate('lastName').shallowDigests());
    addDigests(target, content.assertionWithPredicate(IS_A).shallowDigests());
    addDigests(target, content.assertionWithPredicate(ISSUER).shallowDigests());
    addDigests(target, content.assertionWithPredicate('subject').shallowDigests());
    addDigests(target, content.assertionWithPredicate('expirationDate').shallowDigests());

    return cred.elideRevealingSet(target);
}
