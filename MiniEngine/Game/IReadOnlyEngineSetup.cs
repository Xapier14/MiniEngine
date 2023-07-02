namespace MiniEngine;

public interface IReadOnlyEngineSetup
{
    public bool? StartInFullScreen { get; set; }
    public Size? WindowSize { get; set; }
    public float? FpsLimit { get; set; }
    public bool? DisableGamePad { get; set; }
    public string? InitialWindowTitle { get; set; }
    public string? AssetsFile { get; set; }
}