import Foundation
import Testing
@testable import KnownValues

@Suite("Directory Loading Tests")
struct DirectoryLoadingTests {
    @Test func testGlobalRegistryStillWorks() {
        let store = KnownValuesStore.shared
        let foundIsA = store.knownValueNamed("isA")
        #expect(foundIsA != nil)
        #expect(foundIsA?.value == 1)
    }

    @Test func testLoadFromTempDirectory() throws {
        let tempDir = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(
            at: tempDir, withIntermediateDirectories: true
        )
        defer { try? FileManager.default.removeItem(at: tempDir) }

        let json = """
        {
            "entries": [
                {"codepoint": 99999, "name": "integrationTestValue"}
            ]
        }
        """
        try json.write(
            to: tempDir.appendingPathComponent("test_registry.json"),
            atomically: true,
            encoding: .utf8
        )

        var store = KnownValuesStore([KnownValue.isA, KnownValue.note])
        let count = try store.loadFromDirectory(at: tempDir)
        #expect(count == 1)

        let loaded = store.knownValueNamed("integrationTestValue")
        #expect(loaded != nil)
        #expect(loaded?.value == 99999)

        // Original values should still be present
        #expect(store.knownValueNamed("isA") != nil)
        #expect(store.knownValueNamed("note") != nil)
    }

    @Test func testOverrideHardcodedValue() throws {
        let tempDir = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(
            at: tempDir, withIntermediateDirectories: true
        )
        defer { try? FileManager.default.removeItem(at: tempDir) }

        let json = """
        {
            "entries": [
                {"codepoint": 1, "name": "overriddenIsA"}
            ]
        }
        """
        try json.write(
            to: tempDir.appendingPathComponent("override.json"),
            atomically: true,
            encoding: .utf8
        )

        var store = KnownValuesStore([KnownValue.isA])
        _ = try store.loadFromDirectory(at: tempDir)

        #expect(store.knownValueNamed("isA") == nil)

        let overridden = store.knownValueNamed("overriddenIsA")
        #expect(overridden != nil)
        #expect(overridden?.value == 1)
    }

    @Test func testMultipleFilesInDirectory() throws {
        let tempDir = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(
            at: tempDir, withIntermediateDirectories: true
        )
        defer { try? FileManager.default.removeItem(at: tempDir) }

        try #"{"entries": [{"codepoint": 10001, "name": "valueOne"}]}"#
            .write(
                to: tempDir.appendingPathComponent("registry1.json"),
                atomically: true,
                encoding: .utf8
            )
        try #"{"entries": [{"codepoint": 10002, "name": "valueTwo"}]}"#
            .write(
                to: tempDir.appendingPathComponent("registry2.json"),
                atomically: true,
                encoding: .utf8
            )

        var store = KnownValuesStore()
        let count = try store.loadFromDirectory(at: tempDir)
        #expect(count == 2)
        #expect(store.knownValueNamed("valueOne") != nil)
        #expect(store.knownValueNamed("valueTwo") != nil)
    }

    @Test func testDirectoryConfigCustomPaths() throws {
        let tempDir1 = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
        let tempDir2 = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(
            at: tempDir1, withIntermediateDirectories: true
        )
        try FileManager.default.createDirectory(
            at: tempDir2, withIntermediateDirectories: true
        )
        defer {
            try? FileManager.default.removeItem(at: tempDir1)
            try? FileManager.default.removeItem(at: tempDir2)
        }

        try #"{"entries": [{"codepoint": 20001, "name": "fromDirOne"}]}"#
            .write(
                to: tempDir1.appendingPathComponent("a.json"),
                atomically: true,
                encoding: .utf8
            )
        try #"{"entries": [{"codepoint": 20002, "name": "fromDirTwo"}]}"#
            .write(
                to: tempDir2.appendingPathComponent("b.json"),
                atomically: true,
                encoding: .utf8
            )

        let config = DirectoryConfig(paths: [tempDir1, tempDir2])
        var store = KnownValuesStore()
        let result = store.loadFromConfig(config)

        #expect(result.valuesCount == 2)
        #expect(store.knownValueNamed("fromDirOne") != nil)
        #expect(store.knownValueNamed("fromDirTwo") != nil)
    }

