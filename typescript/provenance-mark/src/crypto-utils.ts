/**
 * Cryptographic utilities: SHA-256, HKDF-HMAC-SHA256, ChaCha20 obfuscation.
 */

import { createHash, createHmac } from 'node:crypto';

export const SHA256_SIZE = 32;

export function sha256(data: Uint8Array): Uint8Array {
  const hash = createHash('sha256');
  hash.update(data);
  return new Uint8Array(hash.digest());
}

export function sha256Prefix(data: Uint8Array, prefix: number): Uint8Array {
  return sha256(data).slice(0, prefix);
}

export function extendKey(data: Uint8Array): Uint8Array {
  return new Uint8Array(hkdfHmacSha256(data, new Uint8Array(0), 32));
}

/**
 * Compute HKDF-HMAC-SHA-256 for the provided key material.
 */
export function hkdfHmacSha256(
  keyMaterial: Uint8Array,
  salt: Uint8Array,
  keyLen: number,
): Uint8Array {
  // HKDF extract
  const effectiveSalt = salt.length > 0 ? salt : new Uint8Array(32);
  const prk = createHmac('sha256', effectiveSalt).update(keyMaterial).digest();

  // HKDF expand with empty info
  const n = Math.ceil(keyLen / 32);
  const okm = new Uint8Array(n * 32);
  let prev: Uint8Array = new Uint8Array(0);
  for (let i = 1; i <= n; i++) {
    const hmac = createHmac('sha256', prk);
    hmac.update(prev);
    hmac.update(new Uint8Array([i]));
    prev = new Uint8Array(hmac.digest());
    okm.set(new Uint8Array(prev), (i - 1) * 32);
  }
  return okm.slice(0, keyLen);
}

export function obfuscate(key: Uint8Array, message: Uint8Array): Uint8Array {
  if (message.length === 0) {
    return new Uint8Array(0);
  }

  const extKey = extendKey(key);
  // IV is the last 12 bytes of the extended key, reversed
  const iv = new Uint8Array(12);
  for (let i = 0; i < 12; i++) {
    iv[i] = extKey[31 - i]!;
  }

  const buffer = new Uint8Array(message);
  chacha20Apply(extKey, iv, buffer);
  return buffer;
}

// ---------- ChaCha20 (RFC 7539) ----------

function chacha20Apply(key: Uint8Array, nonce: Uint8Array, data: Uint8Array): void {
  const state = new Uint32Array(16);
  // "expand 32-byte k"
  state[0] = 0x61707865;
  state[1] = 0x3320646e;
  state[2] = 0x79622d32;
  state[3] = 0x6b206574;

  for (let i = 0; i < 8; i++) {
    state[4 + i] = readU32LE(key, i * 4);
  }
  state[12] = 0; // counter
  for (let i = 0; i < 3; i++) {
    state[13 + i] = readU32LE(nonce, i * 4);
  }

  const keystream = new Uint8Array(64);
  let offset = 0;

  while (offset < data.length) {
    chacha20Block(state, keystream);
    state[12] = (state[12]! + 1) >>> 0;

    const remaining = data.length - offset;
    const count = Math.min(64, remaining);
    for (let i = 0; i < count; i++) {
      data[offset + i] = data[offset + i]! ^ keystream[i]!;
    }
    offset += count;
  }
}

function chacha20Block(state: Uint32Array, out: Uint8Array): void {
  const working = new Uint32Array(16);
  working.set(state);

  for (let i = 0; i < 10; i++) {
    quarterRound(working, 0, 4, 8, 12);
    quarterRound(working, 1, 5, 9, 13);
    quarterRound(working, 2, 6, 10, 14);
    quarterRound(working, 3, 7, 11, 15);
    quarterRound(working, 0, 5, 10, 15);
    quarterRound(working, 1, 6, 11, 12);
    quarterRound(working, 2, 7, 8, 13);
    quarterRound(working, 3, 4, 9, 14);
  }

  for (let i = 0; i < 16; i++) {
    working[i] = (working[i]! + state[i]!) >>> 0;
  }

  for (let i = 0; i < 16; i++) {
    writeU32LE(out, i * 4, working[i]!);
  }
}

function quarterRound(s: Uint32Array, a: number, b: number, c: number, d: number): void {
  s[a] = (s[a]! + s[b]!) >>> 0; s[d] = rotl32(s[d]! ^ s[a]!, 16);
  s[c] = (s[c]! + s[d]!) >>> 0; s[b] = rotl32(s[b]! ^ s[c]!, 12);
  s[a] = (s[a]! + s[b]!) >>> 0; s[d] = rotl32(s[d]! ^ s[a]!, 8);
  s[c] = (s[c]! + s[d]!) >>> 0; s[b] = rotl32(s[b]! ^ s[c]!, 7);
}

function rotl32(v: number, n: number): number {
  return ((v << n) | (v >>> (32 - n))) >>> 0;
}

function readU32LE(buf: Uint8Array, offset: number): number {
  return (
    (buf[offset]! |
      (buf[offset + 1]! << 8) |
      (buf[offset + 2]! << 16) |
      (buf[offset + 3]! << 24)) >>> 0
  );
}

function writeU32LE(buf: Uint8Array, offset: number, value: number): void {
  buf[offset] = value & 0xff;
  buf[offset + 1] = (value >>> 8) & 0xff;
  buf[offset + 2] = (value >>> 16) & 0xff;
  buf[offset + 3] = (value >>> 24) & 0xff;
}
