namespace C3D.Extensions.Aspire.VisualStudioDebug.WellKnown;

public class Transports
{
    [System.ComponentModel.DataAnnotations.Display(Name = "Default")]
    public const string Default = "{708C1ECA-FF48-11D2-904F-00C04FA302A1}";
    
    [System.ComponentModel.DataAnnotations.Display(Name = "Microsoft Azure App Services")]
    public const string AzureApp = "{2068E361-2346-47FC-8CC2-53EEFF309368}";
    
    [System.ComponentModel.DataAnnotations.Display(Name = "Windows Subsystem for Linux (WSL)")]
    public const string WSL = "{267B1341-AC92-44DC-94DF-2EE4205DD17E}";

    //[System.ComponentModel.DataAnnotations.Display(Name = "")]
    //public const string Unknown = "{2B16AB68-A988-4B2A-9060-5D6801DE25C3}";

    [System.ComponentModel.DataAnnotations.Display(Name = "Remote (Windows - No Authentication)")]
    public const string Remote = "{3B476D38-A401-11D2-AAD4-00C04F990171}";

    [System.ComponentModel.DataAnnotations.Display(Name = "SSH")]
    public const string SSH = "{3FDDF14E-E758-4695-BE0C-7509920432C9}";

    [System.ComponentModel.DataAnnotations.Display(Name = "JavaScript and TypeScript (Chrome DevTools/V8 Inspector)")]
    public const string V8Inspector = "{61D1E397-7AA6-4ED9-815A-0C5CA0E728B4}";

    [System.ComponentModel.DataAnnotations.Display(Name = "Docker (Windows Container)")]
    public const string DockerWindows = "{971E52EF-BDE3-4DED-A040-EFC534BC8110}";

    [System.ComponentModel.DataAnnotations.Display(Name = "Docker (Linux Container)")]
    public const string DockerLinux = "{A2BBC114-47E4-473F-A49C-69EE89711243}";

    [System.ComponentModel.DataAnnotations.Display(Name = ".NET nanoFramework Device")]
    public const string NanoFrameword = "{D7240956-FE4A-4324-93C9-C56975AF351E}";

    [System.ComponentModel.DataAnnotations.Display(Name = "MonoPortSupplier")]
    public const string MonoPortSupplier = "{A2C0CC70-C265-4807-901D-2E5A6378BF43}";
}
