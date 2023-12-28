
public class GuidStream : VirtualLineStream
{
    private Queue<string?> guidQueue = new();
    private long numberOfLines;

    // Required for PgpCore only.
    public override long Length => numberOfLines * Guid.NewGuid().ToString().Length + 1;

    public GuidStream(long numberOfLines)
    {
        this.numberOfLines = numberOfLines;
    }

    protected override Task<string?> GetNextLineAsync()
    {
        if(guidQueue.Count == 0)
        {
            for(int i = 0; i < 5 && numberOfLines > 0; i++)
            {
                guidQueue.Enqueue(Guid.NewGuid().ToString());
                numberOfLines--;
            }
        }

        if(guidQueue.Count == 0)
            return Task.FromResult<string?>(null);
        else
            return Task.FromResult(guidQueue.Dequeue());
    }
}