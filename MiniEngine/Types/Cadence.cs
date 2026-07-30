namespace MiniEngine
{
    /// <summary>
    /// How often a registered system runs. Simulation must advance in whole steps of a
    /// known size for a server and a client to agree on what a tick means, while
    /// presentation should run as often as the frame loop allows.
    /// </summary>
    public enum Cadence
    {
        /// <summary>
        /// Runs once per frame, with a <see cref="System.DeltaTime"/> of however long the
        /// last frame actually took. The default, and the right choice for anything that
        /// draws or reads the window.
        /// </summary>
        Frame,

        /// <summary>
        /// Runs zero or more times per frame, always with a <see cref="System.DeltaTime"/>
        /// of exactly <see cref="WorldClock.FixedDelta"/>. Gameplay and physics belong
        /// here: the step size never varies, so the same tick means the same elapsed time
        /// on every machine.
        /// </summary>
        Fixed,
    }
}
