using System;

namespace MiniFB;

/// <summary>A mouse button</summary>
public enum MouseButton : int
{
#pragma warning disable CS1591 // This is self-documenting

	[Obsolete($"Use '{nameof(None)}' instead")] Button0,
	[Obsolete($"Use '{nameof(LeftButton)}' instead")] Button1,
	[Obsolete($"Use '{nameof(RightButton)}' instead")] Button2,
	[Obsolete($"Use '{nameof(MiddleButton)}' instead")] Button3,

	Button4,
	Button5,
	Button6,
	Button7,

#pragma warning disable CS0618 // These are the alternatives for the obsolete members

	None = Button0,
	LeftButton = Button1,
	RightButton = Button2,
	MiddleButton = Button3,

#pragma warning restore CS0618
#pragma warning restore CS1591
}
