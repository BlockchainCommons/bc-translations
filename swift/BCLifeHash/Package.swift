// swift-tools-version: 6.0
// BCLifeHash — Blockchain Commons LifeHash

import PackageDescription

let package = Package(
    name: "BCLifeHash",
    platforms: [
        .macOS(.v13),
        .iOS(.v16),
    ],
    products: [
        .library(
            name: "BCLifeHash",
            targets: ["BCLifeHash"]
        ),
    ],
    targets: [
        .target(
            name: "BCLifeHash"
        ),
        .testTarget(
            name: "BCLifeHashTests",
            dependencies: ["BCLifeHash"],
            resources: [
                .copy("test-vectors.json"),
            ]
        ),
    ]
)
