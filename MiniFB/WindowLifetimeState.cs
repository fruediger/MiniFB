namespace MiniFB;

/// <summary>Represents a certain point (state) in the lifetime of a <see cref="Window"/> handled by the managed side</summary>
public enum WindowLifetimeState
{
	/// <summary>
	/// Indicates that a <see cref="Window"/> is not handled by the managed side.
	/// <see cref="Window"/>s handled by the managed always have a <see cref="Window.LifetimeState">LifetimeState</see>
	/// whose value is not equal to <see cref="Undefined">Undefined</see> after construction.
	/// </summary>
	Undefined = default,

	/// <summary>The <see cref="Window"/> instance is currently getting constructed</summary>
	Initializing,

	/// <summary>The <see cref="Window"/> is ready to be operated on</summary>
	/// <remarks>This is the value of <see cref="Window.LifetimeState"/> in between frame updates</remarks>
	Ready,

	/// <summary>The <see cref="Window"/> is currently updating and potentially updating its frame buffer</summary>
	/// <remarks>NOTE: No claims about the fixation of potentially involved input buffers can be made!</remarks>
	Updating,

	/// <summary>The <see cref="Window"/> is currently updating and its frame buffer is updated from a pinned/<see langword="fixed"/> input buffer</summary>
	UpdatingWithFixedBuffer,

	/// <summary>The <see cref="Window"/> is currently waiting for frame synchronization</summary>
	WaitForSync,

	/// <summary>
	/// The <see cref="Window"/> either waits for pending <see cref="Window.Closing">closing events</see> to be handled,
	/// or the <see cref="Window"/> is no longer alive due to a call to one of its 'Update*' methods or due to a call to <see cref="Window.WaitForSync">WaitForSync</see>
	/// </summary>
	Closing,

	/// <summary>The <see cref="Window"/> instance is currently getting disposed</summary>
	Disposing,

	/// <summary>The <see cref="Window"/> instance is alreay disposed and should not get used any longer</summary>
	Disposed
}
