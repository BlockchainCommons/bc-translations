import Foundation
import Testing
import DCBOR

private func countVisits(_ cbor: CBOR) -> Int {
    var count = 0
    cbor.walk(()) { _, _, _, state in
        count += 1
        return (state, false)
    }
    return count
}

struct WalkTests {
    @Test func testTraversalCounts() {
        let array: CBOR = [1, 2, 3]
        #expect(countVisits(array) == 4)

        var map = Map()
        map.insert("a", 1)
        map.insert("b", 2)
        #expect(countVisits(map.cbor) == 7)

        let tagged = CBOR.tagged(42, CBOR(100))
        #expect(countVisits(tagged) == 2)

        var innerMap = Map()
        innerMap.insert("x", [1, 2])
        var outerMap = Map()
        outerMap.insert("inner", innerMap)
        outerMap.insert("simple", 42)
        #expect(countVisits(outerMap.cbor) == 12)
    }

    @Test func testVisitorStateThreading() {
        let array: CBOR = [1, 2, 3]
        var states: [Int] = []

        array.walk(0) { _, _, _, state in
            states.append(state)
            return (state + 1, false)
        }

        #expect(states == [0, 1, 1, 1])
    }

    @Test func testEarlyTermination() {
        let cbor: CBOR = [
            ["should", "see", "this"],
            "abort_marker",
            ["should", "not", "see"],
        ]

        var foundAbort = false
        var visits: [(Int, String)] = []

        cbor.walk(()) { element, level, edge, state in
            visits.append((level, element.diagnosticFlat))

            if case .single(let node) = element,
               case .text(let text) = node,
               text == "abort_marker" {
                foundAbort = true
                return (state, true)
            }

            let stop = foundAbort && edge == .arrayElement(2) && level == 1
            return (state, stop)
        }

        #expect(visits.contains { $0.1.contains("abort_marker") })
        #expect(visits.contains { $0.1 == "\"should\"" })
        #expect(visits.contains { $0.1 == "\"this\"" })
        #expect(visits.contains { $0.1.contains("[\"should\", \"not\", \"see\"]") })

        if let index = visits.firstIndex(where: { $0.0 == 1 && $0.1.contains("[\"should\", \"not\", \"see\"]") }) {
            let hasLevel2After = visits.suffix(from: index + 1).contains { $0.0 == 2 }
            #expect(!hasLevel2After)
        } else {
            #expect(Bool(false))
        }
    }

    @Test func testDepthLimitedTraversal() {
        var level3 = Map()
        level3.insert("deep", "value")

        var level2 = Map()
        level2.insert("level3", level3)

        var level1 = Map()
        level1.insert("level2", level2)

        let root = level1.cbor
        let maxDepth = 2
        var maxSeenLevel = 0
        var sawDeepValue = false

        root.walk(()) { element, level, _, state in
            maxSeenLevel = max(maxSeenLevel, level)
            if case .single(let cbor) = element,
               case .text(let text) = cbor,
               text == "value" {
                sawDeepValue = true
            }
            return (state, level >= maxDepth)
        }

        #expect(maxSeenLevel == 2)
        #expect(!sawDeepValue)
    }

    @Test func testTextExtraction() {
        var profile = Map()
        profile.insert("name", "Alice")
        profile.insert("city", "Paris")

        let doc: CBOR = [
            "root",
            profile,
            ["notes", "hello", "world"],
            CBOR.tagged(300, CBOR("tagged-text")),
        ]

        var texts: [String] = []
        doc.walk(()) { element, _, _, state in
            if case .single(let cbor) = element,
               case .text(let text) = cbor {
                texts.append(text)
            }
            return (state, false)
        }

        #expect(texts.contains("root"))
        #expect(texts.contains("Alice"))
        #expect(texts.contains("Paris"))
        #expect(texts.contains("hello"))
        #expect(texts.contains("world"))
        #expect(texts.contains("tagged-text"))
    }

    @Test func testTraversalOrderAndEdgeTypes() {
        var map = Map()
        map.insert("items", [1, 2])
        map.insert("flag", true)

        var edges: [EdgeType] = []
        map.cbor.walk(()) { _, _, edge, state in
            edges.append(edge)
            return (state, false)
        }

        #expect(edges.first == EdgeType.none)
        #expect(edges.contains(.mapKeyValue))
        #expect(edges.contains(.mapKey))
        #expect(edges.contains(.mapValue))
        #expect(edges.contains(.arrayElement(0)))
        #expect(edges.contains(.arrayElement(1)))

        if let kv = edges.firstIndex(of: .mapKeyValue),
           let key = edges.firstIndex(of: .mapKey) {
            #expect(kv < key)
        } else {
            #expect(Bool(false))
        }
    }

