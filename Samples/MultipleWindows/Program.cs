using MiniFB;
using System;
using static MiniFB.MiniFB;

AppDomain.CurrentDomain.UnhandledException += static (_, e) => Console.Error.WriteLine(e.ExceptionObject?.ToString());

const int widthA = 800, heightA = 600;
var bufferA = new Argb[widthA * heightA];

const int widthB = 320, heightB = 240;
var bufferB = new Argb[widthB * heightB];

static void active(Window window, bool isActive) => Console.WriteLine($"{window.UserData as string} > active: {isActive}");

static void resize(Window window, int width, int height) => Console.WriteLine($"{window.UserData as string} > resize: {width}, {height}");

static void keyboard(Window window, Key key, KeyModifier modifier, PressedState state)
{
	Console.WriteLine($"{window.UserData as string} > keyboard: key: {key.GetName()} (pressed: {state}) [key_mod: {modifier}]");

	if (key is Key.Escape)
	{
		window.Close();
	}
};

static void charInput(Window window, uint keyCode) => Console.WriteLine($"{window.UserData as string} > charCode: {keyCode}");

static void mouseBtn(Window window, MouseButton button, KeyModifier modifier, PressedState state) =>
	Console.WriteLine($"{window.UserData as string} > mouse_btn: button: {button} (pressed: {state}) (at: {window.MousePositionX}, {window.MousePositionY}) [key_mod: {modifier}]");

static void mouseMove(Window window, int x, int y) => Console.WriteLine($"{window.UserData as string} > mouse_move: {x}, {y}");

static void mouseScroll(Window window, KeyModifier modifier, float deltaX, float deltaY) =>
	Console.WriteLine($"{window.UserData as string} > mouse_scroll: x: {deltaX}, y: {deltaY} [key_mod: {modifier}]");

uint noise, carry, seed = 0xbeef;

using var windowA = new Window("Multiple Windows Test", widthA, heightA, WindowFlags.Resizable);

windowA.Active += active;
windowA.Resize += resize;
windowA.KeyChange += keyboard;
windowA.CharacterInput += charInput;
windowA.MouseButtonChange += mouseBtn;
windowA.MouseMove += mouseMove;
windowA.MouseScroll += mouseScroll;

windowA.UserData = "Window A";
windowA.TrySetViewport(25, 25, widthA - 50, heightA - 50);

using var windowB = new Window("Secondary Window", widthB, heightB, WindowFlags.Resizable);

windowB.Active += active;
windowB.Resize += resize;
windowB.KeyChange += keyboard;
windowB.CharacterInput += charInput;
windowB.MouseButtonChange += mouseBtn;
windowB.MouseMove += mouseMove;
windowB.MouseScroll += mouseScroll;

windowB.UserData = "Window B";

var pallete = new Argb[512];
const float inc = 90f / 64f;
for (var c = 0; c < 64; ++c)
{
	var col = (int)(255f * MathF.Sin(c * inc * MathF.PI / 180f) + .5f);
	pallete[64 * 0 + c] = new(255, (byte)col, 0, 0);
	pallete[64 * 1 + c] = new(255, 255, (byte)col, 0);
	pallete[64 * 2 + c] = new(255, (byte)(255 - col), 255, 0);
	pallete[64 * 3 + c] = new(255, 0, 255, (byte)col);
	pallete[64 * 4 + c] = new(255, 0, (byte)(255 - col), 255);
	pallete[64 * 5 + c] = new(255, (byte)col, 0, 255);
	pallete[64 * 6 + c] = new(255, 255, 0, (byte)(255 - col));
	pallete[64 * 7 + c] = new(255, (byte)(255 - col), 0, 0);
}

TargetFPS = 10;

for (var time = 0f; ;)
{
	if (windowA) // is 'windowA' truthy?
	{
		for (var i = 0; i < bufferA.Length; ++i)
		{
			noise = seed;
			noise >>= 3;
			noise ^= seed;
			carry = noise & 1;
			noise >>= 1;
			seed >>= 1;
			seed |= carry << 30;
			noise &= 0xff;
			bufferA[i] = new(0xff, (byte)noise, (byte)noise, (byte)noise);
		}

		if (windowA.Update(bufferA) is not UpdateState.Ok)
		{
			windowA.Dispose(); // make 'windowA' falsy
							   // also, multiple disposes are allowed on MiniFB.NativeObject
		}
	}

	if (windowB) // is 'windowB' truthy?
	{
		var (timeX, timeY) = MathF.SinCos(time * MathF.PI / 180f);

		var i = 0;
		for (var y = 0; y < heightB; ++y)
		{
			var dy = MathF.Cos((y * timeY) * MathF.PI / 180f);

			for (var x = 0; x < widthB; ++x)
			{
				var dx = MathF.Sin((x * timeX) * MathF.PI / 180f);

				bufferB[i++] = pallete[(int)((2f + dx + dy) * .25f * 511f)];
			}
		}
		time += .1f;

		if (windowB.Update(bufferB) is not UpdateState.Ok)
		{
			windowB.Dispose(); // make 'windowB' falsy
							   // also, multiple disposes are allowed on MiniFB.NativeObject
		}
	}

	// Don't need to wait for sync for both windows in the same thread
	if (windowA) // is 'windowA' still truthy?
	{
		if (!windowA.WaitForSync())
		{
			windowA.Dispose(); // make 'windowA' falsy
		}
	}
	else if (windowB) // is 'windowB' still truthy?
	{
		if (!windowB.WaitForSync())
		{
			windowB.Dispose(); // make 'windowB' falsy
		}
	}
	else // both are falsy
	{
		break;
	}
}