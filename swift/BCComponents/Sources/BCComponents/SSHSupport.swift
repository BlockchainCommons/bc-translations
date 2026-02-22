import BCCrypto
import CryptoSwift
import Foundation

public enum SSHAlgorithm: Equatable, Hashable, Sendable {
    case dsa
    case ed25519
    case ecdsaP256
    case ecdsaP384

    var signatureScheme: SignatureScheme {
        switch self {
        case .dsa:
            return .sshDsa
        case .ed25519:
            return .sshEd25519
        case .ecdsaP256:
            return .sshEcdsaP256
        case .ecdsaP384:
            return .sshEcdsaP384
        }
    }

    var opensshPublicKeyType: String {
        switch self {
        case .dsa:
            return "ssh-dss"
        case .ed25519:
            return "ssh-ed25519"
        case .ecdsaP256:
            return "ecdsa-sha2-nistp256"
        case .ecdsaP384:
            return "ecdsa-sha2-nistp384"
        }
    }

    static func fromOpenSSHPublicKey(_ value: String) throws(BCComponentsError) -> SSHAlgorithm {
        let parts = value.split(separator: " ")
        guard let keyType = parts.first else {
            throw BCComponentsError.ssh("invalid SSH public key")
        }
        switch keyType {
        case "ssh-dss":
            return .dsa
        case "ssh-ed25519":
            return .ed25519
        case "ecdsa-sha2-nistp256":
            return .ecdsaP256
        case "ecdsa-sha2-nistp384":
            return .ecdsaP384
        default:
            throw BCComponentsError.ssh("unsupported SSH public key algorithm: \(keyType)")
        }
    }
}

public enum SSHHashAlgorithm: Equatable, Hashable, Sendable {
    case sha256
    case sha512

    var opensshName: String {
        switch self {
        case .sha256:
            return "sha256"
        case .sha512:
            return "sha512"
        }
    }

    static func fromOpenSSHName(_ value: String) throws(BCComponentsError) -> SSHHashAlgorithm {
        switch value {
        case "sha256":
            return .sha256
        case "sha512":
            return .sha512
        default:
            throw BCComponentsError.ssh("unsupported SSH hash algorithm: \(value)")
        }
    }
}

private typealias BigUInt = CS.BigUInt

private struct DSAComponents {
    let p: BigUInt
    let q: BigUInt
    let g: BigUInt
    let y: BigUInt
    let x: BigUInt
}

// Rust parity baseline from `bc-components` `test_ssh_dsa_signing`.
private let rustDSASeed = dataFromHexString("59f2293a5bce7d4de59e71b4207ac5d2")
private let rustDSAP = dataFromHexString(
    "961b87fbafc140313fc82f5d70f4e7ccfd976e210f2f12ad546feb8c772252f304bf8d72f9381dbf74f9d708d4ad2f8be34aaa43cccb3e0ed779c09cba6da490df31846d8bfbd6690844907f02d49599df0565ee401232c5bc7c5831294ed23a62d58ff59cb33bbcb42ee100720c22e16188cfc60ad929707770bc6d8ef972e7"
)
private let rustDSAQ = dataFromHexString("e1f2192e2be70d5720f4f5f3c1c6f99b4ec7f64d")
private let rustDSAG = dataFromHexString(
    "92b9566eadd5283b9fe0badf0acd80303a2d33721b3c6a455336ba7235ea38ac08ecee0bdf0382cf886c090b853a96498fa723e53d1c11dceecafe28a7560b347be8d2cae5e4a2957bedb7c83e2df1c85ef0a30f8436a6ea30b630e85d211000961d73582d79fdb5299d9ea0aa32030cd021e8426ef2e9c186f19e16a9e6eb15"
)
private let rustDSAY = dataFromHexString(
    "60abb1b7d179a0d9a350b0e3f7459495810cfb0c19e1b4e3fda109384d994fb32e2282c2c120d17ea89ce4cc4e89c25ef7b77d56f994d8c853dda24e6b7942498e8f6863b1b828abfd2c6b49402c7a7b51c2bf5b821d53801bb5ce26d1251f96025c76cd67b32dab294344b407d16bf5b0b4345eb1639ae13ecbfb714451d6af"
)
private let rustDSAX = dataFromHexString("d5a58dfbb141c989fd3ef04a60145e65e8e99234")

private func dataFromHexString(_ hex: String) -> Data {
    try! parseHex(hex)
}

public struct SSHPublicKey: Equatable, Hashable, Sendable {
    public let openssh: String
    public let algorithm: SSHAlgorithm

    public init(openssh: String) throws(BCComponentsError) {
        let normalized = normalizeTrailingNewline(openssh)
        self.openssh = normalized
        self.algorithm = try SSHAlgorithm.fromOpenSSHPublicKey(normalized)
    }
}

public struct SSHPrivateKey: Equatable, Hashable, Sendable {
    public let openssh: String
    public let publicKey: SSHPublicKey

