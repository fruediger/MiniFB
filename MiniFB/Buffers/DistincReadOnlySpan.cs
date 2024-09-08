using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MiniFB.Buffers;

/// <summary>A representive view to an sequential collection of items of type <typeparamref name="TValue"/> with the ability to <see cref="this[TKey]">access</see> them indexed by an <see cref="Enum">enumeration value</see> of type <typeparamref name="TKey"/></summary>
/// <typeparam name="TKey">The <see cref="Enum">enumeration</see> which defines the access indices for the collection</typeparam>
/// <typeparam name="TValue">The type of the items in the collection</typeparam>
/// <param name="reference">A reference to the item which represents the sequential start of the collection</param>
/// <param name="length">The length of the collection</param>
/// <exception cref="ArgumentOutOfRangeException"><paramref name="reference"/> is a <see langword="null"/> reference or <paramref name="length"/> is negative</exception>
public readonly ref struct DistincReadOnlySpan<TKey, TValue>(ref readonly TValue reference, nint length)
	where TKey : unmanaged, Enum
{
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static ref readonly TValue ValidateReference(ref readonly TValue reference)
	{
		if (Unsafe.IsNullRef(in reference))
		{
			failReferenceArgumentNull();
		}

		return ref reference;

		[DoesNotReturn]
		static void failReferenceArgumentNull() => throw new ArgumentOutOfRangeException(nameof(reference), message: $"'{nameof(reference)}' must be not a null reference");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static nint ValidateLength(nint length)
	{
		if (length is < 0)
		{
			failLengthArgumentNegative();
		}

		return length;

		[DoesNotReturn]
		static void failLengthArgumentNegative() => throw new ArgumentOutOfRangeException(nameof(length), message: $"'{nameof(length)}' must be non-negative");
	}

	private readonly ref readonly TValue mReference = ref ValidateReference(in reference);

	private readonly nint mLength = ValidateLength(length);

	/// <summary>Gets the length (number of items) in the collection</summary>
	/// <value>The length (number of items) in the collection</value>
	public readonly nint Length { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mLength; }

	/// <summary>Gets a reference to the item indexed by <paramref name="key"/></summary>
	/// <param name="key">The index of the item to which a reference should be obtained</param>
	/// <returns>A reference to the item indexed by <paramref name="key"/></returns>
	/// <exception cref="ArgumentOutOfRangeException">The numeric value of <paramref name="key"/> is negative or not less than the collections <see cref="Length">length</see></exception>
	/// <exception cref="NotSupportedException">The underlying type of the index key <see cref="Enum">enumeration</see> <typeparamref name="TKey"/> is unsupported</exception>
	public readonly ref readonly TValue this[TKey key]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			var idx = convertKey(key);

			if (idx is < 0 || idx >= Length)
			{
				failKeyArgumentOutOfRange();
			}

			return ref Unsafe.Add(ref Unsafe.AsRef(in mReference), idx);

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			static nint convertKey(TKey key)
				=> unchecked(Unsafe.SizeOf<TKey>() switch
				{
					sizeof(byte) => (nint)Unsafe.BitCast<TKey, byte>(key),
					sizeof(ushort) => (nint)Unsafe.BitCast<TKey, ushort>(key),
					sizeof(uint) => (nint)Unsafe.BitCast<TKey, uint>(key),
					sizeof(ulong) => (nint)Unsafe.BitCast<TKey, ulong>(key),
					_ => failEnumTypeMismatch()
				});

			[DoesNotReturn]
			static void failKeyArgumentOutOfRange() => throw new ArgumentOutOfRangeException(nameof(key), message: $"'{nameof(key)}' must be a enum type value whose numeric value is non-negative and less than the span's length");

			[DoesNotReturn]
			static nint failEnumTypeMismatch() => throw new NotSupportedException("enum type mismatch");
		}
	}
}
