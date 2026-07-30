namespace MiniEngine.Networking.Common.Components;

/// <summary>
/// <para>What a player is asking their entity to do on a given tick.</para>
/// <para>Gameplay reads this instead of <c>InputManager</c>, which is SDL-backed and so
/// exists only on a client. The same component is filled from the local keyboard on a
/// client and from a received command on the server, which is what lets one piece of
/// gameplay code run on both and is the only way a server can be authoritative over
/// something a player initiated.</para>
/// <para>Buttons are an opaque bitmask: what bit 3 means is a decision for a game, not for
/// an engine.</para>
/// </summary>
public sealed class InputState : Component
{
    /// <summary>
    /// The simulation tick this input was sampled on. A server applies a command to the
    /// tick it was meant for, and a client later uses it to replay unacknowledged input
    /// after the server corrects it.
    /// </summary>
    public ulong Tick;

    /// <summary>Held buttons, one bit each. Meaning is defined by the game.</summary>
    public uint Buttons;

    /// <summary>Primary analogue axis, conventionally movement.</summary>
    public Vector2F Axis0 = Vector2F.Zero;

    /// <summary>Secondary analogue axis, conventionally aim or look.</summary>
    public Vector2F Axis1 = Vector2F.Zero;

    public bool IsPressed(int button)
        => (Buttons & (1u << button)) != 0;

    public void Set(int button, bool pressed)
    {
        if (pressed)
            Buttons |= 1u << button;
        else
            Buttons &= ~(1u << button);
    }

    /// <summary>Copies the sampled values, leaving the target's identity alone.</summary>
    public void CopyTo(InputState target)
    {
        target.Tick = Tick;
        target.Buttons = Buttons;
        target.Axis0 = Axis0;
        target.Axis1 = Axis1;
    }
}
