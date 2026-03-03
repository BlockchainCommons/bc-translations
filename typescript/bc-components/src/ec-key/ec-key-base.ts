export interface ECKeyBase {
    data(): Uint8Array;
    hex(): string;
}

export interface ECKey extends ECKeyBase {
    publicKey(): import('./ec-public-key.js').ECPublicKey;
}
