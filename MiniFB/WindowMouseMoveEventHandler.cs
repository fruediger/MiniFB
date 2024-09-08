namespace MiniFB;

/// <summary>Represents an event handler handling <see cref="Window.MouseMove"/> events</summary>
/// <param name="window">The <see cref="Window"/> which was the source of the event</param>
/// <param name="x">The horizontal mouse position relative to the origin of the <see cref="Window"/></param>
/// <param name="y">The vertical mouse position relative to the origin of the <see cref="Window"/></param>
public delegate void WindowMouseMoveEventHandler(Window window, int x, int y);
