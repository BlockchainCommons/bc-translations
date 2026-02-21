package com.blockchaincommons.dcbor

import kotlin.test.Test
import kotlin.test.assertContains
import kotlin.test.assertEquals
import kotlin.test.assertTrue

class WalkTest {

    private fun countVisits(cbor: Cbor): Int {
        var count = 0
        cbor.walk(Unit) { _, _, _, state ->
            count++
            state to false
        }
        return count
    }

    @Test
    fun testTraversalCounts() {
        // Simple array
        val array = listOf(1.toCbor(), 2.toCbor(), 3.toCbor()).toCbor()
        assertEquals(4, countVisits(array))

        // Simple map
        val map = CborMap()
        map.insert("a".toCbor(), 1.toCbor())
        map.insert("b".toCbor(), 2.toCbor())
        assertEquals(7, countVisits(Cbor.fromMap(map)))

        // Tagged value
        val tagged = Cbor.taggedValue(42uL, 100.toCbor())
        assertEquals(2, countVisits(tagged))

        // Nested structure
        val innerMap = CborMap()
        innerMap.insert("x".toCbor(), listOf(1.toCbor(), 2.toCbor()).toCbor())
        val outerMap = CborMap()
        outerMap.insert("inner".toCbor(), Cbor.fromMap(innerMap))
        outerMap.insert("simple".toCbor(), 42.toCbor())
        assertEquals(12, countVisits(Cbor.fromMap(outerMap)))
    }

    @Test
    fun testVisitorStateThreading() {
        val array = listOf(1.toCbor(), 2.toCbor(), 3.toCbor(), 4.toCbor(), 5.toCbor()).toCbor()
        var evenCount = 0

        array.walk(Unit) { element, _, _, state ->
            if (element is WalkElement.Single) {
                val case = element.cbor.cborCase
                if (case is CborCase.Unsigned && case.value % 2uL == 0uL) {
                    evenCount++
                }
            }
            state to false
        }

        assertEquals(2, evenCount)
    }

    @Test
    fun testDepthLimitedTraversal() {
        val level3 = CborMap()
        level3.insert("deep".toCbor(), "value".toCbor())

        val level2 = CborMap()
        level2.insert("level3".toCbor(), Cbor.fromMap(level3))

        val level1 = CborMap()
        level1.insert("level2".toCbor(), Cbor.fromMap(level2))

        val root = Cbor.fromMap(level1)
        val elementsByLevel = mutableMapOf<Int, Int>()

        root.walk(Unit) { _, level, _, state ->
            elementsByLevel[level] = (elementsByLevel[level] ?: 0) + 1
            val stop = level >= 2
            state to stop
        }

        assertEquals(1, elementsByLevel[0] ?: 0)
        assertEquals(3, elementsByLevel[1] ?: 0)
        assertEquals(1, elementsByLevel[2] ?: 0)
        assertEquals(0, elementsByLevel[3] ?: 0)
    }

    @Test
    fun testTextExtraction() {
        val metadata = CborMap()
        metadata.insert("title".toCbor(), "Important Document".toCbor())
        metadata.insert("author".toCbor(), "Alice Smith".toCbor())

        val content = CborMap()
        content.insert("body".toCbor(), "Lorem ipsum dolor sit amet".toCbor())
        content.insert("footer".toCbor(), "Copyright 2024".toCbor())

        val document = CborMap()
        document.insert("metadata".toCbor(), Cbor.fromMap(metadata))
        document.insert("content".toCbor(), Cbor.fromMap(content))
        document.insert("tags".toCbor(), listOf("urgent".toCbor(), "confidential".toCbor(), "draft".toCbor()).toCbor())

        val cbor = Cbor.fromMap(document)
        val texts = mutableListOf<String>()

        cbor.walk(Unit) { element, _, _, state ->
            when (element) {
                is WalkElement.Single -> {
                    val case = element.cbor.cborCase
                    if (case is CborCase.Text) texts.add(case.value)
                }
                is WalkElement.KeyValue -> {
                    val kCase = element.key.cborCase
                    val vCase = element.value.cborCase
                    if (kCase is CborCase.Text) texts.add(kCase.value)
                    if (vCase is CborCase.Text) texts.add(vCase.value)
                }
            }
            state to false
        }

        assertContains(texts, "Important Document")
        assertContains(texts, "Alice Smith")
        assertContains(texts, "Lorem ipsum dolor sit amet")
        assertContains(texts, "Copyright 2024")
        assertContains(texts, "urgent")
        assertContains(texts, "confidential")
        assertContains(texts, "draft")
        assertContains(texts, "title")
        assertContains(texts, "author")
        assertContains(texts, "body")
        assertContains(texts, "footer")
        assertContains(texts, "metadata")
        assertContains(texts, "content")
        assertContains(texts, "tags")
    }

