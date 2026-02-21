// swift-tools-version: 6.0

import PackageDescription

let package = Package(
    name: "BCTags",
    platforms: [
        .macOS(.v13),
        .iOS(.v14),
        .macCatalyst(.v14)
    ],
    products: [
        .library(
            name: "BCTags",
            targets: ["BCTags"]),
    ],
    dependencies: [
    ],
    targets: [
        .target(
            name: "BCTags",
            dependencies: [
            ]
        ),
        .testTarget(
            name: "BCTagsTests",
            dependencies: ["BCTags"]),
    ]
)
