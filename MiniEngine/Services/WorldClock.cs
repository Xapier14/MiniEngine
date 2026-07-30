using System;
using System.Diagnostics;

namespace MiniEngine
{
    /// <summary>
    /// <para>The time source for one world.</para>
    /// <para>Frame time is accumulated and spent in whole steps of <see cref="FixedDelta"/>,
    /// so <see cref="Cadence.Fixed"/> systems always see the same delta and <see cref="Tick"/>
    /// counts the same amount of simulated time everywhere. <see cref="Cadence.Frame"/>
    /// systems see <see cref="FrameDelta"/> instead, which is whatever the frame took.</para>
    /// </summary>
    public sealed class WorldClock
    {
        /// <summary>
        /// Largest single frame, in seconds, that may be added to the step budget. A frame
        /// longer than this (a breakpoint, a stalled load) is treated as this long, so time
        /// spent not running is not simulated afterwards.
        /// </summary>
        public const double MaxFrameDelta = 0.25;

        private readonly Func<double> _timeSource;
        private double _accumulator;
        private double _lastTimestamp;
        private bool _started;
        private int _stepsThisFrame;

        /// <summary>
        /// Number of fixed steps taken since this clock was created. Never decreases and
        /// advances exactly once per <see cref="ConsumeFixedStep"/> that returns true.
        /// </summary>
        public ulong Tick { get; private set; }

        /// <summary>
        /// The size of one simulation step, in seconds. Constant for the life of the clock.
        /// </summary>
        public double FixedDelta { get; }

        /// <summary>
        /// How long the last frame took, in seconds, after <see cref="TimeScale"/> and the
        /// <see cref="MaxFrameDelta"/> clamp.
        /// </summary>
        public double FrameDelta { get; private set; }

        /// <summary>
        /// Multiplier on the passage of time. 1.0 is realtime, 0.5 is half speed, 0 pauses.
        /// Scaling changes how many fixed steps are taken per second, never how big a step
        /// is, so slowing time down does not change the result of a step.
        /// </summary>
        public double TimeScale { get; set; } = 1.0;

        /// <summary>
        /// Most fixed steps allowed in a single frame. Once reached, the outstanding
        /// backlog is dropped rather than carried, so a world that cannot keep up runs slow
        /// instead of falling further behind every frame.
        /// </summary>
        public int MaxStepsPerFrame { get; }

        /// <summary>
        /// How far the accumulator sits into the next step, 0 to 1. Renderers use this to
        /// interpolate between the last simulated state and the next one.
        /// </summary>
        public float Alpha => (float)Math.Clamp(_accumulator / FixedDelta, 0.0, 1.0);

        /// <param name="fixedDelta">Seconds per simulation step. Defaults to 30 steps/second.</param>
        /// <param name="maxStepsPerFrame">Catch-up limit for a single frame.</param>
        /// <param name="timeSource">
        /// Monotonically increasing seconds. Defaults to a process-wide <see cref="Stopwatch"/>.
        /// Supply one to drive a world from an explicit time in tests.
        /// </param>
        public WorldClock(double fixedDelta = 1.0 / 30.0, int maxStepsPerFrame = 5, Func<double>? timeSource = null)
        {
            if (fixedDelta <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(fixedDelta), fixedDelta,
                    "A simulation step must be longer than zero seconds.");
            if (maxStepsPerFrame < 1)
                throw new ArgumentOutOfRangeException(nameof(maxStepsPerFrame), maxStepsPerFrame,
                    "A frame must be allowed at least one simulation step.");

            FixedDelta = fixedDelta;
            MaxStepsPerFrame = maxStepsPerFrame;
            _timeSource = timeSource ?? DefaultTimeSource;
        }

        // Stopwatch rather than DateTime.Now: monotonic, so an NTP correction or a DST
        // boundary cannot hand back a negative frame.
        private static readonly Stopwatch _processStopwatch = Stopwatch.StartNew();

        private static double DefaultTimeSource()
            => _processStopwatch.ElapsedTicks / (double)Stopwatch.Frequency;

        /// <summary>
        /// Samples elapsed time and refills the step budget. Call once per frame, before
        /// draining <see cref="ConsumeFixedStep"/>.
        /// </summary>
        public void BeginFrame()
        {
            var now = _timeSource();
            _stepsThisFrame = 0;

            if (!_started)
            {
                // Nothing has been drawn yet, so there is no previous frame to measure.
                _started = true;
                _lastTimestamp = now;
                FrameDelta = 0.0;
                return;
            }

            var elapsed = Math.Max(now - _lastTimestamp, 0.0);
            _lastTimestamp = now;

            FrameDelta = Math.Min(elapsed, MaxFrameDelta) * TimeScale;
            _accumulator += FrameDelta;
        }

        /// <summary>
        /// Takes one step out of the budget, returning false once the frame is out of time
        /// or has hit <see cref="MaxStepsPerFrame"/>. Drive it as a while loop.
        /// </summary>
        public bool ConsumeFixedStep()
        {
            if (_accumulator < FixedDelta)
                return false;

            if (_stepsThisFrame >= MaxStepsPerFrame)
            {
                // Give up on the backlog. Keeping it would mean owing steps that the next
                // frame is no likelier to afford, and the debt would compound.
                _accumulator = 0.0;
                return false;
            }

            _accumulator -= FixedDelta;
            _stepsThisFrame++;
            Tick++;
            return true;
        }
    }
}
