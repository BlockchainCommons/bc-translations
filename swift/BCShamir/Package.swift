// swift-tools-version: 6.0
import PackageDescription

let package = Package(
    name: "BCShamir",
    platforms: [
        .macOS(.v13),
        .iOS(.v16),
    ],
    products: [
        .library(name: "BCShamir", targets: ["BCShamir"]),
    ],
    dependencies: [
        .package(path: "../BCRand"),
        .package(path: "../BCCrypto"),
    ],
    targets: [
        .target(
            name: "BCShamir",
            dependencies: ["BCRand", "BCCrypto"]
        ),
        .testTarget(
            name: "BCShamirTests",
            dependencies: ["BCShamir", "BCRand"]
        ),
    ]
)