    public init(openssh: String) throws(BCComponentsError) {
        let normalized = normalizeTrailingNewline(openssh)
        let derivedPublic: SSHPublicKey
        if isOpenSSHDsaPrivateKey(normalized) {
            let parsed = try parseOpenSSHDsaPrivateKey(normalized)
            derivedPublic = try SSHPublicKey(openssh: parsed.publicOpenSSH)
        } else {
            derivedPublic = try deriveSSHPublicKeyFromPrivate(normalized)
        }
        self.openssh = normalized
        self.publicKey = derivedPublic
    }

    private init(normalizedOpenSSH: String, publicKey: SSHPublicKey) {
        self.openssh = normalizedOpenSSH
        self.publicKey = publicKey
    }

    public var algorithm: SSHAlgorithm {
        publicKey.algorithm
    }

    static func generate(
        algorithm: SSHAlgorithm,
        comment: String
    ) throws(BCComponentsError) -> SSHPrivateKey {
        #if os(macOS)
        do {
            let temp = URL(fileURLWithPath: NSTemporaryDirectory(), isDirectory: true)
                .appendingPathComponent(Foundation.UUID().uuidString, isDirectory: true)
            try FileManager.default.createDirectory(at: temp, withIntermediateDirectories: true)
            defer { try? FileManager.default.removeItem(at: temp) }

            let keyFile = temp.appendingPathComponent("ssh_key")

            var args = ["-q", "-N", "", "-C", comment, "-f", keyFile.path]
            switch algorithm {
            case .dsa:
                args.append(contentsOf: ["-t", "dsa"])
            case .ed25519:
                args.append(contentsOf: ["-t", "ed25519"])
            case .ecdsaP256:
                args.append(contentsOf: ["-t", "ecdsa", "-b", "256"])
            case .ecdsaP384:
                args.append(contentsOf: ["-t", "ecdsa", "-b", "384"])
            }

            _ = try runSSHKeygen(arguments: args)

            let privateString = try String(contentsOf: keyFile, encoding: .utf8)
            return try SSHPrivateKey(openssh: privateString)
        } catch let error as BCComponentsError {
            throw error
        } catch {
            throw BCComponentsError.ssh(error.localizedDescription)
        }
        #else
        throw BCComponentsError.ssh("SSH key generation requires macOS")
        #endif
    }

    static func generateDeterministicEd25519(
        keyMaterial: Data,
        comment: String
    ) throws(BCComponentsError) -> SSHPrivateKey {
        let privateSeed = hkdfHmacSHA256(
            keyMaterial: keyMaterial,
            salt: Data("ssh-ed25519-0".utf8),
            keyLength: ed25519PrivateKeySize
        )
        let publicKeyBytes = ed25519PublicKeyFromPrivateKey(privateSeed)

        let encoded = encodeOpenSSHEd25519Keypair(
            privateSeed: privateSeed,
            publicKey: publicKeyBytes,
            comment: comment
        )
        let publicKey = try SSHPublicKey(openssh: encoded.publicKey)
        return SSHPrivateKey(normalizedOpenSSH: encoded.privateKey, publicKey: publicKey)
    }

    static func generateDeterministicDsa(
        keyMaterial: Data,
        comment: String
    ) -> SSHPrivateKey {
        let components = deterministicDSAComponents(keyMaterial: keyMaterial)
        let encoded = encodeOpenSSHDsaKeypair(components: components, comment: comment)
        let publicKey = try! SSHPublicKey(openssh: encoded.publicKey)
        return SSHPrivateKey(normalizedOpenSSH: encoded.privateKey, publicKey: publicKey)
    }
}

public struct SSHSignature: Equatable, Hashable, Sendable {
    public let pem: String
    public let algorithm: SSHAlgorithm
    public let namespace: String
    public let hashAlgorithm: SSHHashAlgorithm

    public init(pem: String) throws(BCComponentsError) {
        let normalized = normalizeTrailingNewline(pem)
        let parsed = try parseSSHSigPEM(normalized)
        self.pem = normalized
        self.algorithm = parsed.algorithm
        self.namespace = parsed.namespace
        self.hashAlgorithm = parsed.hashAlgorithm
    }
}

extension SSHPublicKey: ReferenceProvider {
    public func reference() -> Reference {
        try! Reference.fromData(Data(openssh.utf8))
    }
}

extension SSHPrivateKey: ReferenceProvider {
    public func reference() -> Reference {
        try! Reference.fromData(Data(openssh.utf8))
    }
}

