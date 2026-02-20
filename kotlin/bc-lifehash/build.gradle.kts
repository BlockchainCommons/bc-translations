plugins {
    kotlin("jvm") version "1.9.22"
}

group = "com.blockchaincommons"
version = "0.1.0"

description = "LifeHash visual hashing algorithm"

repositories {
    mavenCentral()
}

dependencies {
    testImplementation(kotlin("test"))
    testImplementation("com.fasterxml.jackson.core:jackson-databind:2.17.2")
    testImplementation("com.fasterxml.jackson.module:jackson-module-kotlin:2.17.2")
    testRuntimeOnly("org.junit.platform:junit-platform-launcher")
}

tasks.test {
    useJUnitPlatform()
}

kotlin {
    jvmToolchain(21)
}
