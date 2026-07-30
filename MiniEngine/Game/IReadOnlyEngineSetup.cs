using System.Collections.Generic;

namespace MiniEngine;

public interface IReadOnlyEngineSetup
{
    public bool? StartInFullScreen { get; }
    public Size? WindowSize { get; }
    public float? FpsLimit { get; }
    public float? GameSpeed { get; }
    public float? FixedTickRate { get; }
    public bool? DisableGamePad { get; }
    public bool? DisableCursor { get; }
    public bool? InvertYAxis { get; }
    public string? InitialWindowTitle { get; }
    public string? AssetsFile { get; }
    public IDictionary<string, object>? AdditionalConfiguration { get; }
}