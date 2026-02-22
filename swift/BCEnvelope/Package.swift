// swift-tools-version: 6.0

import PackageDescription

let package = Package(
    name: "BCEnvelope",
    platforms: [
        .macOS(.v13),
        .iOS(.v16),
    ],
    products: [
        .library(
            name: "BCEnvelope",
            targets: ["BCEnvelope"]
        ),
    ],
    dependencies: [
        .package(path: "../BCComponents"),
        .package(path: "../KnownValues"),
        .package(url: "https://github.com/WolfMcNally/WolfBase.git", from: "7.0.0"),
        .package(url: "https://github.com/WolfMcNally/Graph.git", from: "2.0.0"),
        .package(url: "https://github.com/WolfMcNally/GraphMermaid.git", from: "2.0.0"),
        .package(url: "https://github.com/objecthub/swift-numberkit", from: "2.6.0"),
    ],
    targets: [
        .target(
            name: "BCEnvelope",
            dependencies: [
                "BCComponents",
                "KnownValues",
                "WolfBase",
                "Graph",
                "GraphMermaid",
                .product(name: "NumberKit", package: "swift-numberkit"),
            ]
        ),
        .testTarget(
            name: "BCEnvelopeTests",
            dependencies: [
                "BCEnvelope",
                "BCComponents",
                "KnownValues",
                "WolfBase",
            ]
        ),
    ]
)
