plugins {
    kotlin("jvm") version "2.2.21"
}

group = "com.blockchaincommons"
version = "0.14.0"

repositories {
    mavenCentral()
}

dependencies {
    implementation("com.blockchaincommons:bc-rand:0.5.0")
    implementation("org.bouncycastle:bcprov-jdk18on:1.79")
    implementation("fr.acinq.secp256k1:secp256k1-kmp-jvm:0.22.0")
    implementation("fr.acinq.secp256k1:secp256k1-kmp-jni-jvm:0.22.0")

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
