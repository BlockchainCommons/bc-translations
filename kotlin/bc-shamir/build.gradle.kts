plugins {
    kotlin("jvm") version "2.2.21"
}

group = "com.blockchaincommons"
version = "0.13.0"
description = "Shamir's Secret Sharing (SSS) for Kotlin."

repositories {
    mavenCentral()
}

dependencies {
    implementation("com.blockchaincommons:bc-rand:0.5.0")
    implementation("com.blockchaincommons:bc-crypto:0.14.0")

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
