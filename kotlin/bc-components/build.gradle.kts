plugins {
    kotlin("jvm") version "2.2.21"
}

group = "com.blockchaincommons"
version = "0.31.1"
description = "Secure Components for Kotlin."

repositories {
    mavenCentral()
}

dependencies {
    implementation("com.blockchaincommons:bc-rand:0.5.0")
    implementation("com.blockchaincommons:bc-crypto:0.14.0")
    implementation("com.blockchaincommons:dcbor:0.25.1")
    implementation("com.blockchaincommons:bc-tags:0.12.0")
    implementation("com.blockchaincommons:bc-ur:0.19.0")
    implementation("com.blockchaincommons:sskr:0.12.0")
    implementation("org.bouncycastle:bcprov-jdk18on:1.79")

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
            "-opt-in=kotlin.ExperimentalStdlibApi"
        )
    }
}
