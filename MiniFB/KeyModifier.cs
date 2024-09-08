using System;

namespace MiniFB;

/// <summary>A keyboard modifier key (mostly used in combination with a <see cref="Key"/> or <see cref="MouseButton"/>)</summary>
[Flags]
public enum KeyModifier : int
{
#pragma warning disable CS1591 // This is self-documenting

	None = 0b_000000,

	Shift = 0b_000001,
	Control = 0b_000010,
	Alt = 0b_000100,
	Super = 0b_001000,
	CapsLock = 0b_010000,
	NumLock = 0b_100000,

#pragma warning restore CS1591
}
