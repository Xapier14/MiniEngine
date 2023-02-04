// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;
[assembly: SuppressMessage("Minor Code Smell",
    "S1104:Fields should not have public accessibility",
    Justification = "A component's Transform component should be able to be modified by doing something similar to 'transform.Translate.X = 0;'",
    Scope = "member",
    Target = "~F:MiniEngine.Components.Transform.Translate")]
[assembly: SuppressMessage("Minor Code Smell",
    "S1104:Fields should not have public accessibility",
    Justification = "A component's Transform component should be able to be modified by doing something similar to 'transform.Scale.X = 0;'",
    Scope = "member",
    Target = "~F:MiniEngine.Components.Transform.Scale")]
[assembly: SuppressMessage("Minor Code Smell",
    "S1104:Fields should not have public accessibility",
    Justification = "A component's Motion component should be able to be modified by doing something similar to 'motion.Velocity.X = 0f;'",
    Scope = "member",
    Target = "~F:MiniEngine.Components.Motion.Velocity")]
[assembly: SuppressMessage("Minor Code Smell",
    "S1104:Fields should not have public accessibility",
    Justification = "A component's Motion component should be able to be modified by doing something similar to 'motion.Acceleration.X = 0f;'",
    Scope = "member",
    Target = "~F:MiniEngine.Components.Motion.Acceleration")]
