using Aspire.Hosting.ApplicationModel;
using C3D.Extensions.Aspire.OutputWatcher.Annotations;

namespace C3D.Extensions.Aspire.OutputWatcher;

public class OutputWatcherAnnotationValueProvider<TBase> : IValueProvider, IManifestExpressionProvider, IValueWithReferences
    where TBase : OutputWatcherAnnotationBase
{
    private Func<object?, ValueTask<string?>> formatter;

    public OutputWatcherAnnotationValueProvider(IResource resource, TBase annotation,
        string property, Func<object?, ValueTask<string?>>? formatter)
    {
        Resource = resource;
        Annotation = annotation;
        Property = property;
        this.formatter = formatter ?? (o => ValueTask.FromResult(o?.ToString()));
    }


    public TBase Annotation { get; }
    public string Property { get; }

    public string ValueExpression => Property;

    public IEnumerable<object> References => [Annotation, Resource];

    public IResource Resource { get; }

    public async ValueTask<string?> GetValueAsync(CancellationToken cancellationToken = default) =>
        await formatter(Annotation.Properties[Property]);
}