    @Test func testLaterDirectoryOverridesEarlier() throws {
        let tempDir1 = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
        let tempDir2 = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(
            at: tempDir1, withIntermediateDirectories: true
        )
        try FileManager.default.createDirectory(
            at: tempDir2, withIntermediateDirectories: true
        )
        defer {
            try? FileManager.default.removeItem(at: tempDir1)
            try? FileManager.default.removeItem(at: tempDir2)
        }

        try #"{"entries": [{"codepoint": 30000, "name": "firstVersion"}]}"#
            .write(
                to: tempDir1.appendingPathComponent("first.json"),
                atomically: true,
                encoding: .utf8
            )
        try #"{"entries": [{"codepoint": 30000, "name": "secondVersion"}]}"#
            .write(
                to: tempDir2.appendingPathComponent("second.json"),
                atomically: true,
                encoding: .utf8
            )

        let config = DirectoryConfig(paths: [tempDir1, tempDir2])
        var store = KnownValuesStore()
        store.loadFromConfig(config)

        let found = store.knownValueNamed("secondVersion")
        #expect(found != nil)
        #expect(found?.value == 30000)
        #expect(store.knownValueNamed("firstVersion") == nil)
    }

    @Test func testNonexistentDirectoryIsOk() throws {
        var store = KnownValuesStore()
        let result = try store.loadFromDirectory(
            at: URL(fileURLWithPath: "/nonexistent/path/12345")
        )
        #expect(result == 0)
    }

