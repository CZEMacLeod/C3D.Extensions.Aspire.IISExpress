using Aspire.Hosting.ApplicationModel;
using System.Text.RegularExpressions;

namespace C3D.Extensions.Aspire.OutputWatcher.Annotations;

public class OutputWatcherRegExAnnotation : OutputWatcherAnnotationBase, IValueProvider
{
    private readonly Regex matcher;

    public OutputWatcherRegExAnnotation(
        Regex matcher,
        bool isSecret,
        string? key = null)
        : base(isSecret, key)
    {
        this.matcher = matcher;
    }

    public override string PredicateName => matcher.ToString();

    public Func<OutputWatcherRegExAnnotation, ValueTask<string?>> ValueFunc { get; set; } = 
        static async (annotation) =>
            await Task.FromResult(annotation.properties["Match"]?.ToString());

    public async ValueTask<string?> GetValueAsync(CancellationToken cancellationToken = default) => await ValueFunc(this);

    public override bool IsMatch(string message)
    {
        if (matcher.IsMatch(message))
        {
            properties["Match"] = matcher.Match(message).Value;
            foreach (var groupName in matcher.GetGroupNames())
            {
                properties[groupName] = matcher.Match(message).Groups[groupName].Value;
            }
        }
        return matcher.IsMatch(message);
    }
}