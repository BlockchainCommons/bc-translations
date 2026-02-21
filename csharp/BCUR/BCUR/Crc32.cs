namespace BlockchainCommons.BCUR;

/// <summary>
/// CRC32/ISO-HDLC (also known as CRC-32, CRC-32/V.42, CRC-32/XZ, PKZIP).
/// Polynomial: 0x04C11DB7 (reflected: 0xEDB88320).
/// </summary>
internal static class Crc32
{
    private static readonly uint[] Table = GenerateTable();

    private static uint[] GenerateTable()
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            var crc = i;
            for (int j = 0; j < 8; j++)
            {
                crc = (crc & 1) != 0
                    ? (crc >> 1) ^ 0xEDB88320u
                    : crc >> 1;
            }
            table[i] = crc;
        }
        return table;
    }

    internal static uint Checksum(ReadOnlySpan<byte> data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in data)
        {
            crc = Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
        }
        return crc ^ 0xFFFFFFFFu;
    }
}
