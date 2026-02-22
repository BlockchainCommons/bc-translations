plugins {
    kotlin("jvm") version "2.2.21"
}

group = "com.blockchaincommons"
version = "0.15.4"
description = "Blockchain Commons Known Values."

repositories {
    mavenCentral()
}

dependencies {
    implementation("com.blockchaincommons:bc-components:0.31.1")
    implementation("com.blockchaincommons:dcbor:0.25.1")
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
