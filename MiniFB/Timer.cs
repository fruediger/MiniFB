using MiniFB.SourceGeneration;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MiniFB;

/// <summary>A timer to keep track of passed time</summary>
public partial class Timer : NativeObject
{
	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial IntPtr mfb_timer_create();

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial void mfb_timer_destroy(IntPtr tmr);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial void mfb_timer_reset(IntPtr tmr);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial double mfb_timer_now(IntPtr tmr);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial double mfb_timer_delta(IntPtr tmr);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial double mfb_timer_get_frequency();

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial double mfb_timer_get_resolution();

	private static IntPtr ValidateInstantiation(IntPtr handle)
	{
		if (handle is 0)
		{
			failCouldNotInstantiateNativeTimerObject();
		}

		return handle;

		[DoesNotReturn]
		static void failCouldNotInstantiateNativeTimerObject() => throw new NativeOperationException($"Could not instanciate a native {nameof(Timer)} object");
	}

	/// <summary>Get the global <see cref="Timer"/> frequency</summary>
	/// <value>The global <see cref="Timer"/> frequency in ticks per second</value>
	/// <remarks>This value is the reciprocal of <see cref="Resolution"/></remarks>
	public static double Frequency => mfb_timer_get_frequency();

	/// <summary>Gets the global <see cref="Timer"/> resolution</summary>
	/// <value>The global <see cref="Timer"/> resolution in seconds per tick</value>
	/// <remarks>This value is the reciprocal of <see cref="Frequency"/></remarks>
	public static double Resolution => mfb_timer_get_resolution();

	/// <inheritdoc/>
	/// <remarks>The native object pointed to by <paramref name="handle"/> is assumed to be <see cref="Timer"/> object</remarks>
	protected Timer(IntPtr handle) : base(handle) { }

	/// <summary>Creates a new <see cref="Timer"/></summary>
	/// <exception cref="NativeOperationException">A new <see cref="Timer"/> could not be instantiated</exception>
	public Timer() : this(ValidateInstantiation(mfb_timer_create())) { }

	/// <inheritdoc/>
	protected override void Dispose(bool disposing)
	{
		mfb_timer_destroy(Handle);

		base.Dispose(disposing);
	}

	/// <summary>Gets the passed time between the last call to <see cref="Delta"/>, the last call to <see cref="Reset"/>, or the creation of this <see cref="Timer"/> (whichever was the last)</summary>
	/// <value>The delta time in seconds on this <see cref="Timer"/></value>
	/// <remarks>Obtaining this value resets the <see cref="Delta">delta time</see> on this <see cref="Timer"/></remarks>
	public double Delta => mfb_timer_delta(Handle);

	/// <summary>Gets the passed time between the last call to <see cref="Reset"/> or the creation of this <see cref="Timer"/> (whichever was the last)</summary>
	/// <value>The current time in seconds on this <see cref="Timer"/></value>
	public double Now => mfb_timer_now(Handle);

	/// <summary>Resets the <see cref="Now">current time</see> as well as the <see cref="Delta">delta time</see> on this <see cref="Timer"/> to <c>0</c></summary>
	public void Reset() => mfb_timer_reset(Handle);
}
