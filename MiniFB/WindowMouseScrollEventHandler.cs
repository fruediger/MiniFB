namespace MiniFB;

/// <summary>Represents an event handler handling <see cref="Window.MouseScroll"/> events</summary>
/// <param name="window">The <see cref="Window"/> which was the source of the event</param>
/// <param name="modifier">A set of <see cref="KeyModifier"/>s associated with the event</param>
/// <param name="deltaX">The change in horizontal mouse scrollwheel position</param>
/// <param name="deltaY">The change in vertical mouse scrollwheel position</param>
public delegate void WindowMouseScrollEventHandler(Window window, KeyModifier modifier, float deltaX, float deltaY);
