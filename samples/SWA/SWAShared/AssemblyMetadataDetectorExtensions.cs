using OpenTelemetry.Resources;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

#nullable enable

namespace OpenTelemetry.Resources;

internal static class AssemblyMetadataDetectorExtensions
{
    
    public static ResourceBuilder AddAssemblyMetadataDetector(this ResourceBuilder builder)
    {
        builder.AddAttributes(GetAssemblyMetadata());
        return builder;
    }

    private static IEnumerable<KeyValuePair<string, object>> GetAssemblyMetadata()
    {
        yield return new KeyValuePair<string, object>("vcs.system", "git");

        var asm = typeof(AssemblyMetadataDetectorExtensions).Assembly;
        var config = asm.GetCustomAttribute<AssemblyConfigurationAttribute>();
        if (config is not null)
        {
            yield return new KeyValuePair<string, object>("build.config", config.Configuration);
        }

        foreach (var amd in typeof(AssemblyMetadataDetectorExtensions).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            (string? key, object? value) = amd.Key switch
            {
                "GitCommitId" => ("vcs.commit.id", amd.Value),
                "GitCommitDateTicks" =>
                    long.TryParse(amd.Value, out var ticks) ?    
                        ("vcs.commit.date", new DateTime(ticks, DateTimeKind.Utc).ToString("O")) :
                        (null, null),
                _ => (null,null)
            };
            if (!string.IsNullOrEmpty(key) && value is not null)
            {
                yield return new KeyValuePair<string, object>(key!, value);
            }
        }
    }
}
