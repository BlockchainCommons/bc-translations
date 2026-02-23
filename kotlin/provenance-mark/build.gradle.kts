plugins {
    kotlin("jvm") version "2.2.21"
}

group = "com.blockchaincommons"
version = "0.23.0"
description = "A cryptographically-secured system for establishing and verifying the authenticity of works."

repositories {
    mavenCentral()
}

dependencies {
    implementation("com.blockchaincommons:bc-rand:0.5.0")
    implementation("com.blockchaincommons:dcbor:0.25.1")
    implementation("com.blockchaincommons:bc-ur:0.19.0")
    implementation("com.blockchaincommons:bc-tags:0.12.0")
    implementation("com.blockchaincommons:bc-envelope:0.43.0")

    implementation("com.fasterxml.jackson.core:jackson-databind:2.17.2")
    implementation("com.fasterxml.jackson.module:jackson-module-kotlin:2.17.2")

    testImplementation(kotlin("test"))
    testRuntimeOnly("org.junit.platform:junit-platform-launcher")
}

tasks.test {
    useJUnitPlatform()
}

kotlin {
    jvmToolchain(21)
    compilerOptions {
        freeCompilerArgs.addAll(
            "-opt-in=kotlin.ExperimentalUnsignedTypes",
            "-opt-in=kotlin.ExperimentalStdlibApi",
        )
    }
}
