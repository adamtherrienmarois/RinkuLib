using RinkuLib.Tools;
using Xunit;

namespace RinkuLib.Tests.Tools; 
public class BinaryConverterTests {
    [Fact]
    public void ConvertBinary_sbyte() {
        Assert.Equal("10000000", sbyte.MinValue.ConvertBinary());
        Assert.Equal("01111111", sbyte.MaxValue.ConvertBinary());
        Assert.Equal("00000000", ((sbyte)0).ConvertBinary());
    }
    [Fact]
    public void ConvertBinary_byte() {
        Assert.Equal("11111111", byte.MaxValue.ConvertBinary());
        Assert.Equal("00000000", ((byte)0).ConvertBinary());
    }
    [Fact]
    public void ConvertBinary_short() {
        Assert.Equal("10000000 00000000", short.MinValue.ConvertBinary());
        Assert.Equal("01111111 11111111", short.MaxValue.ConvertBinary());
        Assert.Equal("00000000 00000000", ((short)0).ConvertBinary());
    }
    [Fact]
    public void ConvertBinary_ushort() {
        Assert.Equal("11111111 11111111", ushort.MaxValue.ConvertBinary());
        Assert.Equal("00000000 00000000", ((ushort)0).ConvertBinary());
    }
    [Fact]
    public void ConvertBinary_char() {
        Assert.Equal("11111111 11111111", char.MaxValue.ConvertBinary());
        Assert.Equal("00000000 00000000", ((char)0).ConvertBinary());
    }
    [Fact]
    public void ConvertBinary_int() {
        Assert.Equal("10000000 00000000 00000000 00000000", int.MinValue.ConvertBinary());
        Assert.Equal("01111111 11111111 11111111 11111111", int.MaxValue.ConvertBinary());
        Assert.Equal("00000000 00000000 00000000 00000000", ((int)0).ConvertBinary());
    }
    [Fact]
    public void ConvertBinary_uint() {
        Assert.Equal("11111111 11111111 11111111 11111111", uint.MaxValue.ConvertBinary());
        Assert.Equal("00000000 00000000 00000000 00000000", ((uint)0).ConvertBinary());
    }
    [Fact]
    public void ConvertBinary_long() {
        Assert.Equal("10000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000", long.MinValue.ConvertBinary());
        Assert.Equal("01111111 11111111 11111111 11111111 11111111 11111111 11111111 11111111", long.MaxValue.ConvertBinary());
        Assert.Equal("00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000", ((long)0).ConvertBinary());
    }
    [Fact]
    public void ConvertBinary_ulong() {
        Assert.Equal("11111111 11111111 11111111 11111111 11111111 11111111 11111111 11111111", ulong.MaxValue.ConvertBinary());
        Assert.Equal("00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000", ((ulong)0).ConvertBinary());
    }
}
