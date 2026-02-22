/// Internal representation of a single SSKR share with its metadata.
struct SSKRShare: Sendable {
    let identifier: UInt16
    let groupIndex: Int
    let groupThreshold: Int
    let groupCount: Int
    let memberIndex: Int
    let memberThreshold: Int
    let value: Secret
}
