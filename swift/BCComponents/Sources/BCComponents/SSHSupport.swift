import BCCrypto
import Foundation

public enum SSHAlgorithm: Equatable, Hashable, Sendable {
    case ed25519
    case ecdsaP256
    case ecdsaP384

    var signatureScheme: SignatureScheme {
        switch self {
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
        let derivedPublic = try deriveSSHPublicKeyFromPrivate(normalized)
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

private func parseSSHSigPEM(_ pem: String) throws(BCComponentsError) -> ParsedSSHSig {
    let payloadData = try decodePEM(
        pem,
        begin: "-----BEGIN SSH SIGNATURE-----",
        end: "-----END SSH SIGNATURE-----"
    )

    var cursor = 0
    func readBytes(_ length: Int) throws(BCComponentsError) -> Data {
        guard cursor + length <= payloadData.count else {
            throw BCComponentsError.ssh("invalid SSH signature payload")
        }
        let slice = payloadData[cursor..<(cursor + length)]
        cursor += length
        return Data(slice)
    }

    func readUInt32() throws(BCComponentsError) -> UInt32 {
        let data = try readBytes(4)
        return data.withUnsafeBytes { ptr in
            let base = ptr.bindMemory(to: UInt8.self)
            return (UInt32(base[0]) << 24)
                | (UInt32(base[1]) << 16)
                | (UInt32(base[2]) << 8)
                | UInt32(base[3])
        }
    }

    func readSSHString() throws(BCComponentsError) -> Data {
        let length = Int(try readUInt32())
        return try readBytes(length)
    }

    let magic = try readBytes(6)
    guard magic == Data("SSHSIG".utf8) else {
        throw BCComponentsError.ssh("invalid SSH signature preamble")
    }
    _ = try readUInt32() // version
    _ = try readSSHString() // public key blob
    guard let namespace = String(data: try readSSHString(), encoding: .utf8) else {
        throw BCComponentsError.ssh("invalid SSH signature namespace")
    }
    _ = try readSSHString() // reserved
    guard let hash = String(data: try readSSHString(), encoding: .utf8) else {
        throw BCComponentsError.ssh("invalid SSH signature hash algorithm")
    }
    let signatureBlob = try readSSHString()

    var sigCursor = 0
    func readSigUInt32() throws(BCComponentsError) -> UInt32 {
        guard sigCursor + 4 <= signatureBlob.count else {
            throw BCComponentsError.ssh("invalid SSH signature blob")
        }
        let bytes = signatureBlob[sigCursor..<(sigCursor + 4)]
        sigCursor += 4
        return bytes.reduce(UInt32(0)) { ($0 << 8) | UInt32($1) }
    }

    func readSigString() throws(BCComponentsError) -> Data {
        let len = Int(try readSigUInt32())
        guard sigCursor + len <= signatureBlob.count else {
            throw BCComponentsError.ssh("invalid SSH signature blob")
        }
        let value = signatureBlob[sigCursor..<(sigCursor + len)]
        sigCursor += len
        return Data(value)
    }

    guard let algorithmName = String(data: try readSigString(), encoding: .utf8) else {
        throw BCComponentsError.ssh("invalid SSH signature algorithm")
    }
    let algorithm: SSHAlgorithm
    switch algorithmName {
    case "ssh-ed25519":
        algorithm = .ed25519
    case "ecdsa-sha2-nistp256":
        algorithm = .ecdsaP256
    case "ecdsa-sha2-nistp384":
        algorithm = .ecdsaP384
    default:
        throw BCComponentsError.ssh("unsupported SSH signature algorithm: \(algorithmName)")
    }

    return ParsedSSHSig(
        algorithm: algorithm,
        namespace: namespace,
        hashAlgorithm: try SSHHashAlgorithm.fromOpenSSHName(hash)
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
