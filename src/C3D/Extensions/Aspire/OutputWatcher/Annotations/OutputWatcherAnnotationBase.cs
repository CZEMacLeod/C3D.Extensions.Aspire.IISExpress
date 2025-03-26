using Aspire.Hosting.ApplicationModel;
using System.Diagnostics;

namespace C3D.Extensions.Aspire.OutputWatcher.Annotations;

[DebuggerDisplay("{Key} {Predicate}")]
public abstract class OutputWatcherAnnotationBase : IResourceAnnotation
{
    public bool IsSecret { get; }
    public string Key { get; }
    public abstract string PredicateName { get; }

    protected OutputWatcherAnnotationBase(bool isSecret, string? key = null)
    {
        IsSecret = isSecret;
        Key = key ?? $"ow-{Guid.NewGuid()}";
    }

    protected Dictionary<string, object> properties = new();

    public IReadOnlyDictionary<string, object> Properties => properties.AsReadOnly();

    public abstract bool IsMatch(string message);


    #region "WellKnown Properties"
    public DateTimeOffset? TimeStamp
    {
        get => properties.TryGetValue(nameof(TimeStamp), out var value) ? value as DateTime? : null;
        internal set
        {
            if (value is null)
            {
                properties.Remove(nameof(TimeStamp));
                return;
            }
            properties[nameof(TimeStamp)] = value;
        }
    }
    public string? Message
    {
        get => properties.TryGetValue(nameof(Message), out var value) ? value.ToString() : null;
        internal set
        {
            if (value is null)
            {
                properties.Remove(nameof(Message));
                return;
            }
            properties[nameof(Message)] = value;
        }
    }
    #endregion
}