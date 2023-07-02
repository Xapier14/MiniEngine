namespace MiniEngine;

public struct EngineSetup : IReadOnlyEngineSetup
{
    public static EngineSetup Default => new()
    {
        StartInFullScreen = false,
        WindowSize = (800, 600),
        InitialWindowTitle = "MiniEngine Game",
        AssetsFile = "assets.mea"
    };

    public bool? StartInFullScreen { get; set; }
    public Size? WindowSize { get; set; }
    public float? FpsLimit { get; set; }
    public bool? DisableGamePad { get; set; }
    public string? InitialWindowTitle { get; set; }
    public string? AssetsFile { get; set; }

    public void Apply(EngineSetup setup)
    {
        if (setup.StartInFullScreen != null)
            StartInFullScreen = setup.StartInFullScreen;
        if (setup.WindowSize != null)
            WindowSize = setup.WindowSize;
        if (setup.FpsLimit != null)
            FpsLimit = setup.FpsLimit;
        if (setup.DisableGamePad != null)
            DisableGamePad = setup.DisableGamePad;
        if (setup.InitialWindowTitle != null)
            InitialWindowTitle = setup.InitialWindowTitle;
        if (setup.AssetsFile != null)
            AssetsFile = setup.AssetsFile;
    }
}