using MiniFB;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static MiniFB.MiniFB;

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

window.TrySetViewport(50, 50, width - 50 - 50, height - 50 -50);
resize(window, (int)width - 100, (int)height - 100); // to resize buffer

TargetFPS = 10;

using var timer = new Timer();
var avg = new BinomialMovingAverageFilter(30);

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
            [var head, ..var tail] when (iteration = (iteration + 1) % titleMarqueeRate) is 0 => $"{tail}{head}",
            var title => title
        }} - {avg.Advance(1 / timer.Delta):0.00} frame/s";
    }
}
while (
    window.Update(buffer, width, height) is UpdateState.Ok
    && window.WaitForSync()
);

/* *** */

file sealed class BinomialMovingAverageFilter
{
    private readonly double[] mBuffer;

    public BinomialMovingAverageFilter(int size)
    {
        if (size is < 2 or > 64)
        {
            throw new IndexOutOfRangeException(nameof(size));
        }

        mBuffer = new double[2 * size];

        size--;

        var bufferSpan = mBuffer.AsSpan();

        ref var factor = ref MemoryMarshal.GetReference(bufferSpan);
        ref var factorsEnd = ref Unsafe.Add(ref factor, size);

        var k = 1;
        var l = size;

        for (; Unsafe.IsAddressLessThan(ref factor, ref factorsEnd); factor = ref Unsafe.Add(ref factor, 1))
        {
            factor = (double)k++ / (double)l--;
        }

        factor = 1d / (1L << size);
    }

    public double Value { get; private set; }

    public double Advance(double value)
    {
        var bufferSpan = mBuffer.AsSpan();

        ref var factor = ref MemoryMarshal.GetReference(bufferSpan);
        ref var factorsEnd = ref Unsafe.Add(ref factor, bufferSpan.Length / 2 - 1);
        ref var item = ref Unsafe.Add(ref factorsEnd, 1);
        ref var itemsEnd = ref Unsafe.Add(ref factor, bufferSpan.Length);

        var sum = 0d;

        for (ref var nextItem = ref Unsafe.Add(ref item, 1); Unsafe.IsAddressLessThan(ref nextItem, ref itemsEnd); factor = ref Unsafe.Add(ref factor, 1), item = ref nextItem, nextItem = ref Unsafe.Add(ref nextItem, 1))
        {
            sum += item = factor * nextItem;
        }

        sum += item = factor * value;

        return Value = sum;
    }
}
