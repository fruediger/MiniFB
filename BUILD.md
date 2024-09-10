# Building

## Requirements

-	[git](https://git-scm.com/), if you want to clone the [official MiniFB repository from GitHub](TODO--fill-me-out)
-	[.NET8+-SDK](https://dotnet.microsoft.com/en-us/)
-	`dotnet` CLI tool (this should come with the .NET-SDK)
-	[Cake 4.0](https://cakebuild.net/)

	Make sure you have Cake installed as a `dotnet` tool. If not, have a look at the ["Setup" section in the offical Cake documentation](https://cakebuild.net/docs/running-builds/runners/dotnet-tool#setup).

## Building and packing the MiniFB library

### Cloning the offical repository

From a commandline, run:

```console
$ git clone git@github.com:fruediger/MiniFB.git
```
<small>*The HTTPS repository url is https://github.com/fruediger/MiniFB.git*</small>

<details>
<summary><em>(Optional)</em> Clone the official native libraries repository</summary>

If you want to bundle your own version of the native libraries, you might want to clone their offical repository as well, so you can make local changes:

```console
$ git clone git@github.com:fruediger/minifb-native.git
```
<small>*The HTTPS repository url is https://github.com/fruediger/minifb-native.git*</small>
</details>

After that change into the newly cloned directory; **all of the following steps are assumed to be executed from there**:

```console
$ cd MiniFB
```

### Install the native libraries

If you want to bundle the latest official release of the native libraries (which is **highly recommend**) you can simply run:

```console
$ ./update-native-libraries.sh
```
<small>*Adjust the style of invoking a script and it's file extension to your prefered shell and platform. There is a `.sh`-file for sh's, a `.ps1`-file for PowerShell and a `.cmd` for the Windows command interpreter.*</small>

If that doesn't work, you can still simply run the related target using Cake:

```console
$ dotnet cake --target=Update-Native-Libraries
```

> [!NOTE]
> **What is does**: Executing the command above downloads the latest archived release of the native libraries from their [official repository on GitHub](https://github.com/fruediger/minifb-native),
> extract it, and places it under [`MiniFB/runtimes`](MiniFB/runtimes). Previously existing [`MiniFB/runtimes`](MiniFB/runtimes) directories will get archived
> and backed up under [`MiniFB/.old`](MiniFB/.old).

> [!TIP]
> Run the command above on a regular basis, so you always bundle the latest release of the native libraries with your build of MiniFB.

<details>
<summary><em>(Alternatively)</em> Bundle your own version of the native libraries</summary>

Instead of using the official release of the native libraries, you can bundle your own build of them. To do so, simply place your shared libraries under [a `runtimes` directory inside `MiniFB`](MiniFB/runtimes).
For the correct file system layout, follow the structure described at ["Including native libraries in .NET packages"](https://learn.microsoft.com/en-us/nuget/create-packages/native-files-in-net-packages#example-1)
or have a look at how the [official release of the native libraries](https://github.com/fruediger/minifb-native/releases/latest) is structured.

> [!WARNING]
> You can't just use any arbitrary build of the native libraries. Especially an unmodified build of the original [minifb](https://github.com/emoon/minifb) wouldn't just work.
> MiniFB uses a custom fork of the original [minifb](https://github.com/emoon/minifb), which, for example, exports additional symbols required by MiniFB.
> If you want to build your own version of the native libraries, [their official repository](https://github.com/fruediger/minifb-native) is a good starting point.

</details>

### Build the MiniFB library and packing it

Once you've installed the native libraries in their correct location, you can build the managed library by simply running:

```console
$ ./build.sh
```
<small>*Adjust the style of invoking a script and it's file extension to your prefered shell and platform. There is a `.sh`-file for sh's, a `.ps1`-file for PowerShell and a `.cmd` for the Windows command interpreter.*</small>

If that doesn't work, you can still build it using Cake:

```console
$ dotnet cake
```
<small>*This is equivalent to specifying `--target=Build`, as `Build` is the default target.*</small>

There are some options, you can specify for the command above:

- `--configuration=<Configuration>` where `<Configuration>` is either `Debug` or `Release`:

	Determines the build configuration and produces either a debug or a release build.

- `--target=<Target>`:
  - when `<Target>` is `Build`: build the MiniFB library
  - when `<Target>` is `Publish`: publishes the MiniFB library
  - when `<Target>` is `Pack`: packs the MiniFB library into a NuGet package

## Building and running the sample applications

The MiniFB repository comes with some sample application, which are reimaginations of their analog from the original [minifb](https://github.com/emoon/minifb).

To build a sample application by simply running:

```console
$ ./build.sh --target=Build-Sample-<Sample>
```
<small>*Adjust the style of invoking a script and it's file extension to your prefered shell and platform. There is a `.sh`-file for sh's, a `.ps1`-file for PowerShell and a `.cmd` for the Windows command interpreter.*</small>

Where you have to substitute `<Sample>` with one of the provided sample application names: `Noise`, `Timer`, `InputEvents`, `Fullscreen`, `MultipleWindows`, or `HiDpi`.

If that doesn't work, you can still build it using Cake:

```console
$ dotnet cake --target=Build-Sample-<Sample>
```

For example, if you want to build the `Timer` sample application, you would run:

```console
$ ./build.sh --target=Build-Sample-Timer
```

If you want to build all of the sample applications at once, you can specify `--target=Build-Samples` instead.

You can also run a sample application directly (even without explicitly building it first), by specifying `--target=Run-Sample-<Sample>`.
