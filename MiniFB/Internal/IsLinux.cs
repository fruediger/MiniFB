using MiniFB.SourceGeneration;
using System;
using System.Runtime.CompilerServices;

namespace MiniFB.Internal;

internal sealed class IsLinux : INativeImportCondition
{
	private IsLinux() { }

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool Evaluate() => OperatingSystem.IsLinux();
}
