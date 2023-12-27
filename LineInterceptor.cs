public class LineInterceptor : Stream
{
    MemoryStream tempStream = new();
    private readonly Action<MemoryStream> lineHandler;

    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    private long length = 0;
    public override long Length => length;

    private long position = 0;
    public override long Position { get => position; set => throw new NotSupportedException(); }

    public LineInterceptor(Action<MemoryStream> lineHandler)
    {
        this.lineHandler = lineHandler;
    }

    public override void Flush()
    {
        // Do nothing...
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
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
        for(var i = offset; i < offset + count; i++)
        {
            tempStream.WriteByte(buffer[i]);

            if(buffer[i] == '\n')
                HandleLineAndReset();
        }       
    }

    private void HandleLineAndReset()
    {
        lineHandler(tempStream);

        // Reset
        tempStream = new();
    }

    public override void Close()
    {
        if(tempStream.Length > 0)
            HandleLineAndReset();
    }
}