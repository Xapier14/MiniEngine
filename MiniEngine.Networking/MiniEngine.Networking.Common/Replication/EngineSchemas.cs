using MiniEngine.Components;
using MiniEngine.Networking.Common.Components;

namespace MiniEngine.Networking.Common.Replication;

/// <summary>
/// The replicated shape of the components the engine ships with. A game calls
/// <see cref="RegisterAll"/> once at startup, then declares its own components the same
/// way through <see cref="ReplicationSchema.Register{T}"/>.
/// </summary>
public static class EngineSchemas
{
    private static bool _registered;

    /// <summary>
    /// Declares what replicates on the built-in components. Safe to call more than once.
    /// </summary>
    public static void RegisterAll()
    {
        if (_registered)
            return;
        _registered = true;

        // Position is the whole point: it is what the server is authoritative over.
        ReplicationSchema.Register<Transform>()
            .Field(transform => transform.Translate);

        // Velocity travels so a client can extrapolate between snapshots. Acceleration is
        // an input to the simulation the server owns, and is sent so a predicting client
        // can run the same step.
        ReplicationSchema.Register<Motion>()
            .Field(motion => motion.Velocity)
            .Field(motion => motion.Acceleration);

        // Drawable is presentation, but these few fields are decided by game logic rather
        // than by the renderer. Sprite, Offset and the draw event stay local: they are a
        // texture handle, a layout detail and a delegate, none of which mean anything on
        // another machine.
        ReplicationSchema.Register<Drawable>()
            .Field(drawable => drawable.Rotation)
            .Field(drawable => drawable.Depth)
            .Field(drawable => drawable.FlipX)
            .Field(drawable => drawable.FlipY);

        ReplicationSchema.Register<NetworkIdentity>()
            .Field(identity => identity.OwnerClientId);

        // Input travels the other way, as a command rather than a snapshot, but it is the
        // same problem and there is no reason to invent a second encoder for it.
        ReplicationSchema.Register<InputState>()
            .Field(input => input.Tick)
            .Field(input => input.Buttons)
            .Field(input => input.Axis0)
            .Field(input => input.Axis1);
    }

    /// <summary>
    /// Drops the built-in registrations. For tests that rebuild the schema set.
    /// </summary>
    public static void Reset()
        => _registered = false;
}
