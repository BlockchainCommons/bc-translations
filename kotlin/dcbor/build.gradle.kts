plugins {
    kotlin("jvm") version "1.9.22"
}

group = "com.blockchaincommons"
version = "0.25.1"

repositories {
    mavenCentral()
}

dependencies {
    testImplementation(kotlin("test"))
    testRuntimeOnly("org.junit.platform:junit-platform-launcher")
}

tasks.test {
    useJUnitPlatform()
}

kotlin {
    jvmToolchain(19)
    compilerOptions {
        freeCompilerArgs.addAll(
            "-opt-in=kotlin.ExperimentalStdlibApi",
            "-opt-in=kotlin.ExperimentalUnsignedTypes"
        )
    }
}
