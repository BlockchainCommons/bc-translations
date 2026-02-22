/**
 * Bytewords encoding and decoding with CRC32 checksums.
 *
 * Bytewords encodes byte data using a dictionary of 256 four-letter words.
 * Three encoding styles are supported:
 *   - STANDARD: words separated by spaces
 *   - URI: words separated by dashes
 *   - MINIMAL: first and last letter of each word, concatenated
 */

import { URError } from "./error.js";

/** Bytewords encoding style. */
export const Style = {
  STANDARD: "standard",
  URI: "uri",
  MINIMAL: "minimal",
} as const;
export type Style = (typeof Style)[keyof typeof Style];

// --- CRC32 (same table used in fountain.ts; duplicated here to keep bytewords self-contained) ---

const CRC32_TABLE = /* @__PURE__ */ (() => {
  const table = new Uint32Array(256);
  for (let i = 0; i < 256; i++) {
    let value = i;
    for (let bit = 0; bit < 8; bit++) {
      if ((value & 1) === 1) {
        value = (value >>> 1) ^ 0xedb8_8320;
      } else {
        value >>>= 1;
      }
    }
    table[i] = value >>> 0;
  }
  return table;
})();

const crc32 = (data: Uint8Array): number => {
  let crc = 0xffff_ffff;
  for (const byte of data) {
    crc = (CRC32_TABLE[(crc ^ byte) & 0xff] ?? 0) ^ (crc >>> 8);
  }
  return (crc ^ 0xffff_ffff) >>> 0;
};

// --- Reverse lookup tables (built once) ---

/** Map from full 4-letter word to byte index (0..255). */
const WORD_TO_INDEX: Map<string, number> = new Map();

/** Map from 2-char minimal form to byte index (0..255). */
const MINIMAL_TO_INDEX: Map<string, number> = new Map();

/** Minimal (2-char) form for each byte index. */
const MINIMALS: string[] = [];

const initLookups = (): void => {
  for (let i = 0; i < BYTEWORDS.length; i++) {
    const word = BYTEWORDS[i]!;
    WORD_TO_INDEX.set(word, i);
    const minimal = word[0]! + word[word.length - 1]!;
    MINIMAL_TO_INDEX.set(minimal, i);
    MINIMALS.push(minimal);
  }
};

// --- Encode / Decode ---

/**
 * Encodes bytes as bytewords text in the selected style.
 *
 * A CRC32 checksum is appended before encoding so that decoders can verify
 * data integrity.
 */
export const encode = (data: Uint8Array, style: Style): string => {
  const checksum = crc32(data);
  const withChecksum = new Uint8Array(data.length + 4);
  withChecksum.set(data);
  new DataView(withChecksum.buffer, withChecksum.byteOffset).setUint32(
    data.length,
    checksum,
    false, // big-endian
  );

  if (style === Style.MINIMAL) {
    let result = "";
    for (const byte of withChecksum) {
      result += MINIMALS[byte]!;
    }
    return result;
  }

  const separator = style === Style.STANDARD ? " " : "-";
  const words: string[] = [];
  for (const byte of withChecksum) {
    words.push(BYTEWORDS[byte]!);
  }
  return words.join(separator);
};

/**
 * Decodes bytewords text in the selected style back to bytes.
 *
 * Verifies the appended CRC32 checksum and throws on mismatch.
 */
export const decode = (encoded: string, style: Style): Uint8Array => {
  let bytes: Uint8Array;

  if (style === Style.MINIMAL) {
    bytes = decodeMinimal(encoded);
  } else {
    const separator = style === Style.STANDARD ? " " : "-";
    const parts = encoded.split(separator);
    bytes = decodeWords(parts, WORD_TO_INDEX);
  }

  return stripChecksum(bytes);
};

