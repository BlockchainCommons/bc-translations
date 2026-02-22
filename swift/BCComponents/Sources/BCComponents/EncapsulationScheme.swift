import BCRand

public enum EncapsulationScheme: Equatable, Hashable, Sendable {
    case x25519
    case mlkem512
    case mlkem768
    case mlkem1024

    public static var `default`: EncapsulationScheme {
        .x25519
    }

    public func keypair() -> (EncapsulationPrivateKey, EncapsulationPublicKey) {
        switch self {
        case .x25519:
            let (privateKey, publicKey) = X25519PrivateKey.keypair()
            return (.x25519(privateKey), .x25519(publicKey))
        case .mlkem512:
            let (privateKey, publicKey) = MLKEM.mlkem512.keypair()
            return (.mlkem(privateKey), .mlkem(publicKey))
        case .mlkem768:
            let (privateKey, publicKey) = MLKEM.mlkem768.keypair()
            return (.mlkem(privateKey), .mlkem(publicKey))
        case .mlkem1024:
            let (privateKey, publicKey) = MLKEM.mlkem1024.keypair()
            return (.mlkem(privateKey), .mlkem(publicKey))
        }
    }

    public func keypairUsing<G: BCRandomNumberGenerator>(
        _ rng: inout G
    ) throws(BCComponentsError) -> (EncapsulationPrivateKey, EncapsulationPublicKey) {
        switch self {
        case .x25519:
            let (privateKey, publicKey) = X25519PrivateKey.keypairUsing(rng: &rng)
            return (.x25519(privateKey), .x25519(publicKey))
        case .mlkem512, .mlkem768, .mlkem1024:
            throw BCComponentsError.general(
                "Deterministic keypair generation not supported for this encapsulation scheme"
            )
        }
    }
}
