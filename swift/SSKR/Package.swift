// swift-tools-version: 6.0

import PackageDescription

let package = Package(
    name: "SSKR",
    platforms: [
        .macOS(.v13),
        .iOS(.v16),
    ],
    products: [
        .library(
            name: "SSKR",
            targets: ["SSKR"]
        ),
    ],
    dependencies: [
        .package(path: "../BCRand"),
        .package(path: "../BCShamir"),
    ],
    targets: [
        .target(
            name: "SSKR",
            dependencies: ["BCRand", "BCShamir"]
        ),
        .testTarget(
            name: "SSKRTests",
            dependencies: ["SSKR", "BCRand"]
        ),
    ]
)
