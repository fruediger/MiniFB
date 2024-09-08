namespace MiniFB;

/// <summary>Represents an event handler handling <see cref="Window.MouseButtonChange"/> events</summary>
/// <param name="window">The <see cref="Window"/> which was the source of the event</param>
/// <param name="button">The <see cref="MouseButton"/> associated with the event</param>
/// <param name="modifier">A set of <see cref="KeyModifier"/>s associated with the event</param>
/// <param name="state">A value indicating the <see cref="PressedState"/> of the <paramref name="button"/></param>
public delegate void WindowMouseButtonChangeEventHandler(Window window, MouseButton button, KeyModifier modifier, PressedState state);