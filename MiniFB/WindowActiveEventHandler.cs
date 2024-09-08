namespace MiniFB;

/// <summary>Represents an event handler handling <see cref="Window.Active"/> events</summary>
/// <param name="window">The <see cref="Window"/> which was the source of the event</param>
/// <param name="isActive">A value indicating the <see cref="Window"/>'s <see cref="Window.IsActive">IsActive</see> state</param>
public delegate void WindowActiveEventHandler(Window window, bool isActive);
