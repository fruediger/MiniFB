# MiniFB

MiniFB is a library for visualizing self-drawn frame buffers for .Net applications.

MiniFB is available on [![NuGet](https://img.shields.io/nuget/vpre/MiniFB?logo=nuget&label=NuGet)](https://www.nuget.org/packages/MiniFB/)
 and [![GitHub Release](https://img.shields.io/github/v/release/fruediger/MiniFB?include_prereleases&display_name=tag&logo=github&label=GitHub)](https://github.com/fruediger/MiniFB)!

The library itself provides managed wrappings around a [custom version](https://github.com/fruediger/minifb-native) of the [native original minifb library](https://github.com/emoon/minifb).
___

You can draw the contents of your frame buffer yourself - with every method C# (or other .Net languages) and the .Net ecosystem offer you.
Then you can display your frame buffer in a window - in an easy and platform-independent way.

MiniFB also offers you a platform-independent way for most common scenarios when using desktop windows - like, for example, handling window events.
___

## Requirements

-   .NET8 or above
-   currently Windows and Linux only, but you've got all relevant architectures there (`x64`, `x86`, `arm64`, `arm`)

    There's a plan to further expand the supported platforms in the future.

-   for additional usage dependencies per platform see [`USAGE.md`](https://github.com/fruediger/MiniFB/blob/main/USAGE.md)

-   for building requirements see [`BUILD.md`](https://github.com/fruediger/MiniFB/blob/main/BUILD.md)

## Usage

See the [quickstart guide](#quickstart).

Alternatively, you can see [`USAGE.md`](https://github.com/fruediger/MiniFB/blob/main/USAGE.md) for a more in depth look on how to use MiniFB, or have a look at the [sample applications](Samples).

## Building

See [`BUILD.md`](https://github.com/fruediger/MiniFB/blob/main/BUILD.md).

## Quickstart

### Add a reference to your .NET8+ C#-project

In your `.csproj` file, add reference to the MiniFB package from NuGet:

```xml
<ItemGroup>
    <PackageReference Include="MiniFB" Version="0.7.0">
</ItemGroup>
```

### Add a using directive to import from the `MiniFB` namespace

Add a using directive to the top a code file, where you want to reference parts of MiniFB:

```csharp
using MiniFB;
```

### Prepare your frame buffer

The frame buffer holds the data for each individual pixel for each frame:

```csharp
var buffer = new Argb[800 * 600];
```

### Create a window

You can create a new `Window` by simply calling it's constructor:

```csharp
using var window = new Window("Hello World!", 800, 600);
```

### Establish a main loop and redraw your frame buffer in each frame

In each frame, you'll most likely want to redraw the contents of your frame `buffer`:

```csharp
do
{
    // your frame redraw logic goes here
}
while (...);
```

### Update your window at the end of each frame

At the end of each frame, you want to update the window with the contents of your redrawn frame `buffer` and after that wait for frame synchronization.

Inside the tailing `while` statement from above, insert the following, so it looks like that:

```csharp
while (
    // update the window with the contents of the frame buffer ...
    window.Update(buffer) is UpdateState.Ok

    // ... and then wait for frame synchronization
    && window.WaitForSync();
);
```
