using System;
using System.Diagnostics.CodeAnalysis;

namespace MiniFB;

/// <summary>A common base class for managed representations of native objects provided by <see cref="N:MiniFB"/></summary>
public abstract class NativeObject : IEquatable<NativeObject>, IDisposable
{
	private IntPtr mHandle;

	/// <summary>Creates a new <see cref="NativeObject"/> which wrapps the native object pointed to by <paramref name="handle"/></summary>
	/// <param name="handle">A native handle to native object which should get wrapped</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="handle"/> is <see langword="null"/> (numerically equal to <c>0</c>)</exception>
	protected NativeObject(IntPtr handle)
	{
		if (handle is 0)
		{
			failHandleArgumentIsZero();
		}

		mHandle = handle;

		[DoesNotReturn]
		static void failHandleArgumentIsZero() => throw new ArgumentOutOfRangeException(nameof(handle), message: $"'{nameof(handle)}' must be non-zero.");
	}

#pragma warning disable CS1591 // I'm not going to document a finalizer...
	~NativeObject()
	{
		if (mHandle is not 0)
		{
			Dispose(disposing: false);
		}
	}
#pragma warning restore CS1591

	/// <summary>Gets a native handle for the wrapped native object</summary>
	/// <value>A native handle for the wrapped native object</value>
	protected IntPtr Handle => mHandle;

	/// <inheritdoc/>
	public void Dispose()
	{
		if (mHandle is not 0)
		{
			Dispose(disposing: true);
		}

		GC.SuppressFinalize(this);
	}

	/// <summary>Disposes resources handled by this <see cref="NativeObject"/></summary>
	/// <param name="disposing">
	/// Indicates whether to dispose the managed state of this <see cref="NativeObject"/>.
	/// <list type="bullet">
	/// <item><description>If <see langword="true"/> the dispose was initiated by a call to <see cref="Dispose()"/> (this includes <c>using</c> statements and blocks) and managed resources should be safely disposed manually.</description></item>
	/// <item><description>If <see langword="false"/> the dispose was initiated by the finalizer and it wouldn't be safe to dispose mananged resources manually.</description></item>
	/// </list>
	/// </param>
	/// <remarks>You should only ever call this method once. After which <see cref="Handle"/> gets set to <c>0</c> and subsequent calls to <see cref="Dispose()"/> will no longer divert to a call to this method.</remarks>
	protected virtual void Dispose(bool disposing) => mHandle = 0;

	/// <inheritdoc/>
	public bool Equals(NativeObject? other) => other is NativeObject { mHandle: var otherHandle } && mHandle == otherHandle;

	/// <inheritdoc/>
	public override bool Equals(object? obj) => Equals(obj as NativeObject);

	/// <inheritdoc/>
	public override int GetHashCode() => mHandle.GetHashCode();

	/// <summary>Gets a value indicating whether <paramref name="value"/> is not a valid <see cref="NativeObject"/></summary>
	/// <param name="value">The <see cref="NativeObject"/> to check if its not valid</param>
	/// <returns><see langword="true"/> when <paramref name="value"/> is <see langword="null"/> or its <see cref="Handle"/> is numerically equal to <c>0</c> (<see langword="null"/>); otherwise, <see langword="false"/></returns>
	public static bool operator false(NativeObject? value) => value is not { mHandle: not 0 };

	/// <summary>Gets a value indicating whether <paramref name="value"/> is a valid <see cref="NativeObject"/></summary>
	/// <param name="value">The <see cref="NativeObject"/> to check if its valid</param>
	/// <returns><see langword="true"/> when <paramref name="value"/> is not <see langword="null"/> and its <see cref="Handle"/> is not numerically equal to <c>0</c> (<see langword="null"/>); otherwise <see langword="false"/></returns>
	public static bool operator true(NativeObject? value) => value is { mHandle: not 0 };
}
