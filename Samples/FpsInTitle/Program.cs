using MiniFB;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

AppDomain.CurrentDomain.UnhandledException += static (_, e) => Console.Error.WriteLine(e.ExceptionObject?.ToString());

const string titlePrefix = "FPS in Title ";
const int titleMarqueeRate = 30;

uint width = 800, height = 600, size = width * height;
uint noise, carry, seed = 0xbeef;
uint iteration = 0;

using var window = new Window(titlePrefix, width, height, WindowFlags.Resizable);

var buffer = new Argb[size];

void resize(Window window, int newWidth, int newHeight)
{
	width = (uint)newWidth;
	height = (uint)newHeight;

	// It's not safe to resize the buffer while it's pinned. That would be case if, for example, 'window.LifetimeState is WindowLifetimeState.UpdatingWithFixedBuffer'.
	// Instead we signalize the need to resize the buffer by setting 'size' and do the resize at the begin of the next frame.
	//
	// Array.Resize(ref buffer, newWidth * newHeight);

	size = width * height;
}

window.Resize += resize;

window.TrySetViewport(50, 50, width - 50 - 50, height - 50 - 50);
resize(window, (int)width - 100, (int)height - 100); // to resize buffer

using var timer = new Timer();
var avg = new BinomialMovingAverageFilter<double>(30);

do
{
	if (buffer.Length < size)
	{
		Array.Resize(ref buffer, (int)size);
	}

	for (var i = 0; i < size; ++i)
	{
		noise = seed;
		noise >>= 3;
		noise ^= seed;
		carry = noise & 1;
		noise >>= 1;
		seed >>= 1;
		seed |= carry << 30;
		noise &= 0xff;
		buffer[i] = new(0xff, (byte)noise, (byte)noise, (byte)noise);
	}

	if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
	{
		window.Title = $"{window.Title.AsSpan()[..titlePrefix.Length] switch
		{
		[var head, .. var tail] when (iteration = (iteration + 1) % titleMarqueeRate) is 0 => $"{tail}{head}",
			var title => title
		}} - {avg.Advance(1 / timer.Delta):0.00} frame/s";
	}
}
while (
	window.Update(buffer, width, height) is UpdateState.Ok
	&& window.WaitForSync()
);

/* *** */

file sealed class BinomialMovingAverageFilter<T>
	where T : struct, IFloatingPointIeee754<T>
{
	private readonly T[] mFactors;
	private readonly T[] mValues;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public BinomialMovingAverageFilter(int size)
	{
		if (size is < 2 or > 256)
		{
			throw new IndexOutOfRangeException(nameof(size));
		}

		mFactors = GC.AllocateUninitializedArray<T>(size);
		mValues = new T[size];

		var factorsSpan = mFactors.AsSpan();
		ref var factorRef = ref MemoryMarshal.GetReference(factorsSpan);
		ref var factorEndRef = ref Unsafe.Add(ref factorRef, factorsSpan.Length);

		factorRef = T.Exp2(T.CreateTruncating(-(--size)));

		var k = 1;
		for (ref var factorNextRef = ref Unsafe.Add(ref factorRef, 1); Unsafe.IsAddressLessThan(ref factorNextRef, ref factorEndRef); factorRef = ref factorNextRef, factorNextRef = ref Unsafe.Add(ref factorNextRef, 1))
		{
			factorNextRef = (T.CreateTruncating(size--) / T.CreateTruncating(k++)) * factorRef;
		}
	}

	public T Value { get; private set; }

	public T Advance(T value)
	{
		var factorsSpan = mFactors.AsSpan();
		var valuesSpan = mValues.AsSpan();

		ref var factorRef = ref MemoryMarshal.GetReference(factorsSpan);
		ref var valueRef = ref MemoryMarshal.GetReference(valuesSpan);
		ref var valueEndRef = ref Unsafe.Add(ref valueRef, valuesSpan.Length);

		var sum = T.Zero;

		for (ref var valueNextRef = ref Unsafe.Add(ref valueRef, 1); Unsafe.IsAddressLessThan(ref valueNextRef, ref valueEndRef); valueRef = ref valueNextRef, valueNextRef = ref Unsafe.Add(ref valueNextRef, 1), factorRef = ref Unsafe.Add(ref factorRef, 1))
		{
			sum += (valueRef = valueNextRef) * factorRef;
		}

		sum += (valueRef = value) * factorRef;

		return Value = sum;
	}
}
