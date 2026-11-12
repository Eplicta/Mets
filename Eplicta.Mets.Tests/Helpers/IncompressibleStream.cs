using System;
using System.IO;

namespace Eplicta.Mets.Tests.Helpers;

internal sealed class IncompressibleStream : Stream
{
    private readonly long _length;
    private readonly Random _random;
    private long _position;

    public IncompressibleStream(long length, int seed = 4691)
    {
        _length = length;
        _random = new Random(seed);
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => _length;

    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = (int)Math.Min(count, _length - _position);
        if (read <= 0) return 0;

        _random.NextBytes(buffer.AsSpan(offset, read));
        _position += read;
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }
}
