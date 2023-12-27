public class RandomDataStream : Stream
{
    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    private readonly long guidLength = Guid.NewGuid().ToString().Length + 1;
    private readonly long length;
    public override long Length => length;

    private long position;
    public override long Position { get => position; set => throw new NotSupportedException(); }

    public RandomDataStream(int lines)
    {
        length = guidLength * lines;
    }

    public override void Flush()
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var guid = Guid.NewGuid().ToString() + "\n";

        var curOffset = offset;
        var curCount = count;

        while(curCount > 0 && position < length)
        {
            var positionInLine = position % guidLength;
            var partialGuid = guid.Substring((int)positionInLine);

            var charsToCopy = Math.Min(partialGuid.Length, curCount);

            for(int i = 0; i < charsToCopy; i++)
                buffer[curOffset + i] = (byte)partialGuid.ElementAt(i);

            guid = Guid.NewGuid().ToString() + "\n";

            curOffset += charsToCopy;
            position += charsToCopy;
            curCount -= charsToCopy;
        }

        return count - curCount;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
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