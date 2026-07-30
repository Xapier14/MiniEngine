using MiniEngine.Networking.Common.Components;
using MiniEngine.Networking.Common.Replication;

namespace MiniEngine.Networking.Client;

/// <summary>
/// <para>Samples the local player's input into their <see cref="InputState"/> once per
/// simulation step, and keeps a copy of each sample to send to the server.</para>
/// <para>Register at <see cref="Cadence.Fixed"/>: a command belongs to exactly one tick,
/// and sampling per frame instead would produce more or fewer commands than there were
/// ticks to apply them to.</para>
/// </summary>
[HandlesComponent<InputState>]
public class LocalInputSystem : System
{
    /// <summary>Most unsent commands to keep before dropping the oldest.</summary>
    public const int MaxPending = 128;

    private readonly Action<InputState> _sample;
    private readonly Queue<InputState> _pending = [];

    /// <summary>
    /// Which client this process is. Assigned by the server at connect; zero until then,
    /// which is also what an entity with no network presence reports, so a single-player
    /// or not-yet-connected world still drives its own input.
    /// </summary>
    public uint LocalClientId { get; set; }

    /// <param name="sample">
    /// Maps physical input onto the bitmask and axes, closing over whatever it reads from
    /// (the engine's <c>InputManager</c> in a real client). Supplied by the game, because
    /// what a button means is not something the engine can know. Passing it in this way,
    /// rather than handing the system an input source, is also what keeps this system
    /// runnable with no SDL and no window.
    /// </param>
    public LocalInputSystem(Action<InputState> sample)
        => _sample = sample;

    /// <summary>
    /// Takes the commands sampled since the last call, oldest first, for sending upstream.
    /// </summary>
    public IReadOnlyList<InputState> DrainPending()
    {
        if (_pending.Count == 0)
            return [];
        var drained = _pending.ToArray();
        _pending.Clear();
        return drained;
    }

    public void HandleComponent(InputState inputState)
    {
        if (!IsLocallyOwned(inputState))
            return;

        inputState.Tick = Ecs.Clock.Tick;
        _sample(inputState);

        var copy = new InputState();
        inputState.CopyTo(copy);
        _pending.Enqueue(copy);

        // An unsent backlog is worthless: the server has long since simulated those ticks.
        while (_pending.Count > MaxPending)
            _pending.Dequeue();
    }

    /// <summary>
    /// Whether this input belongs to the player at this keyboard. An entity with no
    /// network identity is local by definition; a replicated one has to say so, or a
    /// client would drive every other player's entity from its own keys.
    /// </summary>
    private bool IsLocallyOwned(InputState inputState)
    {
        var identity = Ecs.GetSibling<NetworkIdentity>(inputState);
        return identity == null || identity.OwnerClientId == LocalClientId;
    }
}
