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

    private func view(for state: FragmentState) -> AnyView {
        switch state {
        case .off:
            return AnyView(Color.blue)
        case .on:
            return AnyView(Color.blue.brightness(0.2))
        case .highlighted:
            return AnyView(Color.blue.brightness(0.4))
        }
    }
}
