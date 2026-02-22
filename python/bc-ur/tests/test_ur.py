"""Tests for UR encoding and decoding."""

import pytest

from bc_ur._ur_encoding import decode_ur, encode_ur
from bc_ur.error import (
    InvalidSchemeError,
    InvalidTypeError,
    NotSinglePartError,
    TypeUnspecifiedError,
    URDecoderError,
    URError,
)
from bc_ur.multipart_decoder import MultipartDecoder
from bc_ur.multipart_encoder import MultipartEncoder
from bc_ur.ur import UR
from dcbor import CBOR

from tests.conftest import make_message_ur


def test_single_part_ur():
    ur_data = make_message_ur(50, "Wolf")
    encoded = encode_ur(ur_data, "bytes")
    expected = "ur:bytes/hdeymejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtgwdpfnsboxgwlbaawzuefywkdplrsrjynbvygabwjldapfcsdwkbrkch"
    assert encoded == expected

    kind, decoded = decode_ur(encoded)
    assert kind == "single"
    assert decoded == ur_data


def test_ur_roundtrip():
    cbor = CBOR.from_value([1, 2, 3])
    ur = UR("test", cbor)
    ur_string = ur.string()
    assert ur_string == "ur:test/lsadaoaxjygonesw"

    ur2 = UR.from_ur_string(ur_string)
    assert ur2.ur_type_str == "test"
    assert ur2.cbor == cbor

    # Uppercase input should also work
    ur3 = UR.from_ur_string("UR:TEST/LSADAOAXJYGONESW")
    assert ur3.ur_type_str == "test"
    assert ur3.cbor == cbor


def test_ur_encoder_20_parts():
    ur_data = make_message_ur(256, "Wolf")
    cbor = CBOR.from_data(ur_data)
    ur = UR("bytes", cbor)
    encoder = MultipartEncoder(ur, 30)

    expected = [
        "ur:bytes/1-9/lpadascfadaxcywenbpljkhdcahkadaemejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtdkgslpgh",
        "ur:bytes/2-9/lpaoascfadaxcywenbpljkhdcagwdpfnsboxgwlbaawzuefywkdplrsrjynbvygabwjldapfcsgmghhkhstlrdcxaefz",
        "ur:bytes/3-9/lpaxascfadaxcywenbpljkhdcahelbknlkuejnbadmssfhfrdpsbiegecpasvssovlgeykssjykklronvsjksopdzmol",
        "ur:bytes/4-9/lpaaascfadaxcywenbpljkhdcasotkhemthydawydtaxneurlkosgwcekonertkbrlwmplssjtammdplolsbrdzcrtas",
        "ur:bytes/5-9/lpahascfadaxcywenbpljkhdcatbbdfmssrkzmcwnezelennjpfzbgmuktrhtejscktelgfpdlrkfyfwdajldejokbwf",
        "ur:bytes/6-9/lpamascfadaxcywenbpljkhdcackjlhkhybssklbwefectpfnbbectrljectpavyrolkzczcpkmwidmwoxkilghdsowp",
        "ur:bytes/7-9/lpatascfadaxcywenbpljkhdcavszmwnjkwtclrtvaynhpahrtoxmwvwatmedibkaegdosftvandiodagdhthtrlnnhy",
        "ur:bytes/8-9/lpayascfadaxcywenbpljkhdcadmsponkkbbhgsoltjntegepmttmoonftnbuoiyrehfrtsabzsttorodklubbuyaetk",
        "ur:bytes/9-9/lpasascfadaxcywenbpljkhdcajskecpmdckihdyhphfotjojtfmlnwmadspaxrkytbztpbauotbgtgtaeaevtgavtny",
        "ur:bytes/10-9/lpbkascfadaxcywenbpljkhdcahkadaemejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtwdkiplzs",
        "ur:bytes/11-9/lpbdascfadaxcywenbpljkhdcahelbknlkuejnbadmssfhfrdpsbiegecpasvssovlgeykssjykklronvsjkvetiiapk",
        "ur:bytes/12-9/lpbnascfadaxcywenbpljkhdcarllaluzmdmgstospeyiefmwejlwtpedamktksrvlcygmzemovovllarodtmtbnptrs",
        "ur:bytes/13-9/lpbtascfadaxcywenbpljkhdcamtkgtpknghchchyketwsvwgwfdhpgmgtylctotzopdrpayoschcmhplffziachrfgd",
        "ur:bytes/14-9/lpbaascfadaxcywenbpljkhdcapazewnvonnvdnsbyleynwtnsjkjndeoldydkbkdslgjkbbkortbelomueekgvstegt",
        "ur:bytes/15-9/lpbsascfadaxcywenbpljkhdcaynmhpddpzmversbdqdfyrehnqzlugmjzmnmtwmrouohtstgsbsahpawkditkckynwt",
        "ur:bytes/16-9/lpbeascfadaxcywenbpljkhdcawygekobamwtlihsnpalnsghenskkiynthdzotsimtojetprsttmukirlrsbtamjtpd",
        "ur:bytes/17-9/lpbyascfadaxcywenbpljkhdcamklgftaxykpewyrtqzhydntpnytyisincxmhtbceaykolduortotiaiaiafhiaoyce",
        "ur:bytes/18-9/lpbgascfadaxcywenbpljkhdcahkadaemejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtntwkbkwy",
        "ur:bytes/19-9/lpbwascfadaxcywenbpljkhdcadekicpaajootjzpsdrbalpeywllbdsnbinaerkurspbncxgslgftvtsrjtksplcpeo",
        "ur:bytes/20-9/lpbbascfadaxcywenbpljkhdcayapmrleeleaxpasfrtrdkncffwjyjzgyetdmlewtkpktgllepfrltataztksmhkbot",
    ]

    assert encoder.parts_count == 9
    for i, expected_str in enumerate(expected):
        assert encoder.current_index == i
        assert encoder.next_part() == expected_str


