using MiniFB;
using System;

AppDomain.CurrentDomain.UnhandledException += static (_, e) => Console.Error.WriteLine(e.ExceptionObject?.ToString());

const int width = 960, height = 640;

var buffer = new Argb[width * height];
var active = true;

uint noise, carry, seed = 0xbeef;

using var window = new Window("full screen auto", width, height, WindowFlags.Fullscreen);

window.Active += (window, isActive) => active = isActive;

window.KeyChange += static (window, key, modifier, state) =>
{
	if (key is Key.Escape)
	{
		window.Close();
	}
};

window.TrySetViewportBestFit(width, height);

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