    @Test
    fun testTraversalOrderAndEdgeTypes() {
        val map = CborMap()
        map.insert("a".toCbor(), listOf(1.toCbor(), 2.toCbor()).toCbor())
        map.insert("b".toCbor(), 42.toCbor())
        val cbor = Cbor.fromMap(map)

        val edgeTypes = mutableListOf<EdgeType>()

        cbor.walk(Unit) { _, _, edge, state ->
            edgeTypes.add(edge)
            state to false
        }

        assertEquals(EdgeType.None, edgeTypes[0])
        assertContains(edgeTypes, EdgeType.MapKeyValue)
        assertContains(edgeTypes, EdgeType.MapKey)
        assertContains(edgeTypes, EdgeType.MapValue)
        assertTrue(edgeTypes.any { it is EdgeType.ArrayElement && it.index == 0 })
        assertTrue(edgeTypes.any { it is EdgeType.ArrayElement && it.index == 1 })
    }

    @Test
    fun testTaggedValueTraversal() {
        val innerTagged = Cbor.taggedValue(123uL, listOf(1.toCbor(), 2.toCbor(), 3.toCbor()).toCbor())
        val outerTagged = Cbor.taggedValue(456uL, innerTagged)

        val edges = mutableListOf<EdgeType>()

        outerTagged.walk(Unit) { _, _, edge, state ->
            edges.add(edge)
            state to false
        }

        assertEquals(EdgeType.None, edges[0])
        assertEquals(EdgeType.TaggedContent, edges[1])
        assertEquals(EdgeType.TaggedContent, edges[2])
        assertEquals(EdgeType.ArrayElement(0), edges[3])
        assertEquals(EdgeType.ArrayElement(1), edges[4])
        assertEquals(EdgeType.ArrayElement(2), edges[5])
    }

    @Test
    fun testMapKeyValueSemantics() {
        val map = CborMap()
        map.insert("simple".toCbor(), 42.toCbor())
        map.insert("nested".toCbor(), listOf(1.toCbor(), 2.toCbor()).toCbor())
        val cbor = Cbor.fromMap(map)

        var kvCount = 0
        var individualCount = 0

        cbor.walk(Unit) { element, _, edge, state ->
            when (element) {
                is WalkElement.KeyValue -> {
                    kvCount++
                    assertEquals(EdgeType.MapKeyValue, edge)
                }
                is WalkElement.Single -> {
                    if (edge == EdgeType.MapKey || edge == EdgeType.MapValue) {
                        individualCount++
                    }
                }
            }
            state to false
        }

        assertEquals(2, kvCount)
        assertEquals(4, individualCount)
    }