def test_decoder_error_cases():
    # Invalid scheme
    with pytest.raises(InvalidSchemeError):
        decode_ur("uhr:bytes/aeadaolazmjendeoti")

    # Missing type (no slash)
    with pytest.raises(TypeUnspecifiedError):
        decode_ur("ur:aeadaolazmjendeoti")

    # Invalid characters in type
    with pytest.raises(InvalidTypeError):
        decode_ur("ur:bytes#4/aeadaolazmjendeoti")

    # Invalid indices in multi-part
    with pytest.raises(URDecoderError):
        decode_ur("ur:bytes/1-1a/aeadaolazmjendeoti")

    # Too many slashes (the third slash means the indices section contains a slash)
    with pytest.raises(URDecoderError):
        decode_ur("ur:bytes/1-1/toomuch/aeadaolazmjendeoti")

    # Valid single-part
    kind, _ = decode_ur("ur:bytes/aeadaolazmjendeoti")
    assert kind == "single"

    # Valid custom type
    kind, _ = decode_ur("ur:whatever-12/aeadaolazmjendeoti")
    assert kind == "single"


def test_custom_encoder():
    data = b"Ten chars!"
    max_length = 5
    from bc_ur._fountain_encoder import FountainEncoder
    from bc_ur.bytewords import BytewordsStyle, encode as bw_encode

    encoder = FountainEncoder(data, max_length)
    part = encoder.next_part()
    body = bw_encode(part.to_cbor(), BytewordsStyle.MINIMAL)
    ur_string = f"ur:my-scheme/{part.sequence_id()}/{body}"
    assert ur_string == "ur:my-scheme/1-2/lpadaobkcywkwmhfwnfeghihjtcxiansvomopr"


def test_multipart_ur():
    ur_data = make_message_ur(32767, "Wolf")
    cbor = CBOR.from_data(ur_data)
    ur = UR("bytes", cbor)
    encoder = MultipartEncoder(ur, 1000)
    decoder = MultipartDecoder()
    while not decoder.is_complete:
        assert decoder.message() is None
        part_str = encoder.next_part()
        decoder.receive(part_str)
    received_ur = decoder.message()
    assert received_ur is not None
    assert received_ur == ur
