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
    public string AssetsFile { get; set; }

    public void Apply(EngineSetup setup)
    {
        StartInFullScreen ??= setup.StartInFullScreen;
        WindowSize ??= setup.WindowSize;
        FpsLimit ??= setup.FpsLimit;
        DisableGamePad ??= setup.DisableGamePad;
        InitialWindowTitle ??= setup.InitialWindowTitle;
        AssetsFile ??= setup.AssetsFile;
    }
}