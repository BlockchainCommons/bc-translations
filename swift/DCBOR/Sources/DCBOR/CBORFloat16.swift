import Foundation
import BCFloat16

#if ((os(macOS) || targetEnvironment(macCatalyst)) && arch(x86_64))
public typealias CBORFloat16 = BCFloat16
#else
public typealias CBORFloat16 = Float16
#endif
