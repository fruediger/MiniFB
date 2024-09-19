using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MiniFB;

// with a pack of 4, we achieve binary compatability with the analogous C struct type declaration (on 64-bit as well as on 32-bit)
/// <summary>A record of a reference to sequential buffer containing icon pixel data together the width and the height of that buffer</summary>
[StructLayout(LayoutKind.Sequential, Pack = sizeof(uint))]
public readonly ref struct IconInfo
{
	// with that layout we're somewhat binary compatable to the analogous C struct type declaration (see https://github.com/fruediger/minifb-native/blob/master/include/MiniFB_types.h#L179)
	private readonly ref readonly Argb mBuffer;
	private readonly uint mWidth, mHeight;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private IconInfo(ref readonly Argb buffer, uint width, uint height)
	{
		mBuffer = ref buffer;
		mWidth = width;
		mHeight = height;
	}

	/// <summary>Create a new <see cref="IconInfo"/> from a sequential pixel data buffer</summary>
	/// <param name="pixelBuffer">The sequential buffer containing the icon pixel data</param>
	/// <param name="width">The width of the icon</param>
	/// <param name="height">The height of the icon</param>
	/// <exception cref="ArgumentNullException"><paramref name="pixelBuffer"/> refers to the <c><see langword="null"/></c>-reference</exception>
	/// <exception cref="ArgumentException"><paramref name="pixelBuffer"/>'s <see cref="ReadOnlySpan{T}.Length">length</see> is less than <c><paramref name="width"/> * <paramref name="height"/></c></exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public IconInfo(ReadOnlySpan<Argb> pixelBuffer, uint width, uint height) : this(in MemoryMarshal.GetReference(pixelBuffer),	width, height)
	{
		if (Unsafe.IsNullRef(in MemoryMarshal.GetReference(pixelBuffer)))
		{
			failPixelBufferArgumentNull();
		}

		if (pixelBuffer.Length < width * height)
		{
			failPixelBufferArgumentToSmall();
		}

		[DoesNotReturn]
		static void failPixelBufferArgumentNull() => throw new ArgumentNullException(paramName: nameof(pixelBuffer), message: $"'{nameof(pixelBuffer)}' must be not a null reference");

		[DoesNotReturn]
		static void failPixelBufferArgumentToSmall() => throw new ArgumentException(paramName: nameof(pixelBuffer), message: $"'{nameof(pixelBuffer)}'s length must be at least '{nameof(width)}' * '{nameof(height)}'");
	}

	/// <summary>Create a new <see cref="IconInfo"/> from pointer to a sequential pixel data buffer</summary>
	/// <param name="pixelBuffer">A pointer to the start of the buffer containing the icon pixel data</param>
	/// <param name="width">The width of the icon</param>
	/// <param name="height">The height of the icon</param>
	/// <remarks>NOTE: no additional validity checks are made (unlike <see cref="IconInfo(ReadOnlySpan{Argb}, uint, uint)"/>), so use with caution</remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public unsafe IconInfo(Argb* pixelBuffer, uint width, uint height) : this(in Unsafe.AsRef<Argb>(pixelBuffer), width, height)
	{ }

	internal readonly ref readonly Argb Buffer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => ref mBuffer; }

	internal readonly uint Width { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mWidth; }

	internal readonly uint Height { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mHeight; }
}
