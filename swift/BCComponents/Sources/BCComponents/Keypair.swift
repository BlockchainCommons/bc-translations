import BCRand

public func keypair() -> (PrivateKeys, PublicKeys) {
    keypairOpt(
        signatureScheme: .default,
        encapsulationScheme: .default
    )
}

public func keypairUsing<G: BCRandomNumberGenerator>(
    _ rng: inout G
) throws(BCComponentsError) -> (PrivateKeys, PublicKeys) {
    try keypairOptUsing(
        signatureScheme: .default,
        encapsulationScheme: .default,
        rng: &rng
    )
}

public func keypairOpt(
    signatureScheme: SignatureScheme,
    encapsulationScheme: EncapsulationScheme
) -> (PrivateKeys, PublicKeys) {
    let (signingPrivateKey, signingPublicKey) = signatureScheme.keypairOpt("")
    let (encapsulationPrivateKey, encapsulationPublicKey) = encapsulationScheme.keypair()

    let privateKeys = PrivateKeys.withKeys(
        signingPrivateKey,
        encapsulationPrivateKey
    )
    let publicKeys = PublicKeys.new(
        signingPublicKey,
        encapsulationPublicKey
    )
    return (privateKeys, publicKeys)
}

public func keypairOptUsing<G: BCRandomNumberGenerator>(
    signatureScheme: SignatureScheme,
    encapsulationScheme: EncapsulationScheme,
    rng: inout G
) throws(BCComponentsError) -> (PrivateKeys, PublicKeys) {
    let (signingPrivateKey, signingPublicKey) = try signatureScheme.keypairUsing(
        &rng,
        comment: ""
    )
    let (encapsulationPrivateKey, encapsulationPublicKey) = try encapsulationScheme.keypairUsing(
        &rng
    )

    let privateKeys = PrivateKeys.withKeys(
        signingPrivateKey,
        encapsulationPrivateKey
    )
    let publicKeys = PublicKeys.new(
        signingPublicKey,
        encapsulationPublicKey
    )
    return (privateKeys, publicKeys)
}
