using MiniFB;
using System;

AppDomain.CurrentDomain.UnhandledException += static (_, e) => Console.Error.WriteLine(e.ExceptionObject?.ToString());

const int dimenLow = 512;
var bufferLow = new Argb[dimenLow * dimenLow];

const int dimenHigh = 2 * dimenLow;
var bufferHigh = new Argb[dimenHigh * dimenHigh];

static void prettySquare(Span<Argb> p, int dimen)
{
	p.Fill(new(127, 127, 127, 127));

	var oneHalfDimen = dimen / 2;
	var oneQuarterDimen = oneHalfDimen / 2;
	var threeQuarterDimen = oneHalfDimen + oneQuarterDimen;

	for (var x = oneQuarterDimen; x < threeQuarterDimen; x++)
	{
		for (var y = oneQuarterDimen; y < threeQuarterDimen; y++)
		{
			p[y * dimen + x] = (x & 1) switch
			{
				not 0 => new(0xff, 223, 0, (byte)((255 * (x - oneQuarterDimen)) / oneHalfDimen)),
				_ => new(0xff, 0, 0, 0)
			};
		}
	}
}

prettySquare(bufferLow, dimenLow);
prettySquare(bufferHigh, dimenHigh);

using var windowLow = new Window("LowRes", dimenLow, dimenLow);
using var windowHigh = new Window("HighRes", dimenHigh / 2, dimenHigh / 2);

for (; ; )
{
	if (windowLow) // is 'windowLow' truthy?
	{
		if (windowLow.Update(bufferLow, dimenLow, dimenLow) is not UpdateState.Ok)
		{
			windowLow.Dispose(); // make 'windowLow' falsy
								 // also, multiple disposes are allowed on MiniFB.NativeObject
		}
	}

	if (windowHigh) // is 'windowHigh' truthy?
	{
		if (windowHigh.Update(bufferHigh, dimenHigh, dimenHigh) is not UpdateState.Ok)
		{
			windowHigh.Dispose(); // make 'windowHigh' falsy
								  // also, multiple disposes are allowed on MiniFB.NativeObject
		}
	}

	if (windowHigh) // is 'windowHigh' still truthy?
	{
		if (!windowHigh.WaitForSync())
		{
			windowHigh.Dispose(); // make 'windowHigh' falsy
		}
	}
	else if (windowLow) // is 'windowLow' still truthy?
	{
		if (!windowLow.WaitForSync())
		{
			windowLow.Dispose(); // make 'windowLow' falsy
		}
	}
	else
	{
		break;
	}
}