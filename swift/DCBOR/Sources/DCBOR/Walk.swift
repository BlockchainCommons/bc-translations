import Foundation

/// An element presented to a CBOR walk visitor.
public enum WalkElement: Sendable, Equatable {
    case single(CBOR)
    case keyValue(key: CBOR, value: CBOR)

    public func asSingle() -> CBOR? {
        if case .single(let cbor) = self {
            return cbor
        }
        return nil
    }

    public func asKeyValue() -> (CBOR, CBOR)? {
        if case .keyValue(let key, let value) = self {
            return (key, value)
        }
        return nil
    }

    public var diagnosticFlat: String {
        switch self {
        case .single(let cbor):
            return cbor.diagnosticFlat
        case .keyValue(let key, let value):
            return "\(key.diagnosticFlat): \(value.diagnosticFlat)"
        }
    }
}

/// Relationship between a visited node and its parent during traversal.
public enum EdgeType: Hashable, Sendable {
    case none
    case arrayElement(Int)
    case mapKeyValue
    case mapKey
    case mapValue
    case taggedContent

    public var label: String? {
        switch self {
        case .none:
            return nil
        case .arrayElement(let index):
            return "arr[\(index)]"
        case .mapKeyValue:
            return "kv"
        case .mapKey:
            return "key"
        case .mapValue:
            return "val"
        case .taggedContent:
            return "content"
        }
    }
}

public typealias Visitor<State> = (WalkElement, Int, EdgeType, State) -> (State, Bool)

public extension CBOR {
    func walk<State>(_ state: State, _ visit: Visitor<State>) {
        _walk(level: 0, incomingEdge: .none, state: state, visit)
    }

    private func _walk<State>(
        level: Int,
        incomingEdge: EdgeType,
        state: State,
        _ visit: Visitor<State>
    ) {
        var state = state
        let stop: Bool
        (state, stop) = visit(.single(self), level, incomingEdge, state)
        if stop {
            return
        }

        let nextLevel = level + 1
        switch self {
        case .array(let array):
            for (index, item) in array.enumerated() {
                item._walk(
                    level: nextLevel,
                    incomingEdge: .arrayElement(index),
                    state: state,
                    visit
                )
            }
        case .map(let map):
            for (key, value) in map {
                let newState: State
                let stopPair: Bool
                (newState, stopPair) = visit(
                    .keyValue(key: key, value: value),
                    nextLevel,
                    .mapKeyValue,
                    state
                )
                if stopPair {
                    continue
                }
                key._walk(level: nextLevel, incomingEdge: .mapKey, state: newState, visit)
                value._walk(level: nextLevel, incomingEdge: .mapValue, state: newState, visit)
            }
        case .tagged(_, let content):
            content._walk(
                level: nextLevel,
                incomingEdge: .taggedContent,
                state: state,
                visit
            )
        case .unsigned, .negative, .bytes, .text, .simple:
            break
        }
    }
}
