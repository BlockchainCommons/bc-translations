/** The display state of a single fragment in a multi-part UR. */
export enum FragmentState {
  /** Not yet displayed or received. */
  Off = "off",
  /** Currently being displayed or received. */
  On = "on",
  /** Just received / highlighted. */
  Highlighted = "highlighted",
}
