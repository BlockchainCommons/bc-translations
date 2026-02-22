import Foundation

/// A store that maps between Known Values and their assigned names.
///
/// The `KnownValuesStore` provides a bidirectional mapping between:
/// - Numeric values (UInt64) and their corresponding KnownValue instances
/// - String names and their corresponding KnownValue instances
///
/// This enables efficient lookup in both directions, making it possible to:
/// - Find the name for a given numeric value
/// - Find the numeric value for a given name
/// - Retrieve complete KnownValue instances by either name or value
public struct KnownValuesStore: Sendable {
    private var knownValuesByRawValue: [UInt64: KnownValue]
    private var knownValuesByAssignedName: [String: KnownValue]

    /// Creates a new KnownValuesStore with the provided Known Values.
    public init<S: Sequence>(_ knownValues: S) where S.Element == KnownValue {
        var byRawValue = [UInt64: KnownValue]()
        var byAssignedName = [String: KnownValue]()
        for knownValue in knownValues {
            Self.doInsert(
                knownValue,
                into: &byRawValue,
                nameIndex: &byAssignedName
            )
        }
        self.knownValuesByRawValue = byRawValue
        self.knownValuesByAssignedName = byAssignedName
    }

    /// Creates an empty KnownValuesStore.
    public init() {
        self.init(EmptyCollection<KnownValue>())
    }

    /// Inserts a KnownValue into the store.
    ///
    /// If the KnownValue has an assigned name, it will be indexed by both its
    /// raw value and its name. If a KnownValue with the same raw value or name
    /// already exists in the store, it will be replaced.
    public mutating func insert(_ knownValue: KnownValue) {
        Self.doInsert(
            knownValue,
            into: &knownValuesByRawValue,
            nameIndex: &knownValuesByAssignedName
        )
    }

    /// Returns the assigned name for a KnownValue, if present in the store.
    public func assignedName(for knownValue: KnownValue) -> String? {
        knownValuesByRawValue[knownValue.value]?.assignedName
    }

    /// Returns a human-readable name for a KnownValue.
    ///
    /// If the KnownValue has an assigned name in the store, that name is
    /// returned. Otherwise, the KnownValue's default name (which may be its
    /// numeric value as a string) is returned.
    public func name(for knownValue: KnownValue) -> String {
        assignedName(for: knownValue) ?? knownValue.name
    }

    /// Looks up a KnownValue by its assigned name.
    ///
    /// Returns the KnownValue if found, or nil if no KnownValue
    /// with the given name exists in the store.
    public func knownValueNamed(_ assignedName: String) -> KnownValue? {
        knownValuesByAssignedName[assignedName]
    }

    /// Retrieves a KnownValue for a raw value, using a store if provided.
    ///
    /// If a store is provided and contains a mapping for the raw value, that
    /// KnownValue is returned. Otherwise, a new unnamed KnownValue is created.
    public static func knownValue(
        forRawValue rawValue: UInt64,
        in store: KnownValuesStore? = nil
    ) -> KnownValue {
        if let store, let found = store.knownValuesByRawValue[rawValue] {
            return found
        }
        return KnownValue(rawValue)
    }

    /// Attempts to find a KnownValue by its name, using a store if provided.
    ///
    /// If a store is provided and contains a mapping for the name, that
    /// KnownValue is returned. Otherwise, nil is returned.
    public static func knownValue(
        forName name: String,
        in store: KnownValuesStore? = nil
    ) -> KnownValue? {
        store?.knownValueNamed(name)
    }

    /// Returns a human-readable name for a KnownValue, using a store if
    /// provided.
    public static func name(
        for knownValue: KnownValue,
        in store: KnownValuesStore? = nil
    ) -> String {
        if let store, let assignedName = store.assignedName(for: knownValue) {
            return assignedName
        }
        return knownValue.name
    }

    // MARK: - Directory Loading

    /// Loads and inserts known values from a directory containing JSON registry
    /// files.
    ///
    /// Scans the specified directory for `.json` files and parses them as known
    /// value registries. Values from JSON files override existing values in the
    /// store when codepoints match.
    ///
    /// - Parameter path: The directory to scan for JSON registry files.
    /// - Returns: The number of values loaded.
    public mutating func loadFromDirectory(at path: URL) throws -> Int {
        let values = try DirectoryLoader.loadFromDirectory(at: path)
        for value in values {
            insert(value)
        }
        return values.count
    }

    /// Loads known values from multiple directories using the provided
    /// configuration.
    ///
    /// Directories are processed in order. When multiple entries have the same
    /// codepoint, values from later directories override values from earlier
    /// directories.
    ///
    /// - Parameter config: The directory configuration specifying search paths.
    /// - Returns: A ``LoadResult`` containing information about the loading
    ///   operation.
    @discardableResult
    public mutating func loadFromConfig(
        _ config: DirectoryConfig
    ) -> LoadResult {
        let result = DirectoryLoader.loadFromConfig(config)
        for value in result.values.values {
            insert(value)
        }
        return result
    }

    // MARK: - Private

    private static func doInsert(
        _ knownValue: KnownValue,
        into byRawValue: inout [UInt64: KnownValue],
        nameIndex byAssignedName: inout [String: KnownValue]
    ) {
        // If there's an existing value with the same codepoint, remove its name
        // from the name index to avoid stale entries
        if let oldValue = byRawValue[knownValue.value],
           let oldName = oldValue.assignedName {
            byAssignedName.removeValue(forKey: oldName)
        }

        byRawValue[knownValue.value] = knownValue
        if let name = knownValue.assignedName {
            byAssignedName[name] = knownValue
        }
    }
}

// MARK: - ExpressibleByArrayLiteral

extension KnownValuesStore: ExpressibleByArrayLiteral {
    public init(arrayLiteral elements: KnownValue...) {
        self.init(elements)
    }
}