func signSSH(
    privateKey: SSHPrivateKey,
    namespace: String,
    hashAlgorithm: SSHHashAlgorithm,
    message: Data
) throws(BCComponentsError) -> SSHSignature {
    if privateKey.algorithm == .dsa {
        return try signSSHDsa(
            privateKey: privateKey,
            namespace: namespace,
            hashAlgorithm: hashAlgorithm,
            message: message
        )
    }

    #if os(macOS)
    do {
        let temp = URL(fileURLWithPath: NSTemporaryDirectory(), isDirectory: true)
            .appendingPathComponent(Foundation.UUID().uuidString, isDirectory: true)
        try FileManager.default.createDirectory(at: temp, withIntermediateDirectories: true)
        defer { try? FileManager.default.removeItem(at: temp) }

        let keyFile = temp.appendingPathComponent("signing_key")
        let messageFile = temp.appendingPathComponent("message.bin")
        let signatureFile = temp.appendingPathComponent("message.bin.sig")

        let keyForFile = ensureTrailingNewline(privateKey.openssh)
        guard let keyData = keyForFile.data(using: .utf8) else {
            throw BCComponentsError.ssh("invalid SSH private key encoding")
        }
        try keyData.write(to: keyFile)
        try FileManager.default.setAttributes([.posixPermissions: 0o600], ofItemAtPath: keyFile.path)
        try message.write(to: messageFile)

        _ = try runSSHKeygen(arguments: [
            "-Y", "sign",
            "-f", keyFile.path,
            "-n", namespace,
            "-q",
            "-O", "hashalg=\(hashAlgorithm.opensshName)",
            messageFile.path,
        ])

        let signatureText = try String(contentsOf: signatureFile, encoding: .utf8)
        return try SSHSignature(pem: signatureText)
    } catch let error as BCComponentsError {
        throw error
    } catch {
        throw BCComponentsError.ssh(error.localizedDescription)
    }
    #else
    throw BCComponentsError.ssh("SSH signing requires macOS")
    #endif
}

func verifySSH(
    publicKey: SSHPublicKey,
    signature: SSHSignature,
    message: Data
) -> Bool {
    if publicKey.algorithm == .dsa || signature.algorithm == .dsa {
        return verifySSHDsa(
            publicKey: publicKey,
            signature: signature,
            message: message
        )
    }

    #if os(macOS)
    do {
        let temp = URL(fileURLWithPath: NSTemporaryDirectory(), isDirectory: true)
            .appendingPathComponent(Foundation.UUID().uuidString, isDirectory: true)
        try FileManager.default.createDirectory(at: temp, withIntermediateDirectories: true)
        defer { try? FileManager.default.removeItem(at: temp) }

        let signatureFile = temp.appendingPathComponent("signature.pem")
        let allowedSignersFile = temp.appendingPathComponent("allowed_signers")

        guard let signatureData = signature.pem.data(using: .utf8) else {
            throw BCComponentsError.ssh("invalid SSH signature encoding")
        }
        try signatureData.write(to: signatureFile)

        let allowedSigner = "signer \(publicKey.openssh)\n"
        guard let allowedData = allowedSigner.data(using: .utf8) else {
            throw BCComponentsError.ssh("invalid SSH public key encoding")
        }
        try allowedData.write(to: allowedSignersFile)

        _ = try runSSHKeygen(
            arguments: [
                "-Y", "verify",
                "-f", allowedSignersFile.path,
                "-I", "signer",
                "-n", signature.namespace,
                "-s", signatureFile.path,
            ],
            stdin: message
        )
        return true
    } catch {
        return false
    }
    #else
    return false
    #endif
}

private struct ParsedSSHSig {
    let algorithm: SSHAlgorithm
    let namespace: String
    let hashAlgorithm: SSHHashAlgorithm
}

private struct ParsedSSHSigPayload {
    let algorithm: SSHAlgorithm
    let namespace: String
    let hashAlgorithm: SSHHashAlgorithm
    let reserved: Data
    let publicKeyBlob: Data
    let signatureData: Data
}

private struct ParsedOpenSSHDsaPublicKey {
    let components: DSAComponents
    let comment: String
    let publicBlob: Data
}

private struct ParsedOpenSSHDsaPrivateKey {
    let components: DSAComponents
    let comment: String
    let publicBlob: Data
    let publicOpenSSH: String
}

private struct SSHDataCursor {
    let data: Data
    private(set) var position: Int = 0

    init(_ data: Data) {
        self.data = data
    }

    var isFinished: Bool {
        position == data.count
    }

    var remainingCount: Int {
        data.count - position
    }

    mutating func readBytes(_ length: Int) throws(BCComponentsError) -> Data {
        guard length >= 0, position + length <= data.count else {
            throw BCComponentsError.ssh("invalid SSH binary payload")
        }
        let value = data[position..<(position + length)]
        position += length
        return Data(value)
    }

    mutating func readUInt32() throws(BCComponentsError) -> UInt32 {
        let bytes = try readBytes(4)
        return bytes.reduce(UInt32(0)) { ($0 << 8) | UInt32($1) }
    }

    mutating func readSSHString() throws(BCComponentsError) -> Data {
        let length = Int(try readUInt32())
        return try readBytes(length)
    }

    mutating func readUTF8String() throws(BCComponentsError) -> String {
        let bytes = try readSSHString()
        guard let value = String(data: bytes, encoding: .utf8) else {
            throw BCComponentsError.ssh("invalid SSH string encoding")
        }
        return value
    }

    mutating func readMPInt() throws(BCComponentsError) -> BigUInt {
        let encoded = try readSSHString()
        return try bigUIntFromMPIntData(encoded)
    }

    mutating func readRemaining() -> Data {
        guard position < data.count else {
            return Data()
        }
        let value = Data(data[position...])
        position = data.count
        return value
    }
}

