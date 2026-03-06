import { describe, test, expect } from 'vitest';
import { bytesToHex } from '@bc/dcbor';
import { sha256, extendKey, obfuscate } from '../src/index.js';

describe('crypto-utils', () => {
  test('sha256', () => {
    const data = new TextEncoder().encode('Hello World');
    const result = sha256(data);
    expect(bytesToHex(result)).toBe(
      'a591a6d40bf420404a011733cfb7b190d62c65bf0bcda32b57b277d9ad9f146e',
    );
  });

  test('extend_key', () => {
    const data = new TextEncoder().encode('Hello World');
    const result = extendKey(data);
    expect(bytesToHex(result)).toBe(
      '813085a508d5fec645abe5a1fb9a23c2a6ac6bef0a99650017b3ef50538dba39',
    );
  });

  test('obfuscate', () => {
    const key = new TextEncoder().encode('Hello');
    const message = new TextEncoder().encode('World');
    const obfuscated = obfuscate(key, message);
    expect(bytesToHex(obfuscated)).toBe('c43889aafa');

    const deobfuscated = obfuscate(key, obfuscated);
    expect(bytesToHex(deobfuscated)).toBe(bytesToHex(message));
  });
});
