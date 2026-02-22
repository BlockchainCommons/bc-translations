import BCRand
import Foundation

public enum SignatureScheme: Equatable, Hashable, Sendable {
    case schnorr
    case ecdsa
    case ed25519
    case sshDsa
    case sshEd25519
    case sshEcdsaP256
    case sshEcdsaP384
    case mldsa44
    case mldsa65
    case mldsa87

    public static var `default`: SignatureScheme {
        .schnorr
    }

    public func keypair() -> (SigningPrivateKey, SigningPublicKey) {
        keypairOpt("")
    }

    public func keypairOpt(_ comment: String) -> (SigningPrivateKey, SigningPublicKey) {
        switch self {
        case .schnorr:
            let privateKey = SigningPrivateKey.newSchnorr(ECPrivateKey.new())
            return (privateKey, try! privateKey.publicKey())
        case .ecdsa:
            let privateKey = SigningPrivateKey.newEcdsa(ECPrivateKey.new())
            return (privateKey, try! privateKey.publicKey())
        case .ed25519:
            let privateKey = SigningPrivateKey.newEd25519(Ed25519PrivateKey.new())
            return (privateKey, try! privateKey.publicKey())
        case .sshDsa:
            let privateKey = try! PrivateKeyBase.new().sshSigningPrivateKey(
                .dsa,
                comment: comment
            )
            return (privateKey, try! privateKey.publicKey())
        case .sshEd25519:
            let privateKey = try! PrivateKeyBase.new().sshSigningPrivateKey(
                .ed25519,
                comment: comment
            )
            return (privateKey, try! privateKey.publicKey())
        case .sshEcdsaP256:
            let privateKey = try! PrivateKeyBase.new().sshSigningPrivateKey(
                .ecdsaP256,
                comment: comment
            )
            return (privateKey, try! privateKey.publicKey())
        case .sshEcdsaP384:
            let privateKey = try! PrivateKeyBase.new().sshSigningPrivateKey(
                .ecdsaP384,
                comment: comment
            )
            return (privateKey, try! privateKey.publicKey())
        case .mldsa44:
            let (privateKey, publicKey) = MLDSA.mldsa44.keypair()
            return (.mldsa(privateKey), .mldsa(publicKey))
        case .mldsa65:
            let (privateKey, publicKey) = MLDSA.mldsa65.keypair()
            return (.mldsa(privateKey), .mldsa(publicKey))
        case .mldsa87:
            let (privateKey, publicKey) = MLDSA.mldsa87.keypair()
            return (.mldsa(privateKey), .mldsa(publicKey))
        }
    }

    public func keypairUsing<G: BCRandomNumberGenerator>(
        _ rng: inout G,
        comment: String = ""
    ) throws(BCComponentsError) -> (SigningPrivateKey, SigningPublicKey) {
        switch self {
        case .schnorr:
            let privateKey = SigningPrivateKey.newSchnorr(
                ECPrivateKey.newUsing(rng: &rng)
            )
            return (privateKey, try privateKey.publicKey())
        case .ecdsa:
            let privateKey = SigningPrivateKey.newEcdsa(
                ECPrivateKey.newUsing(rng: &rng)
            )
            return (privateKey, try privateKey.publicKey())
        case .ed25519:
            let privateKey = SigningPrivateKey.newEd25519(
                Ed25519PrivateKey.newUsing(rng: &rng)
            )
            return (privateKey, try privateKey.publicKey())
        case .sshDsa:
            let privateKey = try PrivateKeyBase.newUsing(rng: &rng).sshSigningPrivateKey(
                .dsa,
                comment: comment
            )
            return (privateKey, try privateKey.publicKey())
        case .sshEd25519:
            let privateKey = try PrivateKeyBase.newUsing(rng: &rng).sshSigningPrivateKey(
                .ed25519,
                comment: comment
            )
            return (privateKey, try privateKey.publicKey())
        case .sshEcdsaP256:
            let privateKey = try PrivateKeyBase.newUsing(rng: &rng).sshSigningPrivateKey(
                .ecdsaP256,
                comment: comment
            )
            return (privateKey, try privateKey.publicKey())
        case .sshEcdsaP384:
            let privateKey = try PrivateKeyBase.newUsing(rng: &rng).sshSigningPrivateKey(
                .ecdsaP384,
                comment: comment
            )
            return (privateKey, try privateKey.publicKey())
        case .mldsa44, .mldsa65, .mldsa87:
            throw BCComponentsError.general(
                "Deterministic keypair generation not supported for this signature scheme"
            )
        }
    }
}
