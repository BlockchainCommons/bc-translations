import SwiftUI

struct DarkMode: ViewModifier {
    func body(content: Content) -> some View {
        content.environment(\.colorScheme, .dark)
    }
}

extension View {
    func darkMode() -> some View {
        modifier(DarkMode())
    }
}

struct LightMode: ViewModifier {
    func body(content: Content) -> some View {
        content.environment(\.colorScheme, .light)
    }
}

extension View {
    func lightMode() -> some View {
        modifier(LightMode())
    }
}
