using Aspire.Hosting.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3D.Extensions.Aspire.IISExpress.Annotations;

public class SystemWebAdaptersAnnotation : IResourceAnnotation
{
    public SystemWebAdaptersAnnotation(Guid key, string envNameKey, string envNameUrl)
    {
        Key = key;
        EnvNameKey = envNameKey;
        EnvNameUrl = envNameUrl;
    }

    public Guid Key { get; }
    public string EnvNameKey { get; }
    public string EnvNameUrl { get; }
}
