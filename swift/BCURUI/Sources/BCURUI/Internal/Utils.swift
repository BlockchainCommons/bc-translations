import Foundation

extension String {
    var utf8Data: Data {
        data(using: .utf8)!
    }
}
