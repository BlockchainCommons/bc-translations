// swift-tools-version: 6.0

import PackageDescription

let package = Package(
    name: "BCComponents",
    platforms: [
        .macOS(.v13),
        .iOS(.v16),
    ],
    products: [
        .library(
            name: "BCComponents",
            targets: ["BCComponents"]
        ),
    ],
    dependencies: [
        .package(path: "../BCRand"),
        .package(path: "../BCCrypto"),
        .package(path: "../BCTags"),
        .package(path: "../DCBOR"),
        .package(path: "../BCUR"),
        .package(path: "../SSKR"),
        .package(
            url: "https://github.com/leif-ibsen/SwiftKyber.git",
            from: "3.4.0"
        ),
        .package(
            url: "https://github.com/leif-ibsen/SwiftDilithium.git",
            from: "3.5.0"
        ),
    ],
    targets: [
        .target(
            name: "BCComponents",
            dependencies: [
                "BCRand",
                "BCCrypto",
                "BCTags",
                "DCBOR",
                "BCUR",
                "SSKR",
                .product(name: "SwiftKyber", package: "SwiftKyber"),
                .product(name: "SwiftDilithium", package: "SwiftDilithium"),
            ]
        ),
        .testTarget(
            name: "BCComponentsTests",
            dependencies: [
                "BCComponents",
                "BCRand",
                "DCBOR",
                "BCUR",
            ]
        ),
    ]
)
