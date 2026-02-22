// swift-tools-version: 6.0

import PackageDescription

let package = Package(
    name: "BCUR",
    platforms: [
        .macOS(.v13),
        .iOS(.v16),
    ],
    products: [
        .library(
            name: "BCUR",
            targets: ["BCUR"]
        ),
    ],
    dependencies: [
        .package(path: "../DCBOR"),
    ],
    targets: [
        .target(
            name: "BCUR",
            dependencies: ["DCBOR"]
        ),
        .testTarget(
            name: "BCURTests",
            dependencies: ["BCUR", "DCBOR"]
        ),
    ]
)
