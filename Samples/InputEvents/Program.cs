using MiniFB;
using System;

AppDomain.CurrentDomain.UnhandledException += static (_, e) => Console.Error.WriteLine(e.ExceptionObject?.ToString());

const int width = 800, height = 600;

var buffer = new Argb[width * height];
var active = true;

uint noise, carry, seed = 0xbeef;

using var window = new Window("Input Events Test", width, height, WindowFlags.Resizable);

window.Active += (window, isActive) =>
{
	Console.WriteLine($"{window.UserData as string} > active: {isActive}");

	active = isActive;
};

window.Resize += static (window, newWidth, newHeight) =>
{
	var (x, y) = (0u, 0u);

	Console.WriteLine($"{window.UserData as string} > resize: {newWidth}, {newHeight}");

	if (newWidth is > width)
	{
		x = (uint)(newWidth - width) >> 1;
		newWidth = width;
	}

	if (newHeight is > height)
	{
		y = (uint)(newHeight - height) >> 1;
		newHeight = height;
	}

	window.TrySetViewport(x, y, width, height);
};

window.Closing += static window =>
{
	Console.WriteLine($"{window.UserData as string} > close");

	return true; // true => confirm close
	             // false => don't close
};

window.KeyChange += static (window, key, modifier, state) =>
{
	Console.WriteLine($"{window.UserData as string} > keyboard: key: {key.GetName()} (pressed: {state}) [key_mod: {modifier}]");

	if (key is Key.Escape)
	{
		window.Close();
	}
};

window.CharacterInput += static (window, keyCode) =>
	Console.WriteLine($"{window.UserData as string} > charCode: {keyCode}");

window.MouseButtonChange += static (window, button, modifier, state) =>
	Console.WriteLine($"{window.UserData as string} > mouse_btn: button: {button} (pressed: {state}) (at: {window.MousePositionX}, {window.MousePositionY}) [key_mod: {modifier}]");

window.MouseMove += static (window, x, y) =>
	Console.WriteLine($"{window.UserData as string} > mouse_move: {x}, {y}");

window.MouseScroll += static (window, modifier, deltaX, deltaY) =>
	Console.WriteLine($"{window.UserData as string} > mouse_scroll: x: {deltaX}, y: {deltaY} [key_mod: {modifier}]");

window.UserData = "Input Event Test";

UpdateState state;
do
{
	if (active)
	{
		for (var i = 0; i < buffer.Length; ++i)
		{
			noise = seed;
			noise >>= 3;
			noise ^= seed;
			carry = noise & 1;
			noise >>= 1;
			seed >>= 1;
			seed |= carry << 30;
			noise &= 0xff;
			buffer[i] = new(0xff, (byte)noise, (byte)noise, (byte)noise);
		}

		state = window.Update(buffer);
	}
	else
	{
		state = window.UpdateEventsOnly();
	}
}
while (
	state is UpdateState.Ok
	&& window.WaitForSync()
);