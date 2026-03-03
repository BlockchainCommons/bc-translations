import SwiftUI

/// Displays which fragments of a multi-part UR are currently displayed or being captured.
public struct URFragmentBar: View {
    @Binding var states: [FragmentState]

    public enum FragmentState: Sendable {
        case off
        case on
        case highlighted
    }

    public init(states: Binding<[FragmentState]>) {
        self._states = states
    }

    public var body: some View {
        HStack(spacing: 0) {
            ForEach(0..<states.count, id: \.self) { i in
                view(for: states[i])
            }
        }
        .frame(height: 14)
        .clipShape(Capsule())
    }

    // iOS system blue (#007AFF) desaturated by blending toward white.
    private static let highlightColor = Color(red: 128.0/255, green: 189.0/255, blue: 1.0) // #80BDFF
    private static let onColor = Color(red: 64.0/255, green: 156.0/255, blue: 1.0)         // #409CFF

    private func view(for state: FragmentState) -> AnyView {
        switch state {
        case .off:
            return AnyView(Color.blue)
        case .on:
            return AnyView(Self.onColor)
        case .highlighted:
            return AnyView(Self.highlightColor)
        }
    }
}
