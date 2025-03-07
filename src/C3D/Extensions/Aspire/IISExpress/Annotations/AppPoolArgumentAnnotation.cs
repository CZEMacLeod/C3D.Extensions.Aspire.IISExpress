namespace C3D.Extensions.Aspire.IISExpress.Annotations;

public class AppPoolArgumentAnnotation : IISExpressArgumentAnnotation
{
    private readonly string appPool;

    public AppPoolArgumentAnnotation(string appPool = "Clr4IntegratedAppPool") => this.appPool = appPool;

    public override string ToString() => $"/apppool:{appPool}";
}
