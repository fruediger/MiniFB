using MiniFB.SourceGeneration;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MiniFB;

/// <summary>Provides a set of <see langword="static"/> members and extension methods in regard to using <see cref="N:MiniFB"/></summary>
public static partial class MiniFB
{
	internal sealed class Library : INativeImportLibrary
	{
		private const string libminifb = nameof(libminifb);

		private const DllImportSearchPath DefaultSearchPath
			= DllImportSearchPath.AssemblyDirectory
			| DllImportSearchPath.UseDllDirectoryForDependencies
			| DllImportSearchPath.ApplicationDirectory
			| DllImportSearchPath.UserDirectories;

		private static string? mLibraryName = true switch
		{
#if USE_METAL
			_ when OperatingSystem.IsMacOS() => $"{libminifb}-metal",
#endif
#if USE_WAYLAND
			// TODO: before we can use Wayland, we need to fix it in the native library: https://github.com/fruediger/minifb-native/blob/master/src/wayland/WaylandMiniFB.c
			_ when OperatingSystem.IsLinux() => $"{libminifb}-wayland",
#endif
			_ => libminifb
		}; // at first, initialize 'mLibraryName' with the name of a specialized library (which we unconditionally prefer), when we're on a appropriate platform...

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static (string? libraryName, DllImportSearchPath? searchPath) GetLibraryNameAndSearchPath()
			=> (mLibraryName, DefaultSearchPath);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static bool HandleLibraryImportError(string? libraryName, DllImportSearchPath? searchPath, ExceptionDispatchInfo? libraryLoadErrorInfo)
		{
			// this feels a bit hacky, but it get's the job done
			//
			// This works, because a library can't be loaded if one or more of it's dependencies can't be loaded.
			// For example: On Linux, first we try to load 'libminifb-wayland'. If the dependend Wayland libraries do not exist on the executing Linux platform,
			// or can't get loaded, we try again. But this time we try to load 'libminifb' (which itself depends on X instead of Wayland).

			if (libraryName is not (null or libminifb))
			{
				// ...if that doesn't work and we tried with the name of a specialized library, then we'll try again with the name of the unspecialized library

				mLibraryName = libminifb;

				return true;
			}

			libraryLoadErrorInfo?.Throw();

			mLibraryName = null;

			return false;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	private readonly struct BuildVersion { public readonly byte Major, Minor, Patch; private readonly byte _Reserved; }

	[NativeImportSymbol<MiniFB.Library>(Kind = NativeImportSymbolKind.Reference)]
	private static partial ref readonly BuildVersion mfb_build_version();

	[NativeImportSymbol<MiniFB.Library>(Kind = NativeImportSymbolKind.Reference)]
	private static partial ref readonly byte mfb_build_variant();

	[NativeImportSymbol<MiniFB.Library>(Kind = NativeImportSymbolKind.Reference)]
	private static partial ref readonly bool g_use_hardware_sync();

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl)])]
	private static partial void mfb_set_target_fps(uint fps);

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static partial uint mfb_get_target_fps();

	[NativeImportFunction<MiniFB.Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
	private static unsafe partial byte* mfb_get_key_name(Key key);

	private static SpinLock mVariantLock; // no need to initialize since SpinLock is a value type anyways (though, the only real difference is that this enables thread owner tracking)
	private static volatile string? mVariant;

	private static SpinLock mVersionLock; // no need to initialize since SpinLock is a value type anyways (though, the only real difference is that this enables thread owner tracking)
	private static volatile Version? mVersion;

	/// <summary>Gets or sets the target frames per second (fps) for the application</summary>
	/// <value>The application-wide target frames per second (fps)</value>
	/// <remarks>If you want to virtually unlimit the target frames per second for the application, try setting <see cref="TargetFPS">TargetFPS</see> to <see cref="uint.MaxValue"/></remarks>
	public static uint TargetFPS { get => mfb_get_target_fps(); set => mfb_set_target_fps(value); }

	/// <summary>Gets a value indicating if <see cref="Window">Window</see>s are using a hardware based approach when <see cref="Window.WaitForSync">synchronizing</see> instead of a software based one</summary>
	/// <value>A value indicating if <see cref="Window">Window</see>s are using a hardware based approach when <see cref="Window.WaitForSync">synchronizing</see> instead of a software based one</value>
	public static bool UsesHardwareSync => g_use_hardware_sync();

	/// <summary>Gets a string indicating the build variant of the underlying native MiniFB libary</summary>
	/// <value>A string indicating the build variant of the underlying native MiniFB libary</value>
	/// <remarks>
	/// The build variant string consists of words separated by hyphens (<c>'-'</c>) which indicate various properties of the underlying native MiniFB libary:
	/// <list type="bullet">
	///		<item><description>If the library targets Android, the build variant string starts with the word <c>"android"</c></description></item>
	///		<item><description>If the library targets Windows, the build variant string starts with the word <c>"windows"</c></description></item>
	///		<item><description>If the library targets iOS, the build variant string starts with the word <c>"ios"</c></description></item>
	///		<item><description>If the library targets MacOS, the build variant string starts with the word <c>"macos"</c></description></item>
	///		<item><description>If the library targets Linux/Unix, the build variant string starts with the word <c>"unix"</c></description></item>
	///		<item><description>If the library targets Wasm (via Emscripten), the build variant string starts with the word <c>"emscripten"</c></description></item>
	///		<item><description>If the library uses the OpenGL API, the build variant string contains the word <c>"opengl"</c></description></item>
	///		<item><description>If the library uses Apple's Metal API, the build variant string contains the word <c>"metal"</c></description></item>
	///		<item><description>If the library uses the Wayland API, the build variant string contains the word <c>"wayland"</c></description></item>
	///		<item><description>If the library virtually inverts the Y-axis of mouse inputs (on MacOS), the build variant string contains the word <c>"invertedy"</c></description></item>
	///		<item><description>If the library is a debug build, the build variant string ends in the word <c>"debug"</c></description></item>
	/// </list>
	/// </remarks>
	public static string Variant
	{
		get
		{
			return mVariant ?? synchronouslyCreateVariantFromNativeBuildVariant();

			static unsafe string synchronouslyCreateVariantFromNativeBuildVariant()
			{
				var lockTaken = false;
				try
				{
					mVariantLock.Enter(ref lockTaken);

					if (mVariant is not null)
					{
						return mVariant;
					}

					return mVariant = Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)Unsafe.AsPointer(ref Unsafe.AsRef(in mfb_build_variant()))));
				}
				finally
				{
					if (lockTaken)
					{
						mVariantLock.Exit();
					}
				}
			}
		}
	}

	/// <summary>Gets the build version of the underlying native MiniFB libary</summary>
	/// <value>The build version of the underlying native MiniFB libary</value>
	public static Version Version
	{
		get
		{
			return mVersion ?? synchronouslyCreateVersionFromNativeBuildVersion();

			static Version synchronouslyCreateVersionFromNativeBuildVersion()
			{
				var lockTaken = false;
				try
				{
					mVersionLock.Enter(ref lockTaken);

					if (mVersion is not null)
					{
						return mVersion;
					}

					var version = mfb_build_version();

					return mVersion = new(version.Major, version.Minor, version.Patch);
				}
				finally
				{
					if (lockTaken)
					{
						mVersionLock.Exit();
					}
				}
			}
		}
	}

	/// <summary>Gets a commonly known name for a <see cref="Key"/></summary>
	/// <param name="key">The <see cref="Key"/> whose commonly known name shouldk be obtained</param>
	/// <returns>A commonly known name for the given <paramref name="key"/></returns>
	/// <remarks>Don't call this method too excessively, as it performs some decoding operations on each call. Instead try to cache to returned <see cref="string"/> values.</remarks>
	public static string GetName(this Key key)
	{
		unsafe
		{
			return Encoding.UTF8.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(mfb_get_key_name(key)));
		}
	}
}
