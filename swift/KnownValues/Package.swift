// swift-tools-version: 6.0

import PackageDescription

let package = Package(
    name: "KnownValues",
    platforms: [
        .macOS(.v13),
        .iOS(.v16),
    ],
    products: [
        .library(
            name: "KnownValues",
            targets: ["KnownValues"]
        ),
    ],
    dependencies: [
        .package(path: "../DCBOR"),
        .package(path: "../BCTags"),
        .package(path: "../BCComponents"),
    ],
    targets: [
        .target(
            name: "KnownValues",
            dependencies: [
                "DCBOR",
                "BCTags",
                "BCComponents",
            ]
        ),
        .testTarget(
            name: "KnownValuesTests",
            dependencies: [
                "KnownValues",
                "DCBOR",
                "BCTags",
            ]
        ),
    ]
)
