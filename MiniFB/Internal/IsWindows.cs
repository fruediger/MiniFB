using MiniFB.SourceGeneration;
using System;
using System.Runtime.CompilerServices;

namespace MiniFB.Internal;

internal sealed class IsWindows : INativeImportCondition
{
	private IsWindows() { }

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool Evaluate() => OperatingSystem.IsWindows();
}
