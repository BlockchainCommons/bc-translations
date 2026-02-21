/// A type that can map between tags and their names.
public protocol TagsStoreProtocol {
    /// Returns the name assigned to the tag in this store, or `nil` if unregistered.
    func assignedName(for tag: Tag) -> String?

    /// Returns the name assigned to the tag, falling back to the tag's numeric value as a string.
    func name(for tag: Tag) -> String

    /// Returns the tag registered for the given numeric value, or `nil` if not found.
    func tag(for value: UInt64) -> Tag?

    /// Returns the tag registered for the given name, or `nil` if not found.
    func tag(for name: String) -> Tag?
}

/// Returns the name for a tag using the given tags store, or the tag's numeric value as a string.
public func name(for tag: Tag, knownTags: TagsStoreProtocol?) -> String {
    knownTags?.name(for: tag) ?? String(tag.value)
}

/// A bidirectional mapping between tags and their names.
///
/// Tags can be looked up by numeric value or by name. The store also
/// conforms to `Sequence`, iterating tags in ascending numeric order.
public final class TagsStore: TagsStoreProtocol, @unchecked Sendable {
    /// All registered tags indexed by their numeric value.
    public private(set) var tagsByValue: [UInt64: Tag]

    /// All registered tags indexed by each of their names.
    public private(set) var tagsByName: [String: Tag]

    /// Creates a store pre-populated with the given tags.
    public init<T>(_ tags: T) where T: Sequence, T.Element == Tag {
        tagsByValue = [:]
        tagsByName = [:]
        for tag in tags {
            Self._insert(tag, tagsByValue: &tagsByValue, tagsByName: &tagsByName)
        }
    }

    /// Creates an empty store.
    public convenience init() {
        self.init(EmptyCollection<Tag>())
    }

    /// Inserts a tag into the store.
    ///
    /// The tag must have at least one name (i.e., `tag.names` must not be empty).
    @MainActor
    public func insert(_ tag: Tag) {
        Self._insert(tag, tagsByValue: &tagsByValue, tagsByName: &tagsByName)
    }

    /// Inserts all tags from the given sequence.
    ///
    /// Each tag must have at least one name (i.e., `tag.names` must not be empty).
    @MainActor
    public func insertAll<T>(_ tags: T) where T: Sequence, T.Element == Tag {
        for tag in tags {
            Self._insert(tag, tagsByValue: &tagsByValue, tagsByName: &tagsByName)
        }
    }

    public func assignedName(for tag: Tag) -> String? {
        self.tag(for: tag.value)?.name
    }

    public func name(for tag: Tag) -> String {
        assignedName(for: tag) ?? String(tag.value)
    }

    public func tag(for name: String) -> Tag? {
        tagsByName[name]
    }

    public func tag(for value: UInt64) -> Tag? {
        tagsByValue[value]
    }

    static func _insert(_ tag: Tag, tagsByValue: inout [UInt64: Tag], tagsByName: inout [String: Tag]) {
        precondition(!tag.names.isEmpty)
        tagsByValue[tag.value] = tag
        for name in tag.names {
            tagsByName[name] = tag
        }
    }
}

extension TagsStore: Sequence {
    public func makeIterator() -> TagsIterator {
        TagsIterator(tagsByValue: tagsByValue)
    }
}

/// An iterator that yields tags in ascending numeric order.
public struct TagsIterator: IteratorProtocol {
    private var sortedTags: [Tag]
    private var currentIndex = 0

    init(tagsByValue: [UInt64: Tag]) {
        self.sortedTags = tagsByValue.values.sorted { $0.value < $1.value }
    }

    public mutating func next() -> Tag? {
        guard currentIndex < sortedTags.count else { return nil }
        let tag = sortedTags[currentIndex]
        currentIndex += 1
        return tag
    }
}

extension TagsStore: ExpressibleByArrayLiteral {
    public convenience init(arrayLiteral elements: Tag...) {
        self.init(elements)
    }
}

/// The shared global tag store.
public let globalTags = TagsStore()
