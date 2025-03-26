using OpenTelemetry.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace SWAShared
{
    internal static class GitAttributesDetectorExtensions
    {
        private static readonly Dictionary<string, object> GitResourceAttributes = new() {
            { "vcs.system",      "git" },
            { "vcs.commit.id",   typeof(GitAttributesDetectorExtensions).Assembly.get },
            { "vcs.commit.date", ThisAssembly.GitCommitDate.ToString("O") }
        };

        private static IEnumerable<KeyValuePair<>>

        public static ResourceBuilder AddAssemblyMetadataDetector(this ResourceBuilder builder)
        {
            builder.AddAttributes(GetAssemblyAttributes());
            return builder;
        }
    }
}
