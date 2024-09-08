using MiniFB.SourceGeneration;
using System.Runtime.CompilerServices;

namespace MiniFB.Internal;

internal static class NativeImportConditionExpressions
{
	internal sealed class Not<T> : INativeImportCondition
		where T : notnull, INativeImportCondition
	{
		private Not() { }

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static bool Evaluate() => !INativeImportCondition.Evaluate<T>();
	}

	internal sealed class And<TLeft, TRight> : INativeImportCondition
		where TLeft : notnull, INativeImportCondition
		where TRight : notnull, INativeImportCondition
	{
		private And() { }

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static bool Evaluate() => INativeImportCondition.Evaluate<TLeft>() & INativeImportCondition.Evaluate<TRight>();
	}

	internal sealed class Or<TLeft, TRight> : INativeImportCondition
		where TLeft : notnull, INativeImportCondition
		where TRight : notnull, INativeImportCondition
	{
		private Or() { }

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static bool Evaluate() => INativeImportCondition.Evaluate<TLeft>() | INativeImportCondition.Evaluate<TRight>();
	}

	internal sealed class AndAlso<TLeft, TRight> : INativeImportCondition
		where TLeft : notnull, INativeImportCondition
		where TRight : notnull, INativeImportCondition
	{
		private AndAlso() { }

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static bool Evaluate() => INativeImportCondition.Evaluate<TLeft>() && INativeImportCondition.Evaluate<TRight>();
	}

	internal sealed class OrElse<TLeft, TRight> : INativeImportCondition
		where TLeft : notnull, INativeImportCondition
		where TRight : notnull, INativeImportCondition
	{
		private OrElse() { }

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static bool Evaluate() => INativeImportCondition.Evaluate<TLeft>() || INativeImportCondition.Evaluate<TRight>();
	}
}
