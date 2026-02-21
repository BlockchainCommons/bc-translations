using System.Text;

namespace BlockchainCommons.BCUR.Tests;

public class Crc32Tests
{
    [Fact]
    public void Crc32HelloWorld()
    {
        Assert.Equal(0xEBE6C6E6u, Crc32.Checksum(Encoding.UTF8.GetBytes("Hello, world!")));
    }

    [Fact]
    public void Crc32Wolf()
    {
        Assert.Equal(0x598C84DCu, Crc32.Checksum(Encoding.UTF8.GetBytes("Wolf")));
    }
}
