import pathlib

from PIL import Image as PILImage

import bc_lifehash

OUT_DIR = pathlib.Path(__file__).resolve().parent.parent / "out"


def test_generate_pngs():
    versions = [
        ("version1", bc_lifehash.Version.VERSION1),
        ("version2", bc_lifehash.Version.VERSION2),
        ("detailed", bc_lifehash.Version.DETAILED),
        ("fiducial", bc_lifehash.Version.FIDUCIAL),
        ("grayscale_fiducial", bc_lifehash.Version.GRAYSCALE_FIDUCIAL),
    ]

    for name, version in versions:
        version_dir = OUT_DIR / name
        version_dir.mkdir(parents=True, exist_ok=True)

        for i in range(100):
            image = bc_lifehash.make_from_utf8(str(i), version, 1, False)

            pil_image = PILImage.frombytes(
                "RGB", (image.width, image.height), image.colors
            )
            pil_image.save(version_dir / f"{i}.png")
