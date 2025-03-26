namespace C3D.Extensions.Aspire.OutputWatcher.Annotations;

public class OutputWatcherAnnotation : OutputWatcherAnnotationBase
{

    private readonly Func<string, bool> predicate;
    private readonly string predicateName;

    public OutputWatcherAnnotation(
        Func<string, bool> predicate,
        bool isSecret,
        string? key = null, 
        string? predicateName = null)
        : base(isSecret,key)
    {
        this.predicate = predicate;
        this.predicateName = predicateName ?? string.Empty;
        ;
    }

    public override string PredicateName => predicateName;

    public override bool IsMatch(string message) => predicate(message);
}
