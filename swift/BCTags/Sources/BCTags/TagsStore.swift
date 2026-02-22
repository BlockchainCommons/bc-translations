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

    /// Returns the summarizer registered for a tag, or `nil` if none exists.
    func summarizer(for tag: Tag) -> TagSummarizer?
}

/// Returns the name for a tag using the given tags store, or the tag's numeric value as a string.
public func name(for tag: Tag, knownTags: TagsStoreProtocol?) -> String {
    knownTags?.name(for: tag) ?? String(tag.value)
}

/// A closure used to summarize tagged CBOR values in diagnostic output.
///
/// The first argument is the untagged payload object. Callers are expected to
/// pass a `CBOR` value, but the type is `Any` here to avoid introducing a
/// dependency cycle between `BCTags` and `DCBOR`.
public typealias TagSummarizer = @Sendable (_ untagged: Any, _ flat: Bool) throws -> String

/// A bidirectional mapping between tags and their names.
///
/// Tags can be looked up by numeric value or by name. The store also
/// conforms to `Sequence`, iterating tags in ascending numeric order.
public final class TagsStore: TagsStoreProtocol, @unchecked Sendable {
    /// All registered tags indexed by their numeric value.
    public private(set) var tagsByValue: [UInt64: Tag]

    /// All registered tags indexed by each of their names.
    public private(set) var tagsByName: [String: Tag]

    /// Per-tag summary closures used by diagnostic formatting.
    private var summarizersByValue: [UInt64: TagSummarizer]

    /// Creates a store pre-populated with the given tags.
    public init<T>(_ tags: T) where T: Sequence, T.Element == Tag {
        tagsByValue = [:]
        tagsByName = [:]
        summarizersByValue = [:]
        for tag in tags {
            Self.insertUnchecked(tag, tagsByValue: &tagsByValue, tagsByName: &tagsByName)
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
        Self.insertUnchecked(tag, tagsByValue: &tagsByValue, tagsByName: &tagsByName)
    }

    /// Inserts all tags from the given sequence.
    ///
    /// Each tag must have at least one name (i.e., `tag.names` must not be empty).
    @MainActor
    public func insertAll<T>(_ tags: T) where T: Sequence, T.Element == Tag {
        for tag in tags {
            Self.insertUnchecked(tag, tagsByValue: &tagsByValue, tagsByName: &tagsByName)
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

    public func summarizer(for tag: Tag) -> TagSummarizer? {
        summarizersByValue[tag.value]
    }

    /// Registers or replaces a summarizer for a tag.
    @MainActor
    public func setSummarizer(
        _ tag: Tag,
        _ summarizer: @escaping TagSummarizer
    ) {
        summarizersByValue[tag.value] = summarizer
    }

    private static func insertUnchecked(
        _ tag: Tag,
        tagsByValue: inout [UInt64: Tag],
        tagsByName: inout [String: Tag]
    ) {
        precondition(!tag.names.isEmpty, "tag must have at least one name")
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
    private let sortedTags: [Tag]
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
