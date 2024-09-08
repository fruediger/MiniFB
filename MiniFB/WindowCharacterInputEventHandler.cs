namespace MiniFB;

/// <summary>Represents an event handler handling <see cref="Window.CharacterInput"/> events</summary>
/// <param name="window">The <see cref="Window"/> which was the source of the event</param>
/// <param name="keyCode">The key code of the character input associated the event</param>
public delegate void WindowCharacterInputEventHandler(Window window, uint keyCode);
