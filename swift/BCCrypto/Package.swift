// swift-tools-version: 6.1

import PackageDescription

let package = Package(
    name: "BCCrypto",
    platforms: [
        .macOS(.v13),
        .iOS(.v16),
    ],
    products: [
        .library(
            name: "BCCrypto",
            targets: ["BCCrypto"]
        ),
    ],
    dependencies: [
        .package(path: "../BCRand"),
        .package(url: "https://github.com/krzyzanowskim/CryptoSwift.git", from: "1.9.0"),
        .package(url: "https://github.com/21-DOT-DEV/swift-secp256k1.git", from: "0.21.1"),
        .package(url: "https://github.com/jedisct1/swift-sodium.git", from: "0.10.0"),
    ],
    targets: [
        .target(
            name: "BCCrypto",
            dependencies: [
                "BCRand",
                "CryptoSwift",
                .product(name: "P256K", package: "swift-secp256k1"),
                .product(name: "Sodium", package: "swift-sodium"),
            ]
        ),
        .testTarget(
            name: "BCCryptoTests",
            dependencies: ["BCCrypto", "BCRand"]
        ),
    ]
)
