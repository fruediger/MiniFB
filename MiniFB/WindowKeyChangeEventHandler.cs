namespace MiniFB;

/// <summary>Represents an event handler handling <see cref="Window.KeyChange"/> events</summary>
/// <param name="window">The <see cref="Window"/> which was the source of the event</param>
/// <param name="key">The <see cref="Key"/> associated with the event</param>
/// <param name="modifier">A set of <see cref="KeyModifier"/>s associated with the event</param>
/// <param name="state">A value indicating the <see cref="PressedState"/> of the <paramref name="key"/></param>
public delegate void WindowKeyChangeEventHandler(Window window, Key key, KeyModifier modifier, PressedState state);
