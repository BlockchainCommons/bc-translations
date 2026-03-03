import { describe, expect, test } from 'vitest';
import { createFakeRandomNumberGenerator } from '@bc/rand';

import {
    ECPrivateKey,
    SigningPrivateKey,
    SigningPublicKey,
    X25519PrivateKey,
    X25519PublicKey,
    registerTags,
} from '../src/index.js';
import { utf8 } from './test-helpers.js';

describe('lib vectors', () => {
    test('x25519 UR vectors match Rust', () => {
        registerTags();
        const rng = createFakeRandomNumberGenerator();

        const privateKey = X25519PrivateKey.newUsing(rng);
        const privateKeyUr = privateKey.urString();
        expect(privateKeyUr).toBe(
            'ur:agreement-private-key/hdcxkbrehkrkrsjztodseytknecfgewmgdmwfsvdvysbpmghuozsprknfwkpnehydlweynwkrtct',
        );
        expect(X25519PrivateKey.fromURString(privateKeyUr).equals(privateKey)).toBe(true);

        const publicKey = privateKey.publicKey();
        const publicKeyUr = publicKey.urString();
        expect(publicKeyUr).toBe(
            'ur:agreement-public-key/hdcxwnryknkbbymnoxhswmptgydsotwswsghfmrkksfxntbzjyrnuornkildchgswtdahehpwkrl',
        );
        expect(X25519PublicKey.fromURString(publicKeyUr).equals(publicKey)).toBe(true);

        const derivedPrivate = X25519PrivateKey.deriveFromKeyMaterial(utf8('password'));
        expect(derivedPrivate.urString()).toBe(
            'ur:agreement-private-key/hdcxkgcfkomeeyiemywkftvabnrdolmttlrnfhjnguvaiehlrldmdpemgyjlatdthsnecytdoxat',
        );
    });

    test('x25519 shared key agreement', () => {
        const rng = createFakeRandomNumberGenerator();

        const alicePrivate = X25519PrivateKey.newUsing(rng);
        const alicePublic = alicePrivate.publicKey();

        const bobPrivate = X25519PrivateKey.newUsing(rng);
        const bobPublic = bobPrivate.publicKey();

        const aliceShared = alicePrivate.sharedKeyWith(bobPublic);
        const bobShared = bobPrivate.sharedKeyWith(alicePublic);
        expect(aliceShared.equals(bobShared)).toBe(true);
    });

    test('signing key UR vectors match Rust', () => {
        registerTags();
        const rng = createFakeRandomNumberGenerator();

        const schnorrPrivate = SigningPrivateKey.newSchnorr(ECPrivateKey.newUsing(rng));
        const schnorrPrivateUr = schnorrPrivate.urString();
        expect(schnorrPrivateUr).toBe(
            'ur:signing-private-key/hdcxkbrehkrkrsjztodseytknecfgewmgdmwfsvdvysbpmghuozsprknfwkpnehydlweynwkrtct',
        );
        expect(SigningPrivateKey.fromURString(schnorrPrivateUr).equals(schnorrPrivate)).toBe(true);

        const ecdsaPrivate = SigningPrivateKey.newEcdsa(ECPrivateKey.newUsing(rng));
        const ecdsaPublic = ecdsaPrivate.publicKey();
        const ecdsaPublicUr = ecdsaPublic.urString();
        expect(ecdsaPublicUr).toBe(
            'ur:signing-public-key/lfadhdclaxbzutckgevlpkmdfnuoemlnvsgllokicfdekesswnfdtibkylrskomwgubaahyntaktbksbdt',
        );
        expect(SigningPublicKey.fromURString(ecdsaPublicUr).equals(ecdsaPublic)).toBe(true);

        const schnorrPublic = schnorrPrivate.publicKey();
        const schnorrPublicUr = schnorrPublic.urString();
        expect(schnorrPublicUr).toBe(
            'ur:signing-public-key/hdcxjsrhdnidbgosndmobzwntdglzonnidmwoyrnuomdrpsptkcskerhfljssgaoidjewyjymhcp',
        );
        expect(SigningPublicKey.fromURString(schnorrPublicUr).equals(schnorrPublic)).toBe(true);

        const derivedPrivate = SigningPrivateKey.newSchnorr(
            ECPrivateKey.deriveFromKeyMaterial(utf8('password')),
        );
        expect(derivedPrivate.urString()).toBe(
            'ur:signing-private-key/hdcxahsfgobtpkkpahmnhsfmhnjnmkmkzeuraonneshkbysseyjkoeayrlvtvsmndicwkkvattfs',
        );
    });
});
