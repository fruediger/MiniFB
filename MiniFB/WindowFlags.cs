using System;

namespace MiniFB;

/// <summary>A flag indicating a certain behavior of a <see cref="Window"/></summary>
[Flags]
public enum WindowFlags : int
{
	/// <summary>No flags</summary>
	None = 0b_00000,

	/// <summary>The <see cref="Window"/> should be resizable</summary>
	Resizable = 0b_00001,

	/// <summary>The <see cref="Window"/> should be in fullscreen mode</summary>
	Fullscreen = 0b_00010,

	/// <summary>The <see cref="Window"/> should be in fullscreen desktop mode</summary>
	FullscreenDesktop = 0b_00100,

	/// <summary>The <see cref="Window"/> should be rendered without a border or chrome</summary>
	Borderless = 0b_01000,

	/// <summary>The <see cref="Window"/> should be always visible on top of other non-always-on-top windows</summary>
	AlwaysOnTop = 0b_10000,
}
