/**
 * LifeHash algorithm version.
 *
 * - **Version1** / **Version2** — 16x16 grid, up to 150 generations.
 * - **Detailed** — 32x32 grid, up to 300 generations, richer color gradients.
 * - **Fiducial** — 32x32, designed for use as fiducial markers.
 * - **GrayscaleFiducial** — Same as Fiducial but rendered in grayscale.
 */
export enum Version {
    Version1 = 'version1',
    Version2 = 'version2',
    Detailed = 'detailed',
    Fiducial = 'fiducial',
    GrayscaleFiducial = 'grayscale_fiducial',
}