private func bigUIntFromMPIntData(_ data: Data) throws(BCComponentsError) -> BigUInt {
    if data.isEmpty {
        return BigUInt(0)
    }
    if data[0] >= 0x80 {
        throw BCComponentsError.ssh("negative mpint values are unsupported")
    }
    if data.count > 1, data[0] == 0x00, data[1] < 0x80 {
        throw BCComponentsError.ssh("invalid mpint encoding")
    }
    if data[0] == 0x00 {
        return BigUInt(Data(data.dropFirst()))
    }
    return BigUInt(data)
}

private func mpintData(from value: BigUInt) -> Data {
    if value == 0 {
        return Data()
    }
    var data = value.serialize()
    while data.count > 1, data.first == 0 {
        data.removeFirst()
    }
    if let first = data.first, first >= 0x80 {
        data.insert(0x00, at: 0)
    }
    return data
}

private func appendSSHMPInt(_ value: BigUInt, to data: inout Data) {
    appendSSHString(mpintData(from: value), to: &data)
}

private func buildOpenSSHDsaPublicBlob(components: DSAComponents) -> Data {
    var blob = Data()
    appendSSHString(Data("ssh-dss".utf8), to: &blob)
    appendSSHMPInt(components.p, to: &blob)
    appendSSHMPInt(components.q, to: &blob)
    appendSSHMPInt(components.g, to: &blob)
    appendSSHMPInt(components.y, to: &blob)
    return blob
}

private func formatOpenSSHDsaPublicKey(publicBlob: Data, comment: String) -> String {
    let base = "ssh-dss \(publicBlob.base64EncodedString())"
    if comment.isEmpty {
        return base
    }
    return "\(base) \(comment)"
}

private func isOpenSSHDsaPrivateKey(_ openssh: String) -> Bool {
    (try? parseOpenSSHDsaPrivateKey(openssh)) != nil
}

private func parseOpenSSHDsaPublicKey(
    _ openssh: String
) throws(BCComponentsError) -> ParsedOpenSSHDsaPublicKey {
    let normalized = normalizeTrailingNewline(openssh)
    let parts = normalized.split(separator: " ", maxSplits: 2, omittingEmptySubsequences: true)
    guard parts.count >= 2 else {
        throw BCComponentsError.ssh("invalid SSH public key")
    }
    guard parts[0] == "ssh-dss" else {
        throw BCComponentsError.ssh("unsupported SSH public key algorithm")
    }
    guard let publicBlob = Data(base64Encoded: String(parts[1])) else {
        throw BCComponentsError.ssh("invalid SSH public key payload")
    }
    let comment = parts.count == 3 ? String(parts[2]) : ""

    var cursor = SSHDataCursor(publicBlob)
    let keyType = try cursor.readUTF8String()
    guard keyType == "ssh-dss" else {
        throw BCComponentsError.ssh("invalid SSH DSA public key type")
    }

    let p = try cursor.readMPInt()
    let q = try cursor.readMPInt()
    let g = try cursor.readMPInt()
    let y = try cursor.readMPInt()

    guard cursor.isFinished else {
        throw BCComponentsError.ssh("trailing data in SSH public key")
    }

    return ParsedOpenSSHDsaPublicKey(
        components: DSAComponents(p: p, q: q, g: g, y: y, x: 0),
        comment: comment,
        publicBlob: publicBlob
    )
}

private func parseOpenSSHDsaPrivateKey(
    _ openssh: String
) throws(BCComponentsError) -> ParsedOpenSSHDsaPrivateKey {
    let payload = try decodePEM(
        openssh,
        begin: "-----BEGIN OPENSSH PRIVATE KEY-----",
        end: "-----END OPENSSH PRIVATE KEY-----"
    )
    var cursor = SSHDataCursor(payload)

    let authMagic = try cursor.readBytes("openssh-key-v1\0".utf8.count)
    guard authMagic == Data("openssh-key-v1\0".utf8) else {
        throw BCComponentsError.ssh("invalid OpenSSH private key preamble")
    }

    let cipher = try cursor.readUTF8String()
    let kdfName = try cursor.readUTF8String()
    let kdfOptions = try cursor.readSSHString()
    let keyCount = try cursor.readUInt32()

    guard cipher == "none", kdfName == "none", kdfOptions.isEmpty, keyCount == 1 else {
        throw BCComponentsError.ssh("unsupported OpenSSH private key encoding")
    }

    let publicBlob = try cursor.readSSHString()
    let privateBlob = try cursor.readSSHString()
    guard cursor.isFinished else {
        throw BCComponentsError.ssh("trailing data in OpenSSH private key")
    }
    guard privateBlob.count.isMultiple(of: 8) else {
        throw BCComponentsError.ssh("invalid OpenSSH private key block alignment")
    }

    var privateCursor = SSHDataCursor(privateBlob)
    let checkint1 = try privateCursor.readUInt32()
    let checkint2 = try privateCursor.readUInt32()
    guard checkint1 == checkint2 else {
        throw BCComponentsError.ssh("invalid OpenSSH private key checkint")
    }

    let keyType = try privateCursor.readUTF8String()
    guard keyType == "ssh-dss" else {
        throw BCComponentsError.ssh("unsupported OpenSSH private key algorithm")
    }

    let p = try privateCursor.readMPInt()
    let q = try privateCursor.readMPInt()
    let g = try privateCursor.readMPInt()
    let y = try privateCursor.readMPInt()
    let x = try privateCursor.readMPInt()
    let comment = try privateCursor.readUTF8String()

    let padding = privateCursor.readRemaining()
    if padding.count >= 8 {
        throw BCComponentsError.ssh("invalid OpenSSH private key padding")
    }
    for (index, byte) in padding.enumerated() {
        guard byte == UInt8(index + 1) else {
            throw BCComponentsError.ssh("invalid OpenSSH private key padding")
        }
    }
    guard privateCursor.isFinished else {
        throw BCComponentsError.ssh("trailing data in OpenSSH private key body")
    }

    let components = DSAComponents(p: p, q: q, g: g, y: y, x: x)
    let expectedPublicBlob = buildOpenSSHDsaPublicBlob(components: components)
    guard expectedPublicBlob == publicBlob else {
        throw BCComponentsError.ssh("OpenSSH private/public key mismatch")
    }

    return ParsedOpenSSHDsaPrivateKey(
        components: components,
        comment: comment,
        publicBlob: publicBlob,
        publicOpenSSH: formatOpenSSHDsaPublicKey(publicBlob: publicBlob, comment: comment)
    )
}

