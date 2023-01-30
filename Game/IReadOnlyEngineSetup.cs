namespace MiniEngine;

public interface IReadOnlyEngineSetup
{
    public bool? StartInFullScreen { get; }
    public Size? WindowSize { get; }
}