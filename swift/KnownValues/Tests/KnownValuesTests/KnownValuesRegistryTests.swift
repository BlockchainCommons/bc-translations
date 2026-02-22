import Testing
@testable import KnownValues

@Suite("KnownValues Registry Tests")
struct KnownValuesRegistryTests {
    @Test func testBasicKnownValue() {
        #expect(KnownValue.isA.value == 1)
        #expect(KnownValue.isA.name == "isA")
        let store = KnownValuesStore.shared
        #expect(store.knownValueNamed("isA")?.value == 1)
    }

    @Test func testKnownValueNew() {
        let kv = KnownValue(42)
        #expect(kv.value == 42)
        #expect(kv.assignedName == nil)
        #expect(kv.name == "42")
    }

    @Test func testKnownValueWithName() {
        let kv = KnownValue(value: 1, name: "isA")
        #expect(kv.value == 1)
        #expect(kv.assignedName == "isA")
        #expect(kv.name == "isA")
    }

    @Test func testKnownValueEquality() {
        let a = KnownValue(value: 1, name: "isA")
        let b = KnownValue(value: 1, name: "differentName")
        let c = KnownValue(1)
        #expect(a == b)
        #expect(a == c)
        #expect(a != KnownValue(2))
    }

    @Test func testKnownValueHashing() {
        let a = KnownValue(value: 1, name: "isA")
        let b = KnownValue(value: 1, name: "differentName")
        var set = Set<KnownValue>()
        set.insert(a)
        set.insert(b)
        #expect(set.count == 1)
    }

    @Test func testKnownValueDescription() {
        #expect(KnownValue.isA.description == "isA")
        #expect(KnownValue(42).description == "42")
    }

    @Test func testKnownValueComparable() {
        #expect(KnownValue.isA < KnownValue.note)
        #expect(KnownValue(1) < KnownValue(2))
        let sorted = [KnownValue.note, KnownValue.isA, KnownValue.id].sorted()
        #expect(sorted.map(\.value) == [1, 2, 4])
    }

    @Test func testKnownValueIntegerLiteral() {
        let kv: KnownValue = 42
        #expect(kv.value == 42)
        #expect(kv.assignedName == nil)
    }

    @Test func testKnownValuesStoreCreation() {
        let store = KnownValuesStore([
            KnownValue.isA, KnownValue.note, KnownValue.signed,
        ])
        #expect(store.knownValueNamed("isA")?.value == 1)
        #expect(store.knownValueNamed("note")?.value == 4)
        #expect(store.knownValueNamed("signed")?.value == 3)
    }

    @Test func testKnownValuesStoreArrayLiteral() {
        let store: KnownValuesStore = [.isA, .note]
        #expect(store.knownValueNamed("isA")?.value == 1)
        #expect(store.knownValueNamed("note")?.value == 4)
    }

    @Test func testKnownValuesStoreInsert() {
        var store = KnownValuesStore()
        store.insert(KnownValue(value: 100, name: "customValue"))
        #expect(store.knownValueNamed("customValue")?.value == 100)
    }

    @Test func testKnownValuesStoreAssignedName() {
        let store = KnownValuesStore([KnownValue.isA, KnownValue.note])
        #expect(store.assignedName(for: .isA) == "isA")
        #expect(store.assignedName(for: KnownValue(999)) == nil)
    }

    @Test func testKnownValuesStoreName() {
        let store = KnownValuesStore([KnownValue.isA, KnownValue.note])
        #expect(store.name(for: .isA) == "isA")
        #expect(store.name(for: KnownValue(999)) == "999")
    }

    @Test func testKnownValueForRawValue() {
        let store = KnownValuesStore([KnownValue.isA, KnownValue.note])
        let found = KnownValuesStore.knownValue(forRawValue: 1, in: store)
        #expect(found.name == "isA")
        let unknown = KnownValuesStore.knownValue(forRawValue: 999, in: store)
        #expect(unknown.name == "999")
        let noStore = KnownValuesStore.knownValue(forRawValue: 1)
        #expect(noStore.name == "1")
    }

    @Test func testKnownValueForName() {
        let store = KnownValuesStore([KnownValue.isA, KnownValue.note])
        let found = KnownValuesStore.knownValue(forName: "isA", in: store)
        #expect(found?.value == 1)
        let notFound = KnownValuesStore.knownValue(
            forName: "unknown", in: store
        )
        #expect(notFound == nil)
        let noStore: KnownValue? = KnownValuesStore.knownValue(forName: "isA")
        #expect(noStore == nil)
    }

    @Test func testNameForKnownValue() {
        let store = KnownValuesStore([KnownValue.isA, KnownValue.note])
        #expect(KnownValuesStore.name(for: .isA, in: store) == "isA")
        #expect(
            KnownValuesStore.name(for: KnownValue(999), in: store) == "999"
        )
        #expect(KnownValuesStore.name(for: .isA) == "isA")
    }

    @Test func testInsertOverridesOldName() {
        var store = KnownValuesStore([KnownValue.isA])
        store.insert(KnownValue(value: 1, name: "overridden"))
        #expect(store.knownValueNamed("isA") == nil)
        #expect(store.knownValueNamed("overridden")?.value == 1)
    }

    @Test func testRawValueConstants() {
        #expect(KnownValue.unitRaw == 0)
        #expect(KnownValue.isARaw == 1)
        #expect(KnownValue.idRaw == 2)
        #expect(KnownValue.noteRaw == 4)
        #expect(KnownValue.graphRaw == 600)
        #expect(KnownValue.selfRaw == 706)
    }

    @Test func testAllRegistryConstants() {
        #expect(KnownValue.unit.value == 0)
        #expect(KnownValue.unit.name == "")
        #expect(KnownValue.isA.value == 1)
        #expect(KnownValue.note.value == 4)
        #expect(KnownValue.attachment.value == 50)
        #expect(KnownValue.allow.value == 60)
        #expect(KnownValue.privilegeAll.value == 70)
        #expect(KnownValue.body.value == 100)
        #expect(KnownValue.seedType.value == 200)
        #expect(KnownValue.asset.value == 300)
        #expect(KnownValue.network.value == 400)
        #expect(KnownValue.bip32KeyType.value == 500)
        #expect(KnownValue.graph.value == 600)
        #expect(KnownValue.node.value == 700)
        #expect(KnownValue.`self`.value == 706)
    }
}