    @Test
    fun testStopFlagPreventsDescent() {
        val nested = listOf(
            listOf(1.toCbor(), 2.toCbor(), 3.toCbor()).toCbor(),
            listOf(4.toCbor(), 5.toCbor(), 6.toCbor()).toCbor(),
            listOf(7.toCbor(), 8.toCbor(), 9.toCbor()).toCbor()
        ).toCbor()

        val visitLog = mutableListOf<String>()

        nested.walk(Unit) { element, level, edge, state ->
            val desc = "L$level: $edge - ${element.diagnosticFlat}"
            visitLog.add(desc)
            // Stop descent into the first nested array (at index 0)
            val stop = level == 1 && edge is EdgeType.ArrayElement && edge.index == 0
            state to stop
        }

        val logStr = visitLog.joinToString("\n")

        // Should visit the first array but not descend into it
        assertTrue(logStr.contains("[1, 2, 3]"))

        // Level 2 visits should not contain values from the first array
        val level2Lines = visitLog.filter { it.startsWith("L2:") }
        for (line in level2Lines) {
            assertTrue(!line.endsWith(" - 1") && !line.endsWith(" - 2") && !line.endsWith(" - 3"),
                "Unexpected value from first array at level 2: $line")
        }

        // Should visit second and third arrays with descent
        assertTrue(logStr.contains("[4, 5, 6]"))
        assertTrue(logStr.contains("[7, 8, 9]"))
    }

    @Test
    fun testEmptyStructures() {
        assertEquals(1, countVisits(listOf<Cbor>().toCbor()))
        val emptyMap = CborMap()
        assertEquals(1, countVisits(Cbor.fromMap(emptyMap)))
    }

    @Test
    fun testPrimitiveValues() {
        assertEquals(1, countVisits(42.toCbor()))
        assertEquals(1, countVisits("hello".toCbor()))
        assertEquals(1, countVisits(true.toCbor()))
        assertEquals(1, countVisits(Cbor.`null`()))
    }

    @Test
    fun testRealWorldDocument() {
        val person = CborMap()
        person.insert("name".toCbor(), "John Doe".toCbor())
        person.insert("age".toCbor(), 30.toCbor())
        person.insert("email".toCbor(), "john@example.com".toCbor())

        val address = CborMap()
        address.insert("street".toCbor(), "123 Main St".toCbor())
        address.insert("city".toCbor(), "Anytown".toCbor())
        address.insert("zipcode".toCbor(), "12345".toCbor())
        person.insert("address".toCbor(), Cbor.fromMap(address))

        person.insert("hobbies".toCbor(), listOf("reading".toCbor(), "cycling".toCbor(), "cooking".toCbor()).toCbor())

        val skills = CborMap()
        skills.insert("programming".toCbor(), listOf("Rust".toCbor(), "Python".toCbor(), "JavaScript".toCbor()).toCbor())
        skills.insert("languages".toCbor(), listOf("English".toCbor(), "Spanish".toCbor()).toCbor())
        person.insert("skills".toCbor(), Cbor.fromMap(skills))

        val document = Cbor.fromMap(person)
        val strings = mutableListOf<String>()

        document.walk(Unit) { element, _, _, state ->
            when (element) {
                is WalkElement.Single -> {
                    val case = element.cbor.cborCase
                    if (case is CborCase.Text) strings.add(case.value)
                }
                is WalkElement.KeyValue -> {
                    val kCase = element.key.cborCase
                    val vCase = element.value.cborCase
                    if (kCase is CborCase.Text) strings.add(kCase.value)
                    if (vCase is CborCase.Text) strings.add(vCase.value)
                }
            }
            state to false
        }

        assertContains(strings, "John Doe")
        assertContains(strings, "john@example.com")
        assertContains(strings, "123 Main St")
        assertContains(strings, "Anytown")
        assertContains(strings, "12345")
        assertContains(strings, "reading")
        assertContains(strings, "cycling")
        assertContains(strings, "cooking")
        assertContains(strings, "Rust")
        assertContains(strings, "Python")
        assertContains(strings, "JavaScript")
        assertContains(strings, "English")
        assertContains(strings, "Spanish")
        assertContains(strings, "name")
        assertContains(strings, "age")
        assertContains(strings, "email")
        assertContains(strings, "address")
        assertContains(strings, "hobbies")
        assertContains(strings, "skills")
        assertContains(strings, "programming")
        assertContains(strings, "languages")
    }
}
