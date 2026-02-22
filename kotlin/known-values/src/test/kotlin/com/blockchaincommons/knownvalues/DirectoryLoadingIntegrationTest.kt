package com.blockchaincommons.knownvalues

import java.nio.file.Files
import java.nio.file.Path
import java.util.Comparator
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertNotNull
import kotlin.test.assertNull
import kotlin.test.assertTrue

class DirectoryLoadingIntegrationTest {
    @Test
    fun testGlobalRegistryStillWorks() {
        val store = KNOWN_VALUES
        val isA = store.knownValueNamed("isA")
        assertNotNull(isA)
        assertEquals(1uL, isA.value)
    }

    @Test
    fun testLoadFromTempDirectory() {
        withTempDirectory { tempDir ->
            val filePath = tempDir.resolve("test_registry.json")
            val json =
                """{
                    "entries": [
                        {"codepoint": 99999, "name": "integrationTestValue"}
                    ]
                }""".trimIndent()
            Files.writeString(filePath, json)

            val store = KnownValuesStore(listOf(IS_A, NOTE))
            val count = store.loadFromDirectory(tempDir)
            assertEquals(1, count)

            val loaded = store.knownValueNamed("integrationTestValue")
            assertNotNull(loaded)
            assertEquals(99999uL, loaded.value)

            assertNotNull(store.knownValueNamed("isA"))
            assertNotNull(store.knownValueNamed("note"))
        }
    }

    @Test
    fun testOverrideHardcodedValue() {
        withTempDirectory { tempDir ->
            val filePath = tempDir.resolve("override.json")
            val json =
                """{
                    "entries": [
                        {"codepoint": 1, "name": "overriddenIsA"}
                    ]
                }""".trimIndent()
            Files.writeString(filePath, json)

            val store = KnownValuesStore(listOf(IS_A))
            store.loadFromDirectory(tempDir)

            assertNull(store.knownValueNamed("isA"))
            val overridden = store.knownValueNamed("overriddenIsA")
            assertNotNull(overridden)
            assertEquals(1uL, overridden.value)
        }
    }

    @Test
    fun testMultipleFilesInDirectory() {
        withTempDirectory { tempDir ->
            val file1 = tempDir.resolve("registry1.json")
            val file2 = tempDir.resolve("registry2.json")

            Files.writeString(file1, """{"entries": [{"codepoint": 10001, "name": "valueOne"}]}""")
            Files.writeString(file2, """{"entries": [{"codepoint": 10002, "name": "valueTwo"}]}""")

            val store = KnownValuesStore()
            val count = store.loadFromDirectory(tempDir)

            assertEquals(2, count)
            assertNotNull(store.knownValueNamed("valueOne"))
            assertNotNull(store.knownValueNamed("valueTwo"))
        }
    }

    @Test
    fun testDirectoryConfigCustomPaths() {
        withTempDirectoryPair { tempDir1, tempDir2 ->
            Files.writeString(
                tempDir1.resolve("a.json"),
                """{"entries": [{"codepoint": 20001, "name": "fromDirOne"}]}""",
            )
            Files.writeString(
                tempDir2.resolve("b.json"),
                """{"entries": [{"codepoint": 20002, "name": "fromDirTwo"}]}""",
            )

            val config = DirectoryConfig.withPaths(listOf(tempDir1, tempDir2))
            val store = KnownValuesStore()
            val result = store.loadFromConfig(config)

            assertEquals(2, result.valuesCount)
            assertNotNull(store.knownValueNamed("fromDirOne"))
            assertNotNull(store.knownValueNamed("fromDirTwo"))
        }
    }

    @Test
    fun testLaterDirectoryOverridesEarlier() {
        withTempDirectoryPair { tempDir1, tempDir2 ->
            Files.writeString(
                tempDir1.resolve("first.json"),
                """{"entries": [{"codepoint": 30000, "name": "firstVersion"}]}""",
            )
            Files.writeString(
                tempDir2.resolve("second.json"),
                """{"entries": [{"codepoint": 30000, "name": "secondVersion"}]}""",
            )

            val config = DirectoryConfig.withPaths(listOf(tempDir1, tempDir2))
            val store = KnownValuesStore()
            store.loadFromConfig(config)

            val value = store.knownValueNamed("secondVersion")
            assertNotNull(value)
            assertEquals(30000uL, value.value)
            assertNull(store.knownValueNamed("firstVersion"))
        }
    }

    @Test
    fun testNonexistentDirectoryIsOk() {
        val store = KnownValuesStore()
        val count = store.loadFromDirectory(Path.of("/nonexistent/path/12345"))
        assertEquals(0, count)
    }

