using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MiniFB;

partial class Window
{
	// we choose a pack of 4 here, since everything (except for the bools at the end)
	// conveniently aligns at 4-byte boundaries in a 64-bit ABI as well as in a 32-bit ABI.
	// For the bools at the end we use a bit of a hack ("struct PaddedStates")
	// to ensure a correct padding at the end.
	/// <seealso href="https://github.com/fruediger/minifb-native/blob/master/src/WindowData.h"/>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	private ref struct Data
	{
		public ref byte Specific;
		public ref byte UserData;

		public unsafe WindowActiveCallback ActiveFunc;
		public unsafe WindowResizeCallback ResizeFunc;
		public unsafe WindowCloseCallback CloseFunc;
		public unsafe WindowKeyboardCallback KeyboardFunc;
		public unsafe WindowCharInputCallback CharInputFunc;
		public unsafe WindowMouseButtonCallback MouseBtnFunc;
		public unsafe WindowMouseMoveCallback MouseMoveFunc;
		public unsafe WindowMouseScrollCallback MouseWheelFunc;

		public uint WindowWidth;
		public uint WindowHeight;

		public uint DstOffsetX;
		public uint DstOffsetY;
		public uint DstWidth;
		public uint DstHeight;
		public float FactorX;
		public float FactorY;
		public float FactorWidth;
		public float FactorHeight;

		public ref Argb DrawBuffer;
		public uint BufferWidth;
		public uint BufferHeight;
		public uint BufferStride;

		public int MousePosX;
		public int MousePosY;
		public float MouseWheelX;
		public float MouseWheelY;
		public MouseButtonStatusArray MouseButtonStatus;
		public KeyStatusArray KeyStatus;
		public KeyModifier ModKeys;

		public PaddedStates States;

#pragma warning disable IDE0044, IDE0051
		[InlineArray(MouseButtonBufferSize)]
		public struct MouseButtonStatusArray
		{
			private PressedState _;
		}

		[InlineArray(KeyBufferSize)]
		public struct KeyStatusArray
		{
			private PressedState _;
		}

		[StructLayout(LayoutKind.Explicit, Pack = 1)]
		public struct PaddedStates
		{
			[FieldOffset(0 * sizeof(bool))] public bool IsActive;
			[FieldOffset(1 * sizeof(bool))] public bool IsInitialized;
			[FieldOffset(2 * sizeof(bool))] public bool Close;

			// to ensure the correct padding
			[FieldOffset(0)] private nuint _;
		}
#pragma warning restore IDE0044, IDE0051
	}
}