const decodeMinimal = (encoded: string): Uint8Array => {
  if (encoded.length % 2 !== 0) {
    throw URError.bytewords("invalid length");
  }

  const parts: string[] = [];
  for (let i = 0; i < encoded.length; i += 2) {
    parts.push(encoded.slice(i, i + 2));
  }
  return decodeWords(parts, MINIMAL_TO_INDEX);
};

const decodeWords = (words: string[], index: Map<string, number>): Uint8Array => {
  const result = new Uint8Array(words.length);
  for (let i = 0; i < words.length; i++) {
    const byteIndex = index.get(words[i]!);
    if (byteIndex === undefined) {
      throw URError.bytewords("invalid word");
    }
    result[i] = byteIndex;
  }
  return result;
};

const stripChecksum = (data: Uint8Array): Uint8Array => {
  if (data.length < 5) {
    // At least 1 byte of payload + 4 bytes of checksum
    throw URError.bytewords("invalid checksum");
  }

  const payload = data.slice(0, data.length - 4);
  const checksumBytes = data.slice(data.length - 4);
  const expected = crc32(payload);
  const actual = new DataView(
    checksumBytes.buffer,
    checksumBytes.byteOffset,
  ).getUint32(0, false);

  if (actual !== expected) {
    throw URError.bytewords("invalid checksum");
  }

  return payload;
};

// --- Identifier helpers ---

const lookupWord = (table: readonly string[], value: number): string => {
  if (!Number.isInteger(value) || value < 0 || value > 255) {
    throw URError.bytewords(`invalid byte value ${value}`);
  }
  const word = table[value];
  if (word === undefined) {
    throw URError.bytewords(`missing lookup value for byte ${value}`);
  }
  return word;
};

/**
 * Encodes a 4-byte slice as space-separated bytewords for identification.
 */
export const identifier = (data: Uint8Array): string => {
  if (data.length !== 4) {
    throw URError.bytewords("identifier input must be exactly 4 bytes");
  }

  return Array.from(data, (value) => lookupWord(BYTEWORDS, value)).join(" ");
};

/**
 * Encodes a 4-byte slice as space-separated bytemojis for identification.
 */
export const bytemojiIdentifier = (data: Uint8Array): string => {
  if (data.length !== 4) {
    throw URError.bytewords("bytemoji identifier input must be exactly 4 bytes");
  }

  return Array.from(data, (value) => lookupWord(BYTEMOJIS, value)).join(" ");
};

// --- Word tables ---

export const BYTEWORDS = [
  "able", "acid", "also", "apex", "aqua", "arch", "atom", "aunt", "away",
  "axis", "back", "bald", "barn", "belt", "beta", "bias", "blue", "body",
  "brag", "brew", "bulb", "buzz", "calm", "cash", "cats", "chef", "city",
  "claw", "code", "cola", "cook", "cost", "crux", "curl", "cusp", "cyan",
  "dark", "data", "days", "deli", "dice", "diet", "door", "down", "draw",
  "drop", "drum", "dull", "duty", "each", "easy", "echo", "edge", "epic",
  "even", "exam", "exit", "eyes", "fact", "fair", "fern", "figs", "film",
  "fish", "fizz", "flap", "flew", "flux", "foxy", "free", "frog", "fuel",
  "fund", "gala", "game", "gear", "gems", "gift", "girl", "glow", "good",
  "gray", "grim", "guru", "gush", "gyro", "half", "hang", "hard", "hawk",
  "heat", "help", "high", "hill", "holy", "hope", "horn", "huts", "iced",
  "idea", "idle", "inch", "inky", "into", "iris", "iron", "item", "jade",
  "jazz", "join", "jolt", "jowl", "judo", "jugs", "jump", "junk", "jury",
  "keep", "keno", "kept", "keys", "kick", "kiln", "king", "kite", "kiwi",
  "knob", "lamb", "lava", "lazy", "leaf", "legs", "liar", "limp", "lion",
  "list", "logo", "loud", "love", "luau", "luck", "lung", "main", "many",
  "math", "maze", "memo", "menu", "meow", "mild", "mint", "miss", "monk",
  "nail", "navy", "need", "news", "next", "noon", "note", "numb", "obey",
  "oboe", "omit", "onyx", "open", "oval", "owls", "paid", "part", "peck",
  "play", "plus", "poem", "pool", "pose", "puff", "puma", "purr", "quad",
  "quiz", "race", "ramp", "real", "redo", "rich", "road", "rock", "roof",
  "ruby", "ruin", "runs", "rust", "safe", "saga", "scar", "sets", "silk",
  "skew", "slot", "soap", "solo", "song", "stub", "surf", "swan", "taco",
  "task", "taxi", "tent", "tied", "time", "tiny", "toil", "tomb", "toys",
  "trip", "tuna", "twin", "ugly", "undo", "unit", "urge", "user", "vast",
  "very", "veto", "vial", "vibe", "view", "visa", "void", "vows", "wall",
  "wand", "warm", "wasp", "wave", "waxy", "webs", "what", "when", "whiz",
  "wolf", "work", "yank", "yawn", "yell", "yoga", "yurt", "zaps", "zero",
  "zest", "zinc", "zone", "zoom",
] as const;

