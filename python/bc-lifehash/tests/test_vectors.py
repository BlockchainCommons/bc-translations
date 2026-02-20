import json
import pathlib

import bc_lifehash

VECTORS_PATH = pathlib.Path(__file__).parent / "test-vectors.json"


def _parse_version(s: str) -> bc_lifehash.Version:
    return bc_lifehash.Version(s)


def test_all_vectors():
    with open(VECTORS_PATH) as f:
        vectors = json.load(f)

    assert len(vectors) == 35, f"Expected 35 test vectors, got {len(vectors)}"

    for i, tv in enumerate(vectors):
        version = _parse_version(tv["version"])

        if tv["input_type"] == "hex":
            if tv["input"] == "":
                data = b""
            else:
                data = bytes.fromhex(tv["input"])
            image = bc_lifehash.make_from_data(
                data, version, tv["module_size"], tv["has_alpha"]
            )
        else:
            image = bc_lifehash.make_from_utf8(
                tv["input"], version, tv["module_size"], tv["has_alpha"]
            )

        assert image.width == tv["width"], (
            f"Vector {i}: width mismatch for input={tv['input']!r} "
            f"version={tv['version']}"
        )
        assert image.height == tv["height"], (
            f"Vector {i}: height mismatch for input={tv['input']!r} "
            f"version={tv['version']}"
        )

        expected_colors = bytes(tv["colors"])
        assert len(image.colors) == len(expected_colors), (
            f"Vector {i}: colors length mismatch for input={tv['input']!r} "
            f"version={tv['version']}"
        )

        if image.colors != expected_colors:
            # Find first mismatch
            components = 4 if tv["has_alpha"] else 3
            for j, (got, expected) in enumerate(
                zip(image.colors, expected_colors)
            ):
                if got != expected:
                    pixel = j // components
                    component = j % components
                    comp_name = ["R", "G", "B", "A"][component]
                    raise AssertionError(
                        f"Vector {i}: pixel data mismatch for "
                        f"input={tv['input']!r} version={tv['version']}\n"
                        f"First diff at byte {j} (pixel {pixel}, "
                        f"{comp_name}): got {got}, expected {expected}"
                    )
