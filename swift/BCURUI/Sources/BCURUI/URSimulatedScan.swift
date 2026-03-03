import SwiftUI

/// Displays a simulated camera scan by showing the QR codes being "scanned"
/// as visual feedback while `URSimulatedScanState` feeds parts to a `URScanState`.
public struct URSimulatedScan: View {
    var state: URSimulatedScanState
    let showFragmentBar: Bool

    public init(state: URSimulatedScanState, showFragmentBar: Bool = true) {
        self.state = state
        self.showFragmentBar = showFragmentBar
    }

    public var body: some View {
        VStack(spacing: 0) {
            ZStack {
                Color.black

                URQRCode(
                    data: Binding(
                        get: { state.currentPart },
                        set: { _ in }
                    ),
                    foregroundColor: .white,
                    backgroundColor: .black
                )
                .padding()
            }

            if showFragmentBar {
                URFragmentBar(states: Binding(
                    get: { state.fragmentStates },
                    set: { _ in }
                ))

                Text("Simulated Scan")
                    .font(.caption)
                    .foregroundStyle(.secondary)
                    .padding(.top, 4)
            }
        }
        .onAppear { state.run() }
        .onDisappear { state.stop() }
    }
}
