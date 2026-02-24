import Foundation

public extension Parameter {
    static let blank = Parameter(1, "_")
    static let lhs = Parameter(2, "lhs")
    static let rhs = Parameter(3, "rhs")
}

public let globalParameters: ParametersStore = {
    var store: ParametersStore = [
        .blank,
        .lhs,
        .rhs,
        .seedDigest,
        .derivationPath,
        .isPrivate,
        .useInfo,
        .isDerivable,
        .psbt,
        .name,
        .challenge,
    ]
    return store
}()
