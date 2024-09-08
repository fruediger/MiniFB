namespace MiniFB;

/// <summary>Represents an event handler handling <see cref="Window.Resize"/> events</summary>
/// <param name="window">The <see cref="Window"/> which was the source of the event</param>
/// <param name="width">The <see cref="Window"/>'s width</param>
/// <param name="height">The <see cref="Window"/>'s height</param>
public delegate void WindowResizeEventHandler(Window window, int width, int height);