    @Test func testTaggedValueTraversal() {
        let cbor = CBOR.tagged(100, CBOR.tagged(200, CBOR("hello")))
        var edges: [EdgeType] = []
        var sawHello = false

        cbor.walk(()) { element, _, edge, state in
            edges.append(edge)
            if case .single(let node) = element,
               case .text(let text) = node,
               text == "hello" {
                sawHello = true
            }
            return (state, false)
        }

        #expect(edges == [.none, .taggedContent, .taggedContent])
        #expect(sawHello)
    }

    @Test func testMapKeyValueSemantics() {
        var map = Map()
        map.insert("name", "Alice")
        map.insert("age", 30)

        var keyValueVisits = 0
        var keyVisits = 0
        var valueVisits = 0

        map.cbor.walk(()) { element, _, edge, state in
            switch element {
            case .keyValue:
                if edge == .mapKeyValue {
                    keyValueVisits += 1
                }
            case .single:
                if edge == .mapKey {
                    keyVisits += 1
                } else if edge == .mapValue {
                    valueVisits += 1
                }
            }
            return (state, false)
        }

        #expect(keyValueVisits == 2)
        #expect(keyVisits == 2)
        #expect(valueVisits == 2)
    }

    @Test func testStopFlagPreventsDescent() {
        let cbor: CBOR = [[1, 2, 3], [4, 5, 6]]
        var visits: [String] = []

        cbor.walk(()) { element, level, edge, state in
            visits.append("L\(level): \(element.diagnosticFlat)")
            if edge == .arrayElement(0) && level == 1 {
                return (state, true)
            }
            return (state, false)
        }

        #expect(visits.contains { $0.contains("[1, 2, 3]") })
        #expect(!visits.contains { $0 == "L2: 1" || $0 == "L2: 2" || $0 == "L2: 3" })
        #expect(visits.contains { $0 == "L2: 4" })
        #expect(visits.contains { $0 == "L2: 5" })
        #expect(visits.contains { $0 == "L2: 6" })
    }

    @Test func testEmptyStructures() {
        let emptyArray: CBOR = []
        #expect(countVisits(emptyArray) == 1)

        let emptyMap = Map()
        #expect(countVisits(emptyMap.cbor) == 1)
    }

    @Test func testPrimitiveValues() {
        let values: [CBOR] = [
            CBOR(42),
            CBOR(-1),
            CBOR("text"),
            Data([0x01, 0x02]).cbor,
            .true,
            .false,
            .null,
        ]

        for value in values {
            #expect(countVisits(value) == 1)
        }
    }

    @Test func testRealWorldDocument() {
        var contact = Map()
        contact.insert("name", "Alice")
        contact.insert("role", "Engineer")

        var doc = Map()
        doc.insert("version", 1)
        doc.insert("title", "Example")
        doc.insert("contact", contact)
        doc.insert("tags", ["swift", "dcbor", "walk"])

        var texts: [String] = []
        doc.cbor.walk(()) { element, _, _, state in
            if case .single(let cbor) = element,
               case .text(let text) = cbor {
                texts.append(text)
            }
            return (state, false)
        }

        #expect(texts.contains("Example"))
        #expect(texts.contains("Alice"))
        #expect(texts.contains("Engineer"))
        #expect(texts.contains("swift"))
        #expect(texts.contains("dcbor"))
        #expect(texts.contains("walk"))
    }
}

struct SetTests {
    @Test func testSetInsertContainsAndRoundTrip() throws {
        let set = DCBOR.Set.fromArray([3, 1, 2, 2])
        #expect(set.count == 3)
        #expect(set.contains(1))
        #expect(set.contains(2))
        #expect(set.contains(3))

        let ordered = set.asArray().compactMap { try? Int64(cbor: $0) }
        #expect(ordered == [1, 2, 3])

        let roundTrip = try DCBOR.Set(cbor: set.cbor)
        #expect(roundTrip == set)
    }

    @Test func testSetTryFromArrayRejectsDuplicate() {
        #expect {
            _ = try DCBOR.Set.tryFromArray([CBOR(1), CBOR(1)])
        } throws: { error in
            try #require(error as? CBORError == CBORError.duplicateMapKey)
            return true
        }
    }

    @Test func testSetTryFromArrayRejectsMisorder() {
        #expect {
            _ = try DCBOR.Set.tryFromArray([CBOR("z"), CBOR("a")])
        } throws: { error in
            try #require(error as? CBORError == CBORError.misorderedMapKey)
            return true
        }
    }

    @Test func testEdgeTypeLabels() {
        #expect(EdgeType.none.label == nil)
        #expect(EdgeType.arrayElement(0).label == "arr[0]")
        #expect(EdgeType.mapKeyValue.label == "kv")
        #expect(EdgeType.mapKey.label == "key")
        #expect(EdgeType.mapValue.label == "val")
        #expect(EdgeType.taggedContent.label == "content")
    }
}
