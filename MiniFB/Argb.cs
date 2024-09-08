using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MiniFB;

/// <summary>
/// Represents a ARGB color by its transparency channel component (<see cref="A">alpha</see>) and its color channel components (<see cref="R">red</see>, <see cref="G">green</see>, <see cref="B">blue</see>)
/// in an appropriate memory layout for the host device
/// </summary>
/// <param name="a">The alpha component to use for the <see cref="A">alpha transparency channel</see></param>
/// <param name="r">The red component to use for the <see cref="R">red color channel</see></param>
/// <param name="g">The green component to use for the <see cref="G">green color channel</see></param>
/// <param name="b">The blue component to use for the <see cref="B">blue color channel</see></param>
/// <remarks>
/// The stored memory layout for an <see cref="Argb"/> object (32 bits) is
/// <list type="bullet">
/// <item><see cref="R">R</see> (8 bits), <see cref="G">G</see> (8 bits), <see cref="B">B</see> (8 bits), <see cref="A">A</see> (8 bits), in that order, for little endian Android™ devices</item>
/// <item><see cref="B">B</see> (8 bits), <see cref="G">G</see> (8 bits), <see cref="R">R</see> (8 bits), <see cref="A">A</see> (8 bits), in that order, everywhere else</item>
/// </list>
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct Argb(byte a, byte r, byte g, byte b) : IEquatable<Argb>, IEqualityOperators<Argb, Argb, bool>
{
	private static bool IsAndroidLE => OperatingSystem.IsAndroid() && BitConverter.IsLittleEndian;

	private byte m0 = !IsAndroidLE ? b : r, m1 = g, m2 = !IsAndroidLE ? r : b, m3 = a;

	/// <inheritdoc cref="Argb(byte, byte, byte, byte)"/>
	/// <remarks>The <see cref="A">alpha transparency channel</see> is set to be fully opaque (<c>255</c>)</remarks>
	public Argb(byte r, byte g, byte b) : this(byte.MaxValue, r, g, b) { }

	/// <summary>Gets or sets the alpha component for the <see cref="A">alpha transparency channel</see></summary>
	/// <value>The alpha component of the <see cref="A">alpha transparency channel</see></value>
	public byte A { readonly get => m3; set => m3 = value; }

	/// <summary>Gets or sets the red component for the <see cref="R">red color channel</see></summary>
	/// <value>The red component of the <see cref="R">red color channel</see></value>
	public byte R { readonly get => !IsAndroidLE ? m2 : m0; set { if (!IsAndroidLE) { m2 = value; } else { m0 = value; } } }

	/// <summary>Gets or sets the green component for the <see cref="G">green color channel</see></summary>
	/// <value>The green component of the <see cref="G">green color channel</see></value>
	public byte G { readonly get => m1; set => m1 = value; }

	/// <summary>Gets or sets the blue component for the <see cref="B">blue color channel</see></summary>
	/// <value>The blue component of the <see cref="B">blue color channel</see></value>
	public byte B { readonly get => !IsAndroidLE ? m0 : m2; set { if (!IsAndroidLE) { m0 = value; } else { m2 = value; } } }

	/// <summary>Gets or sets the numeric representation of this <see cref="Argb"/></summary>
	/// <value>The numeric representation of this <see cref="Argb"/></value>
	/// <remarks>The returned value has the appropriate bit layout for the host device</remarks>
	public uint NumericValue { readonly get => Unsafe.BitCast<Argb, uint>(this); set => this = Unsafe.BitCast<uint, Argb>(value); }

	/// <summary>
	/// Deconstructs this <see cref="Argb"/> into its <see cref="A">alpha transparency channel component</see> (<paramref name="a"/>),
	/// its <see cref="R">red color channel component</see> (<paramref name="r"/>), its <see cref="G">green channel component</see> (<paramref name="g"/>),
	/// and its <see cref="B">blue color channel component</see> (<paramref name="b"/>)
	/// </summary>
	/// <param name="a">The <see cref="A">alpha transparency channel component</see> of this <see cref="Argb"/></param>
	/// <param name="r">The <see cref="R">red color channel component</see> of this <see cref="Argb"/></param>
	/// <param name="g">The <see cref="G">green color channel component</see> of this <see cref="Argb"/></param>
	/// <param name="b">The <see cref="B">blue color channel component</see> of this <see cref="Argb"/></param>
	public readonly void Deconstruct(out byte a, out byte r, out byte g, out byte b) { a = A; r = R; g = G; b = B; }

	/// <summary>
	/// Deconstructs this <see cref="Argb"/> into its <see cref="R">red color channel component</see> (<paramref name="r"/>),
	/// its <see cref="G">green channel component</see> (<paramref name="g"/>), and its <see cref="B">blue color channel component</see> (<paramref name="b"/>)
	/// </summary>
	/// <param name="r">The <see cref="R">red color channel component</see> of this <see cref="Argb"/></param>
	/// <param name="g">The <see cref="G">green color channel component</see> of this <see cref="Argb"/></param>
	/// <param name="b">The <see cref="B">blue color channel component</see> of this <see cref="Argb"/></param>
	public readonly void Deconstruct(out byte r, out byte g, out byte b) => Deconstruct(out _, out r, out g, out b);

	/// <inheritdoc/>
	public readonly bool Equals(Argb other) => NumericValue == other.NumericValue;

	/// <inheritdoc/>
	public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is Argb other && Equals(other);

	/// <inheritdoc/>
	public readonly override int GetHashCode() => NumericValue.GetHashCode();

	/// <inheritdoc/>
	public readonly override string ToString() => $"{{ A: {A}, R: {R}, G: {G}, B: {B} }}";

	/// <inheritdoc/>
	public static bool operator ==(Argb left, Argb right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(Argb left, Argb right) => !(left == right);

	/// <summary>Converts a value tuple with an alpha transparency channel component, a red color channel component,
	/// a green color channel component, and a blue color channel component into a <see cref="Argb"/> representation</summary>
	/// <param name="argb">The value tuple to convert into a <see cref="Argb"/> representation</param>
	public static implicit operator Argb((byte a, byte r, byte g, byte b) argb) => new(argb.a, argb.r, argb.g, argb.b);

	/// <summary>Converts a value tuple with a red color channel component, a green color channel component,
	/// and a blue color channel component into a <see cref="Argb"/> representation</summary>
	/// <param name="rgb">The value tuple to convert into a <see cref="Argb"/> representation</param>
	/// <remarks>The <see cref="A">alpha transparency channel</see> of the resulting <see cref="Argb"/> is set to be fully opaque (<c>255</c>)</remarks>
	public static implicit operator Argb((byte r, byte g, byte b) rgb) => new(rgb.r, rgb.g, rgb.b);

	/// <summary>
	/// Converts the <paramref name="argb"/> into a value tuple representation with an alpha transparency channel component,
	/// a red color channel component, a green color channel component, and a blue color channel component
	/// </summary>
	/// <param name="argb">The <see cref="Argb"/> to convert into a value tuple representation</param>
	public static implicit operator (byte a, byte r, byte g, byte b)(Argb argb) => (argb.A, argb.R, argb.G, argb.B);

	/// <summary>
	/// Converts the <paramref name="argb"/> into a value tuple representation with an a red color channel component,
	/// a green color channel component, and a blue color channel component
	/// </summary>
	/// <param name="argb">The <see cref="Argb"/> to convert into a value tuple representation</param>
	public static implicit operator (byte r, byte g, byte b)(Argb argb) => (argb.R, argb.G, argb.B);
}
