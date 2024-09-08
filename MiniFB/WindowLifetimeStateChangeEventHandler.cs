namespace MiniFB;

/// <summary>Represents an event handler handling <see cref="Window.LifetimeStateChange"/> events</summary>
/// <param name="window">The <see cref="Window"/> which was the source of the event</param>
/// <param name="previousLifetimeState">The <see cref="Window"/>'s <see cref="WindowLifetimeState"/> before the change to its current <see cref="Window.LifetimeState">LifetimeState</see></param>
public delegate void WindowLifetimeStateChangeEventHandler(Window window, WindowLifetimeState previousLifetimeState);
