/// Specifies which fountain-coded fragment to begin simulated scanning from.
public enum StartFragment: Sendable {
    /// Start from the first fragment (sequence 1).
    case first

    /// Start from a specific fragment index (0-based).
    case index(Int)

    /// Start from a random fragment index.
    case random
}
