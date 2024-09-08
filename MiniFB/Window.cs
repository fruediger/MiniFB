﻿using MiniFB.Buffers;
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
	// these values were pulled from the MiniFB source
	// (https://github.com/fruediger/minifb-native/blob/master/src/WindowData.h#L42 and https://github.com/fruediger/minifb-native/blob/master/src/WindowData.h#L43)
	private const int MouseButtonBufferSize = 8, KeyBufferSize = 512;

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial IntPtr mfb_open(IntPtr title, uint width, uint height);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial IntPtr mfb_open_ex(IntPtr title, uint width, uint height, WindowFlags flags);

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

	private static IntPtr ValidateConvertAndPinTitle([NotNull] string title, out GCHandle pinnedTitleHandle)
	{
		if (title is null)
		{
			failTitleArgumentNull();
		}

		var pooledTitleBuffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(title) + 1);
		Array.Clear(pooledTitleBuffer);

		pinnedTitleHandle = GCHandle.Alloc(pooledTitleBuffer, GCHandleType.Pinned);

		Unsafe.Add(
			ref MemoryMarshal.GetArrayDataReference(pooledTitleBuffer),
			Math.Min(			
				Encoding.UTF8.GetBytes(title, pooledTitleBuffer),
				pooledTitleBuffer.Length - 1
			)
		) = 0;

		return pinnedTitleHandle.AddrOfPinnedObject();

		[DoesNotReturn]
		static void failTitleArgumentNull() => throw new ArgumentNullException(nameof(title));
	}

	private static void ReleasePinnedTitle(GCHandle pinnedTitleHandle)
	{
		if (pinnedTitleHandle.IsAllocated)
		{
			var pooledTitleBuffer = pinnedTitleHandle.Target as byte[];

			pinnedTitleHandle.Free();

			if (pooledTitleBuffer is not null)
			{
				ArrayPool<byte>.Shared.Return(pooledTitleBuffer);
			}
		}
	}

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

	/// <inheritdoc/>
	/// <remarks>
	/// The native object pointed to by <paramref name="handle"/> is assumed to be <see cref="Window"/> object.
	/// Do not use this constructor, if the new <see cref="Window"/> instance should be handled by the managed side
	/// (e.g. <see cref="LifetimeState">LifetimeState</see> wouldn't get updated appropriately),
	/// instead call <see cref="Window(string, uint, uint)"/> or <see cref="Window(string, uint, uint, WindowFlags)"/> as base constructors
	/// </remarks>
	protected Window(IntPtr handle) : base(handle) { }

	private Window(IntPtr handle, GCHandle pinnedTitleHandle) : this(handle)
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

		ReleasePinnedTitle(pinnedTitleHandle);

		LifetimeState = WindowLifetimeState.Ready;
	}

	/// <summary>
	/// Creates a new <see cref="Window"/> with a <paramref name="title"/>, a <paramref name="width"/>, and a <paramref name="height"/>
	/// </summary>
	/// <param name="title">The title of the <see cref="Window"/></param>
	/// <param name="width">
	/// The initial width of the <see cref="Window"/>.
	/// </param>
	/// <param name="height">
	/// The initial height of the <see cref="Window"/>.
	/// </param>
	/// <exception cref="ArgumentNullException"><paramref name="title"/> is <see langword="null"/></exception>
	/// <exception cref="NativeOperationException">A new <see cref="Window"/> could not be instantiated</exception>
	public Window(string title, uint width, uint height) : this(
		ValidateInstantiation(mfb_open(ValidateConvertAndPinTitle(title, out var pinnedTitleHandle), width, height)),
		pinnedTitleHandle
	)
	{ }

	/// <summary>
	/// Creates a new <see cref="Window"/> with a <paramref name="title"/>, a <paramref name="width"/>, a <paramref name="height"/>,
	/// and certain <paramref name="flags"/> to control its behavior
	/// </summary>
	/// <param name="title">The title of the <see cref="Window"/></param>
	/// <param name="width">
	/// The initial width of the <see cref="Window"/>.
	/// </param>
	/// <param name="height">
	/// The initial height of the <see cref="Window"/>.
	/// </param>
	/// <param name="flags">A set of <see cref="WindowFlags">flags</see> that control the behavior of the <see cref="Window"/></param>
	/// <exception cref="ArgumentNullException"><paramref name="title"/> is <see langword="null"/></exception>
	/// <exception cref="NativeOperationException">A new <see cref="Window"/> could not be instantiated</exception>
	public Window(string title, uint width, uint height, WindowFlags flags) : this(
		ValidateInstantiation(mfb_open_ex(ValidateConvertAndPinTitle(title, out var pinnedTitleHandle), width, height, flags)),
		pinnedTitleHandle
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
		}

		base.Dispose(disposing);

		LifetimeState = WindowLifetimeState.Disposed;
	}

	/// <summary>Gets the current height of this <see cref="Window"/></summary>
	/// <value>The current height of this <see cref="Window"/></value>
	public uint Height => mfb_get_window_height(Handle);

	/// <summary>Gets a value indicating whether this <see cref="Window"/> is active</summary>
	/// <value>A value indicating whether this <see cref="Window"/> is active</value>
	public bool IsActive => mfb_is_window_active(Handle);

	/// <summary>Gets the sequential collection of the current <see cref="PressedState"/> of each <see cref="Key"/></summary>
	/// <value>The sequential collection of the current <see cref="PressedState"/> of each <see cref="Key"/></value>
	public DistincReadOnlySpan<Key, PressedState> KeyBuffer
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
	public DistincReadOnlySpan<MouseButton, PressedState> MouseButtonBuffer
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
	[SupportedOSPlatform("windows")]
	[SupportedOSPlatform("linux")]
	public string Title
	{		
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (IntPtr pinnedTitlePtr, int length) data);

				var titleBuffer = (byte*)mfb_get_title(Handle, &getTitleBufferCallback, &data);

				if (titleBuffer is null)
				{
					ReleasePinnedTitle(GCHandle.FromIntPtr(data.pinnedTitlePtr));

					failCouldNotGetTitle();
				}
				
				titleBuffer[data.length - 1] = 0;

				var result = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(titleBuffer));

				ReleasePinnedTitle(GCHandle.FromIntPtr(data.pinnedTitlePtr));

				return result;

				/* *** */

				[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
				static nint getTitleBufferCallback(int* length, void* data)
				{
					var pooledTitleBuffer = ArrayPool<byte>.Shared.Rent(*length);
					Array.Clear(pooledTitleBuffer);

					var pinnedTitleHandle = GCHandle.Alloc(pooledTitleBuffer, GCHandleType.Pinned);

					*((IntPtr pinnedTitlePtr, int length)*)data = (GCHandle.ToIntPtr(pinnedTitleHandle), *length = pooledTitleBuffer.Length);

					return pinnedTitleHandle.AddrOfPinnedObject();
				}

				[DoesNotReturn]
				static void failCouldNotGetTitle() => throw new InvalidOperationException("Something went wrong while trying to get the Window's title.");
			}
		}
		set
		{
			mfb_set_title(
				Handle,
				ValidateConvertAndPinTitle(value, out var pinnedTitleHandle)
			);

			ReleasePinnedTitle(pinnedTitleHandle);
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
	/// The <paramref name="pixelBuffer"/>'s <see cref="ReadOnlyMemory{T}.Length">length</see> must be at least
	/// <c>&lt;known frame buffer width&gt; * &lt;known frame buffer height&gt;</c> (those are the width and height values passed in during
	/// <see cref="Window(string, uint, uint)">construction</see>, or later changed by the last call to <see cref="Update(ReadOnlyMemory{Argb}, uint, uint)">Update</see>).
	/// NOTE: The <paramref name="pixelBuffer"/> will get pinned during the duration of the update! (The <see cref="Window"/>'s <see cref="LifetimeState">LifetimeState</see>
	/// will be set to <c><see cref="WindowLifetimeState.UpdatingWithFixedBuffer">UpdatingWithFixedBuffer</see></c> while updating.)
	/// </remarks>
	public UpdateState Update(ReadOnlyMemory<Argb> pixelBuffer)
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
			using (var pinnedBuffer = pixelBuffer.Pin())
			{
				result = mfb_update(Handle, ref Unsafe.AsRef<Argb>(pinnedBuffer.Pointer));
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
	/// <param name="width">
	/// The <paramref name="pixelBuffer"/>'s horizontal size.
	/// </param>
	/// <param name="height">
	/// The <paramref name="pixelBuffer"/>'s vertical size.
	/// </param>
	/// <returns>An <see cref="UpdateState"/> indicating the result of the update</returns>
	/// <remarks>
	/// The <paramref name="pixelBuffer"/>'s <see cref="ReadOnlyMemory{T}.Length">length</see> must be at least
	/// <c><paramref name="width"/> * <paramref name="height"/></c>.
	/// NOTE: The <paramref name="pixelBuffer"/> will get pinned during the duration of the update! (The <see cref="Window"/>'s <see cref="LifetimeState">LifetimeState</see>
	/// will be set to <c><see cref="WindowLifetimeState.UpdatingWithFixedBuffer">UpdatingWithFixedBuffer</see></c> while updating.)
	/// </remarks>
	public UpdateState Update(ReadOnlyMemory<Argb> pixelBuffer, uint width, uint height)
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
			using (var pinnedBuffer = pixelBuffer.Pin())
			{
				result = mfb_update_ex(Handle, ref Unsafe.AsRef<Argb>(pinnedBuffer.Pointer), width, height);
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
	/// <see cref="Window(string, uint, uint)">construction</see>, or later changed by the last call to <see cref="Update(ReadOnlyMemory{Argb}, uint, uint)">Update</see>).
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
	/// <param name="width">
	/// The <paramref name="pixelBuffer"/>'s horizontal size. 
	/// </param>
	/// <param name="height">
	/// The <paramref name="pixelBuffer"/>'s vertical size. 
	/// </param>
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
}
