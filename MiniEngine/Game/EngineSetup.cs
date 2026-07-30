using System.Collections.Generic;

namespace MiniEngine;

public class EngineSetup : IReadOnlyEngineSetup
{
    public static EngineSetup Default => new()
    {
        StartInFullScreen = false,
        WindowSize = (800, 600),
        InitialWindowTitle = "MiniEngine Game",
        AssetsFile = "assets.mea",
        InvertYAxis = false,
        GameSpeed = 1f
    };

    /// <summary>
    /// Starts the game in fullscreen.
    /// </summary>
    public bool? StartInFullScreen { get; set; }

    /// <summary>
    /// The initial window size.
    /// </summary>
    public Size? WindowSize { get; set; }

    /// <summary>
    /// The fps limit. A zero or negative value would mean unlimited.
    /// </summary>
    public float? FpsLimit { get; set; }

    /// <summary>
    /// How fast time passes, where 1.0 is realtime. Slowing time down takes fewer
    /// simulation steps per second rather than smaller ones, so a step always covers
    /// <see cref="FixedTickRate"/> worth of simulation whatever this is set to.
    /// </summary>
    public float? GameSpeed { get; set; } = 1f;

    /// <summary>
    /// Simulation steps per second. Systems registered at <see cref="Cadence.Fixed"/> run
    /// at this rate regardless of frame rate; everything else runs once per frame.
    /// </summary>
    public float? FixedTickRate { get; set; } = 30f;

    /// <summary>
    /// Disables the Gamepad API.
    /// </summary>
    public bool? DisableGamePad { get; set; }

    /// <summary>
    /// Disables the native cursor.
    /// </summary>
    public bool? DisableCursor { get; set; }

    /// <summary>
    /// Inverts the Y-axis. When enabled, (0, 0) is the bottom-left of screenspace and angles would be in the CCW direction.
    /// </summary>
    public bool? InvertYAxis { get; set; }

    /// <summary>
    /// The initial window title.
    /// </summary>
    public string? InitialWindowTitle { get; set; }

    /// <summary>
    /// The main assets file to load.
    /// </summary>
    public string? AssetsFile { get; set; }
    
    /// <summary>
    /// Additional configuration options.
    /// </summary>
    public IDictionary<string, object>? AdditionalConfiguration { get; set; }

    public EngineSetup()
    {
        
    }

    public void Apply(EngineSetup setup)
    {
        if (setup.StartInFullScreen != null)
            StartInFullScreen = setup.StartInFullScreen;

        if (setup.WindowSize != null)
            WindowSize = setup.WindowSize;

        if (setup.FpsLimit != null)
            FpsLimit = setup.FpsLimit;

        if (setup.GameSpeed != null)
            GameSpeed = setup.GameSpeed;

        if (setup.FixedTickRate != null)
            FixedTickRate = setup.FixedTickRate;

        if (setup.DisableCursor != null)
            DisableCursor = setup.DisableCursor;

        if (setup.InvertYAxis != null)
            InvertYAxis = setup.InvertYAxis;

        if (setup.DisableGamePad != null)
            DisableGamePad = setup.DisableGamePad;

        if (setup.InitialWindowTitle != null)
            InitialWindowTitle = setup.InitialWindowTitle;

        if (setup.AssetsFile != null)
            AssetsFile = setup.AssetsFile;

        if (setup.AdditionalConfiguration != null)
        {
            AdditionalConfiguration ??= new Dictionary<string, object>();
            
            foreach (var (key, value) in setup.AdditionalConfiguration)
            {
                AdditionalConfiguration[key] = value;
            }
        }
    }
}