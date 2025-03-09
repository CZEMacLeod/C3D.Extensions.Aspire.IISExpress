using Aspire.Hosting.ApplicationModel;

namespace C3D.Extensions.Aspire.VisualStudioDebug.Annotations;

public class DebugAttachAnnotation : IResourceAnnotation, IDisposable
{
    public DebugMode DebugMode { get; set; }

    public int? DebuggerProcessId { get; set; }

    public bool Skip { get; set; }

    private readonly SemaphoreSlim @lock = new(1,1);
    public bool TryLock(out IDisposable? token)
    {
        if (@lock.Wait(0))
        {
            token = new LockToken(@lock);
            return true;
        }
        token = null;
        return false;
    }

    public void Dispose() => ((IDisposable)@lock).Dispose();

    private class LockToken : IDisposable
    {
        private readonly SemaphoreSlim @lock;

        public LockToken(SemaphoreSlim @lock) => this.@lock = @lock;

        public void Dispose() => @lock.Release();
    }
}