/**
 * See: https://github.com/BlockchainCommons/Research/blob/master/papers/bcr-2024-008-bytemoji.md
 */
export const BYTEMOJIS = [
  "\u{1f600}", "\u{1f602}", "\u{1f606}", "\u{1f609}", "\u{1f644}", "\u{1f60b}", "\u{1f60e}", "\u{1f60d}", "\u{1f618}", "\u{1f62d}", "\u{1fae0}", "\u{1f971}",
  "\u{1f929}", "\u{1f636}", "\u{1f928}", "\u{1fae5}", "\u{1f975}", "\u{1f976}", "\u{1f633}", "\u{1f92a}", "\u{1f635}", "\u{1f621}", "\u{1f922}", "\u{1f607}",
  "\u{1f920}", "\u{1f921}", "\u{1f973}", "\u{1f97a}", "\u{1f62c}", "\u{1f911}", "\u{1f643}", "\u{1f92f}", "\u{1f608}", "\u{1f479}", "\u{1f47a}", "\u{1f480}",
  "\u{1f47b}", "\u{1f47d}", "\u{1f63a}", "\u{1f639}", "\u{1f63b}", "\u{1f63d}", "\u{1f640}", "\u{1f63f}", "\u{1faf6}", "\u{1f932}", "\u{1f64c}", "\u{1f91d}",
  "\u{1f44d}", "\u{1f44e}", "\u{1f448}", "\u{1f446}", "\u{1f4aa}", "\u{1f444}", "\u{1f9b7}", "\u{1f442}", "\u{1f443}", "\u{1f9e0}", "\u{1f440}", "\u{1f91a}",
  "\u{1f9b6}", "\u{1f34e}", "\u{1f34a}", "\u{1f34b}", "\u{1f34c}", "\u{1f349}", "\u{1f347}", "\u{1f353}", "\u{1fad0}", "\u{1f352}", "\u{1f351}", "\u{1f34d}",
  "\u{1f95d}", "\u{1f346}", "\u{1f951}", "\u{1f966}", "\u{1f345}", "\u{1f33d}", "\u{1f955}", "\u{1fad2}", "\u{1f9c4}", "\u{1f950}", "\u{1f96f}", "\u{1f35e}",
  "\u{1f9c0}", "\u{1f95a}", "\u{1f357}", "\u{1f32d}", "\u{1f354}", "\u{1f35f}", "\u{1f355}", "\u{1f32e}", "\u{1f959}", "\u{1f371}", "\u{1f35c}", "\u{1f364}",
  "\u{1f35a}", "\u{1f960}", "\u{1f368}", "\u{1f366}", "\u{1f382}", "\u{1fab4}", "\u{1f335}", "\u{1f331}", "\u{1f490}", "\u{1f341}", "\u{1f344}", "\u{1f339}",
  "\u{1f33a}", "\u{1f33c}", "\u{1f33b}", "\u{1f338}", "\u{1f4a8}", "\u{1f30a}", "\u{1f4a7}", "\u{1f4a6}", "\u{1f300}", "\u{1f308}", "\u{1f31e}", "\u{1f31d}",
  "\u{1f31b}", "\u{1f31c}", "\u{1f319}", "\u{1f30e}", "\u{1f4ab}", "\u2b50", "\u{1fa90}", "\u{1f310}", "\u{1f49b}", "\u{1f494}", "\u{1f498}", "\u{1f496}",
  "\u{1f495}", "\u{1f3c1}", "\u{1f6a9}", "\u{1f4ac}", "\u{1f4af}", "\u{1f6ab}", "\u{1f534}", "\u{1f537}", "\u{1f7e9}", "\u{1f6d1}", "\u{1f53a}", "\u{1f697}",
  "\u{1f691}", "\u{1f692}", "\u{1f69c}", "\u{1f6f5}", "\u{1f6a8}", "\u{1f680}", "\u{1f681}", "\u{1f6df}", "\u{1f6a6}", "\u{1f3f0}", "\u{1f3a1}", "\u{1f3a2}",
  "\u{1f3a0}", "\u{1f3e0}", "\u{1f514}", "\u{1f511}", "\u{1f6aa}", "\u{1fa91}", "\u{1f388}", "\u{1f48c}", "\u{1f4e6}", "\u{1f4eb}", "\u{1f4d6}", "\u{1f4da}",
  "\u{1f4cc}", "\u{1f9ee}", "\u{1f512}", "\u{1f48e}", "\u{1f4f7}", "\u23f0", "\u231b", "\u{1f4e1}", "\u{1f4a1}", "\u{1f4b0}", "\u{1f9f2}", "\u{1f9f8}",
  "\u{1f381}", "\u{1f380}", "\u{1f389}", "\u{1faad}", "\u{1f451}", "\u{1fad6}", "\u{1f52d}", "\u{1f6c1}", "\u{1f3c6}", "\u{1f941}", "\u{1f3b7}", "\u{1f3ba}",
  "\u{1f3c0}", "\u{1f3c8}", "\u{1f3be}", "\u{1f3d3}", "\u2728", "\u{1f525}", "\u{1f4a5}", "\u{1f455}", "\u{1f45a}", "\u{1f456}", "\u{1fa73}", "\u{1f457}",
  "\u{1f454}", "\u{1f9e2}", "\u{1f453}", "\u{1f9f6}", "\u{1f9f5}", "\u{1f48d}", "\u{1f460}", "\u{1f45f}", "\u{1f9e6}", "\u{1f9e4}", "\u{1f452}", "\u{1f45c}",
  "\u{1f431}", "\u{1f436}", "\u{1f42d}", "\u{1f439}", "\u{1f430}", "\u{1f98a}", "\u{1f43b}", "\u{1f43c}", "\u{1f428}", "\u{1f42f}", "\u{1f981}", "\u{1f42e}",
  "\u{1f437}", "\u{1f438}", "\u{1f435}", "\u{1f414}", "\u{1f425}", "\u{1f986}", "\u{1f989}", "\u{1f434}", "\u{1f984}", "\u{1f41d}", "\u{1f41b}", "\u{1f98b}",
  "\u{1f40c}", "\u{1f41e}", "\u{1f422}", "\u{1f43a}", "\u{1f40d}", "\u{1fabd}", "\u{1f419}", "\u{1f991}", "\u{1fabc}", "\u{1f99e}", "\u{1f980}", "\u{1f41a}",
  "\u{1f9ad}", "\u{1f41f}", "\u{1f42c}", "\u{1f433}",
] as const;

// Build lookup tables at module load time.
initLookups();