    @Test func testInvalidJsonIsError() throws {
        let tempDir = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(
            at: tempDir, withIntermediateDirectories: true
        )
        defer { try? FileManager.default.removeItem(at: tempDir) }

        try "{ this is not valid json }".write(
            to: tempDir.appendingPathComponent("invalid.json"),
            atomically: true,
            encoding: .utf8
        )

        var store = KnownValuesStore()
        #expect(throws: (any Error).self) {
            try store.loadFromDirectory(at: tempDir)
        }
    }

    @Test func testTolerantLoadingContinuesOnError() throws {
        let tempDir = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(
            at: tempDir, withIntermediateDirectories: true
        )
        defer { try? FileManager.default.removeItem(at: tempDir) }

        try #"{"entries": [{"codepoint": 40001, "name": "validValue"}]}"#
            .write(
                to: tempDir.appendingPathComponent("valid.json"),
                atomically: true,
                encoding: .utf8
            )

        try "{ invalid json }".write(
            to: tempDir.appendingPathComponent("invalid.json"),
            atomically: true,
            encoding: .utf8
        )

        let config = DirectoryConfig(paths: [tempDir])
        let result = DirectoryLoader.loadFromConfig(config)

        #expect(result.values[40001] != nil)
        #expect(result.hasErrors)
    }

    @Test func testFullRegistryFormat() throws {
        let tempDir = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(
            at: tempDir, withIntermediateDirectories: true
        )
        defer { try? FileManager.default.removeItem(at: tempDir) }

        let json = """
        {
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
        }
        """
        try json.write(
            to: tempDir.appendingPathComponent("full_format.json"),
            atomically: true,
            encoding: .utf8
        )

        var store = KnownValuesStore()
        let count = try store.loadFromDirectory(at: tempDir)
        #expect(count == 2)
        #expect(store.knownValueNamed("fullFormatValue") != nil)
        #expect(store.knownValueNamed("anotherValue") != nil)
    }

    @Test func testLoadResultMethods() throws {
        let tempDir = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(
            at: tempDir, withIntermediateDirectories: true
        )
        defer { try? FileManager.default.removeItem(at: tempDir) }

        try """
        {"entries": [
            {"codepoint": 60001, "name": "resultTest1"},
            {"codepoint": 60002, "name": "resultTest2"}
        ]}
        """.write(
            to: tempDir.appendingPathComponent("test.json"),
            atomically: true,
            encoding: .utf8
        )

        let config = DirectoryConfig(paths: [tempDir])
        let result = DirectoryLoader.loadFromConfig(config)

        #expect(result.valuesCount == 2)
        #expect(!result.hasErrors)
        #expect(result.filesProcessed.count == 1)
    }

    @Test func testEmptyEntriesArray() throws {
        let tempDir = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(
            at: tempDir, withIntermediateDirectories: true
        )
        defer { try? FileManager.default.removeItem(at: tempDir) }

        try #"{"entries": []}"#.write(
            to: tempDir.appendingPathComponent("empty.json"),
            atomically: true,
            encoding: .utf8
        )

        var store = KnownValuesStore()
        let count = try store.loadFromDirectory(at: tempDir)
        #expect(count == 0)
    }

    @Test func testNonJsonFilesIgnored() throws {
        let tempDir = FileManager.default.temporaryDirectory
            .appendingPathComponent(UUID().uuidString)
        try FileManager.default.createDirectory(
            at: tempDir, withIntermediateDirectories: true
        )
        defer { try? FileManager.default.removeItem(at: tempDir) }

        try #"{"entries": [{"codepoint": 70001, "name": "jsonValue"}]}"#
            .write(
                to: tempDir.appendingPathComponent("valid.json"),
                atomically: true,
                encoding: .utf8
            )

        try "Some text".write(
            to: tempDir.appendingPathComponent("readme.txt"),
            atomically: true,
            encoding: .utf8
        )
        try "<xml/>".write(
            to: tempDir.appendingPathComponent("data.xml"),
            atomically: true,
            encoding: .utf8
        )

        var store = KnownValuesStore()
        let count = try store.loadFromDirectory(at: tempDir)
        #expect(count == 1)
        #expect(store.knownValueNamed("jsonValue") != nil)
    }

    @Test func testParseRegistryJson() throws {
        let json = """
        {
            "ontology": {"name": "test"},
            "entries": [
                {"codepoint": 9999, "name": "testValue", "type": "property"}
            ]
        }
        """
        let data = json.data(using: .utf8)!
        let registry = try JSONDecoder().decode(RegistryFile.self, from: data)
        #expect(registry.entries.count == 1)
        #expect(registry.entries[0].codepoint == 9999)
        #expect(registry.entries[0].name == "testValue")
    }

    @Test func testParseMinimalRegistry() throws {
        let json = #"{"entries": [{"codepoint": 1, "name": "minimal"}]}"#
        let data = json.data(using: .utf8)!
        let registry = try JSONDecoder().decode(RegistryFile.self, from: data)
        #expect(registry.entries.count == 1)
        #expect(registry.entries[0].codepoint == 1)
    }

    @Test func testParseFullEntry() throws {
        let json = """
        {
            "entries": [{
                "codepoint": 100,
                "name": "fullEntry",
                "type": "class",
                "uri": "https://example.com/vocab#fullEntry",
                "description": "A complete entry with all fields"
            }]
        }
        """
        let data = json.data(using: .utf8)!
        let registry = try JSONDecoder().decode(RegistryFile.self, from: data)
        let entry = registry.entries[0]
        #expect(entry.codepoint == 100)
        #expect(entry.name == "fullEntry")
        #expect(entry.entryType == "class")
        #expect(entry.uri == "https://example.com/vocab#fullEntry")
        #expect(entry.description != nil)
    }

    @Test func testDirectoryConfigDefault() {
        let config = DirectoryConfig.defaultOnly()
        #expect(config.paths.count == 1)
        #expect(config.paths[0].lastPathComponent == ".known-values")
    }

    @Test func testDirectoryConfigExplicitPaths() {
        let config = DirectoryConfig(paths: [
            URL(fileURLWithPath: "/a"),
            URL(fileURLWithPath: "/b"),
        ])
        #expect(config.paths.count == 2)
        #expect(config.paths[0].path == "/a")
        #expect(config.paths[1].path == "/b")
    }

    @Test func testDirectoryConfigWithDefault() {
        let config = DirectoryConfig.withPathsAndDefault([
            URL(fileURLWithPath: "/custom")
        ])
        #expect(config.paths.count == 2)
        #expect(config.paths[0].path == "/custom")
        #expect(config.paths[1].lastPathComponent == ".known-values")
    }

    @Test func testLoadFromNonexistentDirectory() throws {
        let result = try DirectoryLoader.loadFromDirectory(
            at: URL(fileURLWithPath: "/nonexistent/path/12345")
        )
        #expect(result.isEmpty)
    }

    @Test func testLoadResultUnit() {
        let result = LoadResult()
        #expect(result.valuesCount == 0)
        #expect(!result.hasErrors)
    }
}
