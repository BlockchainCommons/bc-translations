import Foundation

/// A raw ChaCha20 stream cipher implementation (RFC 7539).
///
/// This is an internal type used by `CryptoUtils.obfuscate` for XOR-based
/// obfuscation. It implements the full 20-round ChaCha20 algorithm with a
/// 256-bit key, 96-bit nonce, and 32-bit block counter starting at 0.
struct ChaCha20: Sendable {
    /// The 16-word (64-byte) working state.
    private var state: [UInt32]

    /// The current keystream block (64 bytes).
    private var keystream: [UInt8]

    /// The byte position within the current keystream block.
    private var position: Int

    // MARK: - Constants

    /// The ChaCha20 constant "expand 32-byte k" as four little-endian UInt32s.
    private static let constants: [UInt32] = [
        0x61707865, // "expa"
        0x3320646e, // "nd 3"
        0x79622d32, // "2-by"
        0x6b206574, // "te k"
    ]

    // MARK: - Initialization

    /// Creates a new ChaCha20 cipher with the given key and nonce.
    ///
    /// - Parameters:
    ///   - key: A 32-byte (256-bit) key.
    ///   - nonce: A 12-byte (96-bit) nonce/IV.
    init(key: [UInt8], nonce: [UInt8]) {
        precondition(key.count == 32, "ChaCha20 key must be 32 bytes")
        precondition(nonce.count == 12, "ChaCha20 nonce must be 12 bytes")

        // Build the 16-word initial state:
        //   Words  0- 3: constant
        //   Words  4-11: key (8 words, little-endian)
        //   Word  12:    block counter (starts at 0)
        //   Words 13-15: nonce (3 words, little-endian)
        var s = [UInt32](repeating: 0, count: 16)

        // Constants
        s[0] = Self.constants[0]
        s[1] = Self.constants[1]
        s[2] = Self.constants[2]
        s[3] = Self.constants[3]

        // Key (8 little-endian UInt32s from 32 bytes)
        for i in 0..<8 {
            let offset = i * 4
            s[4 + i] = UInt32(key[offset])
                | (UInt32(key[offset + 1]) << 8)
                | (UInt32(key[offset + 2]) << 16)
                | (UInt32(key[offset + 3]) << 24)
        }

        // Counter
        s[12] = 0

        // Nonce (3 little-endian UInt32s from 12 bytes)
        for i in 0..<3 {
            let offset = i * 4
            s[13 + i] = UInt32(nonce[offset])
                | (UInt32(nonce[offset + 1]) << 8)
                | (UInt32(nonce[offset + 2]) << 16)
                | (UInt32(nonce[offset + 3]) << 24)
        }

        self.state = s
        self.keystream = [UInt8](repeating: 0, count: 64)
        self.position = 64 // Force generation of the first block on first use
    }

    // MARK: - Processing

    /// XORs the data in-place with the ChaCha20 keystream.
    ///
    /// This function can be called incrementally; the internal counter and
    /// position are maintained across calls.
    mutating func process(_ data: inout [UInt8]) {
        for i in 0..<data.count {
            if position >= 64 {
                generateBlock()
                position = 0
            }
            data[i] ^= keystream[position]
            position += 1
        }
    }

    // MARK: - Block generation

    /// Generates the next 64-byte keystream block and increments the counter.
    private mutating func generateBlock() {
        // Copy the state as the working state
        var working = state

        // 20 rounds = 10 iterations of (column rounds + diagonal rounds)
        for _ in 0..<10 {
            // Column rounds
            quarterRound(&working, 0, 4,  8, 12)
            quarterRound(&working, 1, 5,  9, 13)
            quarterRound(&working, 2, 6, 10, 14)
            quarterRound(&working, 3, 7, 11, 15)

            // Diagonal rounds
            quarterRound(&working, 0, 5, 10, 15)
            quarterRound(&working, 1, 6, 11, 12)
            quarterRound(&working, 2, 7,  8, 13)
            quarterRound(&working, 3, 4,  9, 14)
        }

        // Add the original state to the working state and serialize as
        // little-endian bytes to produce the keystream block.
        for i in 0..<16 {
            let value = working[i] &+ state[i]
            let offset = i * 4
            keystream[offset]     = UInt8(truncatingIfNeeded: value)
            keystream[offset + 1] = UInt8(truncatingIfNeeded: value >> 8)
            keystream[offset + 2] = UInt8(truncatingIfNeeded: value >> 16)
            keystream[offset + 3] = UInt8(truncatingIfNeeded: value >> 24)
        }

        // Increment the block counter (word 12)
        state[12] = state[12] &+ 1
    }

    // MARK: - Quarter round

    /// The ChaCha20 quarter-round function operating on four words of the state.
    private func quarterRound(_ s: inout [UInt32], _ a: Int, _ b: Int, _ c: Int, _ d: Int) {
        s[a] = s[a] &+ s[b]; s[d] ^= s[a]; s[d] = (s[d] << 16) | (s[d] >> 16)
        s[c] = s[c] &+ s[d]; s[b] ^= s[c]; s[b] = (s[b] << 12) | (s[b] >> 20)
        s[a] = s[a] &+ s[b]; s[d] ^= s[a]; s[d] = (s[d] <<  8) | (s[d] >> 24)
        s[c] = s[c] &+ s[d]; s[b] ^= s[c]; s[b] = (s[b] <<  7) | (s[b] >> 25)
    }
}
