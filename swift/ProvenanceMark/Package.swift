// swift-tools-version: 6.0

import PackageDescription

let package = Package(
    name: "ProvenanceMark",
    platforms: [
        .macOS(.v13),
        .iOS(.v16),
    ],
    products: [
        .library(
            name: "ProvenanceMark",
            targets: ["ProvenanceMark"]
        ),
    ],
    dependencies: [
        .package(path: "../BCRand"),
        .package(path: "../DCBOR"),
        .package(path: "../BCUR"),
        .package(path: "../BCTags"),
        .package(path: "../BCEnvelope"),
    ],
    targets: [
        .target(
            name: "ProvenanceMark",
            dependencies: [
                "BCRand",
                "DCBOR",
                "BCUR",
                "BCTags",
                "BCEnvelope",
            ]
        ),
        .testTarget(
            name: "ProvenanceMarkTests",
            dependencies: [
                "ProvenanceMark",
                "BCRand",
                "DCBOR",
                "BCUR",
                "BCTags",
                "BCEnvelope",
            ]
        ),
    ]
)
