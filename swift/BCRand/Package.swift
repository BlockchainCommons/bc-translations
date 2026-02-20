// swift-tools-version: 6.0

import PackageDescription

let package = Package(
    name: "BCRand",
    platforms: [
        .macOS(.v13),
        .iOS(.v16),
    ],
    products: [
        .library(
            name: "BCRand",
            targets: ["BCRand"]
        ),
    ],
    targets: [
        .target(
            name: "BCRand"
        ),
        .testTarget(
            name: "BCRandTests",
            dependencies: ["BCRand"]
        ),
    ]
)
