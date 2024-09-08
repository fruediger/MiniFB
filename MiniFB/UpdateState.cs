namespace MiniFB;

/// <summary>A result state returned by calls to any '<c>*Update*</c>' methods of a <see cref="Window"/></summary>
/// <remarks>You should exit the main update loop for a <see cref="Window"/> whenever a update call returns a negative <see cref="UpdateState"/></remarks>
public enum UpdateState : int
{
	/// <summary>The update call was successfully</summary>
	Ok = 0,

	/// <summary>You should exit the main update loop as the result of a gracefully exit request</summary>
	/// <remarks>
	/// DO NOT use a <see cref="Window"/> after a call to one of its '<c>Update*</c>' methods returned <see cref="Exit">Exit</see>,
	/// as its unmanaged resources might be no longer valid.
	/// It should be safe to call <see cref="NativeObject.Dispose()">Dispose()</see> on it though.
	/// </remarks>
	Exit = -1,

	/// <summary>Either the handle to the <see cref="Window"/> or its current internal state are invalid</summary>
	InvalidWindow = -2,

	/// <summary>The given pixel buffer is invalid (mostly due its address in memory being <see langword="null"/>)</summary>
	InvalidBuffer = -3,

	/// <summary>An internal error happend</summary>
	InternalError = -4,
}