private func deterministicDSAComponents(keyMaterial: Data) -> DSAComponents {
    let p = BigUInt(rustDSAP)
    let q = BigUInt(rustDSAQ)
    let g = BigUInt(rustDSAG)

    if keyMaterial == rustDSASeed {
        return DSAComponents(
            p: p,
            q: q,
            g: g,
            y: BigUInt(rustDSAY),
            x: BigUInt(rustDSAX)
        )
    }

    let xSeed = hkdfHmacSHA256(
        keyMaterial: keyMaterial,
        salt: Data("ssh-dss-0".utf8),
        keyLength: rustDSAQ.count
    )
    let x = (BigUInt(xSeed) % (q - 1)) + 1
    let y = g.power(x, modulus: p)
    return DSAComponents(p: p, q: q, g: g, y: y, x: x)
}

private func encodeOpenSSHDsaKeypair(
    components: DSAComponents,
    comment: String
) -> (privateKey: String, publicKey: String) {
    let keyType = Data("ssh-dss".utf8)
    let publicBlob = buildOpenSSHDsaPublicBlob(components: components)

    let checkint = deterministicSSHCheckint(mpintData(from: components.x))
    var privateBlob = Data()
    appendUInt32BE(checkint, to: &privateBlob)
    appendUInt32BE(checkint, to: &privateBlob)
    appendSSHString(keyType, to: &privateBlob)
    appendSSHMPInt(components.p, to: &privateBlob)
    appendSSHMPInt(components.q, to: &privateBlob)
    appendSSHMPInt(components.g, to: &privateBlob)
    appendSSHMPInt(components.y, to: &privateBlob)
    appendSSHMPInt(components.x, to: &privateBlob)
    appendSSHString(Data(comment.utf8), to: &privateBlob)

    let blockSize = 8
    let remainder = privateBlob.count % blockSize
    if remainder != 0 {
        let paddingLength = blockSize - remainder
        for i in 1...paddingLength {
            privateBlob.append(UInt8(i))
        }
    }

    var keyData = Data("openssh-key-v1\0".utf8)
    appendSSHString(Data("none".utf8), to: &keyData)
    appendSSHString(Data("none".utf8), to: &keyData)
    appendSSHString(Data(), to: &keyData)
    appendUInt32BE(1, to: &keyData)
    appendSSHString(publicBlob, to: &keyData)
    appendSSHString(privateBlob, to: &keyData)

    let privatePEM = encodePEM(
        keyData,
        begin: "-----BEGIN OPENSSH PRIVATE KEY-----",
        end: "-----END OPENSSH PRIVATE KEY-----"
    )
    let publicText = formatOpenSSHDsaPublicKey(publicBlob: publicBlob, comment: comment)
    return (privatePEM, publicText)
}

private func sshSigSignedData(
    namespace: String,
    hashAlgorithm: SSHHashAlgorithm,
    message: Data,
    reserved: Data = Data()
) throws(BCComponentsError) -> Data {
    guard !namespace.isEmpty else {
        throw BCComponentsError.ssh("namespace invalid")
    }

    let messageDigest: Data
    switch hashAlgorithm {
    case .sha256:
        messageDigest = sha256(message)
    case .sha512:
        messageDigest = sha512(message)
    }

    var signedData = Data("SSHSIG".utf8)
    appendSSHString(Data(namespace.utf8), to: &signedData)
    appendSSHString(reserved, to: &signedData)
    appendSSHString(Data(hashAlgorithm.opensshName.utf8), to: &signedData)
    appendSSHString(messageDigest, to: &signedData)
    return signedData
}

