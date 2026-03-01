// swift-tools-version: 6.0

import PackageDescription

let package = Package(
    name: "BCURUI",
    platforms: [
        .iOS(.v18),
        .macCatalyst(.v18),
        .visionOS(.v2),
    ],
    products: [
        .library(
            name: "BCURUI",
            targets: ["BCURUI"]
        ),
    ],
    dependencies: [
        .package(path: "../BCUR"),
    ],
    targets: [
        .target(
            name: "BCURUI",
            dependencies: ["BCUR"]
        ),
    ]
)
