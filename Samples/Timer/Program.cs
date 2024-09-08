using MiniFB;
using System;

AppDomain.CurrentDomain.UnhandledException += static (_, e) => Console.Error.WriteLine(e.ExceptionObject?.ToString());

uint width = 800, height = 600, size = width * height;
uint noise, carry, seed = 0xbeef;

using var window = new Window("Timer Test", width, height, WindowFlags.Resizable);

var buffer = new Argb[size];

void resize(Window window, int newWidth, int newHeight)
{	
	width = (uint)newWidth;
	height = (uint)newHeight;		

	// It's not safe to resize the buffer while it's pinned. That would be case if, for example, 'window.LifetimeState is WindowLifetimeState.UpdatingWithFixedBuffer'.
	// Instead we signalize the need to resize the buffer by setting 'size' and do the resize at the begin of the next frame.
	//
	// Array.Resize(ref buffer, newWidth * newHeight);

	size = width * height;
}

window.Resize += resize;

window.TrySetViewport(50, 50, width - 50 - 50, height - 50 -50);
resize(window, (int)width - 100, (int)height - 100); // to resize buffer

using var timer = new Timer();

do
{
	if (buffer.Length < size)
	{
		Array.Resize(ref buffer, (int)size);
	}

	for (var i = 0; i < size; ++i)
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

	if (window.Update(buffer, width, height) is not UpdateState.Ok)
	{
		break;
	}

	Console.WriteLine($"frame time: {timer.Delta:0.000000}");
}
while (window.WaitForSync());