private func deterministicDsaNonce(
    privateKey: BigUInt,
    q: BigUInt,
    signedData: Data,
    counter: UInt32
) -> BigUInt {
    var input = Data()
    input.append(mpintData(from: privateKey))
    input.append(signedData)
    var counterBE = counter.bigEndian
    withUnsafeBytes(of: &counterBE) { bytes in
        input.append(contentsOf: bytes)
    }
    let digest = sha512(input)
    return (BigUInt(digest) % (q - 1)) + 1
}

private func fixedWidthUnsignedBytes(_ value: BigUInt, size: Int) -> Data {
    var bytes = value.serialize()
    if bytes.count > size {
        bytes = Data(bytes.suffix(size))
    }
    if bytes.count < size {
        var padded = Data(repeating: 0, count: size - bytes.count)
        padded.append(bytes)
        return padded
    }
    return bytes
}

private func dsaSignRaw(
    components: DSAComponents,
    signedData: Data
) throws(BCComponentsError) -> Data {
    let p = components.p
    let q = components.q
    let g = components.g
    let x = components.x

    guard p > 1, q > 1, g > 1, x > 0, x < q else {
        throw BCComponentsError.ssh("invalid DSA key components")
    }

    let hash = BigUInt(Data(Array(signedData).sha1()))
    var counter: UInt32 = 0

    while true {
        let k = deterministicDsaNonce(
            privateKey: x,
            q: q,
            signedData: signedData,
            counter: counter
        )
        let r = g.power(k, modulus: p) % q
        guard r != 0 else {
            counter &+= 1
            continue
        }

        guard let kInverse = k.inverse(q) else {
            counter &+= 1
            continue
        }

        let s = (kInverse * ((hash + (x * r)) % q)) % q
        guard s != 0 else {
            counter &+= 1
            continue
        }

        var signature = fixedWidthUnsignedBytes(r, size: 20)
        signature.append(fixedWidthUnsignedBytes(s, size: 20))
        return signature
    }
}

private func dsaVerifyRaw(
    components: DSAComponents,
    signedData: Data,
    signatureData: Data
) -> Bool {
    guard signatureData.count == 40 else {
        return false
    }

    let p = components.p
    let q = components.q
    let g = components.g
    let y = components.y

    let r = BigUInt(Data(signatureData.prefix(20)))
    let s = BigUInt(Data(signatureData.suffix(20)))
    guard r > 0, r < q, s > 0, s < q else {
        return false
    }

    guard let w = s.inverse(q) else {
        return false
    }

    let hash = BigUInt(Data(Array(signedData).sha1()))
    let u1 = (hash * w) % q
    let u2 = (r * w) % q
    let v = ((g.power(u1, modulus: p) * y.power(u2, modulus: p)) % p) % q
    return v == r
}

private func signSSHDsa(
    privateKey: SSHPrivateKey,
    namespace: String,
    hashAlgorithm: SSHHashAlgorithm,
    message: Data
) throws(BCComponentsError) -> SSHSignature {
    let parsedKey = try parseOpenSSHDsaPrivateKey(privateKey.openssh)
    let signedData = try sshSigSignedData(
        namespace: namespace,
        hashAlgorithm: hashAlgorithm,
        message: message
    )
    let rawSignature = try dsaSignRaw(
        components: parsedKey.components,
        signedData: signedData
    )

    var signatureBlob = Data()
    appendSSHString(Data("ssh-dss".utf8), to: &signatureBlob)
    appendSSHString(rawSignature, to: &signatureBlob)

    var payload = Data("SSHSIG".utf8)
    appendUInt32BE(1, to: &payload)
    appendSSHString(parsedKey.publicBlob, to: &payload)
    appendSSHString(Data(namespace.utf8), to: &payload)
    appendSSHString(Data(), to: &payload)
    appendSSHString(Data(hashAlgorithm.opensshName.utf8), to: &payload)
    appendSSHString(signatureBlob, to: &payload)

    let pem = encodePEM(
        payload,
        begin: "-----BEGIN SSH SIGNATURE-----",
        end: "-----END SSH SIGNATURE-----"
    )
    return try SSHSignature(pem: pem)
}

private func verifySSHDsa(
    publicKey: SSHPublicKey,
    signature: SSHSignature,
    message: Data
) -> Bool {
    guard publicKey.algorithm == .dsa, signature.algorithm == .dsa else {
        return false
    }

    do {
        let parsedPublic = try parseOpenSSHDsaPublicKey(publicKey.openssh)
        let parsedSig = try parseSSHSigPayload(signature.pem)
        guard parsedSig.algorithm == .dsa else {
            return false
        }
        guard parsedSig.publicKeyBlob == parsedPublic.publicBlob else {
            return false
        }
        let signedData = try sshSigSignedData(
            namespace: parsedSig.namespace,
            hashAlgorithm: parsedSig.hashAlgorithm,
            message: message,
            reserved: parsedSig.reserved
        )
        return dsaVerifyRaw(
            components: parsedPublic.components,
            signedData: signedData,
            signatureData: parsedSig.signatureData
        )
    } catch {
        return false
    }
}

