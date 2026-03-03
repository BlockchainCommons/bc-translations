import type { EncapsulationCiphertext } from './encapsulation/encapsulation-ciphertext.js';
import type { EncapsulationPrivateKey } from './encapsulation/encapsulation-private-key.js';
import type { EncapsulationPublicKey } from './encapsulation/encapsulation-public-key.js';
import type { SymmetricKey } from './symmetric/symmetric-key.js';

export interface Encrypter {
    encapsulationPublicKey(): EncapsulationPublicKey;
    encapsulateNewSharedSecret(): [SymmetricKey, EncapsulationCiphertext];
}

export interface Decrypter {
    encapsulationPrivateKey(): EncapsulationPrivateKey;
    decapsulateSharedSecret(ciphertext: EncapsulationCiphertext): SymmetricKey;
}
