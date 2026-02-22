/// A validated UR type string.
public struct URType: Equatable, Hashable, Sendable {
    private let rawValue: String

    /// Creates a new `URType` after validating that every character is
    /// a lowercase ASCII letter, ASCII digit, or `-`.
    public init(_ value: String) throws {
        guard value.isURTypeString else {
            throw URError.invalidType
        }
        self.rawValue = value
    }

    /// Returns the string representation of the UR type.
    public var string: String {
        rawValue
    }
}

extension URType: CustomStringConvertible {
    public var description: String {
        rawValue
    }
}

private extension Character {
    var isURTypeCharacter: Bool {
        if self >= "a" && self <= "z" {
            return true
        }
        if self >= "0" && self <= "9" {
            return true
        }
        return self == "-"
    }
}

private extension String {
    var isURTypeString: Bool {
        allSatisfy { $0.isURTypeCharacter }
    }
}
