plugins {
    kotlin("jvm") version "2.2.21"
}

group = "com.blockchaincommons"
version = "0.12.0"
description = "Sharded Secret Key Reconstruction (SSKR) for Kotlin."

repositories {
    mavenCentral()
}

dependencies {
    implementation("com.blockchaincommons:bc-rand:0.5.0")
    implementation("com.blockchaincommons:bc-shamir:0.13.0")

    testImplementation(kotlin("test"))
    testRuntimeOnly("org.junit.platform:junit-platform-launcher")
}

tasks.test {
    useJUnitPlatform()
}

kotlin {
    jvmToolchain(21)
}
