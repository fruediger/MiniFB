using System;

namespace MiniFB;

/// <summary>A exception that is thrown when there was an error on the unmanaged callside of <see cref="N:MiniFB"/></summary>
/// <param name="message"><inheritdoc/></param>
public class NativeOperationException(string? message = default) : Exception(message);