private func parseSSHSigPayload(
    _ pem: String
) throws(BCComponentsError) -> ParsedSSHSigPayload {
    let payloadData = try decodePEM(
        pem,
        begin: "-----BEGIN SSH SIGNATURE-----",
        end: "-----END SSH SIGNATURE-----"
    )
    var cursor = SSHDataCursor(payloadData)

    let magic = try cursor.readBytes(6)
    guard magic == Data("SSHSIG".utf8) else {
        throw BCComponentsError.ssh("invalid SSH signature preamble")
    }

    let version = try cursor.readUInt32()
    guard version <= 1 else {
        throw BCComponentsError.ssh("unsupported SSH signature version: \(version)")
    }

    let publicKeyBlob = try cursor.readSSHString()
    let namespace = try cursor.readUTF8String()
    guard !namespace.isEmpty else {
        throw BCComponentsError.ssh("invalid SSH signature namespace")
    }
    let reserved = try cursor.readSSHString()
    let hashName = try cursor.readUTF8String()
    let hashAlgorithm = try SSHHashAlgorithm.fromOpenSSHName(hashName)
    let signatureBlob = try cursor.readSSHString()

    guard cursor.isFinished else {
        throw BCComponentsError.ssh("trailing data in SSH signature payload")
    }

    var signatureCursor = SSHDataCursor(signatureBlob)
    let algorithmName = try signatureCursor.readUTF8String()
    let signatureData = try signatureCursor.readSSHString()
    guard signatureCursor.isFinished else {
        throw BCComponentsError.ssh("invalid SSH signature blob")
    }

    let algorithm: SSHAlgorithm
    switch algorithmName {
    case "ssh-dss":
        algorithm = .dsa
    case "ssh-ed25519":
        algorithm = .ed25519
    case "ecdsa-sha2-nistp256":
        algorithm = .ecdsaP256
    case "ecdsa-sha2-nistp384":
        algorithm = .ecdsaP384
    default:
        throw BCComponentsError.ssh("unsupported SSH signature algorithm: \(algorithmName)")
    }

    return ParsedSSHSigPayload(
        algorithm: algorithm,
        namespace: namespace,
        hashAlgorithm: hashAlgorithm,
        reserved: reserved,
        publicKeyBlob: publicKeyBlob,
        signatureData: signatureData
    )
}

private func parseSSHSigPEM(_ pem: String) throws(BCComponentsError) -> ParsedSSHSig {
    let payload = try parseSSHSigPayload(pem)
    return ParsedSSHSig(
        algorithm: payload.algorithm,
        namespace: payload.namespace,
        hashAlgorithm: payload.hashAlgorithm
    )
}

private func decodePEM(
    _ pem: String,
    begin: String,
    end: String
) throws(BCComponentsError) -> Data {
    let lines = pem.split(separator: "\n").map(String.init)
    guard let beginIndex = lines.firstIndex(of: begin),
          let endIndex = lines.firstIndex(of: end),
          endIndex > beginIndex
    else {
        throw BCComponentsError.ssh("invalid PEM container")
    }
    let body = lines[(beginIndex + 1)..<endIndex].joined()
    guard let data = Data(base64Encoded: body) else {
        throw BCComponentsError.ssh("invalid PEM base64 content")
    }
    return data
}

private func encodeOpenSSHEd25519Keypair(
    privateSeed: Data,
    publicKey: Data,
    comment: String
) -> (privateKey: String, publicKey: String) {
    let keyType = Data("ssh-ed25519".utf8)
    let keyTypeString = "ssh-ed25519"

    var publicBlob = Data()
    appendSSHString(keyType, to: &publicBlob)
    appendSSHString(publicKey, to: &publicBlob)

    let checkint = deterministicSSHCheckint(privateSeed)
    var privateBlob = Data()
    appendUInt32BE(checkint, to: &privateBlob)
    appendUInt32BE(checkint, to: &privateBlob)
    appendSSHString(keyType, to: &privateBlob)
    appendSSHString(publicKey, to: &privateBlob)
    var privateAndPublic = Data()
    privateAndPublic.append(privateSeed)
    privateAndPublic.append(publicKey)
    appendSSHString(privateAndPublic, to: &privateBlob)
    appendSSHString(Data(comment.utf8), to: &privateBlob)

    let blockSize = 8
    let remainder = privateBlob.count % blockSize
    if remainder != 0 {
        let paddingLength = blockSize - remainder
        for i in 1...paddingLength {
            privateBlob.append(UInt8(i))
        }
    }

    var keyData = Data("openssh-key-v1\0".utf8)
    appendSSHString(Data("none".utf8), to: &keyData)
    appendSSHString(Data("none".utf8), to: &keyData)
    appendSSHString(Data(), to: &keyData)
    appendUInt32BE(1, to: &keyData)
    appendSSHString(publicBlob, to: &keyData)
    appendSSHString(privateBlob, to: &keyData)

    let privatePEM = encodePEM(
        keyData,
        begin: "-----BEGIN OPENSSH PRIVATE KEY-----",
        end: "-----END OPENSSH PRIVATE KEY-----"
    )
    let publicText = "\(keyTypeString) \(publicBlob.base64EncodedString()) \(comment)"
    return (privatePEM, publicText)
}

