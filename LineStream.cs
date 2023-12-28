
public class LineStream : Stream
{
    private int positionInLine;
    private byte[]? currentLineBytes;

    private Func<Task<string?>>? getNextLineAsyncFunc;

    protected virtual async Task<string?> GetNextLineAsync() 
    { 
        if(getNextLineAsyncFunc == null) 
            throw new NotImplementedException();

        return await getNextLineAsyncFunc();
    }

    private async Task<byte[]?> TryGetNextLineInBytes()
    {
        var line = await GetNextLineAsync();

        if (line == null)
            return null;
        else
            return System.Text.Encoding.UTF8.GetBytes(line + "\n");
    }

    public LineStream(Func<Task<string?>>? getNextLineAsyncFunc = null)
    {
        this.getNextLineAsyncFunc = getNextLineAsyncFunc;
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    long position;
    public override long Position { get => position; set => throw new NotSupportedException(); }

    public override void Flush()
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return ReadAsync(buffer, offset, count, CancellationToken.None).Result;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        int bytesWritten = 0;

        if (currentLineBytes == null)
        {
            currentLineBytes = await TryGetNextLineInBytes();
            if (currentLineBytes == null)
                return bytesWritten;
        }

        for (long i = offset; i < offset + count; i++)
        {
            if (positionInLine >= currentLineBytes.Length)
            {
                currentLineBytes = await TryGetNextLineInBytes();
                if (currentLineBytes == null)
                    return bytesWritten;

                positionInLine = 0;
            }

            buffer[i] = currentLineBytes[positionInLine++];
            bytesWritten++;
            position++;
        }

        return bytesWritten;
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