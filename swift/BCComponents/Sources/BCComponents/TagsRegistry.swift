import BCTags

@MainActor
public func registerTagsIn(_ tagsStore: TagsStore) {
    BCTags.registerTagsIn(tagsStore)
}

@MainActor
public func registerTags() {
    BCTags.registerTags()
}