    @Test
    fun testInvalidJsonIsError() {
        withTempDirectory { tempDir ->
            val filePath = tempDir.resolve("invalid.json")
            Files.writeString(filePath, "{ this is not valid json }")

            val store = KnownValuesStore()
            assertFailsWith<LoadError.Json> {
                store.loadFromDirectory(tempDir)
            }
        }
    }

    @Test
    fun testTolerantLoadingContinuesOnError() {
        withTempDirectory { tempDir ->
            Files.writeString(
                tempDir.resolve("valid.json"),
                """{"entries": [{"codepoint": 40001, "name": "validValue"}]}""",
            )
            Files.writeString(tempDir.resolve("invalid.json"), "{ invalid json }")

            val config = DirectoryConfig.withPaths(listOf(tempDir))
            val result = loadFromConfig(config)

            assertTrue(result.containsValue(40001uL))
            assertTrue(result.hasErrors)
        }
    }

    @Test
    fun testFullRegistryFormat() {
        withTempDirectory { tempDir ->
            val filePath = tempDir.resolve("full_format.json")
            val json =
                """{
                    "ontology": {
                        "name": "test_registry",
                        "source_url": "https://example.com",
                        "start_code_point": 50000,
                        "processing_strategy": "test"
                    },
                    "generated": {
                        "tool": "test"
                    },
                    "entries": [
                        {
                            "codepoint": 50001,
                            "name": "fullFormatValue",
                            "type": "property",
                            "uri": "https://example.com/vocab#fullFormatValue",
                            "description": "A value in full format"
                        },
                        {
                            "codepoint": 50002,
                            "name": "anotherValue",
                            "type": "class"
                        }
                    ],
                    "statistics": {
                        "total_entries": 2
                    }
                }""".trimIndent()
            Files.writeString(filePath, json)

            val store = KnownValuesStore()
            val count = store.loadFromDirectory(tempDir)

            assertEquals(2, count)
            assertNotNull(store.knownValueNamed("fullFormatValue"))
            assertNotNull(store.knownValueNamed("anotherValue"))
        }
    }

    @Test
    fun testLoadResultMethods() {
        withTempDirectory { tempDir ->
            Files.writeString(
                tempDir.resolve("test.json"),
                """{"entries": [
                    {"codepoint": 60001, "name": "resultTest1"},
                    {"codepoint": 60002, "name": "resultTest2"}
                ]}""".trimIndent(),
            )

            val config = DirectoryConfig.withPaths(listOf(tempDir))
            val result = loadFromConfig(config)

            assertEquals(2, result.valuesCount)
            assertTrue(!result.hasErrors)
            assertEquals(1, result.filesProcessed.size)

            val values = result.values().toList()
            assertEquals(2, values.size)
        }
    }

    @Test
    fun testEmptyEntriesArray() {
        withTempDirectory { tempDir ->
            Files.writeString(tempDir.resolve("empty.json"), """{"entries": []}""")

            val store = KnownValuesStore()
            val count = store.loadFromDirectory(tempDir)

            assertEquals(0, count)
        }
    }

    @Test
    fun testNonJsonFilesIgnored() {
        withTempDirectory { tempDir ->
            Files.writeString(
                tempDir.resolve("valid.json"),
                """{"entries": [{"codepoint": 70001, "name": "jsonValue"}]}""",
            )
            Files.writeString(tempDir.resolve("readme.txt"), "Some text")
            Files.writeString(tempDir.resolve("data.xml"), "<xml/>")

            val store = KnownValuesStore()
            val count = store.loadFromDirectory(tempDir)

            assertEquals(1, count)
            assertNotNull(store.knownValueNamed("jsonValue"))
        }
    }

    private fun withTempDirectory(block: (Path) -> Unit) {
        val dir = Files.createTempDirectory("known-values-test")
        try {
            block(dir)
        } finally {
            deleteRecursively(dir)
        }
    }

    private fun withTempDirectoryPair(block: (Path, Path) -> Unit) {
        val dir1 = Files.createTempDirectory("known-values-test-a")
        val dir2 = Files.createTempDirectory("known-values-test-b")
        try {
            block(dir1, dir2)
        } finally {
            deleteRecursively(dir1)
            deleteRecursively(dir2)
        }
    }

    private fun deleteRecursively(path: Path) {
        if (!Files.exists(path)) {
            return
        }

        Files.walk(path).use { walk ->
            walk
                .sorted(Comparator.reverseOrder())
                .forEach { file -> Files.deleteIfExists(file) }
        }
    }
}
