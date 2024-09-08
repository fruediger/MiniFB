namespace MiniFB;

/// <summary>Represents an event handler handling <see cref="Window.Closing"/> events</summary>
/// <param name="window">The <see cref="Window"/> which was the source of the event</param>
/// <returns>An value indicating whether the <see cref="Window"/> should be closed or not</returns>
public delegate bool WindowClosingEventHandler(Window window);
