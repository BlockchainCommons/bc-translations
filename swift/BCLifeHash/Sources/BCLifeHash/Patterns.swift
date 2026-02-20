enum Pattern {
    case snowflake
    case pinwheel
    case fiducial
}

func selectPattern(_ entropy: BitEnumerator, _ version: Version) -> Pattern {
    switch version {
    case .fiducial, .grayscaleFiducial:
        return .fiducial
    default:
        return entropy.next() ? .snowflake : .pinwheel
    }
}
