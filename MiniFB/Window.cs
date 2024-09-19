using MiniFB.Buffers;
using MiniFB.Internal;
using MiniFB.SourceGeneration;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using static MiniFB.Internal.NativeImportConditionExpressions;

namespace MiniFB;

/// <summary>A window to which a pixel buffer can be rendered onto and on which certain input events can be handled</summary>
public partial class Window : NativeObject
{
	#region Native API

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial IntPtr mfb_open(IntPtr title, uint width, uint height);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial IntPtr mfb_open_ex(IntPtr title, uint width, uint height, WindowFlags flags);
	
#pragma warning disable CS8500 // See the comment on a similar suppression in one of the 'CreateWindow' methods to find out why this is okay
	[NativeImportFunction<MiniFB.Library, IsWindows>(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe partial IntPtr mfb_open_with_icons(IntPtr title, uint width, uint height, IconInfo* icon_small, IconInfo* icon_big);

	[NativeImportFunction<MiniFB.Library, IsWindows>(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe partial IntPtr mfb_open_ex_with_icons(IntPtr title, uint width, uint height, WindowFlags flags, IconInfo* icon_small, IconInfo* icon_big);
#pragma warning restore CS8500

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial UpdateState mfb_update(IntPtr window, ref readonly Argb buffer);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial UpdateState mfb_update_ex(IntPtr window, ref readonly Argb buffer, uint width, uint height);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial UpdateState mfb_update_events(IntPtr window);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial void mfb_close(IntPtr window);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial void mfb_set_user_data(IntPtr window, IntPtr user_data);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial IntPtr mfb_get_user_data(IntPtr window);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial bool mfb_set_viewport(IntPtr window, uint offset_x, uint offset_y, uint width, uint height);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial bool mfb_set_viewport_best_fit(IntPtr window, uint old_width, uint old_height);

	[NativeImportFunction<MiniFB.Library, OrElse<IsWindows, IsLinux>>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial void mfb_set_title(IntPtr window, IntPtr title);

	[NativeImportFunction<MiniFB.Library, OrElse<IsWindows, IsLinux>>(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe partial IntPtr mfb_get_title(IntPtr window, WindowGetTitleBufferCallback callback, void* data);

	[Obsolete($"Use '{nameof(mfb_get_monitor_scale)}' instead")]
	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial void mfb_get_monitor_dpi(IntPtr window, out float dpi_x, out float dpi_y);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial void mfb_get_monitor_scale(IntPtr window, out float scaleX, out float scaleY);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe partial void mfb_set_active_callback(IntPtr window, WindowActiveCallback callback);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe partial void mfb_set_resize_callback(IntPtr window, WindowResizeCallback callback);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe partial void mfb_set_close_callback(IntPtr window, WindowCloseCallback callback);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe partial void mfb_set_keyboard_callback(IntPtr window, WindowKeyboardCallback callback);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe partial void mfb_set_char_input_callback(IntPtr window, WindowCharInputCallback callback);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe partial void mfb_set_mouse_button_callback(IntPtr window, WindowMouseButtonCallback callback);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe partial void mfb_set_mouse_move_callback(IntPtr window, WindowMouseMoveCallback callback);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe partial void mfb_set_mouse_scroll_callback(IntPtr window, WindowMouseScrollCallback callback);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial bool mfb_is_window_active(IntPtr window);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial uint mfb_get_window_width(IntPtr window);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial uint mfb_get_window_height(IntPtr window);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial int mfb_get_mouse_x(IntPtr window);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial int mfb_get_mouse_y(IntPtr window);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial float mfb_get_mouse_scroll_x(IntPtr window);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial float mfb_get_mouse_scroll_y(IntPtr window);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static unsafe partial PressedState* mfb_get_mouse_button_buffer(IntPtr window);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static unsafe partial PressedState* mfb_get_key_buffer(IntPtr window);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial bool mfb_wait_sync(IntPtr window);

	#endregion

	#region Callback wrappers implementation

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static void ActiveCallback(nint window, byte isActive)
	{
		var userData = mfb_get_user_data(window);
		if (userData is not 0 && GCHandle.FromIntPtr(userData) is { IsAllocated: true, Target: Window target })
		{
			target.OnActive(Unsafe.BitCast<byte, bool>(isActive));
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static void ResizeCallback(nint window, int width, int height)
	{
		var userData = mfb_get_user_data(window);
		if (userData is not 0 && GCHandle.FromIntPtr(userData) is { IsAllocated: true, Target: Window target })
		{
			target.OnResize(width, height);
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static byte CloseCallback(nint window)
	{
		var result = true;

		var userData = mfb_get_user_data(window);
		if (userData is not 0 && GCHandle.FromIntPtr(userData) is { IsAllocated: true, Target: Window target })
		{
			var previousLifetimeState = target.LifetimeState;
			target.LifetimeState = WindowLifetimeState.Closing;

			result = target.OnClosing();

			if (!result)
			{
				target.LifetimeState = previousLifetimeState;
			}
		}

		return Unsafe.BitCast<bool, byte>(result);
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static void KeyboardCallback(nint window, Key key, KeyModifier modifier, PressedState state)
	{
		var userData = mfb_get_user_data(window);
		if (userData is not 0 && GCHandle.FromIntPtr(userData) is { IsAllocated: true, Target: Window target })
		{
			target.OnKeyChange(key, modifier, state);
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static void CharInputCallback(nint window, uint keyCode)
	{
		var userData = mfb_get_user_data(window);
		if (userData is not 0 && GCHandle.FromIntPtr(userData) is { IsAllocated: true, Target: Window target })
		{
			target.OnCharacterInput(keyCode);
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static void MouseButtonCallback(nint window, MouseButton button, KeyModifier modifier, PressedState state)
	{
		var userData = mfb_get_user_data(window);
		if (userData is not 0 && GCHandle.FromIntPtr(userData) is { IsAllocated: true, Target: Window target })
		{
			target.OnMouseButtonChange(button, modifier, state);
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static void MouseMoveCallback(nint window, int x, int y)
	{
		var userData = mfb_get_user_data(window);
		if (userData is not 0 && GCHandle.FromIntPtr(userData) is { IsAllocated: true, Target: Window target })
		{
			target.OnMouseMove(x, y);
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static void MouseScrollCallback(nint window, KeyModifier modifier, float deltaX, float deltaY)
	{
		var userData = mfb_get_user_data(window);
		if (userData is not 0 && GCHandle.FromIntPtr(userData) is { IsAllocated: true, Target: Window target })
		{
			target.OnMouseScroll(modifier, deltaX, deltaY);
		}
	}

	#endregion

	#region Helpers

	// these values were pulled from the MiniFB source
	// (https://github.com/fruediger/minifb-native/blob/master/src/WindowData.h#L42 and https://github.com/fruediger/minifb-native/blob/master/src/WindowData.h#L43)
	private const int MouseButtonBufferSize = 8, KeyBufferSize = 512;
		
	private static volatile uint mNativeBufferUsers = 0;
	private static unsafe byte* mNativeBufferPtr = null;
	private static nuint mNativeBufferSize = 0;
	private static SpinLock mNativeBufferLock;

	private static unsafe byte* AcquireLockedNativeBuffer(nuint minimumSize, out nuint actualSize, out bool lockTaken)
	{
		const int lockTimeoutMs = 10;
		const int lowerSizeThreshold = 4,  // 2^4 = 16
			      upperSizeThreshold = 22; // 2^22 = 4194304 = 4096 * 1024 (most common default page size on Windows)

		lockTaken = false;

		mNativeBufferLock.TryEnter(lockTimeoutMs, ref lockTaken);
		if (lockTaken)
		{
			if (minimumSize <= mNativeBufferSize)
			{
				actualSize = mNativeBufferSize;
				return mNativeBufferPtr;
			}

			actualSize = requiredSize(minimumSize);
			return mNativeBufferPtr = (byte*)NativeMemory.Realloc(mNativeBufferPtr, mNativeBufferSize = actualSize);
		}
		else
		{
			// in the rare case where we would have a race condition,
			// we simply allocate a new intermediate buffer which gets deallocated straight away later on
			// NOTE: here, lockTaken is false

			actualSize = requiredSize(minimumSize);
			return (byte*)NativeMemory.Alloc(actualSize);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		static nuint requiredSize(nuint minimumSize)
			=> minimumSize switch
			{
				<= (1 << lowerSizeThreshold) => 1 << lowerSizeThreshold, // 2^lowerSizeThreshold as a lower limit
				<= (1 << upperSizeThreshold) => unchecked((nuint)1 << (int)(nuint.Log2(minimumSize - 1) + 1)), // the next biggest power of two in which the requested size fits
				_                            => unchecked((nuint)(-(-(nint)minimumSize >> upperSizeThreshold) << upperSizeThreshold)) // the next biggest multiple of 2^upperSizeThreshold in which the requested size fits
			};
	}

	private static unsafe void ReleaseLockedNativeBuffer(byte* nativeBufferPtr, bool lockTaken)
	{
		if (lockTaken)
		{
			mNativeBufferLock.Exit();
		}
		else
		{
			// simply destroy the intermediate buffer
			NativeMemory.Free(nativeBufferPtr);
		}
	}

	private static IntPtr ValidateConvertTitle(string title, out bool lockTaken)
	{
		if (title is null)
		{
			failTitleArgumentNull();
		}

		var size = Encoding.UTF8.GetByteCount(title) + 1;

		unsafe
		{
			var titleNativeBuffer = AcquireLockedNativeBuffer((nuint)size, out var actualSize, out lockTaken);

			titleNativeBuffer[
				Math.Min( 
					(nuint)Encoding.UTF8.GetBytes(
						title,
						MemoryMarshal.CreateSpan(
							ref Unsafe.AsRef<byte>(titleNativeBuffer),
							actualSize < int.MaxValue ? (int)actualSize : int.MaxValue
						)
					),
					actualSize - 1
				)] = 0;

			return (IntPtr)titleNativeBuffer;
		}
		
		[DoesNotReturn]
		static void failTitleArgumentNull() => throw new ArgumentNullException(nameof(title));
	}

	private static void ReleaseTitleBuffer(IntPtr titleBuffer, bool lockTaken)
	{
		unsafe
		{
			ReleaseLockedNativeBuffer((byte*)titleBuffer, lockTaken);
		}
	}

	private static ref readonly Argb ValidatePinIconBuffer(ReadOnlyMemory<Argb> iconBuffer, uint iconWidth, uint iconHeight, out MemoryHandle iconBufferPin, IntPtr titleNativeBuffer, bool lockTaken)
	{
		if (iconBuffer.Length < iconWidth * iconHeight)
		{
			ReleaseTitleBuffer(titleNativeBuffer, lockTaken);

			failIconBufferArgumentToSmall();
		}

		iconBufferPin = iconBuffer.Pin();

		unsafe
		{
			return ref Unsafe.AsRef<Argb>(iconBufferPin.Pointer);
		}

		[DoesNotReturn]
		static void failIconBufferArgumentToSmall() => throw new ArgumentException($"The {nameof(iconBuffer.Length)} of the given {nameof(iconBuffer)} argument is to small for the given {nameof(iconWidth)} and {nameof(iconHeight)}", nameof(iconBuffer));
	}

	private static IntPtr ValidateInstantiation(IntPtr handle)
	{		
		if (handle is 0)
		{
			failCouldNotInstantiateNativeWindowObject();
		}

		return handle;

		[DoesNotReturn]
		static void failCouldNotInstantiateNativeWindowObject() => throw new NativeOperationException($"Could not instanciate a native {nameof(Window)} object");
	}

	private static IntPtr CreateWindow(string title, uint width, uint height)
	{
		Interlocked.Increment(ref mNativeBufferUsers);

		var titleBuffer = ValidateConvertTitle(title, out var lockTaken);

		try
		{
			return ValidateInstantiation(mfb_open(titleBuffer, width, height));
		}
		finally
		{
			ReleaseTitleBuffer(titleBuffer, lockTaken);
		}
	}

	private static IntPtr CreateWindow(string title, uint width, uint height, WindowFlags flags)
	{
		Interlocked.Increment(ref mNativeBufferUsers);

		var titleBuffer = ValidateConvertTitle(title, out var lockTaken);

		try
		{
			return ValidateInstantiation(mfb_open_ex(titleBuffer, width, height, flags));
		}
		finally
		{
			ReleaseTitleBuffer(titleBuffer, lockTaken);
		}
	}

	private static IntPtr CreateWindow(string title, uint width, uint height, in IconInfo smallIcon, in IconInfo bigIcon)
	{
		Interlocked.Increment(ref mNativeBufferUsers);

		var titleBuffer = ValidateConvertTitle(title, out var lockTaken);

		try
		{
			unsafe
			{
				// I know, I know...
				// If you look at how 'GetPinnableReference' is implemented (https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/ReadOnlySpan.cs,275),
				// you'll see that it doesn't do anything special (which is expected, when you think about how 'fixed' works).
				// So converting the buffer reference to a Span and then fixing it doesn't do much, but it feels much safer, doesn't it?
				// Also: fixing a null pointer is perfectly safe
				fixed (Argb* pinnedSmallIconBuffer = MemoryMarshal.CreateReadOnlySpan(in smallIcon.Buffer, unchecked((int)(smallIcon.Width * smallIcon.Height))),
					         pinnedBigIconBuffer   = MemoryMarshal.CreateReadOnlySpan(in bigIcon.Buffer, unchecked((int)(bigIcon.Width * bigIcon.Height))))
				{
					IconInfo smallIconCopy = new(pinnedSmallIconBuffer, smallIcon.Width, smallIcon.Height),
						     bigIconCopy   = new(pinnedBigIconBuffer, bigIcon.Width, bigIcon.Height);

#pragma warning disable CS8500 // Well, actually... IconInfo is perfectly blittable (and in that sense, as a struct it's unmanaged)
					return ValidateInstantiation(mfb_open_with_icons(titleBuffer, width, height, &smallIconCopy, &bigIconCopy));
#pragma warning restore CS8500
				}
			}
		}
		finally
		{
			ReleaseTitleBuffer(titleBuffer, lockTaken);
		}
	}

	private static IntPtr CreateWindow(string title, uint width, uint height, WindowFlags flags, in IconInfo smallIcon, in IconInfo bigIcon)
	{
		Interlocked.Increment(ref mNativeBufferUsers);

		var titleBuffer = ValidateConvertTitle(title, out var lockTaken);

		try
		{
			unsafe
			{
				// I know, I know...
				// If you look at how 'GetPinnableReference' is implemented (https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/ReadOnlySpan.cs,275),
				// you'll see that it doesn't do anything special (which is expected, when you think about how 'fixed' works).
				// So converting the buffer reference to a Span and then fixing it doesn't do much, but it feels much safer, doesn't it?
				// Also: fixing a null pointer is perfectly safe
				fixed (Argb* pinnedSmallIconBuffer = MemoryMarshal.CreateReadOnlySpan(in smallIcon.Buffer, unchecked((int)(smallIcon.Width * smallIcon.Height))),
					         pinnedBigIconBuffer   = MemoryMarshal.CreateReadOnlySpan(in bigIcon.Buffer, unchecked((int)(bigIcon.Width * bigIcon.Height))))
				{
					IconInfo smallIconCopy = new(pinnedSmallIconBuffer, smallIcon.Width, smallIcon.Height),
						     bigIconCopy   = new(pinnedBigIconBuffer, bigIcon.Width, bigIcon.Height);

#pragma warning disable CS8500 // Well, actually... IconInfo is perfectly blittable (and in that sense, as a struct it's unmanaged)
					return ValidateInstantiation(mfb_open_ex_with_icons(titleBuffer, width, height, flags, &smallIconCopy, &bigIconCopy));
#pragma warning restore CS8500
				}
			}
		}
		finally
		{
			ReleaseTitleBuffer(titleBuffer, lockTaken);
		}
	}

	#endregion

	#region Private implementation

	private WindowLifetimeState mLifetimeState = default;
	private GCHandle mSelfHandle;

	private ref Data AsData
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
#pragma warning disable CS8500
				return ref *(Data*)(void*)Handle;
#pragma warning restore CS8500
			}
		}
	}

	private interface IPrivateConstructorDispatcher;

#pragma warning disable IDE0060
	private Window(IntPtr handle, IPrivateConstructorDispatcher? privateConstructorDispatcher = default) : this(handle)
#pragma warning restore IDE0060
	{	
		LifetimeState = WindowLifetimeState.Initializing;

		mSelfHandle = GCHandle.Alloc(this, GCHandleType.Weak);

		mfb_set_user_data(handle, GCHandle.ToIntPtr(mSelfHandle));

		unsafe
		{
			mfb_set_active_callback(handle, &ActiveCallback);
			mfb_set_resize_callback(handle, &ResizeCallback);
			mfb_set_close_callback(handle, &CloseCallback);
			mfb_set_keyboard_callback(handle, &KeyboardCallback);
			mfb_set_char_input_callback(handle, &CharInputCallback);
			mfb_set_mouse_button_callback(handle, &MouseButtonCallback);
			mfb_set_mouse_move_callback(handle, &MouseMoveCallback);
			mfb_set_mouse_scroll_callback(handle, &MouseScrollCallback);
		}

		LifetimeState = WindowLifetimeState.Ready;
	}

	#endregion

	#region Public API and implementation

	#region Constructors and Disposing

	/// <inheritdoc/>
	/// <remarks>
	/// The native object pointed to by <paramref name="handle"/> is assumed to be <see cref="Window"/> object.
	/// Do not use this constructor, if the new <see cref="Window"/> instance should be handled by the managed side
	/// (e.g. <see cref="LifetimeState">LifetimeState</see> wouldn't get updated appropriately),
	/// instead call <see cref="Window(string, uint, uint)"/> or <see cref="Window(string, uint, uint, WindowFlags)"/> as base constructors
	/// </remarks>
	protected Window(IntPtr handle) : base(handle) { }

	/// <summary>
	/// Creates a new <see cref="Window"/> with a <paramref name="title"/>, a <paramref name="width"/>, and a <paramref name="height"/>
	/// </summary>
	/// <param name="title">The title of the <see cref="Window"/></param>
	/// <param name="width">The initial width of the <see cref="Window"/></param>
	/// <param name="height">The initial height of the <see cref="Window"/></param>
	/// <exception cref="ArgumentNullException"><paramref name="title"/> is <see langword="null"/></exception>
	/// <exception cref="NativeOperationException">A new <see cref="Window"/> could not be instantiated</exception>
	public Window(string title, uint width, uint height) : this(
		CreateWindow(title, width, height),
		privateConstructorDispatcher: default
	)
	{ }

	/// <summary>
	/// Creates a new <see cref="Window"/> with a <paramref name="title"/>, a <paramref name="width"/>, a <paramref name="height"/>,
	/// and certain <paramref name="flags"/> to control its behavior
	/// </summary>
	/// <param name="title">The title of the <see cref="Window"/></param>
	/// <param name="width">The initial width of the <see cref="Window"/></param>
	/// <param name="height">The initial height of the <see cref="Window"/></param>
	/// <param name="flags">A set of <see cref="WindowFlags">flags</see> that control the behavior of the <see cref="Window"/></param>
	/// <exception cref="ArgumentNullException"><paramref name="title"/> is <see langword="null"/></exception>
	/// <exception cref="NativeOperationException">A new <see cref="Window"/> could not be instantiated</exception>
	public Window(string title, uint width, uint height, WindowFlags flags) : this(
		CreateWindow(title, width, height, flags),
		privateConstructorDispatcher: default
	)
	{ }

	/// <summary>
	/// Creates a new <see cref="Window"/> with a <paramref name="title"/>, a <paramref name="width"/>, a <paramref name="height"/>,
	/// and a small and big icon defined by the <paramref name="smallIcon"/> and <paramref name="bigIcon"/> parameters
	/// </summary>
	/// <param name="title">The title of the <see cref="Window"/></param>
	/// <param name="width">The initial width of the <see cref="Window"/></param>
	/// <param name="height">The initial height of the <see cref="Window"/></param>
	/// <param name="smallIcon">
	/// The small icon that should be used by the <see cref="Window"/>,
	/// or <c><see langword="default"/></c> if you don't want to set a custom small icon for the <see cref="Window"/>
	/// </param>
	/// <param name="bigIcon">
	/// The big icon that should be used by the <see cref="Window"/>,
	/// or <c><see langword="default"/></c> if you don't want to set a custom big icon for the <see cref="Window"/>
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="title"/> is <see langword="null"/></exception>
	/// <exception cref="NativeOperationException">A new <see cref="Window"/> could not be instantiated</exception>
	[SupportedOSPlatform("windows")]
	public Window(string title, uint width, uint height, in IconInfo smallIcon = default, in IconInfo bigIcon = default) : this(
		CreateWindow(title, width, height, in smallIcon, in bigIcon),
		privateConstructorDispatcher: default
	)
	{ }

	/// <summary>
	/// Creates a new <see cref="Window"/> with a <paramref name="title"/>, a <paramref name="width"/>, a <paramref name="height"/>,
	/// certain <paramref name="flags"/> to control its behavior, and a small and big icon defined by the <paramref name="smallIcon"/>
	/// and <paramref name="bigIcon"/> parameters
	/// </summary>
	/// <param name="title">The title of the <see cref="Window"/></param>
	/// <param name="width">The initial width of the <see cref="Window"/></param>
	/// <param name="height">The initial height of the <see cref="Window"/></param>
	/// <param name="flags">A set of <see cref="WindowFlags">flags</see> that control the behavior of the <see cref="Window"/></param>
	/// <param name="smallIcon">
	/// The small icon that should be used by the <see cref="Window"/>,
	/// or <c><see langword="default"/></c> if you don't want to set a custom small icon for the <see cref="Window"/>
	/// </param>
	/// <param name="bigIcon">
	/// The big icon that should be used by the <see cref="Window"/>,
	/// or <c><see langword="default"/></c> if you don't want to set a custom big icon for the <see cref="Window"/>
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="title"/> is <see langword="null"/></exception>
	/// <exception cref="NativeOperationException">A new <see cref="Window"/> could not be instantiated</exception>
	[SupportedOSPlatform("windows")]
	public Window(string title, uint width, uint height, WindowFlags flags, in IconInfo smallIcon = default, in IconInfo bigIcon = default) : this(
		CreateWindow(title, width, height, flags, in smallIcon, in bigIcon),
		privateConstructorDispatcher: default
	)
	{ }

	/// <inheritdoc/>
	protected override void Dispose(bool disposing)
	{
		var previousLifetimeState = LifetimeState;
		LifetimeState = WindowLifetimeState.Disposing;

		Close();

		if (previousLifetimeState is not WindowLifetimeState.Undefined)
		{
			unsafe
			{
				mfb_set_active_callback(Handle, null);
				mfb_set_resize_callback(Handle, null);
				mfb_set_close_callback(Handle, null);
				mfb_set_keyboard_callback(Handle, null);
				mfb_set_char_input_callback(Handle, null);
				mfb_set_mouse_button_callback(Handle, null);
				mfb_set_mouse_move_callback(Handle, null);
				mfb_set_mouse_scroll_callback(Handle, null);
			}

			mfb_set_user_data(Handle, 0);

			if (mSelfHandle is { IsAllocated: true, Target: Window target } && ReferenceEquals(this, target))
			{
				mSelfHandle.Free();
			}

			UserData = null;

			if (Interlocked.Decrement(ref mNativeBufferUsers) is <= 0)
			{
				// We are in charge to release the native buffer.
				// To do so, we must synchronize using mNativeBufferLock and spin wait indefinitely. <-- This can't possibly lead to a deadlock, right?

				var lockTaken = false;
				try
				{
					mNativeBufferLock.Enter(ref lockTaken);

					// do we still need to release the native buffer?
					if (mNativeBufferUsers is <= 0)
					{
						unsafe
						{
							NativeMemory.Free(mNativeBufferPtr);

							mNativeBufferPtr = null;
							mNativeBufferSize = 0;
						}
					}
				}
				finally
				{
					if (lockTaken)
					{
						mNativeBufferLock.Exit();
					}
				}
			}
		}

		base.Dispose(disposing);

		LifetimeState = WindowLifetimeState.Disposed;
	}

	#endregion

	#region Properties

	/// <summary>Gets the current height of this <see cref="Window"/></summary>
	/// <value>The current height of this <see cref="Window"/></value>
	public uint Height => mfb_get_window_height(Handle);

	/// <summary>Gets a value indicating whether this <see cref="Window"/> is active</summary>
	/// <value>A value indicating whether this <see cref="Window"/> is active</value>
	public bool IsActive => mfb_is_window_active(Handle);

	/// <summary>Gets the sequential collection of the current <see cref="PressedState"/> of each <see cref="Key"/></summary>
	/// <value>The sequential collection of the current <see cref="PressedState"/> of each <see cref="Key"/></value>
	public DistinctReadOnlySpan<Key, PressedState> KeyBuffer
	{
		get
		{
			unsafe
			{
				return new(in Unsafe.AsRef<PressedState>(mfb_get_key_buffer(Handle)), KeyBufferSize);
			}
		}
	}

	/// <summary>Gets the <see cref="Window"/>'s current <see cref="WindowLifetimeState"/></summary>
	/// <value>The <see cref="Window"/>'s current <see cref="WindowLifetimeState"/></value>
	/// <remarks>A value of <see cref="WindowLifetimeState.Undefined">Undefined</see> indicates that the <see cref="Window"/> is not handled by the managed side</remarks>
	public WindowLifetimeState LifetimeState
	{
		get => mLifetimeState;
		set
		{
			if (mLifetimeState != value)
			{
				var previousLifetimeState = mLifetimeState;
				mLifetimeState = value;

				OnLifetimeStateChange(previousLifetimeState);
			}
		}
	}

	/// <summary></summary>
	/// <remarks>Dot not use. Use <see cref="MonitorScale">MonitorScale</see> instead.</remarks>
	[Obsolete($"Use '{nameof(MonitorScale)}' instead")]
	public (float dpiX, float dpiY) MonitorDpi
	{
		get
		{
			mfb_get_monitor_dpi(Handle, out var dpiX, out var dpiY);

			return (dpiX, dpiY);
		}
	}

	/// <summary>Gets the relative logical pixel scale in each dimension for this <see cref="Window"/></summary>
	/// <value>
	/// The relative logical pixel scale for this <see cref="Window"/>
	/// as an value tuple (with a component for the horizontal scale (scaleX) and a component for the vertical scale (scaleY))
	/// </value>
	public (float scaleX, float scaleY) MonitorScale
	{
		get
		{
			mfb_get_monitor_scale(Handle, out var scaleX, out var scaleY);

			return (scaleX, scaleY);
		}
	}

	/// <summary>Gets the sequential collection of the current <see cref="PressedState"/> of each <see cref="MouseButton"/></summary>
	/// <value>The sequential collection of the current <see cref="PressedState"/> of each <see cref="MouseButton"/></value>
	public DistinctReadOnlySpan<MouseButton, PressedState> MouseButtonBuffer
	{
		get
		{
			unsafe
			{
				return new(in Unsafe.AsRef<PressedState>(mfb_get_mouse_button_buffer(Handle)), MouseButtonBufferSize);
			}
		}
	}

	/// <summary>Gets the current horizontal mouse position relative to the origin of the <see cref="Window"/></summary>
	/// <value>The horizontal mouse position relative to the origin of the <see cref="Window"/></value>
	public int MousePositionX => mfb_get_mouse_x(Handle);

	/// <summary>Gets the current vertical mouse position relative to the origin of the <see cref="Window"/></summary>
	/// <value>The vertical mouse position relative to the origin of the <see cref="Window"/></value>
	public int MousePositionY => mfb_get_mouse_y(Handle);

	/// <summary>Gets the current change in horizontal mouse scrollwheel position</summary>
	/// <value>The current change in horizontal mouse scrollwheel position</value>
	/// <remarks>Obtaining this value resets it for this <see cref="Window"/> instance</remarks>
	public float MouseScrollDeltaX => mfb_get_mouse_scroll_x(Handle);

	/// <summary>Gets the current change in vertical mouse scrollwheel position</summary>
	/// <value>The current change in vertical mouse scrollwheel position</value>
	/// <remarks>Obtaining this value resets it for this <see cref="Window"/> instance</remarks>
	public float MouseScrollDeltaY => mfb_get_mouse_scroll_y(Handle);

	/// <summary>Gets or sets the current title of the <see cref="Window"/></summary>
	/// <value>The current title of the <see cref="Window"/></value>
	/// <remarks>
	/// Currently only supported on Windows and Linux platforms.
	/// Calling this property from a different platform might result in unexpected behavior.
	/// </remarks>
	/// <exception cref="InvalidOperationException">
	/// <list type="bullet">
	///		<item><description>
	///			an attempt to access the <see cref="Title">Title</see> property of a non-ready <see cref="Window"/>
	///			(its <see cref="LifetimeState">LifetimeState</see> is neither <c><see cref="WindowLifetimeState.Ready">Ready</see></c> nor <c><see cref="WindowLifetimeState.Undefined">Undefined</see></c>, or it's closed)
	///		</description></item>
	///		<item><description>something else went wrong</description></item>
	/// </list>
	/// </exception>
	[SupportedOSPlatform("windows")]
	[SupportedOSPlatform("linux")]
	public string Title
	{
		get
		{
			unsafe
			{
				if (LifetimeState is not (WindowLifetimeState.Ready or WindowLifetimeState.Undefined) || AsData.States.Close)
				{
					// still throwing, but that's better than throwing from an attempt to access non-owned memory
					failWindowNotReady();
				}

				var data = (titleBuffer: IntPtr.Zero, requestedSize: (nuint)0, lockTaken: false);

				var titleBuffer = (byte*)mfb_get_title(Handle, &getTitleBufferCallback, &data);

				if (titleBuffer is null)
				{
					ReleaseLockedNativeBuffer((byte*)data.titleBuffer, data.lockTaken);

					failCouldNotGetTitle();
				}

				titleBuffer[data.requestedSize] = 0;

				var result = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(titleBuffer));

				ReleaseLockedNativeBuffer((byte*)data.titleBuffer, data.lockTaken);

				return result;

				/* *** */

				[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
				static byte* getTitleBufferCallback(int* length, void* data)
				{
					var requestedSize = (nuint)(*length);
					var titleBuffer = AcquireLockedNativeBuffer(requestedSize, out var actualSize, out var lockTaken);

					*length = actualSize < int.MaxValue ? (int)actualSize : int.MaxValue;
					*((IntPtr titleBuffer, nuint requestedSize, bool lockTaken)*)data = ((IntPtr)titleBuffer, requestedSize, lockTaken);

					return titleBuffer;
				}

				[DoesNotReturn]
				static void failWindowNotReady()
					=> throw new InvalidOperationException($"Cannot get the title of a non-ready Window (its LifetimeState is neither \"{WindowLifetimeState.Ready}\" nor \"{WindowLifetimeState.Undefined}\", or it's closed).");

				[DoesNotReturn]
				static void failCouldNotGetTitle()
					=> throw new InvalidOperationException("Something went wrong while trying to get the Window's title.");
			}
		}
		set
		{
			if (LifetimeState is not (WindowLifetimeState.Ready or WindowLifetimeState.Undefined) || AsData.States.Close)
			{
				// still throwing, but that's better than throwing from an attempt to access non-owned memory
				failWindowNotReady();
			}

			mfb_set_title(
				Handle,
				(ValidateConvertTitle(value, out var lockTaken) is var titleNativeBuffer) switch { _ => titleNativeBuffer }
			);

			ReleaseTitleBuffer(titleNativeBuffer, lockTaken);

			[DoesNotReturn]
			static void failWindowNotReady()
				=> throw new InvalidOperationException($"Cannot set the title of a non-ready Window (its LifetimeState needs to be \"{WindowLifetimeState.Ready}\" or \"{WindowLifetimeState.Undefined}\", and it's not closed).");
		}
	}

	/// <summary>Gets or sets the additional user data associated with this <see cref="Window"/></summary>
	/// <value>The additional user data associated with this <see cref="Window"/></value>
	/// <remarks>
	/// This not the same user data obtained by a native call to '<c>mfb_get_user_data</c>', but merely a replacement for it,
	/// since the native user data is controlled by the managed <see cref="Window"/> instance
	/// </remarks>
	public object? UserData { get; set; } = null;

	/// <summary>Gets the current width of this <see cref="Window"/></summary>
	/// <value>The current width of this <see cref="Window"/></value>
	public uint Width => mfb_get_window_width(Handle);

	#endregion

	#region Events

	/// <summary>An event that occurs when the <see cref="Window"/> changes its <see cref="IsActive"/> state</summary>
	public event WindowActiveEventHandler? Active;

	/// <summary>An event that occurs when there is a character input event associated with the <see cref="Window"/></summary>
	public event WindowCharacterInputEventHandler? CharacterInput;

	/// <summary>An event that occurs when the <see cref="Window"/> is being closed</summary>
	/// <remarks>
	/// You can return a <see cref="bool">boolean</see> value from the <see cref="WindowClosingEventHandler">event handler</see>
	/// indicating whether the <see cref="Window"/> should actually get closed
	/// </remarks>
	public event WindowClosingEventHandler? Closing;

	/// <summary>An event that occurs when there is a <see cref="Key">keyboard</see> input event associated with the <see cref="Window"/></summary>
	public event WindowKeyChangeEventHandler? KeyChange;

	/// <summary>An event that occurs when the <see cref="Window"/>'s <see cref="LifetimeState">LifetimeState</see> changes</summary>
	public event WindowLifetimeStateChangeEventHandler? LifetimeStateChange;

	/// <summary>An event that occurs when there is a <see cref="MouseButton">mouse</see> input event associated with the <see cref="Window"/></summary>
	public event WindowMouseButtonChangeEventHandler? MouseButtonChange;

	/// <summary>An event that occurs when there is a mouse movement event associated with the <see cref="Window"/></summary>
	public event WindowMouseMoveEventHandler? MouseMove;

	/// <summary>An event that occurs when there is mouse scrollwheel movement associated with the <see cref="Window"/></summary>
	public event WindowMouseScrollEventHandler? MouseScroll;

	/// <summary>An event that occurs when the <see cref="Window"/> changes its size</summary>
	public event WindowResizeEventHandler? Resize;

	#endregion

	#region Methods

	/// <summary>Closes the <see cref="Window"/></summary>
	/// <remarks>
	/// The next call to any of the <see cref="Window"/>'s 'Update*' methods (or to <see cref="WaitForSync">WaitForSync</see>)
	/// will return <see cref="UpdateState.Exit"/> (or <see langword="false"/> respectively), and you should
	/// <see cref="NativeObject.Dispose()">dispose</see> it afterwards
	/// </remarks>
	public void Close() => mfb_close(Handle);

	/// <summary>Raises the <see cref="Active"/> event</summary>
	/// <param name="isActive">A value indicating the <see cref="Window"/>'s <see cref="IsActive">IsActive</see> state</param>
	protected virtual void OnActive(bool isActive) => Active?.Invoke(this, isActive);

	/// <summary>Raises the <see cref="CharacterInput"/> event</summary>
	/// <param name="keyCode">The key code of the character input associated the event</param>
	protected virtual void OnCharacterInput(uint keyCode) => CharacterInput?.Invoke(this, keyCode);

	/// <summary>Raises the <see cref="Closing"/> event</summary>
	/// <returns>An value indicating whether the <see cref="Window"/> should be closed or not</returns>
	/// <remarks>The return value of this method defaults to <see langword="true"/> if there are not handlers attached to the <see cref="Closing">Closing</see> event</remarks>
	protected virtual bool OnClosing() => Closing?.Invoke(this) ?? true;

	/// <summary>Raises the <see cref="KeyChange"/> event</summary>
	/// <param name="key">The <see cref="Key"/> associated with the event</param>
	/// <param name="modifier">A set of <see cref="KeyModifier"/>s associated with the event</param>
	/// <param name="state">A value indicating the <see cref="PressedState"/> of the <paramref name="key"/></param>
	protected virtual void OnKeyChange(Key key, KeyModifier modifier, PressedState state) => KeyChange?.Invoke(this, key, modifier, state);

	/// <summary>Raises the <see cref="LifetimeStateChange"/> event</summary>
	/// <param name="previousLifetimeState">The <see cref="Window"/>'s <see cref="WindowLifetimeState"/> before the change to its current <see cref="LifetimeState">LifetimeState</see></param>
	protected virtual void OnLifetimeStateChange(WindowLifetimeState previousLifetimeState) => LifetimeStateChange?.Invoke(this, previousLifetimeState);

	/// <summary>Raises the <see cref="MouseButtonChange"/> event</summary>
	/// <param name="button">The <see cref="MouseButton"/> associated with the event</param>
	/// <param name="modifier">A set of <see cref="KeyModifier"/>s associated with the event</param>
	/// <param name="state">A value indicating the <see cref="PressedState"/> of the <paramref name="button"/></param>
	protected virtual void OnMouseButtonChange(MouseButton button, KeyModifier modifier, PressedState state) => MouseButtonChange?.Invoke(this, button, modifier, state);

	/// <summary>Raises the <see cref="MouseMove"/> event</summary>
	/// <param name="x">The horizontal mouse position relative to the origin of the <see cref="Window"/></param>
	/// <param name="y">The vertical mouse position relative to the origin of the <see cref="Window"/></param>
	protected virtual void OnMouseMove(int x, int y) => MouseMove?.Invoke(this, x, y);

	/// <summary>Raises the <see cref="MouseScroll"/> event</summary>
	/// <param name="modifier">A set of <see cref="KeyModifier"/>s associated with the event</param>
	/// <param name="deltaX">The change in horizontal mouse scrollwheel position</param>
	/// <param name="deltaY">The change in vertical mouse scrollwheel position</param>
	protected virtual void OnMouseScroll(KeyModifier modifier, float deltaX, float deltaY) => MouseScroll?.Invoke(this, modifier, deltaX, deltaY);

	/// <summary>Raises the <see cref="Resize"/> event</summary>
	/// <param name="width">The <see cref="Window"/>'s width</param>
	/// <param name="height">The <see cref="Window"/>'s height</param>
	protected virtual void OnResize(int width, int height) => Resize?.Invoke(this, width, height);

	/// <summary>
	/// Tries to set a viewport with an offset of <paramref name="offsetX"/>
	/// and <paramref name="offsetY"/>, and a <paramref name="width"/> and an <paramref name="height"/>
	/// for the <see cref="Window"/>
	/// </summary>
	/// <param name="offsetX">An horizontal offset for the viewport</param>
	/// <param name="offsetY">A vertical offset for the viewport</param>
	/// <param name="width">The viewport's width</param>
	/// <param name="height">The viewport's height</param>
	/// <returns><see langword="true"/> if the the viewport was successfully set; otherwise, <see langword="false"/></returns>
	/// <remarks>
	/// <list type="bullet">
	/// <item><description><c><paramref name="offsetX"/> + <paramref name="width"/></c> must be less or equal to the <see cref="Window"/>'s <see cref="Width">current width</see></description></item>
	/// <item><description><c><paramref name="offsetY"/> + <paramref name="height"/></c> must be less or equal to the <see cref="Window"/>'s <see cref="Height">current height</see></description></item>
	/// <item><description>else, this method returns <see langword="false"/></description></item>
	/// </list>
	/// </remarks>
	public bool TrySetViewport(uint offsetX, uint offsetY, uint width, uint height) => mfb_set_viewport(Handle, offsetX, offsetY, width, height);

	/// <summary>
	/// Tries to set a best fittig viewport with a width of <paramref name="oldWidth"/> and an height of <paramref name="oldHeight"/>
	/// for the <see cref="Window"/>
	/// </summary>
	/// <param name="oldWidth">The viewport's width</param>
	/// <param name="oldHeight">The viewport's height</param>
	/// <returns><see langword="true"/> if the the viewport was successfully set; otherwise, <see langword="false"/></returns>
	public bool TrySetViewportBestFit(uint oldWidth, uint oldHeight) => mfb_set_viewport_best_fit(Handle, oldWidth, oldHeight);

	/// <summary>Updates the <see cref="Window"/>'s frame buffer with the contents of the given <paramref name="pixelBuffer"/></summary>
	/// <param name="pixelBuffer">The sequential pixel buffer to which the contents of the <see cref="Window"/>'s frame buffer should get updated to</param>
	/// <returns>An <see cref="UpdateState"/> indicating the result of the update</returns>
	/// <remarks>
	/// The <paramref name="pixelBuffer"/>'s <see cref="ReadOnlySpan{T}.Length">length</see> must be at least
	/// <c>&lt;known frame buffer width&gt; * &lt;known frame buffer height&gt;</c> (those are the width and height values passed in during
	/// <see cref="Window(string, uint, uint)">construction</see>, or later changed by the last call to <see cref="Update(ReadOnlySpan{Argb}, uint, uint)">Update</see>).
	/// NOTE: The <paramref name="pixelBuffer"/> will get pinned during the duration of the update! (The <see cref="Window"/>'s <see cref="LifetimeState">LifetimeState</see>
	/// will be set to <c><see cref="WindowLifetimeState.UpdatingWithFixedBuffer">UpdatingWithFixedBuffer</see></c> while updating.)
	/// </remarks>
	public UpdateState Update(ReadOnlySpan<Argb> pixelBuffer)
	{
		if (Handle is 0)
		{
			return UpdateState.InvalidWindow;
		}

		if (pixelBuffer.Length < AsData.BufferWidth * AsData.BufferHeight)
		{
			return UpdateState.InvalidBuffer;
		}

		var previousLifetimeState = LifetimeState;
		LifetimeState = WindowLifetimeState.UpdatingWithFixedBuffer;

		unsafe
		{
			UpdateState result;
			fixed(Argb* pinnedPixelBuffer = pixelBuffer)
			{
				result = mfb_update(Handle, ref Unsafe.AsRef<Argb>(pinnedPixelBuffer));
			}

			if (previousLifetimeState is WindowLifetimeState.Undefined)
			{
				LifetimeState = WindowLifetimeState.Undefined;
			}
			else if (LifetimeState is not WindowLifetimeState.Closing)
			{
				LifetimeState = WindowLifetimeState.Ready;
			}

			return result;
		}
	}

	/// <summary>
	/// Updates the <see cref="Window"/>'s frame buffer with the contents of the given <paramref name="pixelBuffer"/> of size <paramref name="width"/> × <paramref name="height"/>.
	/// </summary>
	/// <param name="pixelBuffer">The sequential pixel buffer to which the contents of the <see cref="Window"/>'s frame buffer should get updated to</param>
	/// <param name="width">The <paramref name="pixelBuffer"/>'s horizontal size</param>
	/// <param name="height">The <paramref name="pixelBuffer"/>'s vertical size</param>
	/// <returns>An <see cref="UpdateState"/> indicating the result of the update</returns>
	/// <remarks>
	/// The <paramref name="pixelBuffer"/>'s <see cref="ReadOnlySpan{T}.Length">length</see> must be at least
	/// <c><paramref name="width"/> * <paramref name="height"/></c>.
	/// NOTE: The <paramref name="pixelBuffer"/> will get pinned during the duration of the update! (The <see cref="Window"/>'s <see cref="LifetimeState">LifetimeState</see>
	/// will be set to <c><see cref="WindowLifetimeState.UpdatingWithFixedBuffer">UpdatingWithFixedBuffer</see></c> while updating.)
	/// </remarks>
	public UpdateState Update(ReadOnlySpan<Argb> pixelBuffer, uint width, uint height)
	{
		if (pixelBuffer.Length < width * height)
		{
			return UpdateState.InvalidBuffer;
		}

		var previousLifetimeState = LifetimeState;
		LifetimeState = WindowLifetimeState.UpdatingWithFixedBuffer;

		unsafe
		{
			UpdateState result;
			fixed (Argb* pinnedPixelBuffer = pixelBuffer)
			{
				result = mfb_update_ex(Handle, ref Unsafe.AsRef<Argb>(pinnedPixelBuffer), width, height);
			}

			if (previousLifetimeState is WindowLifetimeState.Undefined)
			{
				LifetimeState = WindowLifetimeState.Undefined;
			}
			else if (LifetimeState is not WindowLifetimeState.Closing)
			{
				LifetimeState = WindowLifetimeState.Ready;
			}

			return result;
		}
	}

	/// <summary>Updates the <see cref="Window"/>'s frame buffer with the contents of the given <paramref name="pixelBuffer"/></summary>
	/// <param name="pixelBuffer">A pointer to a sequential <see cref="Argb"/> memory to which the contents of the <see cref="Window"/>'s frame buffer should get updated to</param>
	/// <returns>An <see cref="UpdateState"/> indicating the result of the update</returns>
	/// <remarks>
	/// The <paramref name="pixelBuffer"/> must point to an accessible, readable and non-moving <see cref="Argb"/> memory
	/// of at least <c>&lt;known frame buffer width&gt; * &lt;known frame buffer height&gt;</c> elements (those are the width and height values passed in during
	/// <see cref="Window(string, uint, uint)">construction</see>, or later changed by the last call to <see cref="Update(ReadOnlySpan{Argb}, uint, uint)">Update</see>).
	/// NOTE: No precending checks and operations are performed on the managed side before executing the update!
	/// Therefore <paramref name="pixelBuffer"/> will NOT get additionally <see langword="fixed"/> during the duration of the update!
	/// (The <see cref="Window"/>'s <see cref="LifetimeState">LifetimeState</see> will be set to <c><see cref="WindowLifetimeState.Updating">Updating</see></c> while updating.)
	/// </remarks>
	public unsafe UpdateState Update(Argb* pixelBuffer)
	{
		var previousLifetimeState = LifetimeState;
		LifetimeState = WindowLifetimeState.Updating;

		var result = mfb_update(Handle, ref Unsafe.AsRef<Argb>(pixelBuffer));

		if (previousLifetimeState is WindowLifetimeState.Undefined)
		{
			LifetimeState = WindowLifetimeState.Undefined;
		}
		else if (LifetimeState is not WindowLifetimeState.Closing)
		{
			LifetimeState = WindowLifetimeState.Ready;
		}

		return result;
	}

	/// <summary>Updates the <see cref="Window"/>'s frame buffer with the contents of the given <paramref name="pixelBuffer"/> of size <paramref name="width"/> × <paramref name="height"/></summary>.
	/// <param name="pixelBuffer">A pointer to a sequential <see cref="Argb"/> memory to which contents the <see cref="Window"/>'s frame buffer should get updated to</param>
	/// <param name="width">The <paramref name="pixelBuffer"/>'s horizontal size</param>
	/// <param name="height">The <paramref name="pixelBuffer"/>'s vertical size</param>
	/// <returns>An <see cref="UpdateState"/> indicating the result of the update</returns>
	/// <remarks>
	/// The <paramref name="pixelBuffer"/> must point to an accessible, readable and non-moving <see cref="Argb"/> memory
	/// of at least <c><paramref name="width"/> * <paramref name="height"/></c> elements.
	/// NOTE: No precending checks and operations are performed on the managed side before executing the update!
	/// Therefore <paramref name="pixelBuffer"/> will NOT get additionally <see langword="fixed"/> during the duration of the update!
	/// (The <see cref="Window"/>'s <see cref="LifetimeState">LifetimeState</see> will be set to <c><see cref="WindowLifetimeState.Updating">Updating</see></c> while updating.)
	/// </remarks>
	public unsafe UpdateState Update(Argb* pixelBuffer, uint width, uint height)
	{
		var previousLifetimeState = LifetimeState;
		LifetimeState = WindowLifetimeState.Updating;

		var result = mfb_update_ex(Handle, ref Unsafe.AsRef<Argb>(pixelBuffer), width, height);

		if (previousLifetimeState is WindowLifetimeState.Undefined)
		{
			LifetimeState = WindowLifetimeState.Undefined;
		}
		else if (LifetimeState is not WindowLifetimeState.Closing)
		{
			LifetimeState = WindowLifetimeState.Ready;
		}

		return result;
	}

	/// <summary>Updates and executes the <see cref="Window"/>'s pending events only without changes to its frame buffer</summary>
	/// <returns>An <see cref="UpdateState"/> indicating the result of the update</returns>
	public UpdateState UpdateEventsOnly()
	{
		var previousLifetimeState = LifetimeState;
		LifetimeState = WindowLifetimeState.Updating;

		var result = mfb_update_events(Handle);

		if (previousLifetimeState is WindowLifetimeState.Undefined)
		{
			LifetimeState = WindowLifetimeState.Undefined;
		}
		else if (LifetimeState is not WindowLifetimeState.Closing)
		{
			LifetimeState = WindowLifetimeState.Ready;
		}

		return result;
	}

	/// <summary>
	/// Waits for frame synchronization on the current <see cref="Window"/>
	/// and returns a value indicating whether this <see cref="Window"/> is still alive afterwards
	/// </summary>
	/// <returns><see langword="true"/> if the <see cref="Window"/> is still alive after the call; otherwise, <see langword="false"/></returns>
	/// <remarks>
	/// When a <see cref="Window"/> is already closed or invalid in some other way, or gets closed (dies) or becomes invalid during the wait call,
	/// this method returns <see langword="false"/>
	/// </remarks>
	public bool WaitForSync()
	{
		var previousLifetimeState = LifetimeState;
		LifetimeState = WindowLifetimeState.WaitForSync;

		var result = mfb_wait_sync(Handle);

		if (previousLifetimeState is WindowLifetimeState.Undefined)
		{
			LifetimeState = WindowLifetimeState.Undefined;
		}
		else if (LifetimeState is not WindowLifetimeState.Closing)
		{
			LifetimeState = WindowLifetimeState.Ready;
		}

		return result;
	}

	#endregion

	#endregion
}