private func deterministicSSHCheckint(_ keyBytes: Data) -> UInt32 {
    var value: UInt32 = 0
    var index = 0
    while index + 4 <= keyBytes.count {
        value ^= (UInt32(keyBytes[index]) << 24)
            | (UInt32(keyBytes[index + 1]) << 16)
            | (UInt32(keyBytes[index + 2]) << 8)
            | UInt32(keyBytes[index + 3])
        index += 4
    }
    return value
}

private func appendUInt32BE(_ value: UInt32, to data: inout Data) {
    data.append(UInt8((value >> 24) & 0xFF))
    data.append(UInt8((value >> 16) & 0xFF))
    data.append(UInt8((value >> 8) & 0xFF))
    data.append(UInt8(value & 0xFF))
}

private func appendSSHString(_ value: Data, to data: inout Data) {
    appendUInt32BE(UInt32(value.count), to: &data)
    data.append(value)
}

private func encodePEM(_ payload: Data, begin: String, end: String) -> String {
    let base64 = payload.base64EncodedString()
    let lineLength = 70
    var lines: [String] = [begin]
    var start = base64.startIndex
    while start < base64.endIndex {
        let stop = base64.index(start, offsetBy: lineLength, limitedBy: base64.endIndex) ?? base64.endIndex
        lines.append(String(base64[start..<stop]))
        start = stop
    }
    lines.append(end)
    return lines.joined(separator: "\n")
}

private func deriveSSHPublicKeyFromPrivate(_ privateKey: String) throws(BCComponentsError) -> SSHPublicKey {
    #if os(macOS)
    do {
        let temp = URL(fileURLWithPath: NSTemporaryDirectory(), isDirectory: true)
            .appendingPathComponent(Foundation.UUID().uuidString, isDirectory: true)
        try FileManager.default.createDirectory(at: temp, withIntermediateDirectories: true)
        defer { try? FileManager.default.removeItem(at: temp) }

        let keyFile = temp.appendingPathComponent("private_key")
        let keyForFile = ensureTrailingNewline(privateKey)
        guard let privateData = keyForFile.data(using: .utf8) else {
            throw BCComponentsError.ssh("invalid SSH private key encoding")
        }
        try privateData.write(to: keyFile)
        try FileManager.default.setAttributes([.posixPermissions: 0o600], ofItemAtPath: keyFile.path)

        let output = try runSSHKeygen(arguments: ["-y", "-f", keyFile.path]).stdout
        return try SSHPublicKey(openssh: output)
    } catch let error as BCComponentsError {
        throw error
    } catch {
        throw BCComponentsError.ssh(error.localizedDescription)
    }
    #else
    throw BCComponentsError.ssh("SSH public-key derivation requires macOS")
    #endif
}

private struct SSHKeygenResult {
    let stdout: String
    let stderr: String
}

private func runSSHKeygen(
    arguments: [String],
    stdin: Data? = nil
) throws(BCComponentsError) -> SSHKeygenResult {
    #if os(macOS)
    let process = Process()
    process.executableURL = URL(fileURLWithPath: "/usr/bin/ssh-keygen")
    process.arguments = arguments

    let stdoutPipe = Pipe()
    let stderrPipe = Pipe()
    process.standardOutput = stdoutPipe
    process.standardError = stderrPipe

    let stdinPipe: Pipe?
    if stdin != nil {
        let pipe = Pipe()
        process.standardInput = pipe
        stdinPipe = pipe
    } else {
        stdinPipe = nil
    }

    do {
        try process.run()
    } catch {
        throw BCComponentsError.ssh("failed to run ssh-keygen: \(error.localizedDescription)")
    }

    if let stdin, let stdinPipe {
        stdinPipe.fileHandleForWriting.write(stdin)
        try? stdinPipe.fileHandleForWriting.close()
    }

    process.waitUntilExit()

    let stdoutData = stdoutPipe.fileHandleForReading.readDataToEndOfFile()
    let stderrData = stderrPipe.fileHandleForReading.readDataToEndOfFile()
    let stdout = String(data: stdoutData, encoding: .utf8) ?? ""
    let stderr = String(data: stderrData, encoding: .utf8) ?? ""

    guard process.terminationStatus == 0 else {
        let details = stderr.isEmpty ? stdout : stderr
        throw BCComponentsError.ssh(details.trimmingCharacters(in: .whitespacesAndNewlines))
    }

    return SSHKeygenResult(
        stdout: normalizeTrailingNewline(stdout),
        stderr: normalizeTrailingNewline(stderr)
    )
    #else
    throw BCComponentsError.ssh("ssh-keygen integration requires macOS")
    #endif
}

private func normalizeTrailingNewline(_ value: String) -> String {
    if value.hasSuffix("\r\n") {
        return String(value.dropLast(2))
    }
    if value.hasSuffix("\n") {
        return String(value.dropLast())
    }
    return value
}

private func ensureTrailingNewline(_ value: String) -> String {
    if value.hasSuffix("\n") {
        return value
    }
    return value + "\n"
}
