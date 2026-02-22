package com.blockchaincommons.knownvalues

import java.nio.file.Path
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertNotNull
import kotlin.test.assertTrue

class DirectoryLoaderTest {
    @Test
    fun testParseRegistryJson() {
        val json =
            """{
                "ontology": {"name": "test"},
                "entries": [
                    {"codepoint": 9999, "name": "testValue", "type": "property"}
                ],
                "statistics": {}
            }""".trimIndent()

        val registry = RegistryFile.fromJson(json)
        assertEquals(1, registry.entries.size)
        assertEquals(9999uL, registry.entries[0].codepoint)
        assertEquals("testValue", registry.entries[0].name)
    }

    @Test
    fun testParseMinimalRegistry() {
        val json = """{"entries": [{"codepoint": 1, "name": "minimal"}]}"""

        val registry = RegistryFile.fromJson(json)
        assertEquals(1, registry.entries.size)
        assertEquals(1uL, registry.entries[0].codepoint)
    }

    @Test
    fun testParseFullEntry() {
        val json =
            """{
                "entries": [{
                    "codepoint": 100,
                    "name": "fullEntry",
                    "type": "class",
                    "uri": "https://example.com/vocab#fullEntry",
                    "description": "A complete entry with all fields"
                }]
            }""".trimIndent()

        val registry = RegistryFile.fromJson(json)
        val entry = registry.entries[0]
        assertEquals(100uL, entry.codepoint)
        assertEquals("fullEntry", entry.name)
        assertEquals("class", entry.entryType)
        assertEquals("https://example.com/vocab#fullEntry", entry.uri)
        assertNotNull(entry.description)
    }

    @Test
    fun testDirectoryConfigDefault() {
        val config = DirectoryConfig.defaultOnly()
        assertEquals(1, config.paths().size)
        assertTrue(config.paths()[0].toString().endsWith(".known-values"))
    }

    @Test
    fun testDirectoryConfigCustomPaths() {
        val config = DirectoryConfig.withPaths(
            listOf(
                Path.of("/a"),
                Path.of("/b"),
            ),
        )
        assertEquals(2, config.paths().size)
        assertEquals(Path.of("/a"), config.paths()[0])
        assertEquals(Path.of("/b"), config.paths()[1])
    }

    @Test
    fun testDirectoryConfigWithDefault() {
        val config = DirectoryConfig.withPathsAndDefault(listOf(Path.of("/custom")))
        assertEquals(2, config.paths().size)
        assertEquals(Path.of("/custom"), config.paths()[0])
        assertTrue(config.paths()[1].toString().endsWith(".known-values"))
    }

    @Test
    fun testLoadFromNonexistentDirectory() {
        val result = loadFromDirectory(Path.of("/nonexistent/path/12345"))
        assertTrue(result.isEmpty())
    }

    @Test
    fun testLoadResultMethods() {
        val result = LoadResult()
        assertEquals(0, result.valuesCount)
        assertFalse(result.hasErrors)

        result.putValue(KnownValue.withName(1uL, "test"))
        assertEquals(1, result.valuesCount)
    }
}
